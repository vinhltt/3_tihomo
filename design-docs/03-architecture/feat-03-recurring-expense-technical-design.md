# FEAT-03: RECURRING EXPENSE MANAGEMENT - TECHNICAL DESIGN

**Document Version:** 1.0  
**Created:** 2025-01-17  
**Feature:** Quản lý Chi tiêu Định kỳ & Lập lịch Job  
**Epic 1:** Core Recurring Transaction Management  

## 📋 TECHNICAL OVERVIEW

### Architecture Context
- **Service:** CoreFinance.Api (existing microservice)
- **Database:** PostgreSQL với Entity Framework Core 9
- **Background Jobs:** Hangfire hoặc Quartz.NET
- **Authentication:** JWT với existing Identity service
- **Frontend:** Nuxt 3 integration

### Design Principles
- **Clean Architecture** với Domain-Driven Design
- **CQRS pattern** cho complex read/write operations
- **Repository pattern** với Unit of Work
- **Domain events** cho business logic
- **Resilience patterns** cho external service calls

---

## 🗂️ DATABASE DESIGN

### Core Tables Schema

#### 1. recurring_transactions
```sql
CREATE TABLE recurring_transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,
    user_id UUID NOT NULL,
    account_id UUID NOT NULL REFERENCES accounts(id),
    category_id UUID REFERENCES transaction_categories(id),
    transaction_type VARCHAR(20) NOT NULL CHECK (transaction_type IN ('Income', 'Expense', 'Transfer')),
    amount DECIMAL(18,2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'VND',
    frequency VARCHAR(20) NOT NULL CHECK (frequency IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly')),
    frequency_interval INTEGER NOT NULL DEFAULT 1,
    start_date DATE NOT NULL,
    end_date DATE NULL,
    next_execution_date DATE NOT NULL,
    last_execution_date DATE NULL,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_auto_execute BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by UUID NOT NULL,
    updated_by UUID NOT NULL,
    
    -- Constraints
    CONSTRAINT chk_amount_positive CHECK (amount > 0),
    CONSTRAINT chk_frequency_interval_positive CHECK (frequency_interval > 0),
    CONSTRAINT chk_end_date_after_start CHECK (end_date IS NULL OR end_date >= start_date)
);

-- Indexes
CREATE INDEX idx_recurring_transactions_user_id ON recurring_transactions(user_id);
CREATE INDEX idx_recurring_transactions_next_execution ON recurring_transactions(next_execution_date) WHERE is_active = true;
CREATE INDEX idx_recurring_transactions_account_id ON recurring_transactions(account_id);
CREATE UNIQUE INDEX idx_recurring_transactions_code ON recurring_transactions(code);
```

#### 2. recurring_transaction_executions
```sql
CREATE TABLE recurring_transaction_executions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    recurring_transaction_id UUID NOT NULL REFERENCES recurring_transactions(id) ON DELETE CASCADE,
    execution_date DATE NOT NULL,
    planned_amount DECIMAL(18,2) NOT NULL,
    actual_amount DECIMAL(18,2) NULL,
    status VARCHAR(20) NOT NULL CHECK (status IN ('Pending', 'Executed', 'Failed', 'Skipped')),
    transaction_id UUID NULL REFERENCES transactions(id),
    notes TEXT,
    error_message TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    -- Constraints  
    CONSTRAINT chk_planned_amount_positive CHECK (planned_amount > 0),
    CONSTRAINT chk_actual_amount_positive CHECK (actual_amount IS NULL OR actual_amount > 0)
);

-- Indexes
CREATE INDEX idx_recurring_executions_recurring_id ON recurring_transaction_executions(recurring_transaction_id);
CREATE INDEX idx_recurring_executions_execution_date ON recurring_transaction_executions(execution_date);
CREATE INDEX idx_recurring_executions_status ON recurring_transaction_executions(status);
```

#### 3. recurring_transaction_templates
```sql
CREATE TABLE recurring_transaction_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    description TEXT,
    default_category_id UUID REFERENCES transaction_categories(id),
    default_amount DECIMAL(18,2),
    default_frequency VARCHAR(20) CHECK (default_frequency IN ('Daily', 'Weekly', 'Monthly', 'Quarterly', 'Yearly')),
    default_transaction_type VARCHAR(20) CHECK (default_transaction_type IN ('Income', 'Expense', 'Transfer')),
    is_system_template BOOLEAN NOT NULL DEFAULT false,
    is_user_template BOOLEAN NOT NULL DEFAULT true,
    created_by UUID NULL, -- NULL for system templates
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    -- Constraints
    CONSTRAINT chk_template_type CHECK (is_system_template != is_user_template)
);

-- Indexes
CREATE INDEX idx_recurring_templates_system ON recurring_transaction_templates(is_system_template);
CREATE INDEX idx_recurring_templates_user ON recurring_transaction_templates(created_by) WHERE is_user_template = true;
```

### Migration Scripts

#### Initial Migration
```csharp
public partial class AddRecurringTransactions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create sequences for business codes
        migrationBuilder.Sql("CREATE SEQUENCE recurring_transaction_code_seq START 1;");
        
        // Create tables (SQL above)
        
        // Create function for auto-generating codes
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION generate_recurring_transaction_code()
            RETURNS VARCHAR(50) AS $$
            BEGIN
                RETURN 'RT' || TO_CHAR(NOW(), 'YYYYMM') || LPAD(nextval('recurring_transaction_code_seq')::TEXT, 6, '0');
            END;
            $$ LANGUAGE plpgsql;
        ");
        
        // Create trigger for auto-updating next_execution_date
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION update_next_execution_date()
            RETURNS TRIGGER AS $$
            BEGIN
                IF TG_OP = 'UPDATE' AND NEW.last_execution_date IS DISTINCT FROM OLD.last_execution_date THEN
                    NEW.next_execution_date := calculate_next_execution_date(
                        NEW.last_execution_date, 
                        NEW.frequency, 
                        NEW.frequency_interval
                    );
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;
        ");
    }
    
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop in reverse order
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS update_next_execution_date();");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS generate_recurring_transaction_code();");
        migrationBuilder.Sql("DROP SEQUENCE IF EXISTS recurring_transaction_code_seq;");
        
        migrationBuilder.DropTable("recurring_transaction_executions");
        migrationBuilder.DropTable("recurring_transaction_templates");
        migrationBuilder.DropTable("recurring_transactions");
    }
}
```

---

## 🏗️ DOMAIN MODEL DESIGN

### Core Entities

