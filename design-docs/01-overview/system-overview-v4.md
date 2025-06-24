# TiHoMo System Overview v4

## 1. Tổng quan hệ thống

Hệ thống TiHoMo được thiết kế theo kiến trúc microservices, với các bounded context rõ ràng và độc lập. Mỗi service có database riêng và giao tiếp thông qua API Gateway và message bus.

## 2. Các Bounded Context và Microservices

### 2.1 Identity & Access (Simplified)
- **Identity.Api** (Port 5228): Single service cho social auth, user management, API key management
- **Social Providers**: Google, Facebook, Apple authentication integration
- **Token Management**: JWT generation, refresh token handling
- **Database**: db_identity (PostgreSQL)

### 2.2 Core Finance
- **AccountService**: Quản lý tài khoản
- **TransactionService**: Xử lý giao dịch
- **StatementService**: Quản lý sao kê và import từ danh sách giao dịch
- **Database**: db_finance (PostgreSQL)

### 2.3 Money Management
- **BudgetService**: Quản lý ngân sách
- **JarService**: Quản lý SixJars
- **SharedExpenseService**: Quản lý chi tiêu chia sẻ
- **Database**: db_money (PostgreSQL)

### 2.4 Planning & Investment
- **DebtService**: Quản lý khoản nợ
- **GoalService**: Quản lý mục tiêu tài chính
- **InvestmentService**: Quản lý đầu tư
- **Database**: db_planning (PostgreSQL)

### 2.5 Reporting & Integration
- **ReportingService**: Tạo báo cáo và phân tích
- **NotificationService**: Gửi thông báo
- **IntegrationService**: Kết nối với các dịch vụ bên ngoài
- **StatementParserService**: Phân tích và trích xuất dữ liệu từ file sao kê
- **Database**: db_reporting (PostgreSQL)

## 3. Architecture Principles
- **Bounded Context Isolation**: Mỗi service độc lập với database riêng
- **Event-Driven Communication**: Message bus cho inter-service communication
- **API Gateway Pattern**: Single entry point cho external requests
- **Social Authentication**: Simplified authentication với social providers
- **PostgreSQL Consistency**: Tất cả services sử dụng PostgreSQL

## 4. Technology Stack
- **Backend**: .NET 9, ASP.NET Core, Entity Framework Core
- **Frontend**: Nuxt 3, Vue 3, TypeScript, Tailwind CSS
- **Database**: PostgreSQL
- **Message Bus**: RabbitMQ with MassTransit
- **Gateway**: Ocelot API Gateway
- **Monitoring**: Grafana, Loki, Prometheus
- **Containerization**: Docker, Docker Compose

## 5. Infrastructure Components
- **API Gateway**: Ocelot (Port 5000)
- **Message Queue**: RabbitMQ
- **Database**: PostgreSQL instances
- **Monitoring**: Grafana + Loki + Prometheus stack
- **Frontend**: Nuxt application (Port 3333)
