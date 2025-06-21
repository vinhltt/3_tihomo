# TiHoMo Microservices Architecture - Enhanced Design (v2.0)

> **📋 Expert Review Summary**  
> **Score: 7.5/10** - Solid foundation với room for improvement trong critical areas  
> **Key Improvements Needed**: Saga simplification, Message design optimization, Resilience patterns, Security enhancements, Advanced observability  
> **Reference**: [Expert Review Document](./review_add_log_and_message_queue.md)

## 📊 Version History
- **v1.0** (Initial): Basic microservices architecture với MassTransit và observability
- **v2.0** (Current): Enhanced design addressing expert feedback với production-ready patterns

## 🎯 Executive Summary of Improvements

### ✅ Architectural Enhancements
- **Saga Pattern Redesign**: Split complex Transaction Saga thành 3 focused sagas
- **Message Design Optimization**: Implement minimal coupling với event-carried state transfer
- **Resilience Integration**: Comprehensive Polly patterns với circuit breaker, retry, timeout
- **Security Implementation**: Message encryption, service-to-service authentication, input validation
- **Advanced Observability**: Business metrics, health checks, correlation IDs, SLA monitoring

### 🔧 Technical Improvements  
- **Message Versioning Strategy** cho backward compatibility
- **Dead Letter Queue Handling** cho error management
- **Performance Optimizations** với partitioning và batch processing
- **High Availability Infrastructure** với cluster setup

### 📈 Production Readiness
- **Success Criteria**: >99.5% saga success rate, <100ms P95 latency, >99.9% availability
- **Security Standards**: Financial-grade encryption và compliance
- **Operational Excellence**: Comprehensive monitoring và alerting

---

## 1. Architecture Overview

### 1.1 System Architecture Diagram
```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              Frontend Layer                                    │
├─────────────────────────────────────────────────────────────────────────────────┤
│  Nuxt 3 SPA  │  Mobile App  │  Admin Dashboard  │  Third-party Integrations   │
└─────────────────────────────────────────────────────────────────────────────────┘
                                       │
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              API Gateway (Ocelot)                              │
├─────────────────────────────────────────────────────────────────────────────────┤
│  Authentication  │  Rate Limiting  │  Load Balancing  │  Request Routing      │
└─────────────────────────────────────────────────────────────────────────────────┘
                                       │
┌─────────────────────────────────────────────────────────────────────────────────┐
│                            Microservices Layer                                 │
├─────────────────┬─────────────────┬─────────────────┬─────────────────────────┤
│   Identity      │  CoreFinance    │ MoneyManagement │  PlanningInvestment     │
│   Service       │   Service       │    Service      │      Service            │
├─────────────────┼─────────────────┼─────────────────┼─────────────────────────┤
│ ReportingInteg  │  Notification   │   Audit         │    Payment              │
│    Service      │    Service      │   Service       │    Service              │
└─────────────────┴─────────────────┴─────────────────┴─────────────────────────┘
                                       │
┌─────────────────────────────────────────────────────────────────────────────────┐
│                        Message Bus (MassTransit)                               │
├─────────────────────────────────────────────────────────────────────────────────┤
│  RabbitMQ/Azure Service Bus  │  Saga Orchestration  │  Event Publishing      │
└─────────────────────────────────────────────────────────────────────────────────┘
                                       │
┌─────────────────────────────────────────────────────────────────────────────────┐
│                            Data & Infrastructure                               │
├─────────────────┬─────────────────┬─────────────────┬─────────────────────────┤
│   PostgreSQL    │     Redis       │   File Storage  │    Observability        │
│   Databases     │    Cache        │   (Azure Blob)  │   (Grafana/Loki)        │
└─────────────────┴─────────────────┴─────────────────┴─────────────────────────┘
```

### 1.2 Microservices Details

#### Identity Service
- **Responsibilities**: Authentication, Authorization, User Management
- **Database**: Identity_DB
- **Key Features**: 
  - Social Login (Google, Facebook)
  - JWT Token Management
  - API Key Management
  - Role-based Access Control

#### CoreFinance Service
- **Responsibilities**: Core financial data, Accounts, Transactions
- **Database**: CoreFinance_DB
- **Key Features**:
  - Account Management
  - Transaction Processing
  - Balance Calculations
  - Currency Conversion

#### MoneyManagement Service
- **Responsibilities**: Budgets, Categories, Expense Tracking
- **Database**: MoneyManagement_DB
- **Key Features**:
  - Budget Creation & Monitoring
  - Category Management
  - Expense Analysis
  - Recurring Transactions

#### PlanningInvestment Service
- **Responsibilities**: Investment Portfolio, Goals, Analysis
- **Database**: Investment_DB
- **Key Features**:
  - Portfolio Management
  - Investment Tracking
  - Goal Setting
  - Performance Analysis

#### ReportingIntegration Service
- **Responsibilities**: Reports, Analytics, Data Export
- **Database**: Reporting_DB
- **Key Features**:
  - Financial Reports
  - Dashboard Data
  - Data Export
  - Third-party Integrations

#### Notification Service
- **Responsibilities**: Email, SMS, Push Notifications
- **Database**: Notification_DB
- **Key Features**:
  - Multi-channel Notifications
  - Template Management
  - Delivery Tracking
  - User Preferences

#### Audit Service
- **Responsibilities**: Audit Logging, Compliance
- **Database**: Audit_DB
- **Key Features**:
  - Transaction Audit
  - User Activity Logging
  - Compliance Reporting
  - Data Retention

#### Payment Service
- **Responsibilities**: External Payment Processing
- **Database**: Payment_DB
- **Key Features**:
  - Payment Gateway Integration
  - Transaction Processing
  - Refund Management
  - Payment History

## 2. Enhanced Saga Pattern Implementation

> **🔧 Expert Improvement**: Original Transaction Processing Saga was too complex và violated Single Responsibility Principle. New design splits responsibilities into focused, maintainable sagas.

### 2.1 Improved Saga Architecture

#### 2.1.1 Core Transaction Saga (Simplified)
```csharp
public class CoreTransactionSaga : MassTransitStateMachine<CoreTransactionState>
{
    public State Initiated { get; private set; }
    public State AccountValidated { get; private set; }
    public State TransactionCreated { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    public Event<TransactionInitiated> TransactionInitiated { get; private set; }
    public Event<AccountValidated> AccountValidated { get; private set; }
    public Event<TransactionCreated> TransactionCreated { get; private set; }
    public Event<SagaFaulted> SagaFaulted { get; private set; }

    public CoreTransactionSaga()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(TransactionInitiated)
                .Then(context => {
                    context.Instance.TransactionId = context.Data.TransactionId;
                    context.Instance.CorrelationId = context.Data.CorrelationId;
                    context.Instance.OccurredAt = context.Data.OccurredAt;
                })
                .Activity(x => x.OfType<LogTransactionInitiatedActivity>())
                .PublishAsync(context => context.Init<ValidateAccount>(new
                {
                    TransactionId = context.Instance.TransactionId,
                    CorrelationId = context.Instance.CorrelationId
                    // Minimal data - service fetches what it needs
                }))
                .TransitionTo(Initiated)
        );

        During(Initiated,
            When(AccountValidated)
                .If(context => context.Data.IsValid)
                .PublishAsync(context => context.Init<CreateTransaction>(new
                {
                    TransactionId = context.Instance.TransactionId,
                    CorrelationId = context.Instance.CorrelationId
                }))
                .TransitionTo(AccountValidated)
        );

        During(AccountValidated,
            When(TransactionCreated)
                .Activity(x => x.OfType<CompleteTransactionActivity>())
                .TransitionTo(Completed)
                .Finalize()
        );

        DuringAny(
            When(SagaFaulted)
                .Activity(x => x.OfType<CompensateTransactionActivity>())
                .TransitionTo(Failed)
        );

        SetCompletedWhenFinalized();
    }
}
```

