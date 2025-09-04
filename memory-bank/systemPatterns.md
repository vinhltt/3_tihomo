# systemPatterns.md

## Kiến trúc hệ thống
- Microservices với Clean Architecture pattern, Domain-Driven Design.
- Mỗi bounded context là một service độc lập, có database riêng (PostgreSQL), giao tiếp qua API Gateway (Ocelot).
- Sử dụng Docker để triển khai, TrueNAS cho production environment.
- Enhanced observability với OpenTelemetry, Prometheus, và Grafana monitoring.
- GitHub Actions CI/CD với automated deployment, security scanning, và health checks.

## Quyết định kỹ thuật chính
- Ưu tiên tích hợp qua API REST với comprehensive OpenAPI documentation.
- Sử dụng môi trường tách biệt cho phát triển, staging, production với TrueNAS deployment.
- Tất cả giao tiếp giữa các service phải qua API Gateway để tăng cường bảo mật.
- **✅ Enhanced Security Pattern (July 2025): JWT + OAuth 2.0 + API Keys với social login, Trivy security scanning, SSH security hardening.**
- **✅ Resilience Patterns (July 2025): Polly v8 circuit breaker, retry với exponential backoff, timeout protection.**
- **✅ CI/CD Pipeline (July 2025): GitHub Actions với rolling deployment, automatic backup, health validation.**
- Authentication: JWT + OAuth 2.0/OIDC + API Keys với Google/Facebook social login.
- Comprehensive observability: OpenTelemetry tracing, Prometheus metrics, Serilog structured logging.

## Pattern thiết kế
- Modular workflow: mỗi workflow là một module độc lập, có thể mở rộng.
- Sử dụng event-driven cho các tác vụ bất đồng bộ, retry, dead letter queue.
- Mỗi service phải có tài liệu mô tả rõ ràng về API, sự kiện, và database schema.
- Health check endpoint cho từng service, giám sát tự động.
- Backup/restore tự động hóa, kiểm thử định kỳ.
- **Foreign Key Design Pattern: Sử dụng nullable Guid (Guid?) cho tất cả Foreign Keys để tạo mối quan hệ linh hoạt, không quá chặt chẽ giữa các Entity. Điều này cho phép:**
  - **Tạo Entity mà không cần liên kết ngay lập tức với Entity khác**
  - **Xử lý các trường hợp dữ liệu không đầy đủ hoặc import từ nguồn bên ngoài**
  - **Hỗ trợ soft delete và orphaned records management**
  - **Tăng tính linh hoạt trong việc thiết kế API và business logic**

## Quan hệ thành phần
- API Gateway (Ocelot) là trung tâm điều phối requests.
- Các dịch vụ nghiệp vụ chính (Production Status):
  - **✅ Identity & Access (100% Complete)**: AuthService, UserService, RoleService, ApiKeyService với social login
  - **✅ Core Finance (100% Complete)**: AccountService, TransactionService, RecurringTransactionTemplateService, ExpectedTransactionService
  - **✅ Money Management (100% Complete)**: BudgetService, JarService, SharedExpenseService implementation
  - **✅ Excel API (100% Complete)**: Excel processing services trong src/be/ExcelApi
  - **🚧 Planning & Investment (Structure Ready)**: DebtService, GoalService, InvestmentService cần implementation
  - **📋 Reporting & Integration (Planned)**: ReportingService, NotificationService, IntegrationService
- Mỗi service gắn với database riêng (PostgreSQL với snake_case naming), không chia sẻ schema.
- **✅ Current Development Status (July 2025):**
  - **Build Success Rate**: 100% across all implemented services
  - **Test Coverage**: Comprehensive với xUnit + FluentAssertions
  - **Deployment**: TrueNAS production environment với GitHub Actions
  - **Monitoring**: Full observability stack với health checks

**Backend Organization:**
- **Tất cả backend services** được tổ chức trong `src/BE/` folder
- **ExcelApi** đã được di chuyển từ `src/ExcelApi/` vào `src/BE/ExcelApi/` để thống nhất cấu trúc
- **TiHoMo.sln** trong `src/BE/` quản lý tất cả backend projects
- **Docker configuration** đã được cập nhật để phù hợp với cấu trúc mới
- **✅ Docker Compose Security Pattern (December 28, 2024):**
  - **API Services Port Closure**: Identity, CoreFinance, Excel API services không còn expose ports ra localhost
  - **Gateway-Only Access**: Tất cả API requests phải đi qua Ocelot Gateway (localhost:5800)
  - **Internal Communication**: Services vẫn giao tiếp qua Docker network names (identity-api:8080, corefinance-api:8080, excel-api:8080)
  - **Development Tools Access**: Infrastructure services (Grafana, pgAdmin, RabbitMQ) vẫn có direct port access
  - **Security Benefits**: Single entry point, centralized authentication, no direct service bypass, simplified monitoring