#### 1. RecurringTransaction (Aggregate Root)
```csharp
namespace CoreFinance.Domain.Entities
{
    public class RecurringTransaction : UserOwnedEntity
    {
        public string Code { get; private set; } = string.Empty;
        public Guid AccountId { get; private set; }
        public Guid? CategoryId { get; private set; }
        public TransactionType TransactionType { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "VND";
        public RecurrenceFrequency Frequency { get; private set; }
        public int FrequencyInterval { get; private set; } = 1;
        public DateOnly StartDate { get; private set; }
        public DateOnly? EndDate { get; private set; }
        public DateOnly NextExecutionDate { get; private set; }
        public DateOnly? LastExecutionDate { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool IsAutoExecute { get; private set; } = false;
        
        // Navigation properties
        public Account Account { get; private set; } = null!;
        public TransactionCategory? Category { get; private set; }
        public ICollection<RecurringTransactionExecution> Executions { get; private set; } = new List<RecurringTransactionExecution>();
        
        // Domain events
        public static RecurringTransaction Create(
            string title,
            decimal amount,
            TransactionType transactionType,
            Guid accountId,
            RecurrenceFrequency frequency,
            DateOnly startDate,
            Guid userId,
            Guid? categoryId = null,
            string? description = null,
            DateOnly? endDate = null,
            bool isAutoExecute = false)
        {
            var recurring = new RecurringTransaction
            {
                Id = Guid.NewGuid(),
                Title = title,
                Amount = amount,
                TransactionType = transactionType,
                AccountId = accountId,
                CategoryId = categoryId,
                Frequency = frequency,
                StartDate = startDate,
                EndDate = endDate,
                Description = description,
                IsAutoExecute = isAutoExecute,
                UserId = userId,
                CreatedBy = userId,
                UpdatedBy = userId
            };
            
            recurring.Code = recurring.GenerateCode();
            recurring.NextExecutionDate = recurring.CalculateNextExecutionDate();
            
            // Raise domain event
            recurring.AddDomainEvent(new RecurringTransactionCreatedEvent(recurring));
            
            return recurring;
        }
        
        public void UpdateDetails(
            string title,
            decimal amount,
            RecurrenceFrequency frequency,
            int frequencyInterval = 1,
            Guid? categoryId = null,
            string? description = null,
            DateOnly? endDate = null)
        {
            Title = title;
            Amount = amount;
            CategoryId = categoryId;
            Description = description;
            EndDate = endDate;
            
            // If frequency changed, recalculate next execution
            if (Frequency != frequency || FrequencyInterval != frequencyInterval)
            {
                Frequency = frequency;
                FrequencyInterval = frequencyInterval;
                NextExecutionDate = CalculateNextExecutionDate();
                
                AddDomainEvent(new RecurringTransactionFrequencyChangedEvent(this));
            }
            
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new RecurringTransactionUpdatedEvent(this));
        }
        
        public void Disable()
        {
            IsActive = false;
            AddDomainEvent(new RecurringTransactionDisabledEvent(this));
        }
        
        public void Enable()
        {
            IsActive = true;
            NextExecutionDate = CalculateNextExecutionDate();
            AddDomainEvent(new RecurringTransactionEnabledEvent(this));
        }
        
        public RecurringTransactionExecution CreateExecution(DateOnly executionDate)
        {
            var execution = RecurringTransactionExecution.Create(
                Id, 
                executionDate, 
                Amount,
                UserId
            );
            
            Executions.Add(execution);
            AddDomainEvent(new RecurringTransactionExecutionCreatedEvent(this, execution));
            
            return execution;
        }
        
        public void MarkExecuted(DateOnly executionDate, Guid transactionId, decimal actualAmount)
        {
            LastExecutionDate = executionDate;
            NextExecutionDate = CalculateNextExecutionDate();
            
            AddDomainEvent(new RecurringTransactionExecutedEvent(this, transactionId, actualAmount));
        }
        
        private string GenerateCode()
        {
            // This will be handled by database function
            return $"RT{DateTime.Now:yyyyMM}000000";
        }
        
        private DateOnly CalculateNextExecutionDate()
        {
            var baseDate = LastExecutionDate ?? StartDate;
            
            return Frequency switch
            {
                RecurrenceFrequency.Daily => baseDate.AddDays(FrequencyInterval),
                RecurrenceFrequency.Weekly => baseDate.AddDays(FrequencyInterval * 7),
                RecurrenceFrequency.Monthly => baseDate.AddMonths(FrequencyInterval),
                RecurrenceFrequency.Quarterly => baseDate.AddMonths(FrequencyInterval * 3),
                RecurrenceFrequency.Yearly => baseDate.AddYears(FrequencyInterval),
                _ => throw new InvalidOperationException($"Unknown frequency: {Frequency}")
            };
        }
        
        public bool ShouldExecuteOn(DateOnly date)
        {
            return IsActive && 
                   NextExecutionDate <= date && 
                   (EndDate == null || date <= EndDate);
        }
    }
}
```

#### 2. Value Objects & Enums
```csharp
namespace CoreFinance.Domain.Enums
{
    public enum RecurrenceFrequency
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Quarterly = 4,
        Yearly = 5
    }
    
    public enum ExecutionStatus
    {
        Pending = 1,
        Executed = 2,
        Failed = 3,
        Skipped = 4
    }
}

namespace CoreFinance.Domain.ValueObjects
{
    public record RecurrencePattern(
        RecurrenceFrequency Frequency,
        int Interval,
        DateOnly StartDate,
        DateOnly? EndDate)
    {
        public DateOnly CalculateNext(DateOnly? lastExecution = null)
        {
            var baseDate = lastExecution ?? StartDate;
            
            return Frequency switch
            {
                RecurrenceFrequency.Daily => baseDate.AddDays(Interval),
                RecurrenceFrequency.Weekly => baseDate.AddDays(Interval * 7),
                RecurrenceFrequency.Monthly => baseDate.AddMonths(Interval),
                RecurrenceFrequency.Quarterly => baseDate.AddMonths(Interval * 3),
                RecurrenceFrequency.Yearly => baseDate.AddYears(Interval),
                _ => throw new InvalidOperationException($"Unknown frequency: {Frequency}")
            };
        }
        
        public IEnumerable<DateOnly> GenerateSchedule(DateOnly fromDate, DateOnly toDate)
        {
            var current = StartDate;
            
            while (current <= toDate && (EndDate == null || current <= EndDate))
            {
                if (current >= fromDate)
                    yield return current;
                    
                current = CalculateNext(current);
            }
        }
    }
}
```

---

## 🔧 REPOSITORY & SERVICE DESIGN