#### 2.1.2 Budget Management Saga (Focused)
```csharp
public class BudgetManagementSaga : MassTransitStateMachine<BudgetManagementState>
{
    public State Processing { get; private set; }
    public State BudgetUpdated { get; private set; }
    public State LimitChecked { get; private set; }
    public State Completed { get; private set; }

    public Event<TransactionCreated> TransactionCreated { get; private set; }
    public Event<BudgetUpdated> BudgetUpdated { get; private set; }
    public Event<BudgetLimitChecked> BudgetLimitChecked { get; private set; }

    public BudgetManagementSaga()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(TransactionCreated)
                .Then(context => {
                    context.Instance.TransactionId = context.Data.TransactionId;
                    context.Instance.CorrelationId = context.Data.CorrelationId;
                })
                .PublishAsync(context => context.Init<UpdateBudget>(new
                {
                    TransactionId = context.Instance.TransactionId,
                    CorrelationId = context.Instance.CorrelationId
                }))
                .TransitionTo(Processing)
        );

        During(Processing,
            When(BudgetUpdated)
                .PublishAsync(context => context.Init<CheckBudgetLimits>(new
                {
                    TransactionId = context.Instance.TransactionId,
                    CorrelationId = context.Instance.CorrelationId
                }))
                .TransitionTo(BudgetUpdated)
        );

        During(BudgetUpdated,
            When(BudgetLimitChecked)
                .If(context => context.Data.LimitExceeded)
                .PublishAsync(context => context.Init<SendBudgetAlert>(new
                {
                    UserId = context.Data.UserId,
                    BudgetCategory = context.Data.Category,
                    ExceededAmount = context.Data.ExceededAmount
                }))
                .TransitionTo(Completed)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
```

#### 2.1.3 Notification Saga (Decoupled)
```csharp
public class NotificationSaga : MassTransitStateMachine<NotificationState>
{
    public State Processing { get; private set; }
    public State NotificationSent { get; private set; }
    public State Completed { get; private set; }

    public Event<TransactionCreated> TransactionCreated { get; private set; }
    public Event<BudgetAlertRequired> BudgetAlertRequired { get; private set; }
    public Event<NotificationSent> NotificationSent { get; private set; }

    public NotificationSaga()
    {
        InstanceState(x => x.CurrentState);

        Initially(
            When(TransactionCreated)
                .PublishAsync(context => context.Init<SendNotification>(new
                {
                    UserId = context.Data.UserId,
                    NotificationType = "TransactionCreated",
                    TransactionId = context.Data.TransactionId,
                    CorrelationId = context.Data.CorrelationId
                }))
                .TransitionTo(Processing),
            
            When(BudgetAlertRequired)
                .PublishAsync(context => context.Init<SendNotification>(new
                {
                    UserId = context.Data.UserId,
                    NotificationType = "BudgetLimitExceeded",
                    AlertData = context.Data,
                    CorrelationId = context.Data.CorrelationId
                }))
                .TransitionTo(Processing)
        );

        During(Processing,
            When(NotificationSent)
                .TransitionTo(Completed)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
```

### 2.2 Enhanced Message Design Patterns

> **🔧 Expert Improvement**: Messages now follow minimal coupling principle với proper event-carried state transfer.

#### 2.2.1 Minimal Event Messages
```csharp
// ✅ IMPROVED: Minimal data in events
public class TransactionInitiated
{
    public Guid TransactionId { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Source { get; set; } = "CoreFinance";
    // Services fetch additional data as needed
}

public class TransactionCreated
{
    public Guid TransactionId { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class AccountValidated
{
    public Guid TransactionId { get; set; }
    public Guid CorrelationId { get; set; }
    public bool IsValid { get; set; }
    public string ValidationResult { get; set; }
    public DateTime ValidatedAt { get; set; }
}
```

#### 2.2.2 Message Versioning Strategy
```csharp
[MessageVersion("v1")]
public class TransactionInitiatedV1 : TransactionInitiated
{
    // Version 1 implementation
}

[MessageVersion("v2")]
public class TransactionInitiatedV2 : TransactionInitiated
{
    public string PaymentMethod { get; set; } // New field
    public Dictionary<string, string> ExtendedAttributes { get; set; } = new();
    
    // Backward compatibility
    public static implicit operator TransactionInitiatedV1(TransactionInitiatedV2 v2)
    {
        return new TransactionInitiatedV1
        {
            TransactionId = v2.TransactionId,
            CorrelationId = v2.CorrelationId,
            OccurredAt = v2.OccurredAt,
            Source = v2.Source
        };
    }
}
```

### 2.3 Saga Correlation Improvements
```csharp
public class EnhancedSagaConfiguration
{
    public static void Configure(IBusRegistrationConfigurator configurator)
    {
        configurator.AddSagaStateMachine<CoreTransactionSaga, CoreTransactionState>()
            .EntityFrameworkRepository(r =>
            {
                r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                r.ExistingDbContext<SagaDbContext>();
                
                // Multiple correlation strategies
                r.ConfigureCorrelation<TransactionInitiated>(m => m.TransactionId);
                r.ConfigureCorrelation<AccountValidated>(m => m.TransactionId);
                r.ConfigureCorrelation<TransactionCreated>(m => m.TransactionId);
                
                // Alternative correlation for external events
                r.ConfigureCorrelation<PaymentGatewayResponse>(m => m.ExternalReferenceId);
            });
    }
}
```

## 3. Resilience & Error Handling Implementation

> **🔧 Expert Improvement**: Added comprehensive resilience patterns với Polly library integration để handle external failures gracefully.

### 3.1 Circuit Breaker & Retry Patterns
```csharp
// Enhanced MassTransit Configuration với Polly
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(connectionString);
        
        // Advanced retry configuration
        cfg.UseMessageRetry(r => 
        {
            r.Exponential(
                retryLimit: 5, 
                minInterval: TimeSpan.FromSeconds(1), 
                maxInterval: TimeSpan.FromMinutes(5), 
                intervalDelta: TimeSpan.FromSeconds(1)
            );
              // Specific exception handling
            r.Handle<Npgsql.NpgsqlException>(); // PostgreSQL specific exceptions
            r.Handle<TimeoutException>();
            r.Handle<HttpRequestException>();
            r.Ignore<ValidationException>();
            r.Ignore<SecurityException>();
        });
        
        // Circuit breaker configuration  
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15; // Trip after 15 failures
            cb.ActiveThreshold = 10; // Min requests before tracking
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });
        
        // Timeout configuration
        cfg.UseTimeout(x => x.Timeout = TimeSpan.FromSeconds(30));
        
        // Rate limiting
        cfg.UseRateLimit(r =>
        {
            r.SetRateLimit(1000, TimeSpan.FromMinutes(1));
            r.SetConcurrencyLimit(10);
        });
    });
});
```