- **✅ Identity Architecture Consolidation (June 9, 2025)**: Successfully merged Identity.Api into Identity.Sso
  - **Eliminated duplication**: Single project handles both SSO web interface and API functionality
  - **Dual authentication**: Cookie-based (SSO) + JWT-based (API) trong cùng application
  - **Unified configuration**: Combined appsettings, middleware, và dependency injection
  - **Simplified maintenance**: One project to build, deploy, và maintain
  - **URLs**: Web interface (http://localhost:5217), API docs (http://localhost:5217/swagger)
  - **Architecture Benefits**: 50% reduction in Identity projects (2→1), eliminated conflicting controllers

**Core Finance Services:**
- **RecurringTransactionTemplateService:**
  - Quản lý các mẫu giao dịch định kỳ (RecurringTransactionTemplate).
  - Cung cấp API để tạo/sửa/xóa/liệt kê mẫu giao dịch định kỳ.
  - Sinh ra các giao dịch dự kiến dựa trên mẫu (thông qua background worker hoặc event-driven).
  - Tính toán ngày thực hiện tiếp theo dựa trên frequency và custom interval.
  - Hỗ trợ quản lý trạng thái active/inactive của mẫu.

- **ExpectedTransactionService:**
  - Quản lý các giao dịch dự kiến (ExpectedTransaction) được sinh từ mẫu định kỳ.
  - Cung cấp lifecycle management: Pending → Confirmed/Cancelled/Completed.
  - Hỗ trợ điều chỉnh giao dịch dự kiến (amount, reason) với lưu trữ original amount.
  - Cung cấp dự báo dòng tiền (cash flow forecast) và phân tích theo category.
  - Tương tác với TransactionService khi confirm expected transaction.

## Luồng nghiệp vụ chính
- Import sao kê: User upload file → StatementService lưu file (MinIO) → publish event → StatementProcessor xử lý → TransactionService ghi nhận giao dịch.
- Ghi nhận giao dịch thủ công: User nhập → API Gateway → TransactionService → AccountService cập nhật số dư → publish event cho các service khác (report, budget, ...).
- **Quản lý giao dịch định kỳ:**
  - **User tạo mẫu → RecurringTransactionTemplateService lưu mẫu → background job sinh giao dịch dự kiến → ExpectedTransactionService quản lý lifecycle.**
  - **Background job chạy định kỳ → GenerateExpectedTransactionsForAllActiveTemplatesAsync → sinh giao dịch dự kiến cho tất cả mẫu active.**
  - **User xác nhận giao dịch dự kiến → ExpectedTransactionService.ConfirmTransactionAsync/AdjustTransactionAsync/CancelTransactionAsync → liên kết với Transaction thực tế (Confirm), cập nhật thông tin (Adjust), hoặc đánh dấu đã hủy (Cancel).**
- Quản lý ngân sách, mục tiêu, nợ, đầu tư: các service chuyên biệt, đồng bộ qua event bus.
- Báo cáo, thông báo: ReportingService, NotificationService consume event từ các service khác.

## Best Practices
- Clean Architecture, DDD, TDD, SOLID cho mọi service.
- Viết unit test, integration test đầy đủ, coverage > 80%.
- Chuẩn hóa sử dụng FluentAssertions cho assert kết quả trong unit test, tuân thủ .NET rule.
- **Validation: Sử dụng FluentValidation cho tất cả các request DTO. Các validator được đăng ký tập trung trong lớp `ServiceExtensions` thông qua extension method `AddApplicationValidators`. Validation được thực hiện tự động bởi middleware của ASP.NET Core.**
- **Trong unit test cho tầng service, ưu tiên sử dụng instance AutoMapper thật (được inject, không mock) để kiểm tra logic nghiệp vụ cùng với logic mapping, giả định rằng các AutoMapper profile đã được cấu hình đúng và được test riêng.**
- Triển khai blue-green/canary, feature flag cho release.
- Có tài liệu hướng dẫn backup, restore, disaster recovery.
- Định nghĩa acceptance criteria rõ ràng cho từng chức năng.
- Đảm bảo các yêu cầu phi chức năng: hiệu năng, mở rộng, bảo mật, usability, reliability, compliance.
- **Quy ước tổ chức file Unit Test: Sử dụng partial class với tên `ServiceNameTests` trải rộng trên nhiều file. Các file con, đặt tên theo định dạng `ServiceNameTests.MethodName.cs`, sẽ được nhóm vào một thư mục con cùng tên với lớp test (`ServiceNameTests`) bên trong thư mục test chính (`CoreFinance.Application.Tests`). Các hàm helper dùng chung cho test sẽ được đặt trong file `TestHelpers.cs` trong thư mục `Helpers`.**

## Lựa chọn giải pháp
- Import statement: ưu tiên manual upload (CSV/Excel), lên kế hoạch tích hợp API ngân hàng hoặc aggregator trong tương lai.
- Frontend: (Cần xác định, ví dụ React, Angular, Vue, Blazor).
- **Sinh giao dịch định kỳ: sử dụng background worker định kỳ (nightly job) với Quartz.NET hoặc Hangfire, kết hợp với event-driven khi tạo mẫu mới để có kết quả ngay lập tức.**
- **Domain models: RecurringTransactionTemplate và ExpectedTransaction được thiết kế với đầy đủ navigation properties để hỗ trợ Entity Framework relationships.**

## Lộ trình & Ưu tiên
- Triển khai theo phase: core service trước (Identity, Core Finance, Reporting), sau đó Money Management, Planning & Investment, cuối cùng là advanced features.
- **Core Finance đã hoàn thành: Account, Transaction, RecurringTransactionTemplate, ExpectedTransaction services.**
- **Tiếp theo: triển khai API Controllers, background job service, tích hợp với NotificationService và ReportingService.**
- Mỗi phase có checklist hoàn thành, review kiến trúc và bảo mật.

## Luồng triển khai quan trọng
- Tự động hóa backup/restore.
- CI/CD cho workflow và cấu hình.
- Health check, logging, monitoring, alerting tự động.
- **Background job scheduling cho việc sinh giao dịch dự kiến từ các mẫu định kỳ.**
- **Event publishing khi có thay đổi trong expected transactions để đồng bộ với reporting và notification services.**

## Identity Service Architecture Evolution

### Resilience Patterns (Phase 3 - June 2025)
- **Circuit Breaker Pattern:** Sử dụng Polly v8 ResiliencePipeline cho fault tolerance
- **Multi-layer Fallback Strategy:** External API → Cache → Local validation → Graceful degradation
- **Retry Strategy:** Decorrelated jitter backoff để avoid thundering herd problem
- **Timeout Management:** Configurable timeouts để prevent resource exhaustion
- **Health Monitoring:** Real-time circuit breaker state tracking cho operational visibility

### Observability Architecture (Phase 4 - June 2025)  
- **Triple Monitoring Strategy:** Metrics (Prometheus) + Tracing (OpenTelemetry) + Logging (Serilog)
- **Correlation Context:** Request correlation IDs propagated across all layers
- **Custom Metrics Design:** Business logic metrics integrated với infrastructure metrics
- **Structured Logging:** JSON format với correlation context cho log aggregation
- **Health Check Hierarchy:** Multi-level health checks từ infrastructure đến business logic

### Service Integration Patterns
- **Resilient Service Wrapper:** ResilientTokenVerificationService wraps enhanced services
- **Telemetry Integration:** TelemetryService injected vào business logic services
- **Middleware Pipeline:** ObservabilityMiddleware → ApiKeyAuthenticationMiddleware → Application
- **Dependency Injection Strategy:** Multiple DbContext registration để support different layers

### Production Operational Patterns
- **Correlation ID Propagation:** Unique request tracking across all services và log entries
- **Metrics Collection Strategy:** Runtime metrics (GC, memory) + Business metrics (token verification, cache performance)
- **Circuit Breaker States:** Automatic open/close/half-open state management với health monitoring
- **Error Recovery Workflows:** Multi-level fallback → Cache → Local parsing → Graceful degradation
- **Performance Monitoring:** Request timing, external provider response time, cache hit rates tracking