### Repository Interfaces
```csharp
namespace CoreFinance.Domain.Repositories
{
    public interface IRecurringTransactionRepository : IBaseRepository<RecurringTransaction>
    {
        Task<RecurringTransaction?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<IEnumerable<RecurringTransaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RecurringTransaction>> GetDueForExecutionAsync(DateOnly date, CancellationToken cancellationToken = default);
        Task<IEnumerable<RecurringTransaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
        
        // Query methods for complex scenarios
        Task<PagedResult<RecurringTransaction>> SearchAsync(
            RecurringTransactionSearchCriteria criteria, 
            CancellationToken cancellationToken = default);
            
        Task<IEnumerable<RecurringTransaction>> GetForeccastAsync(
            Guid userId, 
            DateOnly fromDate, 
            DateOnly toDate, 
            CancellationToken cancellationToken = default);
    }
    
    public interface IRecurringTransactionExecutionRepository : IBaseRepository<RecurringTransactionExecution>
    {
        Task<IEnumerable<RecurringTransactionExecution>> GetByRecurringTransactionIdAsync(
            Guid recurringTransactionId, 
            CancellationToken cancellationToken = default);
            
        Task<IEnumerable<RecurringTransactionExecution>> GetPendingExecutionsAsync(
            DateOnly? beforeDate = null, 
            CancellationToken cancellationToken = default);
            
        Task<RecurringTransactionExecution?> GetByTransactionIdAsync(
            Guid transactionId, 
            CancellationToken cancellationToken = default);
    }
}
```

### Application Services
```csharp
namespace CoreFinance.Application.Services
{
    public interface IRecurringTransactionService : IBaseService<RecurringTransaction, RecurringTransactionDto>
    {
        Task<RecurringTransactionDto> CreateAsync(RecurringTransactionCreateRequest request, CancellationToken cancellationToken = default);
        Task<RecurringTransactionDto> UpdateAsync(Guid id, RecurringTransactionUpdateRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<RecurringTransactionDto>> SearchAsync(RecurringTransactionSearchRequest request, CancellationToken cancellationToken = default);
        Task<IEnumerable<RecurringTransactionDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ForecastDto>> GetForecastAsync(ForecastRequest request, CancellationToken cancellationToken = default);
        Task<TransactionDto> ExecuteNowAsync(Guid id, CancellationToken cancellationToken = default);
        Task DisableAsync(Guid id, CancellationToken cancellationToken = default);
        Task EnableAsync(Guid id, CancellationToken cancellationToken = default);
    }
    
    public class RecurringTransactionService : BaseService<RecurringTransaction, RecurringTransactionDto>, IRecurringTransactionService
    {
        private readonly IRecurringTransactionRepository _recurringRepository;
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;
        private readonly IMediator _mediator;
        
        public RecurringTransactionService(
            IRecurringTransactionRepository recurringRepository,
            ITransactionService transactionService,
            IAccountService accountService,
            IMediator mediator,
            IMapper mapper,
            IUserRequest userRequest) 
            : base(recurringRepository, mapper, userRequest)
        {
            _recurringRepository = recurringRepository;
            _transactionService = transactionService;
            _accountService = accountService;
            _mediator = mediator;
        }
        
        public async Task<RecurringTransactionDto> CreateAsync(RecurringTransactionCreateRequest request, CancellationToken cancellationToken = default)
        {
            // Validate account ownership
            await _accountService.ValidateUserOwnershipAsync(request.AccountId, _userRequest.UserId, cancellationToken);
            
            // Create domain entity
            var recurring = RecurringTransaction.Create(
                request.Title,
                request.Amount,
                request.TransactionType,
                request.AccountId,
                request.Frequency,
                request.StartDate,
                _userRequest.UserId,
                request.CategoryId,
                request.Description,
                request.EndDate,
                request.IsAutoExecute
            );
            
            // Save to repository
            await _recurringRepository.AddAsync(recurring, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return _mapper.Map<RecurringTransactionDto>(recurring);
        }
        
        public async Task<TransactionDto> ExecuteNowAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var recurring = await _recurringRepository.GetByIdAsync(id, cancellationToken);
            if (recurring == null)
                throw new NotFoundException($"Recurring transaction with ID {id} not found");
                
            // Validate user ownership
            if (recurring.UserId != _userRequest.UserId)
                throw new ForbiddenException("Access denied");
            
            // Create transaction
            var transactionRequest = new TransactionCreateRequest
            {
                AccountId = recurring.AccountId,
                Amount = recurring.Amount,
                TransactionType = recurring.TransactionType,
                CategoryId = recurring.CategoryId,
                Description = $"Manual execution: {recurring.Title}",
                TransactionDate = DateOnly.FromDateTime(DateTime.Today)
            };
            
            var transaction = await _transactionService.CreateAsync(transactionRequest, cancellationToken);
            
            // Update recurring transaction
            recurring.MarkExecuted(DateOnly.FromDateTime(DateTime.Today), transaction.Id, transaction.Amount);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return transaction;
        }
    }
}
```

---

## 📅 BACKGROUND JOBS ARCHITECTURE

### Job Scheduler Design
```csharp
namespace CoreFinance.Infrastructure.Jobs
{
    public interface IRecurringTransactionJobService
    {
        Task ProcessDailyExecutionsAsync(CancellationToken cancellationToken = default);
        Task SendUpcomingPaymentNotificationsAsync(CancellationToken cancellationToken = default);
        Task GenerateWeeklyAnalyticsAsync(CancellationToken cancellationToken = default);
    }
    
    [BackgroundJob]
    public class RecurringTransactionJobService : IRecurringTransactionJobService
    {
        private readonly IRecurringTransactionRepository _recurringRepository;
        private readonly IRecurringTransactionExecutionRepository _executionRepository;
        private readonly ITransactionService _transactionService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RecurringTransactionJobService> _logger;
        
        public RecurringTransactionJobService(
            IRecurringTransactionRepository recurringRepository,
            IRecurringTransactionExecutionRepository executionRepository,
            ITransactionService transactionService,
            INotificationService notificationService,
            ILogger<RecurringTransactionJobService> logger)
        {
            _recurringRepository = recurringRepository;
            _executionRepository = executionRepository;
            _transactionService = transactionService;
            _notificationService = notificationService;
            _logger = logger;
        }
        
        [RecurringJob("0 6 * * *")] // Daily at 6 AM
        public async Task ProcessDailyExecutionsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting daily recurring transaction processing");
            
            var today = DateOnly.FromDateTime(DateTime.Today);
            var dueTransactions = await _recurringRepository.GetDueForExecutionAsync(today, cancellationToken);
            
            foreach (var recurring in dueTransactions)
            {
                try
                {
                    if (recurring.IsAutoExecute)
                    {
                        await ExecuteRecurringTransactionAsync(recurring, today, cancellationToken);
                    }
                    else
                    {
                        await CreatePendingExecutionAsync(recurring, today, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process recurring transaction {RecurringId}", recurring.Id);
                    
                    // Create failed execution record
                    await CreateFailedExecutionAsync(recurring, today, ex.Message, cancellationToken);
                }
            }
            
            _logger.LogInformation("Completed daily recurring transaction processing");
        }
        
        [RecurringJob("0 8 * * *")] // Daily at 8 AM
        public async Task SendUpcomingPaymentNotificationsAsync(CancellationToken cancellationToken = default)
        {
            var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var dayAfterTomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
            var in3Days = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
            
            var upcomingTransactions = await _recurringRepository.GetDueForExecutionAsync(tomorrow, cancellationToken);
            upcomingTransactions = upcomingTransactions.Concat(await _recurringRepository.GetDueForExecutionAsync(dayAfterTomorrow, cancellationToken));
            upcomingTransactions = upcomingTransactions.Concat(await _recurringRepository.GetDueForExecutionAsync(in3Days, cancellationToken));
            
            foreach (var recurring in upcomingTransactions.Where(r => !r.IsAutoExecute))
            {
                await _notificationService.SendUpcomingPaymentNotificationAsync(recurring, cancellationToken);
            }
        }
        
        private async Task ExecuteRecurringTransactionAsync(RecurringTransaction recurring, DateOnly executionDate, CancellationToken cancellationToken)
        {
            var transactionRequest = new TransactionCreateRequest
            {
                AccountId = recurring.AccountId,
                Amount = recurring.Amount,
                TransactionType = recurring.TransactionType,
                CategoryId = recurring.CategoryId,
                Description = $"Auto execution: {recurring.Title}",
                TransactionDate = executionDate
            };
            
            var transaction = await _transactionService.CreateAsync(transactionRequest, cancellationToken);
            recurring.MarkExecuted(executionDate, transaction.Id, transaction.Amount);
            
            // Create successful execution record
            var execution = recurring.CreateExecution(executionDate);
            execution.MarkAsExecuted(transaction.Id, transaction.Amount);
            
            await _executionRepository.AddAsync(execution, cancellationToken);
        }
        
        private async Task CreatePendingExecutionAsync(RecurringTransaction recurring, DateOnly executionDate, CancellationToken cancellationToken)
        {
            var execution = recurring.CreateExecution(executionDate);
            await _executionRepository.AddAsync(execution, cancellationToken);
        }
        
        private async Task CreateFailedExecutionAsync(RecurringTransaction recurring, DateOnly executionDate, string errorMessage, CancellationToken cancellationToken)
        {
            var execution = recurring.CreateExecution(executionDate);
            execution.MarkAsFailed(errorMessage);
            
            await _executionRepository.AddAsync(execution, cancellationToken);
        }
    }
}
```