### 3.2 Dead Letter Queue Handling
```csharp
// Dead Letter Queue Consumer
public class DeadLetterQueueConsumer : IConsumer<Fault<TransactionInitiated>>
{
    private readonly ILogger<DeadLetterQueueConsumer> _logger;
    private readonly INotificationService _notificationService;
    private readonly IDeadLetterRepository _repository;

    public async Task Consume(ConsumeContext<Fault<TransactionInitiated>> context)
    {
        var fault = context.Message;
        
        // Log detailed error information
        _logger.LogError(
            "Transaction {TransactionId} failed permanently. Reason: {Reason}. CorrelationId: {CorrelationId}",
            fault.Message.TransactionId,
            string.Join(", ", fault.Exceptions.Select(e => e.Message)),
            context.CorrelationId
        );
        
        // Store for manual processing
        await _repository.SaveDeadLetterAsync(new DeadLetterRecord
        {
            MessageType = typeof(TransactionInitiated).Name,
            MessageBody = JsonSerializer.Serialize(fault.Message),
            ErrorDetails = JsonSerializer.Serialize(fault.Exceptions),
            CorrelationId = context.CorrelationId,
            CreatedAt = DateTime.UtcNow,
            Status = DeadLetterStatus.PendingReview
        });
        
        // Notify administrators for critical failures
        if (IsHighPriorityTransaction(fault.Message))
        {
            await _notificationService.NotifyAdministrators(
                $"Critical transaction failure: {fault.Message.TransactionId}",
                fault.Exceptions
            );
        }
    }
}

// Dead Letter Queue configuration
cfg.ReceiveEndpoint("transaction-processing-error", e =>
{
    e.Consumer<DeadLetterQueueConsumer>();
    e.UseMessageRetry(r => r.None()); // No retries for DLQ
    e.PrefetchCount = 10;
    e.ConcurrentMessageLimit = 5;
});
```

### 3.3 Saga Compensation & Recovery
```csharp
public class CompensateTransactionActivity : IActivity<CoreTransactionState, TransactionFailed>
{
    private readonly ILogger<CompensateTransactionActivity> _logger;
    private readonly ICoreFinanceService _coreFinanceService;
    private readonly ICompensationService _compensationService;

    public async Task<ExecutionResult> Execute(BehaviorContext<CoreTransactionState, TransactionFailed> context)
    {
        try
        {
            var correlationId = context.Instance.CorrelationId;
            var transactionId = context.Instance.TransactionId;
            
            _logger.LogWarning(
                "Starting compensation for transaction {TransactionId}. CorrelationId: {CorrelationId}",
                transactionId, correlationId
            );
            
            // Compensate based on saga state
            switch (context.Instance.LastSuccessfulStep)
            {
                case "AccountReserved":
                    await _coreFinanceService.ReleaseAccountReservation(transactionId);
                    break;
                    
                case "TransactionCreated":
                    await _coreFinanceService.ReverseTransaction(transactionId);
                    break;
            }
            
            // Log compensation completion
            await _compensationService.RecordCompensation(new CompensationRecord
            {
                TransactionId = transactionId,
                CorrelationId = correlationId,
                CompensationActions = context.Instance.CompensationActions,
                CompletedAt = DateTime.UtcNow,
                Status = CompensationStatus.Success
            });
            
            return context.Completed();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Compensation failed for transaction {TransactionId}. Manual intervention required.",
                context.Instance.TransactionId
            );
            
            return context.Faulted(ex);
        }
    }
}
```

## 4. Security Implementation

> **🔧 Expert Improvement**: Financial-grade security với message encryption, service authentication, và input validation.

### 4.1 Message Encryption for Sensitive Data
```csharp
public class EncryptedMessage<T> where T : class
{
    public string EncryptedPayload { get; set; }
    public string Signature { get; set; }
    public string KeyId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Algorithm { get; set; } = "AES256-GCM";
}

public class MessageEncryptionService : IMessageEncryptionService
{
    private readonly IAESEncryption _encryption;
    private readonly IKeyManagementService _keyService;
    
    public async Task<EncryptedMessage<T>> EncryptAsync<T>(T message) where T : class
    {
        var keyInfo = await _keyService.GetCurrentEncryptionKey();
        var payload = JsonSerializer.Serialize(message);
        
        var (encryptedData, signature) = await _encryption.EncryptWithSignature(
            payload, 
            keyInfo.Key, 
            keyInfo.SigningKey
        );
        
        return new EncryptedMessage<T>
        {
            EncryptedPayload = Convert.ToBase64String(encryptedData),
            Signature = Convert.ToBase64String(signature),
            KeyId = keyInfo.KeyId,
            Timestamp = DateTime.UtcNow,
            Algorithm = "AES256-GCM"
        };
    }
    
    public async Task<T> DecryptAsync<T>(EncryptedMessage<T> encryptedMessage) where T : class
    {
        var keyInfo = await _keyService.GetEncryptionKey(encryptedMessage.KeyId);
        
        // Verify signature first
        var isValid = await _encryption.VerifySignature(
            Convert.FromBase64String(encryptedMessage.EncryptedPayload),
            Convert.FromBase64String(encryptedMessage.Signature),
            keyInfo.SigningKey
        );
        
        if (!isValid)
            throw new SecurityException("Message signature verification failed");
        
        // Decrypt payload
        var decryptedData = await _encryption.Decrypt(
            Convert.FromBase64String(encryptedMessage.EncryptedPayload),
            keyInfo.Key
        );
        
        return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(decryptedData));
    }
}
```

### 4.2 Service-to-Service Authentication
```csharp
// JWT Service Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Identity:Authority"];
        options.RequireHttpsMetadata = true;
        options.Audience = "tihomo-api";
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            
            // Require service-specific claims
            RoleClaimType = "role",
            NameClaimType = "service_name"
        };
    });

// MassTransit Message Authentication
public class MessageAuthenticationFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly IServiceTokenValidator _tokenValidator;
    
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        // Extract service token from headers
        if (!context.Headers.TryGetHeader("Authorization", out var authHeader))
            throw new UnauthorizedAccessException("Missing authorization header");
        
        var token = authHeader.ToString().Replace("Bearer ", "");
        var validationResult = await _tokenValidator.ValidateServiceToken(token);
        
        if (!validationResult.IsValid)
            throw new UnauthorizedAccessException($"Invalid service token: {validationResult.Error}");
        
        // Add service identity to context
        context.Headers.Set("ServiceName", validationResult.ServiceName);
        context.Headers.Set("ServiceRole", validationResult.Role);
        
        await next.Send(context);
    }
}
```

