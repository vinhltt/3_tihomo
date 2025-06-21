# TiHoMo Message Queue & Logging Implementation Plan

## Executive Summary

This implementation plan focuses on building and testing the message queue and logging infrastructure for TiHoMo microservices. The primary goal is to establish a robust, observable system where file upload triggers a message-driven workflow between ExcelAPI and CoreFinance services.

**Key Objectives:**
- Implement message queue communication between services
- Establish comprehensive logging and observability
- Test message flow and log visualization
- Use stub business logic to focus on infrastructure

**Success Metrics:**
- End-to-end message flow working
- All operations logged with correlation tracking
- Real-time dashboards showing system health
- Sub-second message processing latency

## Technical Architecture

### Core Flow
```
User Upload File → ExcelAPI Extract → RabbitMQ Message → CoreFinance Process → PostgreSQL Save
                     ↓               ↓                ↓                    ↓
                 File Logs      Message Logs    Processing Logs      Database Logs
                     ↓               ↓                ↓                    ↓
                                    ELK Stack → Kibana Dashboards
                                       ↓
                                Grafana Metrics
```

### Services Involved
1. **ExcelAPI**: File upload and data extraction service
2. **CoreFinance**: Transaction processing service
3. **RabbitMQ**: Message broker with clustering
4. **PostgreSQL**: Primary database with logging
5. **ELK Stack**: Elasticsearch, Logstash, Kibana
6. **Grafana**: Metrics visualization

## Implementation Phases

### Phase 1: Infrastructure Setup (Week 1)

#### Day 1-2: RabbitMQ Cluster Setup
**Deliverables:**
- Docker Compose for RabbitMQ cluster
- Management UI accessible
- Exchange and queue configuration
- High availability setup

**Configuration:**
```yaml
# docker-compose.rabbitmq.yml
version: '3.8'
services:
  rabbitmq-node1:
    image: rabbitmq:3.12-management
    environment:
      RABBITMQ_ERLANG_COOKIE: "SWQOKODSQALRPCLNMEQG"
      RABBITMQ_DEFAULT_USER: "admin"
      RABBITMQ_DEFAULT_PASS: "admin123"
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
```

#### Day 3-4: ELK Stack Setup
**Deliverables:**
- Elasticsearch cluster with 3 nodes
- Logstash with custom configuration
- Kibana with initial index patterns
- Log retention policies

**Key Configurations:**
- Elasticsearch: 3GB heap, cluster discovery
- Logstash: JSON parsing, correlation ID extraction
- Kibana: Index patterns for tihomo-logs-*

#### Day 5: Grafana Setup
**Deliverables:**
- Grafana instance with datasources
- Initial dashboards for system metrics
- Alert rules for critical thresholds
- Integration with RabbitMQ and PostgreSQL

#### Day 6-7: PostgreSQL Setup
**Deliverables:**
- PostgreSQL 15 with optimized configuration
- Database schemas for CoreFinance
- Connection pooling setup
- Query logging enabled

### Phase 2: Message Queue Implementation (Week 2)

#### Day 1-2: Message Contracts & Shared Libraries
**Deliverables:**
- Shared.Contracts project with message definitions
- Message versioning strategy
- Serialization configuration
- Correlation ID propagation library

**Message Contract Example:**
```csharp
public class TransactionDataExtracted : IEvent
{
    public Guid EventId { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public string FileName { get; set; }
    public List<TransactionData> Transactions { get; set; }
    public ProcessingMetadata MetaData { get; set; }
}
```

#### Day 3-4: ExcelAPI Publisher Implementation
**Deliverables:**
- File upload controller with logging
- Excel extraction service (stub implementation)
- Message publisher with retry logic
- Circuit breaker implementation