### Job Configuration
```csharp
namespace CoreFinance.Api.Configuration
{
    public static class BackgroundJobsConfiguration
    {
        public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
        {
            // Option 1: Hangfire
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(configuration.GetConnectionString("DefaultConnection")));
                
            services.AddHangfireServer();
            
            // Option 2: Quartz.NET (alternative)
            // services.Configure<QuartzOptions>(configuration.GetSection("Quartz"));
            // services.AddQuartz(q =>
            // {
            //     q.UseMicrosoftDependencyInjection();
            //     q.UsePostgreSqlStorage(configuration.GetConnectionString("DefaultConnection"));
            // });
            // services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            
            // Register job services
            services.AddScoped<IRecurringTransactionJobService, RecurringTransactionJobService>();
            
            return services;
        }
        
        public static IApplicationBuilder UseBackgroundJobs(this IApplicationBuilder app)
        {
            // Configure Hangfire dashboard
            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });
            
            // Schedule recurring jobs
            var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();
            
            recurringJobManager.AddOrUpdate<IRecurringTransactionJobService>(
                "daily-executions",
                service => service.ProcessDailyExecutionsAsync(CancellationToken.None),
                "0 6 * * *", // Daily at 6 AM
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                
            recurringJobManager.AddOrUpdate<IRecurringTransactionJobService>(
                "upcoming-notifications",
                service => service.SendUpcomingPaymentNotificationsAsync(CancellationToken.None),
                "0 8 * * *", // Daily at 8 AM
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                
            recurringJobManager.AddOrUpdate<IRecurringTransactionJobService>(
                "weekly-analytics",
                service => service.GenerateWeeklyAnalyticsAsync(CancellationToken.None),
                "0 9 * * 0", // Weekly on Sunday at 9 AM
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            
            return app;
        }
    }
}
```

---

## 📱 API SPECIFICATION

### Controller Design
```csharp
namespace CoreFinance.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class RecurringTransactionsController : ControllerBase
    {
        private readonly IRecurringTransactionService _recurringTransactionService;
        private readonly IMediator _mediator;
        
        public RecurringTransactionsController(
            IRecurringTransactionService recurringTransactionService,
            IMediator mediator)
        {
            _recurringTransactionService = recurringTransactionService;
            _mediator = mediator;
        }
        
        /// <summary>
        /// Lấy danh sách giao dịch định kỳ / Get recurring transactions list
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<RecurringTransactionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<RecurringTransactionDto>>> GetRecurringTransactions(
            [FromQuery] RecurringTransactionSearchRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _recurringTransactionService.SearchAsync(request, cancellationToken);
            return Ok(result);
        }
        
        /// <summary>
        /// Lấy chi tiết giao dịch định kỳ / Get recurring transaction details
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RecurringTransactionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecurringTransactionDto>> GetRecurringTransaction(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _recurringTransactionService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }
        
        /// <summary>
        /// Tạo giao dịch định kỳ mới / Create new recurring transaction
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RecurringTransactionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecurringTransactionDto>> CreateRecurringTransaction(
            [FromBody] RecurringTransactionCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _recurringTransactionService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetRecurringTransaction), new { id = result.Id }, result);
        }
        
        /// <summary>
        /// Cập nhật giao dịch định kỳ / Update recurring transaction
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(RecurringTransactionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RecurringTransactionDto>> UpdateRecurringTransaction(
            Guid id,
            [FromBody] RecurringTransactionUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _recurringTransactionService.UpdateAsync(id, request, cancellationToken);
            return Ok(result);
        }
        
        /// <summary>
        /// Xóa giao dịch định kỳ / Delete recurring transaction
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRecurringTransaction(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await _recurringTransactionService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        
        /// <summary>
        /// Thực hiện giao dịch định kỳ ngay lập tức / Execute recurring transaction immediately
        /// </summary>
        [HttpPost("{id}/execute-now")]
        [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TransactionDto>> ExecuteNow(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _recurringTransactionService.ExecuteNowAsync(id, cancellationToken);
            return Ok(result);
        }
        
        /// <summary>
        /// Vô hiệu hóa giao dịch định kỳ / Disable recurring transaction
        /// </summary>
        [HttpPost("{id}/disable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Disable(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await _recurringTransactionService.DisableAsync(id, cancellationToken);
            return Ok();
        }
        
        /// <summary>
        /// Kích hoạt giao dịch định kỳ / Enable recurring transaction
        /// </summary>
        [HttpPost("{id}/enable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Enable(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            await _recurringTransactionService.EnableAsync(id, cancellationToken);
            return Ok();
        }
        
        /// <summary>
        /// Dự báo dòng tiền từ giao dịch định kỳ / Forecast cash flow from recurring transactions
        /// </summary>
        [HttpGet("{id}/forecast")]
        [ProducesResponseType(typeof(IEnumerable<ForecastDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ForecastDto>>> GetForecast(
            Guid id,
            [FromQuery] int months = 6,
            CancellationToken cancellationToken = default)
        {
            var request = new ForecastRequest { RecurringTransactionId = id, Months = months };
            var result = await _recurringTransactionService.GetForecastAsync(request, cancellationToken);
            return Ok(result);
        }
        
        /// <summary>
        /// Lấy lịch giao dịch định kỳ / Get recurring transactions calendar
        /// </summary>
        [HttpGet("calendar")]
        [ProducesResponseType(typeof(IEnumerable<CalendarEventDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetCalendar(
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate,
            CancellationToken cancellationToken = default)
        {
            var query = new GetRecurringTransactionCalendarQuery
            {
                FromDate = fromDate,
                ToDate = toDate
            };
            
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
```