### 4.3 Input Validation & Sanitization
```csharp
public class MessageValidationFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly IValidator<T> _validator;
    private readonly ILogger<MessageValidationFilter<T>> _logger;
    
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        // Validate message structure
        var validationResult = await _validator.ValidateAsync(context.Message);
        
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            
            _logger.LogWarning(
                "Message validation failed for {MessageType}. Errors: {Errors}. CorrelationId: {CorrelationId}",
                typeof(T).Name, errors, context.CorrelationId
            );
            
            throw new ValidationException($"Message validation failed: {errors}");
        }
        
        // Sanitize string fields
        SanitizeMessage(context.Message);
        
        await next.Send(context);
    }
    
    private void SanitizeMessage<TMessage>(TMessage message)
    {
        var properties = typeof(TMessage).GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanWrite);
        
        foreach (var property in properties)
        {
            var value = property.GetValue(message) as string;
            if (!string.IsNullOrEmpty(value))
            {
                // Basic HTML/SQL injection prevention
                var sanitized = value
                    .Replace("<script", "&lt;script", StringComparison.OrdinalIgnoreCase)
                    .Replace("'", "''")
                    .Replace("--", "")
                    .Trim();
                
                property.SetValue(message, sanitized);
            }
        }
    }
}

// Message validation rules
public class TransactionInitiatedValidator : AbstractValidator<TransactionInitiated>
{
    public TransactionInitiatedValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("TransactionId must be a valid GUID");
        
        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("CorrelationId must be a valid GUID");
        
        RuleFor(x => x.OccurredAt)
            .NotEmpty()
            .Must(BeWithinReasonableTimeRange)
            .WithMessage("OccurredAt must be within last 24 hours");
        
        RuleFor(x => x.Source)
            .NotEmpty()
            .Length(1, 50)
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("Source must contain only alphanumeric characters, hyphens, and underscores");
    }
    
    private bool BeValidGuid(Guid guid) => guid != Guid.Empty;
    
    private bool BeWithinReasonableTimeRange(DateTime dateTime)
    {
        var now = DateTime.UtcNow;
        return dateTime >= now.AddHours(-24) && dateTime <= now.AddMinutes(5);
    }
}
```

## 5. Advanced Observability & Monitoring

> **🔧 Expert Improvement**: Production-grade observability với business metrics, health checks, correlation IDs, và SLA monitoring.

### 5.1 Enhanced Health Checks
```csharp
// Comprehensive Health Check Configuration
builder.Services.AddHealthChecks()
    
    // Database health checks
    .AddDbContext<CoreFinanceDbContext>(tags: new[] { "database", "corefinance" })
    .AddDbContext<MoneyManagementDbContext>(tags: new[] { "database", "moneymanagement" })
    .AddDbContext<IdentityDbContext>(tags: new[] { "database", "identity" })
    
    // PostgreSQL health checks
    .AddNpgSql(connectionString, tags: new[] { "database", "postgresql" })
    
    // Message broker health checks
    .AddRabbitMQ(connectionString, tags: new[] { "messagebus", "rabbitmq" })
    
    // External dependencies
    .AddRedis(redisConnection, tags: new[] { "cache", "redis" })
    .AddUrlGroup(new Uri("https://api.exchangerate-api.com/v4/latest/USD"), 
        "exchange-api", tags: new[] { "external", "exchange" })
    
    // Custom business health checks
    .AddCheck<SagaProcessingHealthCheck>("saga-processing", tags: new[] { "business", "saga" })
    .AddCheck<MessageQueueHealthCheck>("message-queue-depth", tags: new[] { "business", "queue" })
    .AddCheck<CircuitBreakerHealthCheck>("circuit-breakers", tags: new[] { "resilience" });

// Custom Business Health Checks
public class SagaProcessingHealthCheck : IHealthCheck
{
    private readonly ISagaMetricsService _sagaMetrics;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        var metrics = await _sagaMetrics.GetLast5MinutesMetrics();
        
        var successRate = metrics.TotalProcessed > 0 
            ? (double)metrics.Successful / metrics.TotalProcessed 
            : 1.0;
        
        var data = new Dictionary<string, object>
        {
            ["success_rate"] = successRate,
            ["total_processed"] = metrics.TotalProcessed,
            ["successful"] = metrics.Successful,
            ["failed"] = metrics.Failed,
            ["average_duration_ms"] = metrics.AverageDurationMs
        };
        
        if (successRate < 0.995) // Less than 99.5% success rate
            return HealthCheckResult.Degraded("Saga success rate below threshold", data: data);
        
        if (metrics.AverageDurationMs > 5000) // Slower than 5 seconds
            return HealthCheckResult.Degraded("Saga processing too slow", data: data);
        
        return HealthCheckResult.Healthy("Saga processing healthy", data: data);
    }
}
```

### 5.2 Business Metrics & KPIs
```csharp
public class BusinessMetricsService : IBusinessMetricsService
{
    private readonly IMetrics _metrics;
    private readonly ILogger<BusinessMetricsService> _logger;
    
    // Transaction metrics
    public void RecordTransactionProcessed(string accountType, decimal amount, string currency, TimeSpan duration)
    {
        _metrics.Counter("transactions_processed_total")
            .WithTag("account_type", accountType)
            .WithTag("currency", currency)
            .WithTag("amount_range", GetAmountRange(amount))
            .Increment();
        
        _metrics.Histogram("transaction_processing_duration_ms")
            .WithTag("account_type", accountType)
            .Record((long)duration.TotalMilliseconds);
        
        _metrics.Histogram("transaction_amount")
            .WithTag("currency", currency)
            .WithTag("account_type", accountType)
            .Record((double)amount);
    }
    
    // Saga metrics
    public void RecordSagaCompletion(string sagaType, TimeSpan duration, string outcome)
    {
        _metrics.Counter("saga_completions_total")
            .WithTag("saga_type", sagaType)
            .WithTag("outcome", outcome)
            .Increment();
        
        _metrics.Histogram("saga_duration_ms")
            .WithTag("saga_type", sagaType)
            .Record((long)duration.TotalMilliseconds);
        
        _metrics.Gauge("active_sagas")
            .WithTag("saga_type", sagaType)
            .Set(GetActiveSagaCount(sagaType));
    }
    
    // Budget metrics
    public void RecordBudgetEvent(string eventType, string category, decimal amount, bool limitExceeded)
    {
        _metrics.Counter("budget_events_total")
            .WithTag("event_type", eventType)
            .WithTag("category", category)
            .WithTag("limit_exceeded", limitExceeded.ToString())
            .Increment();
        
        if (limitExceeded)
        {
            _metrics.Counter("budget_limits_exceeded_total")
                .WithTag("category", category)
                .Increment();
        }
    }
    
    // Performance SLA metrics
    public void RecordApiResponse(string endpoint, int statusCode, TimeSpan duration)
    {
        _metrics.Counter("api_requests_total")
            .WithTag("endpoint", endpoint)
            .WithTag("status_code", statusCode.ToString())
            .Increment();
        
        _metrics.Histogram("api_response_duration_ms")
            .WithTag("endpoint", endpoint)
            .Record((long)duration.TotalMilliseconds);
        
        // SLA compliance tracking
        if (duration.TotalMilliseconds > 2000) // 2 second SLA
        {
            _metrics.Counter("sla_violations_total")
                .WithTag("endpoint", endpoint)
                .WithTag("sla_type", "response_time")
                .Increment();
        }
    }
    
    private string GetAmountRange(decimal amount)
    {
        return amount switch
        {
            < 100 => "small",
            < 1000 => "medium", 
            < 10000 => "large",
            _ => "very_large"
        };
    }
}
```

