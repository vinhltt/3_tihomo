# techContext.md

## Công nghệ sử dụng
- Backend: .NET 9 (C#), ASP.NET Core 9.0 với Clean Architecture
- API Gateway: Ocelot Gateway cho request routing
- Message Bus: RabbitMQ (planned/future)
- Database: PostgreSQL với EFCore.NamingConventions (snake_case), mỗi service một DB riêng
- Workflow engine: n8n (planned/future)
- File Storage: MinIO (planned/future)
- Containerization: Docker, docker-compose cho dev, TrueNAS cho production
- Logging/Monitoring: Serilog structured logging, OpenTelemetry tracing, Prometheus metrics, Grafana dashboards
- Authentication: JWT + OAuth 2.0/OIDC + API Keys với social login (Google/Facebook)
- **Frontend: Nuxt 3 + Vue 3 + TypeScript + Tailwind CSS + Pinia + VRISTO admin template**
- **Testing: xUnit + FluentAssertions + Bogus cho fake data generation**
- **Resilience: Polly v8 với circuit breaker, retry, timeout patterns**
- **ORM: Entity Framework Core với AutoMapper cho object mapping**
- **Validation: FluentValidation cho request DTOs**
- **CI/CD: GitHub Actions với TrueNAS deployment, health monitoring**

## Thiết lập phát triển
- Sử dụng file .env cho cấu hình môi trường
- Phân chia môi trường: dev, staging, production
- Hỗ trợ backup/restore qua script, kiểm thử định kỳ
- CI/CD tự động build, test, deploy, rollback
- Hạ tầng mô tả bằng code (IaC), ưu tiên cloud-native, auto-scaling
- **Unit test organization: partial class pattern với thư mục con theo service name**
- **Test coverage target: >80% cho tất cả services**
- **✅ Production Ready Services (July 2025): Identity, CoreFinance, MoneyManagement, ExcelApi achieve production ready status**
- **✅ GitHub Actions CI/CD (July 2025): Complete TrueNAS deployment pipeline với security scanning, rolling updates, health checks**
- **✅ Enhanced Observability (July 2025): OpenTelemetry, Prometheus metrics, Serilog structured logging, circuit breaker monitoring**
- **✅ Consolidated Architecture (July 2025): Identity project merged into single application với dual authentication (Cookie + JWT)**

## Ràng buộc kỹ thuật
- Phải chạy ổn định trên Linux và Windows
- Hạn chế tối đa downtime khi triển khai
- Đảm bảo hiệu năng: API chính <2s với 1000 user đồng thời, import 1000 dòng <30s
- Đảm bảo bảo mật: tuân thủ OWASP Top 10, mã hóa dữ liệu nhạy cảm, RBAC, policy-based authorization
- Đảm bảo maintainability: Clean Architecture, SOLID, test coverage >80%
- Đảm bảo usability: UI trực quan, thao tác chính không quá 3 bước
- Đảm bảo reliability: backup/restore định kỳ, event-driven sync, retry mechanism
- Đảm bảo compliance: (Cần xác định quy định pháp lý nếu có)
- **Background job performance: sinh giao dịch dự kiến cho 1000 mẫu định kỳ trong <5 phút**
- **Database constraints: foreign key relationships được enforce, cascade delete được cấu hình phù hợp**

## Dependency
- n8n >= 1.0
- Docker >= 20.x
- PostgreSQL >= 13
- RabbitMQ >= 3.x
- Ocelot >= 17.x
- OpenIddict >= 4.x
- **Entity Framework Core >= 8.x**
- **Quartz.NET >= 3.x hoặc Hangfire >= 1.8.x (cho RecurringTransactionTemplateService background jobs)**
- **xUnit >= 2.4.x**
- **FluentAssertions >= 6.x**
- **Bogus >= 34.x**
- **AutoMapper >= 12.x**
- **FluentValidation >= 11.x (hoặc phiên bản tương thích)**
- **Nuxt 3 >= 3.13.x với Vue 3, TypeScript, Tailwind CSS**
- **@nuxtjs/i18n >= 8.5.x cho đa ngôn ngữ**
- **@pinia/nuxt >= 0.5.x cho state management**
- **@headlessui/vue >= 1.7.x cho UI components**
- **vue3-apexcharts >= 1.6.x cho charts**
- **@vueuse/core >= 11.1.x cho composables**

## Pattern sử dụng công cụ
- Ưu tiên dùng docker-compose cho local dev/test
- Script hóa các thao tác lặp lại (backup, restore, deploy)
- Tích hợp health check, logging, monitoring, alerting tự động
- **Sử dụng Entity Framework migrations cho database schema changes**
- **Background job scheduling: cron expressions cho việc sinh giao dịch dự kiến định kỳ**
- **Unit test pattern: partial class với file riêng cho mỗi method, helper functions trong TestHelpers.cs**
- **AutoMapper configuration: profile classes riêng cho mỗi domain entity, test mapping logic riêng**
- **Repository pattern với UnitOfWork cho data access layer**
- **Validation: Sử dụng FluentValidation với tính năng tự động validate qua middleware của ASP.NET Core.**

## Code Style and Documentation
- Ưu tiên sử dụng XML comments cho các lớp, phương thức và thuộc tính công khai trong mã C# để tạo tài liệu API tự động (ví dụ: Swagger).
- Khi cần comment song ngữ (tiếng Anh và tiếng Việt) trong XML comments, sử dụng định dạng sau:
  - Viết nội dung tiếng Anh trước, kết thúc bằng ` (EN)`.
  - Sử dụng thẻ `<br/>` để xuống dòng.
  - Viết nội dung tiếng Việt tiếp theo, kết thúc bằng ` (VI)`.
  - Áp dụng cho các thẻ như `<summary>`, `<param>`, `<returns>`, v.v.
- Ví dụ:
  ```csharp
  /// <summary>
  /// English summary here (EN)<br/>
  /// Tóm tắt tiếng Việt ở đây (VI)
  /// </summary>
  /// <param name="paramName">
  /// English description for parameter (EN)<br/>
  /// Mô tả tiếng Việt cho tham số (VI)
  /// </param>
  // ... rest of the code
  ``` 