### DTO Design
```csharp
namespace CoreFinance.Application.DTOs.RecurringTransaction
{
    public record RecurringTransactionDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public Guid AccountId { get; init; }
        public string AccountName { get; init; } = string.Empty;
        public Guid? CategoryId { get; init; }
        public string? CategoryName { get; init; }
        public TransactionType TransactionType { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public RecurrenceFrequency Frequency { get; init; }
        public int FrequencyInterval { get; init; }
        public DateOnly StartDate { get; init; }
        public DateOnly? EndDate { get; init; }
        public DateOnly NextExecutionDate { get; init; }
        public DateOnly? LastExecutionDate { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IsActive { get; init; }
        public bool IsAutoExecute { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public IEnumerable<RecurringTransactionExecutionDto> RecentExecutions { get; init; } = new List<RecurringTransactionExecutionDto>();
    }
    
    public record RecurringTransactionCreateRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; init; } = string.Empty;
        
        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; init; }
        
        [Required]
        public TransactionType TransactionType { get; init; }
        
        [Required]
        public Guid AccountId { get; init; }
        
        public Guid? CategoryId { get; init; }
        
        [Required]
        public RecurrenceFrequency Frequency { get; init; }
        
        [Range(1, 365)]
        public int FrequencyInterval { get; init; } = 1;
        
        [Required]
        public DateOnly StartDate { get; init; }
        
        public DateOnly? EndDate { get; init; }
        
        [MaxLength(1000)]
        public string? Description { get; init; }
        
        public bool IsAutoExecute { get; init; } = false;
    }
    
    public record RecurringTransactionUpdateRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; init; } = string.Empty;
        
        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; init; }
        
        public Guid? CategoryId { get; init; }
        
        [Required]
        public RecurrenceFrequency Frequency { get; init; }
        
        [Range(1, 365)]
        public int FrequencyInterval { get; init; } = 1;
        
        public DateOnly? EndDate { get; init; }
        
        [MaxLength(1000)]
        public string? Description { get; init; }
        
        public bool IsAutoExecute { get; init; }
    }
    
    public record RecurringTransactionSearchRequest : PagedRequest
    {
        public TransactionType? TransactionType { get; init; }
        public Guid? AccountId { get; init; }
        public Guid? CategoryId { get; init; }
        public bool? IsActive { get; init; }
        public RecurrenceFrequency? Frequency { get; init; }
        public string? SearchTerm { get; init; }
        public DateOnly? StartDateFrom { get; init; }
        public DateOnly? StartDateTo { get; init; }
        public DateOnly? NextExecutionFrom { get; init; }
        public DateOnly? NextExecutionTo { get; init; }
        public decimal? AmountFrom { get; init; }
        public decimal? AmountTo { get; init; }
    }
    
    public record ForecastRequest
    {
        public Guid? RecurringTransactionId { get; init; }
        public Guid? AccountId { get; init; }
        public int Months { get; init; } = 6;
        public DateOnly? FromDate { get; init; }
        public DateOnly? ToDate { get; init; }
    }
    
    public record ForecastDto
    {
        public DateOnly Date { get; init; }
        public decimal Amount { get; init; }
        public TransactionType TransactionType { get; init; }
        public string Title { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public string? CategoryName { get; init; }
        public Guid RecurringTransactionId { get; init; }
        public bool IsAutoExecute { get; init; }
    }
    
    public record CalendarEventDto
    {
        public Guid Id { get; init; }
        public DateOnly Date { get; init; }
        public string Title { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public TransactionType TransactionType { get; init; }
        public string AccountName { get; init; } = string.Empty;
        public bool IsAutoExecute { get; init; }
        public bool IsExecuted { get; init; }
    }
}
```

---

## 🔗 INTEGRATION DESIGN

### Service Integration Points
```csharp
namespace CoreFinance.Application.Integration
{
    public interface IAccountIntegrationService
    {
        Task<AccountDto> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task ValidateAccountOwnershipAsync(Guid accountId, Guid userId, CancellationToken cancellationToken = default);
        Task<decimal> GetAccountBalanceAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task<bool> HasSufficientBalanceAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default);
    }
    
    public interface ICategoryIntegrationService
    {
        Task<TransactionCategoryDto> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TransactionCategoryDto>> GetUserCategoriesAsync(Guid userId, CancellationToken cancellationToken = default);
    }
    
    public interface INotificationIntegrationService
    {
        Task SendUpcomingPaymentNotificationAsync(RecurringTransaction recurring, CancellationToken cancellationToken = default);
        Task SendFailedExecutionNotificationAsync(RecurringTransaction recurring, string errorMessage, CancellationToken cancellationToken = default);
        Task SendMonthlyRecurringSummaryAsync(Guid userId, IEnumerable<RecurringTransactionExecutionDto> executions, CancellationToken cancellationToken = default);
    }
}
```