**Key Components:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IExcelProcessingService _excelService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<UploadController> _logger;

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var correlationId = Guid.NewGuid();
        using var activity = Activity.StartActivity("FileUpload");
        activity?.SetTag("correlation.id", correlationId.ToString());

        _logger.LogInformation("File upload started: {FileName}, Size: {FileSize}, CorrelationId: {CorrelationId}",
            file.FileName, file.Length, correlationId);

        // Stub extraction - focus on logging and messaging
        var transactions = await _excelService.ExtractTransactions(file, correlationId);
        
        var eventMessage = new TransactionDataExtracted
        {
            EventId = Guid.NewGuid(),
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            UserId = User.Identity.Name,
            FileName = file.FileName,
            Transactions = transactions,
            MetaData = new ProcessingMetadata
            {
                ProcessingDuration = TimeSpan.FromMilliseconds(100), // Mock
                TotalRecords = transactions.Count
            }
        };

        await _messagePublisher.PublishAsync(eventMessage);
        
        _logger.LogInformation("File processing completed: {CorrelationId}, Records: {RecordCount}",
            correlationId, transactions.Count);

        return Ok(new { CorrelationId = correlationId, RecordCount = transactions.Count });
    }
}
```

#### Day 5-6: CoreFinance Consumer Implementation
**Deliverables:**
- Event handler for TransactionDataExtracted
- Transaction service with PostgreSQL integration
- Dead letter queue handling
- Idempotency implementation

**Key Components:**
```csharp
public class TransactionEventHandler : IEventHandler<TransactionDataExtracted>
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionEventHandler> _logger;

    public async Task Handle(TransactionDataExtracted eventMessage)
    {
        using var activity = Activity.StartActivity("ProcessTransactionData");
        activity?.SetTag("correlation.id", eventMessage.CorrelationId.ToString());

        _logger.LogInformation("Processing transaction data: {CorrelationId}, Records: {RecordCount}",
            eventMessage.CorrelationId, eventMessage.Transactions.Count);

        try
        {
            // Stub processing - focus on logging
            foreach (var transaction in eventMessage.Transactions)
            {
                await _transactionService.SaveTransactionAsync(transaction, eventMessage.CorrelationId);
            }

            _logger.LogInformation("Transaction processing completed: {CorrelationId}",
                eventMessage.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction processing failed: {CorrelationId}",
                eventMessage.CorrelationId);
            throw;
        }
    }
}
```

#### Day 7: Integration Testing
**Deliverables:**
- End-to-end integration tests
- Message flow validation
- Error scenario testing
- Performance baseline establishment

### Phase 3: Logging & Observability (Week 3)

#### Day 1-2: Serilog Configuration
**Deliverables:**
- Serilog setup in both services
- Structured logging configuration
- Log enrichment with correlation IDs
- File and console sinks

**Serilog Configuration:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "ExcelAPI")
    .Enrich.WithCorrelationId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/excel-api-.log", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ServiceName}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
    {
        IndexFormat = "tihomo-logs-{0:yyyy.MM.dd}",
        AutoRegisterTemplate = true
    })
    .CreateLogger();
```

#### Day 3-4: Structured Logging Implementation
**Deliverables:**
- Logging standards implementation
- Custom log enrichers
- Performance logging
- Error context capture

**Logging Standards:**
```csharp
public static class LoggerExtensions
{
    public static void LogFileUploadStarted(this ILogger logger, string fileName, long fileSize, Guid correlationId)
    {
        logger.LogInformation("File upload started: {FileName} ({FileSize} bytes) [{CorrelationId}]",
            fileName, fileSize, correlationId);
    }

    public static void LogMessagePublished(this ILogger logger, string eventType, Guid correlationId, TimeSpan duration)
    {
        logger.LogInformation("Message published: {EventType} [{CorrelationId}] in {Duration}ms",
            eventType, correlationId, duration.TotalMilliseconds);
    }

    public static void LogDatabaseOperation(this ILogger logger, string operation, Guid correlationId, int recordCount, TimeSpan duration)
    {
        logger.LogInformation("Database operation: {Operation} completed [{CorrelationId}] - {RecordCount} records in {Duration}ms",
            operation, correlationId, recordCount, duration.TotalMilliseconds);
    }
}
```

#### Day 5-6: Log Shipping & Kibana Dashboards
**Deliverables:**
- Logstash configuration for log parsing
- Kibana index patterns and mappings
- Log analysis dashboards
- Search templates for common queries

**Kibana Dashboards:**
1. **System Overview**: Total requests, error rates, response times
2. **Message Flow**: Queue depths, processing times, failure rates
3. **Correlation Tracking**: End-to-end request tracing
4. **Error Analysis**: Error patterns, stack traces, frequency