### 5.3 Correlation ID & Distributed Tracing
```csharp
// Correlation ID Middleware
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            // Add to response headers for client tracking
            context.Response.Headers.Add("X-Correlation-ID", correlationId);
            
            await _next(context);
        }
    }
    
    private string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            return correlationId.FirstOrDefault() ?? Guid.NewGuid().ToString();
        }
        
        return Guid.NewGuid().ToString();
    }
}

// MassTransit Correlation Integration
public class CorrelationEnrichmentFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly ILogger<CorrelationEnrichmentFilter<T>> _logger;
    
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var correlationId = context.CorrelationId?.ToString() ?? Guid.NewGuid().ToString();
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["MessageType"] = typeof(T).Name,
            ["MessageId"] = context.MessageId
        }))
        {
            // Add correlation to activity
            Activity.Current?.SetTag("correlation.id", correlationId);
            Activity.Current?.SetTag("message.type", typeof(T).Name);
            
            _logger.LogInformation(
                "Processing message {MessageType} with correlation {CorrelationId}",
                typeof(T).Name, correlationId
            );
            
            await next.Send(context);
        }
    }
}
```

### 5.4 SLA/SLO Monitoring & Alerting
```csharp
public class SlaMonitoringService : ISlaMonitoringService
{
    private readonly IMetrics _metrics;
    private readonly IAlertingService _alerting;
    
    // Define SLAs
    private readonly Dictionary<string, SlaDefinition> _slaDefinitions = new()
    {
        ["transaction_processing"] = new SlaDefinition
        {
            Name = "Transaction Processing",
            SuccessRateThreshold = 0.999, // 99.9%
            ResponseTimeP95Threshold = TimeSpan.FromSeconds(2),
            ResponseTimeP99Threshold = TimeSpan.FromSeconds(5),
            AvailabilityThreshold = 0.995 // 99.5%
        },
        ["saga_completion"] = new SlaDefinition
        {
            Name = "Saga Completion",
            SuccessRateThreshold = 0.995, // 99.5%
            ResponseTimeP95Threshold = TimeSpan.FromSeconds(10),
            ResponseTimeP99Threshold = TimeSpan.FromSeconds(30)
        }
    };
    
    public async Task CheckSlaCompliance()
    {
        foreach (var (key, definition) in _slaDefinitions)
        {
            var metrics = await GetSlaMetrics(key);
            var violations = new List<SlaViolation>();
            
            // Check success rate
            if (metrics.SuccessRate < definition.SuccessRateThreshold)
            {
                violations.Add(new SlaViolation
                {
                    Type = "SuccessRate",
                    Threshold = definition.SuccessRateThreshold,
                    Actual = metrics.SuccessRate,
                    Severity = GetViolationSeverity(metrics.SuccessRate, definition.SuccessRateThreshold)
                });
            }
            
            // Check response times
            if (metrics.ResponseTimeP95 > definition.ResponseTimeP95Threshold)
            {
                violations.Add(new SlaViolation
                {
                    Type = "ResponseTimeP95",
                    Threshold = definition.ResponseTimeP95Threshold.TotalMilliseconds,
                    Actual = metrics.ResponseTimeP95.TotalMilliseconds,
                    Severity = GetViolationSeverity(
                        metrics.ResponseTimeP95.TotalMilliseconds,
                        definition.ResponseTimeP95Threshold.TotalMilliseconds)
                });
            }
            
            // Record violations
            foreach (var violation in violations)
            {
                _metrics.Counter("sla_violations_total")
                    .WithTag("service", key)
                    .WithTag("violation_type", violation.Type)
                    .WithTag("severity", violation.Severity.ToString())
                    .Increment();
                
                // Alert on critical violations
                if (violation.Severity >= SeverityLevel.High)
                {
                    await _alerting.SendAlert(new SlaAlert
                    {
                        Service = definition.Name,
                        ViolationType = violation.Type,
                        Severity = violation.Severity,
                        Message = $"SLA violation detected: {violation.Type} " +
                                $"(Actual: {violation.Actual:F3}, Threshold: {violation.Threshold:F3})"
                    });
                }
            }
        }
    }
    
    private SeverityLevel GetViolationSeverity(double actual, double threshold)
    {
        var ratio = Math.Abs(actual - threshold) / threshold;
        
        return ratio switch
        {
            > 0.1 => SeverityLevel.Critical,
            > 0.05 => SeverityLevel.High,
            > 0.02 => SeverityLevel.Medium,
            _ => SeverityLevel.Low
        };
    }
}
```

## 6. Performance Optimizations

> **🔧 Expert Improvement**: Message partitioning, batch processing, và performance tuning cho high-throughput scenarios.

### 6.1 Message Partitioning Strategy
```csharp
// User-based partitioning for better scalability
public class PartitionedMessageConfiguration
{
    public static void Configure(IRabbitMqBusFactoryConfigurator cfg)
    {
        // Partition by user ID for transaction messages
        cfg.Send<ProcessTransaction>(x =>
        {
            x.UsePartitioner(8, p => p.Message.UserId);
        });
        
        // Partition by account for balance updates
        cfg.Send<UpdateAccountBalance>(x =>
        {
            x.UsePartitioner(4, p => p.Message.AccountId);
        });
        
        // Round-robin for notifications
        cfg.Send<SendNotification>(x =>
        {
            x.UsePartitioner(6);
        });
    }
}

// Custom partition key selector
public class TransactionPartitionKeyProvider : IPartitionKeyProvider<ProcessTransaction>
{
    public byte[] GetPartitionKey(SendContext<ProcessTransaction> context)
    {
        // Use user ID as partition key for transaction affinity
        var userId = context.Message.UserId.ToString();
        return Encoding.UTF8.GetBytes(userId);
    }
}
```