### Domain Events Design
```csharp
namespace CoreFinance.Domain.Events
{
    public record RecurringTransactionCreatedEvent(RecurringTransaction RecurringTransaction) : IDomainEvent;
    
    public record RecurringTransactionUpdatedEvent(RecurringTransaction RecurringTransaction) : IDomainEvent;
    
    public record RecurringTransactionDeletedEvent(Guid RecurringTransactionId, Guid UserId) : IDomainEvent;
    
    public record RecurringTransactionExecutedEvent(RecurringTransaction RecurringTransaction, Guid TransactionId, decimal ActualAmount) : IDomainEvent;
    
    public record RecurringTransactionExecutionFailedEvent(RecurringTransaction RecurringTransaction, string ErrorMessage) : IDomainEvent;
    
    public record RecurringTransactionFrequencyChangedEvent(RecurringTransaction RecurringTransaction) : IDomainEvent;
    
    public record RecurringTransactionDisabledEvent(RecurringTransaction RecurringTransaction) : IDomainEvent;
    
    public record RecurringTransactionEnabledEvent(RecurringTransaction RecurringTransaction) : IDomainEvent;
}

namespace CoreFinance.Application.EventHandlers
{
    public class RecurringTransactionEventHandlers : 
        INotificationHandler<RecurringTransactionCreatedEvent>,
        INotificationHandler<RecurringTransactionExecutedEvent>,
        INotificationHandler<RecurringTransactionExecutionFailedEvent>
    {
        private readonly INotificationIntegrationService _notificationService;
        private readonly ILogger<RecurringTransactionEventHandlers> _logger;
        
        public RecurringTransactionEventHandlers(
            INotificationIntegrationService notificationService,
            ILogger<RecurringTransactionEventHandlers> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }
        
        public async Task Handle(RecurringTransactionCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recurring transaction created: {RecurringId}", notification.RecurringTransaction.Id);
            
            // Additional business logic như audit logging, analytics, etc.
        }
        
        public async Task Handle(RecurringTransactionExecutedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recurring transaction executed: {RecurringId}, Transaction: {TransactionId}", 
                notification.RecurringTransaction.Id, notification.TransactionId);
                
            // Update analytics, send notifications, etc.
        }
        
        public async Task Handle(RecurringTransactionExecutionFailedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Recurring transaction execution failed: {RecurringId}, Error: {Error}", 
                notification.RecurringTransaction.Id, notification.ErrorMessage);
                
            await _notificationService.SendFailedExecutionNotificationAsync(
                notification.RecurringTransaction, 
                notification.ErrorMessage, 
                cancellationToken);
        }
    }
}
```

---

## 🧪 TESTING STRATEGY

### Unit Testing Design
```csharp
namespace CoreFinance.Tests.Unit.Domain
{
    public class RecurringTransactionTests
    {
        [Fact]
        public void Create_WithValidData_ShouldCreateRecurringTransaction()
        {
            // Arrange
            var title = "Monthly Rent";
            var amount = 1000000m;
            var transactionType = TransactionType.Expense;
            var accountId = Guid.NewGuid();
            var frequency = RecurrenceFrequency.Monthly;
            var startDate = DateOnly.FromDateTime(DateTime.Today);
            var userId = Guid.NewGuid();
            
            // Act
            var recurring = RecurringTransaction.Create(
                title, amount, transactionType, accountId, frequency, startDate, userId);
            
            // Assert
            recurring.Should().NotBeNull();
            recurring.Title.Should().Be(title);
            recurring.Amount.Should().Be(amount);
            recurring.TransactionType.Should().Be(transactionType);
            recurring.Frequency.Should().Be(frequency);
            recurring.IsActive.Should().BeTrue();
            recurring.Code.Should().NotBeNullOrEmpty();
            recurring.NextExecutionDate.Should().Be(startDate.AddMonths(1));
        }
        
        [Theory]
        [InlineData(RecurrenceFrequency.Daily, 1, 1)]
        [InlineData(RecurrenceFrequency.Weekly, 1, 7)]
        [InlineData(RecurrenceFrequency.Monthly, 1, 30)] // Approximate
        [InlineData(RecurrenceFrequency.Quarterly, 1, 90)] // Approximate
        [InlineData(RecurrenceFrequency.Yearly, 1, 365)] // Approximate
        public void CalculateNextExecutionDate_WithDifferentFrequencies_ShouldCalculateCorrectly(
            RecurrenceFrequency frequency, int interval, int expectedDaysApprox)
        {
            // Arrange
            var startDate = new DateOnly(2024, 1, 1);
            var recurring = RecurringTransaction.Create(
                "Test", 100m, TransactionType.Expense, Guid.NewGuid(), frequency, startDate, Guid.NewGuid());
            
            // Act
            var nextDate = recurring.NextExecutionDate;
            
            // Assert
            var daysDiff = nextDate.DayNumber - startDate.DayNumber;
            
            if (frequency == RecurrenceFrequency.Monthly)
            {
                nextDate.Should().Be(startDate.AddMonths(1));
            }
            else if (frequency == RecurrenceFrequency.Yearly)
            {
                nextDate.Should().Be(startDate.AddYears(1));
            }
            else
            {
                daysDiff.Should().Be(expectedDaysApprox);
            }
        }
        
        [Fact]
        public void Disable_WhenActive_ShouldSetInactiveAndRaiseDomainEvent()
        {
            // Arrange
            var recurring = CreateValidRecurringTransaction();
            
            // Act
            recurring.Disable();
            
            // Assert
            recurring.IsActive.Should().BeFalse();
            recurring.DomainEvents.Should().ContainSingle(e => e is RecurringTransactionDisabledEvent);
        }
        
        [Fact]
        public void Enable_WhenDisabled_ShouldSetActiveAndRecalculateNextExecution()
        {
            // Arrange
            var recurring = CreateValidRecurringTransaction();
            recurring.Disable();
            recurring.ClearDomainEvents();
            var originalNextExecution = recurring.NextExecutionDate;
            
            // Act
            recurring.Enable();
            
            // Assert
            recurring.IsActive.Should().BeTrue();
            recurring.NextExecutionDate.Should().BeAfter(originalNextExecution);
            recurring.DomainEvents.Should().ContainSingle(e => e is RecurringTransactionEnabledEvent);
        }
        
        private static RecurringTransaction CreateValidRecurringTransaction()
        {
            return RecurringTransaction.Create(
                "Test Transaction",
                1000m,
                TransactionType.Expense,
                Guid.NewGuid(),
                RecurrenceFrequency.Monthly,
                DateOnly.FromDateTime(DateTime.Today),
                Guid.NewGuid()
            );
        }
    }
}

namespace CoreFinance.Tests.Unit.Application
{
    public class RecurringTransactionServiceTests
    {
        private readonly Mock<IRecurringTransactionRepository> _mockRepository;
        private readonly Mock<ITransactionService> _mockTransactionService;
        private readonly Mock<IAccountService> _mockAccountService;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUserRequest> _mockUserRequest;
        private readonly RecurringTransactionService _service;
        
        public RecurringTransactionServiceTests()
        {
            _mockRepository = new Mock<IRecurringTransactionRepository>();
            _mockTransactionService = new Mock<ITransactionService>();
            _mockAccountService = new Mock<IAccountService>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockUserRequest = new Mock<IUserRequest>();
            
            _mockUserRequest.Setup(x => x.UserId).Returns(Guid.NewGuid());
            
            _service = new RecurringTransactionService(
                _mockRepository.Object,
                _mockTransactionService.Object,
                _mockAccountService.Object,
                Mock.Of<IMediator>(),
                _mockMapper.Object,
                _mockUserRequest.Object
            );
        }
        
        [Fact]
        public async Task CreateAsync_WithValidRequest_ShouldCreateRecurringTransaction()
        {
            // Arrange
            var request = new RecurringTransactionCreateRequest
            {
                Title = "Monthly Rent",
                Amount = 1000000m,
                TransactionType = TransactionType.Expense,
                AccountId = Guid.NewGuid(),
                Frequency = RecurrenceFrequency.Monthly,
                StartDate = DateOnly.FromDateTime(DateTime.Today)
            };
            
            _mockAccountService.Setup(x => x.ValidateUserOwnershipAsync(
                request.AccountId, _mockUserRequest.Object.UserId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            var expectedDto = new RecurringTransactionDto { Id = Guid.NewGuid() };
            _mockMapper.Setup(x => x.Map<RecurringTransactionDto>(It.IsAny<RecurringTransaction>()))
                .Returns(expectedDto);
            
            // Act
            var result = await _service.CreateAsync(request);
            
            // Assert
            result.Should().Be(expectedDto);
            _mockRepository.Verify(x => x.AddAsync(It.IsAny<RecurringTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ExecuteNowAsync_WithInsufficientBalance_ShouldThrowInsufficientBalanceException()
        {
            // Arrange
            var recurringId = Guid.NewGuid();
            var recurring = CreateMockRecurringTransaction(recurringId);
            
            _mockRepository.Setup(x => x.GetByIdAsync(recurringId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recurring);
                
            _mockTransactionService.Setup(x => x.CreateAsync(It.IsAny<TransactionCreateRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InsufficientBalanceException("Insufficient balance"));
            
            // Act & Assert
            await _service.Invoking(s => s.ExecuteNowAsync(recurringId))
                .Should().ThrowAsync<InsufficientBalanceException>();
        }
        
        private RecurringTransaction CreateMockRecurringTransaction(Guid id)
        {
            var recurring = RecurringTransaction.Create(
                "Test Transaction",
                1000m,
                TransactionType.Expense,
                Guid.NewGuid(),
                RecurrenceFrequency.Monthly,
                DateOnly.FromDateTime(DateTime.Today),
                _mockUserRequest.Object.UserId
            );
            
            // Use reflection to set Id for testing
            typeof(RecurringTransaction).GetProperty("Id")!.SetValue(recurring, id);
            
            return recurring;
        }
    }
}
```