#### Day 7: Grafana Metrics Dashboards
**Deliverables:**
- Application metrics collection
- System metrics integration
- Business metrics tracking
- Alert rules configuration

**Key Metrics:**
- Message queue depth and throughput
- Database connection pool usage
- API response times and error rates
- System resources (CPU, memory, disk)

### Phase 4: Testing & Documentation (Week 4)

#### Day 1-2: Comprehensive Testing
**Test Scenarios:**

1. **Happy Path Testing:**
   - Upload valid Excel file
   - Verify message publishing
   - Confirm message consumption
   - Validate database save
   - Check end-to-end logging

2. **Error Scenario Testing:**
   - Invalid file format
   - Message queue failure
   - Database connection issues
   - Service downtime simulation

3. **Performance Testing:**
   - Concurrent file uploads
   - Large file processing
   - Message queue throughput
   - Database load testing

4. **Resilience Testing:**
   - Circuit breaker activation
   - Retry mechanism validation
   - Dead letter queue processing
   - Service recovery testing

#### Day 3-4: Performance Validation
**Performance Targets:**
- File upload: < 5 seconds for 10MB files
- Message processing: < 100ms per transaction
- Database save: < 50ms per transaction
- End-to-end: < 10 seconds for 1000 transactions

**Load Testing:**
- 100 concurrent users
- 1000 transactions per minute
- 24-hour endurance test
- Memory leak detection

#### Day 5-6: Documentation
**Documentation Deliverables:**
1. **Setup Guide**: Infrastructure deployment instructions
2. **Developer Guide**: API usage and message contracts
3. **Operations Guide**: Monitoring and troubleshooting
4. **Testing Guide**: Test scenarios and validation
5. **Performance Benchmarks**: Baseline metrics and targets

#### Day 7: Demo & Final Validation
**Demo Scenarios:**
1. Live file upload with real-time log monitoring
2. Error injection and recovery demonstration
3. Dashboard walkthrough showing system health
4. Performance metrics under load

## Success Criteria

### Functional Requirements
- ✅ File upload triggers message flow
- ✅ Messages processed without loss
- ✅ All operations logged with correlation tracking
- ✅ Real-time monitoring dashboards operational
- ✅ Error scenarios handled gracefully

### Non-Functional Requirements
- ✅ Message processing latency < 100ms
- ✅ System availability > 99.5%
- ✅ Log searchability within 5 seconds
- ✅ Dashboard refresh rate < 30 seconds
- ✅ Zero message loss during normal operations

### Quality Requirements
- ✅ Code coverage > 80%
- ✅ All critical paths have integration tests
- ✅ Performance benchmarks documented
- ✅ Security scanning passed
- ✅ Documentation complete and reviewed

## Risk Mitigation

### Technical Risks
1. **Message Queue Failure**
   - Mitigation: Clustering, persistent queues, monitoring
   - Backup: File-based queue fallback

2. **Database Connection Issues**
   - Mitigation: Connection pooling, retry logic, health checks
   - Backup: Circuit breaker with queue buffering

3. **Log Volume Overload**
   - Mitigation: Log level management, retention policies
   - Backup: Log sampling during high load

### Operational Risks
1. **Infrastructure Complexity**
   - Mitigation: Automated deployment, health checks
   - Backup: Simplified deployment mode

2. **Performance Degradation**
   - Mitigation: Load testing, performance monitoring
   - Backup: Auto-scaling configuration

## Next Steps

1. **Infrastructure Provisioning**: Set up development environment
2. **Team Coordination**: Assign responsibilities and timelines
3. **Environment Configuration**: Prepare staging and production configs
4. **Monitoring Setup**: Establish baseline metrics and alerts
5. **Go-Live Planning**: Prepare deployment and rollback procedures

## Conclusion

This implementation plan provides a structured approach to building and testing TiHoMo's message queue and logging infrastructure. By focusing on observability and testing rather than complex business logic, we can establish a solid foundation for future development while demonstrating the system's reliability and performance characteristics.

The 4-week timeline balances thorough implementation with practical delivery, ensuring all stakeholders can see tangible progress and working demonstrations of the system's capabilities.