### 6.2 Batch Processing Implementation  
```csharp
// Batch consumer for high-throughput scenarios
public class BatchTransactionProcessor : IConsumer<Batch<TransactionToProcess>>
{
    private readonly ICoreFinanceService _coreFinanceService;
    private readonly IBusinessMetricsService _metricsService;
    private readonly ILogger<BatchTransactionProcessor> _logger;
    
    public async Task Consume(ConsumeContext<Batch<TransactionToProcess>> context)
    {
        var transactions = context.Message.Select(x => x.Message).ToList();
        var correlationId = context.CorrelationId?.ToString();
        var startTime = DateTime.UtcNow;
        
        _logger.LogInformation(
            "Processing batch of {Count} transactions. CorrelationId: {CorrelationId}",
            transactions.Count, correlationId
        );
        
        try
        {
            // Group by account for optimized processing
            var groupedByAccount = transactions.GroupBy(t => t.AccountId).ToList();
            
            // Process each account group
            var tasks = groupedByAccount.Select(async group =>
            {
                var accountTransactions = group.ToList();
                await _coreFinanceService.ProcessTransactionBatch(accountTransactions);
                
                // Record metrics for this account batch
                _metricsService.RecordBatchProcessed(
                    group.Key.ToString(),
                    accountTransactions.Count,
                    DateTime.UtcNow - startTime
                );
            });
            
            await Task.WhenAll(tasks);
            
            _logger.LogInformation(
                "Successfully processed batch of {Count} transactions in {Duration}ms",
                transactions.Count, (DateTime.UtcNow - startTime).TotalMilliseconds
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process transaction batch of {Count} items. CorrelationId: {CorrelationId}",
                transactions.Count, correlationId
            );
            throw;
        }
    }
}

// Batch configuration
cfg.ReceiveEndpoint("transaction-batch-processor", e =>
{
    e.Consumer<BatchTransactionProcessor>();
    e.Batch<TransactionToProcess>(b =>
    {
        b.MessageLimit = 100; // Max 100 messages per batch
        b.TimeLimit = TimeSpan.FromSeconds(5); // Max 5 seconds wait
        b.SizeLimit = 1024 * 1024; // Max 1MB batch size
    });
    e.PrefetchCount = 200;
    e.ConcurrentMessageLimit = 10;
});
```

### 6.3 Connection & Resource Optimization
```csharp
public class OptimizedMassTransitConfiguration
{
    public static void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMQ"), h =>
                {
                    h.Username("tihomo");
                    h.Password("secure-password");
                    
                    // Connection optimization
                    h.RequestedConnectionTimeout(TimeSpan.FromSeconds(60));
                    h.RequestedHeartbeat(TimeSpan.FromSeconds(30));
                    
                    // Performance tuning
                    h.UseCluster(c =>
                    {
                        c.Node("rabbitmq-1:5672");
                        c.Node("rabbitmq-2:5672");
                        c.Node("rabbitmq-3:5672");
                    });
                });
                
                // Global performance settings
                cfg.PrefetchCount = 100; // Higher prefetch for better throughput
                cfg.ConcurrentMessageLimit = 16; // Optimized for CPU cores
                
                // Message serialization optimization
                cfg.UseJsonSerializer(); // Faster than default XML
                cfg.ConfigureJsonSerializerOptions(options =>
                {
                    options.IgnoreNullValues = true;
                    options.WriteIndented = false; // Smaller payload
                });
                
                // Publishing optimization
                cfg.Publish<TransactionInitiated>(p => 
                {
                    p.Durable = true; // Persistent messages
                    p.AutoDelete = false;
                });
                
                cfg.Publish<TransactionCreated>(p =>
                {
                    p.Durable = true;
                    p.AutoDelete = false;
                    p.Exclude = true; // Don't create topology
                });
                
                // Consumer optimization
                cfg.ReceiveEndpoint("core-finance-transactions", e =>
                {
                    e.PrefetchCount = 50;
                    e.ConcurrentMessageLimit = 8;
                    e.UseMessageRetry(r => r.Exponential(3, TimeSpan.FromSeconds(1), 
                        TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2)));
                    
                    e.Consumer<TransactionProcessor>(c =>
                    {
                        c.UseConcurrentMessageLimit(4);
                    });
                });
            });
        });
    }
}
```

## 7. Production-Ready Infrastructure

> **🔧 Expert Improvement**: High availability setup, monitoring enhancements, và infrastructure best practices.

### 7.1 High Availability RabbitMQ Cluster
```yaml
# docker-compose.production.yml
version: '3.8'
services:
  rabbitmq-1:
    image: rabbitmq:3.11-management
    hostname: rabbitmq-1
    environment:
      RABBITMQ_ERLANG_COOKIE: "tihomo-cluster-secret-key"
      RABBITMQ_DEFAULT_USER: "tihomo"
      RABBITMQ_DEFAULT_PASS: "${RABBITMQ_PASSWORD}"
      RABBITMQ_DEFAULT_VHOST: "tihomo"
    volumes:
      - rabbitmq1-data:/var/lib/rabbitmq
      - ./rabbitmq-cluster.conf:/etc/rabbitmq/rabbitmq.conf
      - ./rabbitmq-definitions.json:/etc/rabbitmq/definitions.json
    ports:
      - "5672:5672"
      - "15672:15672"
    networks:
      - tihomo-cluster
    deploy:
      replicas: 1
      placement:
        constraints:
          - node.labels.rabbitmq-1 == true
      
  rabbitmq-2:
    image: rabbitmq:3.11-management
    hostname: rabbitmq-2
    environment:
      RABBITMQ_ERLANG_COOKIE: "tihomo-cluster-secret-key"
      RABBITMQ_DEFAULT_USER: "tihomo"
      RABBITMQ_DEFAULT_PASS: "${RABBITMQ_PASSWORD}"
    volumes:
      - rabbitmq2-data:/var/lib/rabbitmq
      - ./rabbitmq-cluster.conf:/etc/rabbitmq/rabbitmq.conf
    networks:
      - tihomo-cluster
    depends_on:
      - rabbitmq-1
    command: >
      bash -c "
        rabbitmq-server &
        sleep 30 &&
        rabbitmqctl stop_app &&
        rabbitmqctl join_cluster rabbit@rabbitmq-1 &&
        rabbitmqctl start_app &&
        tail -f /dev/null
      "
      
  rabbitmq-3:
    image: rabbitmq:3.11-management
    hostname: rabbitmq-3
    environment:
      RABBITMQ_ERLANG_COOKIE: "tihomo-cluster-secret-key"
      RABBITMQ_DEFAULT_USER: "tihomo"
      RABBITMQ_DEFAULT_PASS: "${RABBITMQ_PASSWORD}"
    volumes:
      - rabbitmq3-data:/var/lib/rabbitmq
      - ./rabbitmq-cluster.conf:/etc/rabbitmq/rabbitmq.conf
    networks:
      - tihomo-cluster
    depends_on:
      - rabbitmq-1
    command: >
      bash -c "
        rabbitmq-server &
        sleep 45 &&
        rabbitmqctl stop_app &&
        rabbitmqctl join_cluster rabbit@rabbitmq-1 &&
        rabbitmqctl start_app &&
        tail -f /dev/null
      "

  # HAProxy Load Balancer for RabbitMQ
  rabbitmq-lb:
    image: haproxy:2.6
    ports:
      - "5673:5672"  # AMQP load balanced port
      - "15673:15672" # Management UI load balanced port
    volumes:
      - ./haproxy-rabbitmq.cfg:/usr/local/etc/haproxy/haproxy.cfg
    depends_on:
      - rabbitmq-1
      - rabbitmq-2
      - rabbitmq-3
    networks:
      - tihomo-cluster

networks:
  tihomo-cluster:
    driver: overlay
    
volumes:
  rabbitmq1-data:
  rabbitmq2-data:
  rabbitmq3-data:
```