### Integration Testing Design
```csharp
namespace CoreFinance.Tests.Integration.Controllers
{
    public class RecurringTransactionsControllerTests : IntegrationTestBase
    {
        [Fact]
        public async Task CreateRecurringTransaction_WithValidData_ShouldReturn201Created()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var account = await CreateTestAccountAsync(user.Id);
            var category = await CreateTestCategoryAsync(user.Id);
            
            var request = new RecurringTransactionCreateRequest
            {
                Title = "Monthly Internet Bill",
                Amount = 500000m,
                TransactionType = TransactionType.Expense,
                AccountId = account.Id,
                CategoryId = category.Id,
                Frequency = RecurrenceFrequency.Monthly,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                IsAutoExecute = true
            };
            
            // Act
            var response = await AuthenticatedClient.PostAsJsonAsync("/api/v1/recurring-transactions", request);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var result = await response.Content.ReadFromJsonAsync<RecurringTransactionDto>();
            result.Should().NotBeNull();
            result!.Title.Should().Be(request.Title);
            result.Amount.Should().Be(request.Amount);
            result.IsAutoExecute.Should().Be(request.IsAutoExecute);
        }
        
        [Fact]
        public async Task GetRecurringTransactions_WithFilters_ShouldReturnFilteredResults()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var account = await CreateTestAccountAsync(user.Id);
            
            // Create multiple recurring transactions
            await CreateTestRecurringTransactionAsync(user.Id, account.Id, "Monthly Rent", TransactionType.Expense);
            await CreateTestRecurringTransactionAsync(user.Id, account.Id, "Weekly Salary", TransactionType.Income);
            await CreateTestRecurringTransactionAsync(user.Id, account.Id, "Quarterly Tax", TransactionType.Expense);
            
            // Act
            var response = await AuthenticatedClient.GetAsync(
                "/api/v1/recurring-transactions?transactionType=Expense&pageSize=10");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<PagedResult<RecurringTransactionDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().HaveCount(2);
            result.Items.Should().OnlyContain(r => r.TransactionType == TransactionType.Expense);
        }
        
        [Fact]
        public async Task ExecuteNow_WithValidRecurringTransaction_ShouldCreateTransactionAndUpdateRecurring()
        {
            // Arrange
            var user = await CreateTestUserAsync();
            var account = await CreateTestAccountAsync(user.Id, initialBalance: 2000000m);
            var recurring = await CreateTestRecurringTransactionAsync(user.Id, account.Id, "Test Payment", TransactionType.Expense, amount: 100000m);
            
            // Act
            var response = await AuthenticatedClient.PostAsync($"/api/v1/recurring-transactions/{recurring.Id}/execute-now", null);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var transaction = await response.Content.ReadFromJsonAsync<TransactionDto>();
            transaction.Should().NotBeNull();
            transaction!.Amount.Should().Be(100000m);
            
            // Verify recurring transaction was updated
            var updatedRecurring = await GetRecurringTransactionAsync(recurring.Id);
            updatedRecurring.LastExecutionDate.Should().Be(DateOnly.FromDateTime(DateTime.Today));
        }
    }
}
```

---

## 📊 PERFORMANCE & MONITORING

### Performance Considerations
```csharp
namespace CoreFinance.Infrastructure.Performance
{
    public class RecurringTransactionPerformanceService
    {
        private readonly IRecurringTransactionRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RecurringTransactionPerformanceService> _logger;
        
        public RecurringTransactionPerformanceService(
            IRecurringTransactionRepository repository,
            IMemoryCache cache,
            ILogger<RecurringTransactionPerformanceService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }
        
        public async Task<IEnumerable<RecurringTransaction>> GetDueForExecutionCachedAsync(
            DateOnly date, 
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"due_executions_{date:yyyy-MM-dd}";
            
            if (_cache.TryGetValue(cacheKey, out IEnumerable<RecurringTransaction>? cached))
            {
                return cached!;
            }
            
            var result = await _repository.GetDueForExecutionAsync(date, cancellationToken);
            
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
            
            return result;
        }
        
        public async Task WarmupCacheAsync(CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var tomorrow = today.AddDays(1);
            var dayAfter = today.AddDays(2);
            
            // Pre-cache next few days
            await Task.WhenAll(
                GetDueForExecutionCachedAsync(today, cancellationToken),
                GetDueForExecutionCachedAsync(tomorrow, cancellationToken),
                GetDueForExecutionCachedAsync(dayAfter, cancellationToken)
            );
        }
    }
}
```