### 7.2 Enhanced Observability Stack
```yaml
# observability-stack.yml
version: '3.8'
services:
  # Grafana for visualization
  grafana:
    image: grafana/grafana:9.5.0
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=${GRAFANA_PASSWORD}
      - GF_USERS_ALLOW_SIGN_UP=false
      - GF_INSTALL_PLUGINS=grafana-piechart-panel,grafana-worldmap-panel
    volumes:
      - grafana-storage:/var/lib/grafana
      - ./grafana/provisioning:/etc/grafana/provisioning
      - ./grafana/dashboards:/var/lib/grafana/dashboards
    networks:
      - monitoring

  # Loki for log aggregation
  loki:
    image: grafana/loki:2.8.0
    ports:
      - "3100:3100"
    command: -config.file=/etc/loki/local-config.yaml
    volumes:
      - ./loki-config.yaml:/etc/loki/local-config.yaml
      - loki-data:/loki
    networks:
      - monitoring

  # Promtail for log collection
  promtail:
    image: grafana/promtail:2.8.0
    volumes:
      - /var/log:/var/log:ro
      - /var/lib/docker/containers:/var/lib/docker/containers:ro
      - ./promtail-config.yaml:/etc/promtail/config.yml
    command: -config.file=/etc/promtail/config.yml
    networks:
      - monitoring

  # Prometheus for metrics
  prometheus:
    image: prom/prometheus:v2.44.0
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - ./prometheus-rules.yml:/etc/prometheus/rules.yml
      - prometheus-data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--storage.tsdb.retention.time=30d'
      - '--web.enable-lifecycle'
      - '--web.enable-admin-api'
    networks:
      - monitoring

  # Jaeger for distributed tracing
  jaeger:
    image: jaegertracing/all-in-one:1.46
    ports:
      - "16686:16686"  # Jaeger UI
      - "14268:14268"  # HTTP collector
      - "14250:14250"  # gRPC collector
    environment:
      - COLLECTOR_OTLP_ENABLED=true
      - SPAN_STORAGE_TYPE=badger
      - BADGER_EPHEMERAL=false
      - BADGER_DIRECTORY_VALUE=/badger/data
      - BADGER_DIRECTORY_KEY=/badger/key
    volumes:
      - jaeger-badger:/badger
    networks:
      - monitoring

  # AlertManager for alerting
  alertmanager:
    image: prom/alertmanager:v0.25.0
    ports:
      - "9093:9093"
    volumes:
      - ./alertmanager.yml:/etc/alertmanager/alertmanager.yml
      - alertmanager-data:/alertmanager
    command:
      - '--config.file=/etc/alertmanager/alertmanager.yml'
      - '--storage.path=/alertmanager'
      - '--web.external-url=http://localhost:9093'
    networks:
      - monitoring

networks:
  monitoring:
    driver: bridge

volumes:
  grafana-storage:
  loki-data:
  prometheus-data:
  jaeger-badger:
  alertmanager-data:
```

### 7.3 Advanced Grafana Dashboards
```json
{
  "dashboard": {
    "id": null,
    "title": "TiHoMo Microservices - Business KPIs",
    "tags": ["tihomo", "business", "kpi"],
    "timezone": "browser",
    "panels": [
      {
        "id": 1,
        "title": "Transaction Success Rate by Service",
        "type": "stat",
        "targets": [
          {
            "expr": "rate(transactions_processed_total{outcome=\"success\"}[5m]) / rate(transactions_processed_total[5m]) * 100",
            "legendFormat": "{{service}}"
          }
        ],
        "fieldConfig": {
          "defaults": {
            "color": {"mode": "thresholds"},
            "thresholds": {
              "steps": [
                {"color": "red", "value": 0},
                {"color": "yellow", "value": 99},
                {"color": "green", "value": 99.5}
              ]
            },
            "unit": "percent",
            "min": 95,
            "max": 100
          }
        }
      },
      {
        "id": 2,
        "title": "Saga Completion Rates",
        "type": "timeseries",
        "targets": [
          {
            "expr": "rate(saga_completions_total{outcome=\"success\"}[5m])",
            "legendFormat": "{{saga_type}} - Success"
          },
          {
            "expr": "rate(saga_completions_total{outcome=\"failed\"}[5m])",
            "legendFormat": "{{saga_type}} - Failed"
          }
        ]
      },
      {
        "id": 3,
        "title": "Message Processing Latency P95",
        "type": "timeseries",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(message_processing_duration_ms_bucket[5m]))",
            "legendFormat": "{{service}} P95"
          },
          {
            "expr": "histogram_quantile(0.99, rate(message_processing_duration_ms_bucket[5m]))",
            "legendFormat": "{{service}} P99"
          }
        ],
        "fieldConfig": {
          "defaults": {
            "unit": "ms",
            "thresholds": {
              "steps": [
                {"color": "green", "value": 0},
                {"color": "yellow", "value": 100},
                {"color": "red", "value": 1000}
              ]
            }
          }
        }
      },
      {
        "id": 4,
        "title": "Circuit Breaker Status",
        "type": "table",
        "targets": [
          {
            "expr": "circuit_breaker_state",
            "legendFormat": "{{service}} - {{endpoint}}"
          }
        ],
        "transformations": [
          {
            "id": "organize",
            "options": {
              "excludeByName": {},
              "indexByName": {},
              "renameByName": {
                "service": "Service",
                "endpoint": "Endpoint",
                "Value": "State"
              }
            }
          }
        ]
      },
      {
        "id": 5,
        "title": "SLA Violations",
        "type": "timeseries",
        "targets": [
          {
            "expr": "rate(sla_violations_total[5m])",
            "legendFormat": "{{service}} - {{violation_type}}"
          }
        ],
        "alert": {
          "conditions": [
            {
              "evaluator": {"params": [1], "type": "gt"},
              "operator": {"type": "and"},
              "query": {"params": ["A", "5m", "now"]},
              "reducer": {"params": [], "type": "avg"},
              "type": "query"
            }
          ],
          "executionErrorState": "alerting",
          "for": "5m",
          "frequency": "10s",
          "handler": 1,
          "name": "SLA Violation Alert",
          "noDataState": "no_data",
          "notifications": []
        }
      }
    ],
    "time": {
      "from": "now-1h",
      "to": "now"
    },
    "refresh": "30s"
  }
}
```

## 8. Updated Implementation Roadmap

> **🔧 Expert Adjustment**: Revised timeline addressing all critical improvements với prioritized delivery phases.

### Phase 1: Foundation & Critical Fixes (Week 1-2)
**Priority: HIGH - Address Expert Critical Issues**

#### Week 1: Core Architecture Fixes
- ✅ **Saga Pattern Redesign**
  - Implement 3 focused sagas (Core Transaction, Budget Management, Notification)
  - Remove complex Transaction Processing Saga
  - Add proper compensation logic
  
- ✅ **Message Design Optimization**
  - Implement minimal coupling message patterns
  - Add event-carried state transfer
  - Create message versioning strategy

- 🔄 **Security Implementation**
  - Add message encryption for sensitive data
  - Implement service-to-service JWT authentication
  - Add input validation và sanitization

#### Week 2: Resilience & Error Handling
- 🔄 **Polly Integration**
  - Circuit breaker patterns
  - Exponential backoff retry policies
  - Timeout management
  
- 🔄 **Dead Letter Queue**
  - DLQ consumer implementation
  - Error notification system
  - Manual processing workflows

### Phase 2: Advanced Observability (Week 3-4)
**Priority: HIGH - Production Readiness**

#### Week 3: Monitoring & Health Checks
- 🔄 **Health Check Implementation**
  - Service health checks
  - Dependency health checks
  - Business logic health checks
  
- 🔄 **Business Metrics**
  - Transaction processing metrics
  - Saga success/failure rates
  - SLA compliance tracking

#### Week 4: Correlation & Tracing
- 🔄 **Correlation ID Implementation**
  - Request correlation propagation
  - Distributed tracing integration
  - Log correlation enhancement
  
- 🔄 **Advanced Dashboards**
  - Business KPI dashboards
  - Technical performance dashboards
  - SLA monitoring dashboards

### Phase 3: Performance & Scalability (Week 5-6)
**Priority: MEDIUM - Optimization**

#### Week 5: Message Optimization
- 🔄 **Message Partitioning**
  - User-based partitioning strategy
  - Account-based partitioning
  - Performance testing
  
- 🔄 **Batch Processing**
  - Batch message consumers
  - Optimized database operations
  - Throughput testing

#### Week 6: Infrastructure Enhancement
- 🔄 **High Availability Setup**
  - RabbitMQ cluster deployment
  - Load balancer configuration
  - Failover testing
  
- 🔄 **Production Deployment**
  - Container orchestration
  - Environment configuration
  - Disaster recovery procedures

### Phase 4: Testing & Validation (Week 7)
**Priority: HIGH - Quality Assurance**

- 🔄 **Load Testing**
  - Saga pattern performance testing
  - Message throughput testing
  - Chaos engineering scenarios
  
- 🔄 **Security Testing**
  - Penetration testing
  - Message encryption validation
  - Authentication/authorization testing
  
- 🔄 **SLA Validation**
  - Performance benchmarking
  - Availability testing
  - Recovery time testing

## 9. Success Criteria & KPIs

> **🔧 Expert Standards**: Production-ready metrics và financial application compliance.

### 9.1 Technical KPIs
| Metric | Target | Critical Threshold | Measurement |
|--------|--------|-------------------|-------------|
| **Saga Success Rate** | >99.5% | <99% triggers alert | 5-minute rolling average |
| **Message Processing Latency** | P95 <100ms | P95 >500ms triggers alert | Per-service measurement |
| **Service Availability** | >99.9% | <99.5% triggers escalation | Uptime monitoring |
| **Error Rate** | <0.1% | >0.5% triggers alert | Request error percentage |
| **Circuit Breaker Trips** | <5/hour | >20/hour triggers alert | Per-endpoint monitoring |

### 9.2 Business KPIs
| Metric | Target | Measurement Method |
|--------|--------|--------------------|
| **Transaction Processing Time** | <2 seconds end-to-end | User journey tracking |
| **System Downtime** | <4 hours/month | Cumulative outage time |
| **Data Consistency** | 100% | Zero lost transactions |
| **SLA Compliance** | >99.5% | Multi-metric SLA scoring |

### 9.3 Security & Compliance
- **Message Encryption**: 100% sensitive data encrypted
- **Authentication**: 100% service-to-service calls authenticated
- **Audit Trail**: Complete transaction audit với correlation IDs
- **Data Retention**: Compliant với financial regulations
- **GDPR Compliance**: User data handling trong messages

### 9.4 Operational Excellence
- **Zero-Downtime Deployments**: Blue-green deployment capability
- **Automatic Recovery**: Circuit breaker và retry patterns
- **Proactive Monitoring**: Alert-driven incident response
- **Documentation**: Complete runbook và troubleshooting guides

---

## 📋 Conclusion

This enhanced design addresses all expert feedback while maintaining the solid foundation of the original architecture. The improvements focus on:

1. **Production Readiness**: Comprehensive resilience, security, và monitoring
2. **Maintainability**: Simplified saga patterns và clear separation of concerns  
3. **Scalability**: Performance optimizations và high availability infrastructure
4. **Operational Excellence**: Advanced observability và automated recovery

The revised implementation roadmap ensures systematic delivery of improvements with clear success criteria for a financial application requiring 99.9%+ availability.

**Next Steps**: Begin Phase 1 implementation with saga redesign và security enhancements as highest priority items.

### Database Configuration Notes

> **📋 Database Technology**: TiHoMo sử dụng **PostgreSQL** làm primary database với những lợi ích:
> - **ACID Compliance**: Đảm bảo transaction consistency cho financial data
> - **JSON Support**: Native JSON/JSONB support cho flexible metadata
> - **Performance**: Excellent performance với complex queries và large datasets
> - **Extensions**: Rich ecosystem với extensions như TimescaleDB cho time-series data
> - **Open Source**: Cost-effective solution với enterprise-grade features

#### PostgreSQL Connection Configuration
```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=TiHoMo_CoreFinance;Username=tihomo;Password=secure-password;Include Error Detail=true",
    "MoneyManagement": "Host=localhost;Database=TiHoMo_MoneyManagement;Username=tihomo;Password=secure-password;Include Error Detail=true",
    "Identity": "Host=localhost;Database=TiHoMo_Identity;Username=tihomo;Password=secure-password;Include Error Detail=true"
  }
}

// DbContext Configuration
services.AddDbContext<CoreFinanceDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    })
    .UseSnakeCaseNamingConvention() // Convert to snake_case for PostgreSQL
);
```

#### PostgreSQL-Specific Health Checks
```csharp
// Enhanced PostgreSQL health checks
services.AddHealthChecks()
    .AddNpgSql(connectionString, 
        healthQuery: "SELECT 1;", 
        name: "postgresql-corefinance",
        tags: new[] { "database", "postgresql", "corefinance" })
    .AddNpgSql(moneyManagementConnection,
        healthQuery: "SELECT version();",
        name: "postgresql-moneymanagement", 
        tags: new[] { "database", "postgresql", "moneymanagement" })
    .AddCheck<PostgreSQLAdvancedHealthCheck>("postgresql-advanced");

public class PostgreSQLAdvancedHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            // Check connection count
            using var cmd = new NpgsqlCommand(
                "SELECT count(*) FROM pg_stat_activity WHERE state = 'active';", 
                connection);
            var activeConnections = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
            
            // Check database size
            using var sizeCmd = new NpgsqlCommand(
                "SELECT pg_size_pretty(pg_database_size(current_database()));", 
                connection);
            var dbSize = await sizeCmd.ExecuteScalarAsync(cancellationToken) as string;
            
            var data = new Dictionary<string, object>
            {
                ["active_connections"] = activeConnections,
                ["database_size"] = dbSize,
                ["server_version"] = connection.PostgreSqlVersion.ToString()
            };
            
            // Warning if too many active connections
            if (activeConnections > 80)
                return HealthCheckResult.Degraded("High connection count", data: data);
            
            return HealthCheckResult.Healthy("PostgreSQL healthy", data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection failed", ex);
        }
    }
}
```