### Monitoring & Metrics
```csharp
namespace CoreFinance.Infrastructure.Monitoring
{
    public class RecurringTransactionMetrics
    {
        private static readonly Counter RecurringTransactionsCreated = Metrics
            .CreateCounter("recurring_transactions_created_total", "Total number of recurring transactions created");
            
        private static readonly Counter RecurringTransactionsExecuted = Metrics
            .CreateCounter("recurring_transactions_executed_total", "Total number of recurring transactions executed", new[] { "execution_type" });
            
        private static readonly Counter RecurringTransactionExecutionsFailed = Metrics
            .CreateCounter("recurring_transaction_executions_failed_total", "Total number of failed recurring transaction executions", new[] { "error_type" });
            
        private static readonly Histogram RecurringTransactionExecutionDuration = Metrics
            .CreateHistogram("recurring_transaction_execution_duration_seconds", "Duration of recurring transaction execution");
            
        private static readonly Gauge ActiveRecurringTransactions = Metrics
            .CreateGauge("active_recurring_transactions", "Number of active recurring transactions");
        
        public static void IncrementCreated() => RecurringTransactionsCreated.Inc();
        
        public static void IncrementExecuted(string executionType) => RecurringTransactionsExecuted.WithLabels(executionType).Inc();
        
        public static void IncrementExecutionFailed(string errorType) => RecurringTransactionExecutionsFailed.WithLabels(errorType).Inc();
        
        public static IDisposable MeasureExecutionDuration() => RecurringTransactionExecutionDuration.NewTimer();
        
        public static void SetActiveCount(double count) => ActiveRecurringTransactions.Set(count);
    }
}
```

---

## 🔒 SECURITY & VALIDATION

### Security Considerations
```csharp
namespace CoreFinance.Api.Security
{
    public class RecurringTransactionAuthorizationHandler : AuthorizationHandler<ResourceOwnershipRequirement, RecurringTransaction>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ResourceOwnershipRequirement requirement,
            RecurringTransaction resource)
        {
            var userId = context.User.GetUserId();
            
            if (resource.UserId == userId)
            {
                context.Succeed(requirement);
            }
            
            return Task.CompletedTask;
        }
    }
    
    public class RecurringTransactionValidator : AbstractValidator<RecurringTransactionCreateRequest>
    {
        public RecurringTransactionValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200)
                .Matches(@"^[a-zA-Z0-9\s\-_.,()]+$") // Prevent XSS
                .WithMessage("Title contains invalid characters");
                
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .LessThanOrEqualTo(1_000_000_000m)
                .WithMessage("Amount must be between 0 and 1 billion");
                
            RuleFor(x => x.FrequencyInterval)
                .GreaterThan(0)
                .LessThanOrEqualTo(365)
                .WithMessage("Frequency interval must be between 1 and 365");
                
            RuleFor(x => x.StartDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)))
                .WithMessage("Start date cannot be more than 1 day in the past");
                
            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .When(x => x.EndDate.HasValue)
                .WithMessage("End date must be after start date");
                
            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .Matches(@"^[a-zA-Z0-9\s\-_.,()]+$")
                .When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description contains invalid characters");
        }
    }
}
```

---

## 🚀 DEPLOYMENT & CONFIGURATION

### Configuration Settings
```csharp
namespace CoreFinance.Api.Configuration
{
    public class RecurringTransactionSettings
    {
        public const string SectionName = "RecurringTransactionSettings";
        
        public int MaxRecurringTransactionsPerUser { get; set; } = 100;
        public int MaxExecutionRetries { get; set; } = 3;
        public int ExecutionTimeoutMinutes { get; set; } = 5;
        public bool EnableAutoExecution { get; set; } = true;
        public bool EnableNotifications { get; set; } = true;
        public int ForecastMaxMonths { get; set; } = 24;
        public int CacheExpirationMinutes { get; set; } = 30;
        public string JobTimezone { get; set; } = "SE Asia Standard Time";
        public bool EnableMetrics { get; set; } = true;
    }
}

namespace CoreFinance.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRecurringTransactionFeature(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure settings
            services.Configure<RecurringTransactionSettings>(
                configuration.GetSection(RecurringTransactionSettings.SectionName));
            
            // Register repositories
            services.AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>();
            services.AddScoped<IRecurringTransactionExecutionRepository, RecurringTransactionExecutionRepository>();
            
            // Register services
            services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
            services.AddScoped<RecurringTransactionPerformanceService>();
            
            // Register validators
            services.AddTransient<IValidator<RecurringTransactionCreateRequest>, RecurringTransactionValidator>();
            services.AddTransient<IValidator<RecurringTransactionUpdateRequest>, RecurringTransactionUpdateValidator>();
            
            // Register authorization handlers
            services.AddTransient<IAuthorizationHandler, RecurringTransactionAuthorizationHandler>();
            
            // Register background jobs
            services.AddBackgroundJobs(configuration);
            
            return services;
        }
    }
}
```

### Migration Plan
```markdown
## Migration Plan cho Recurring Transaction Feature

### Phase 1: Database Setup (Week 1)
1. Create migration scripts cho recurring transactions tables
2. Setup database indexes và constraints
3. Test migration scripts trên staging environment
4. Performance testing cho database queries

### Phase 2: Core Backend Implementation (Week 2-3)
1. Implement domain entities và repositories
2. Create application services và DTOs
3. Setup API controllers với validation
4. Write unit tests cho core logic

### Phase 3: Background Jobs (Week 4)
1. Setup Hangfire/Quartz.NET infrastructure
2. Implement job processing logic
3. Setup monitoring và error handling
4. Test job execution in staging

### Phase 4: Integration & Testing (Week 5)
1. Integration testing với existing services
2. End-to-end testing scenarios
3. Performance testing và optimization
4. Security testing và validation

### Phase 5: Frontend Integration (Week 6)
1. Create Nuxt 3 pages và components
2. Implement API integration
3. Add UI for recurring transaction management
4. User acceptance testing

### Phase 6: Production Deployment (Week 7)
1. Production database migration
2. Deploy backend services
3. Deploy frontend updates
4. Monitor và gradual rollout
```

---

## 📋 CHECKLIST

### Implementation Checklist
- [ ] Database schema designed và reviewed
- [ ] Entity Framework models implemented
- [ ] Repository pattern implemented
- [ ] Domain services designed
- [ ] API controllers implemented
- [ ] Background jobs configured
- [ ] Integration points identified
- [ ] Security measures implemented
- [ ] Performance optimizations planned
- [ ] Monitoring và metrics setup
- [ ] Testing strategy defined
- [ ] Documentation completed

### Review Checklist
- [ ] Database design follows TiHoMo conventions
- [ ] Clean Architecture principles followed
- [ ] Domain-Driven Design patterns applied
- [ ] Security best practices implemented
- [ ] Performance considerations addressed
- [ ] Error handling comprehensive
- [ ] Integration points well-defined
- [ ] Testing coverage adequate
- [ ] Documentation complete và accurate

---

**Document Status:** ✅ Complete  
**Next Steps:** Review technical design → Start implementation planning  
**File Location:** `design-docs/03-architecture/feat-03-recurring-expense-technical-design.md`