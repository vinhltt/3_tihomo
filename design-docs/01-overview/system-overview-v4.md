# TiHoMo System Overview v4

## 1. Tổng quan hệ thống

Hệ thống TiHoMo được thiết kế theo kiến trúc microservices, với các bounded context rõ ràng và độc lập. Mỗi service có database riêng và giao tiếp thông qua API Gateway và message bus. Hệ thống hỗ trợ dual authentication với JWT tokens cho web users và API keys cho third-party integrations.

## 2. Các Bounded Context và Microservices

### 2.1 Identity & Access (Enhanced with API Key Management)
- **Identity.Api** (Port 5228): Comprehensive authentication và authorization service
- **Core Features**:
  - Social authentication (Google, Facebook, Apple)
  - JWT token management với refresh token handling
  - **Enhanced API Key Management**: Complete API key lifecycle cho third-party integrations
  - User management và role-based access control
- **API Key Features**:
  - Secure key generation với "pfm_" prefix và SHA-256 hashing
  - Scope-based permissions (read, write, transactions:read, etc.)
  - Rate limiting per API key (configurable per minute/day)
  - IP whitelisting với CIDR notation support
  - Usage tracking và comprehensive audit logging
  - Security settings (HTTPS enforcement, CORS configuration)
- **Database**: db_identity (PostgreSQL)

### 2.2 Core Finance
- **AccountService**: Quản lý tài khoản tài chính
- **TransactionService**: Xử lý giao dịch với API key authentication support
- **StatementService**: Quản lý sao kê và import từ danh sách giao dịch
- **API Integration**: Full support cho API key authentication từ third-party apps
- **Database**: db_finance (PostgreSQL)

### 2.3 Money Management
- **BudgetService**: Quản lý ngân sách với API access
- **JarService**: Quản lý SixJars methodology
- **SharedExpenseService**: Quản lý chi tiêu chia sẻ
- **API Integration**: API key support cho budget monitoring apps
- **Database**: db_money (PostgreSQL)

### 2.4 Planning & Investment
- **DebtService**: Quản lý khoản nợ
- **GoalService**: Quản lý mục tiêu tài chính
- **InvestmentService**: Quản lý đầu tư
- **API Integration**: Third-party investment tracking tools integration
- **Database**: db_planning (PostgreSQL)

### 2.5 Reporting & Integration
- **ReportingService**: Tạo báo cáo và phân tích
- **NotificationService**: Gửi thông báo
- **IntegrationService**: Kết nối với các dịch vụ bên ngoài
- **StatementParserService**: Phân tích và trích xuất dữ liệu từ file sao kê
- **API Integration**: External reporting tools và analytics platforms
- **Database**: db_reporting (PostgreSQL)

## 3. Architecture Principles

### 3.1 Core Principles
- **Bounded Context Isolation**: Mỗi service độc lập với database riêng
- **Event-Driven Communication**: Message bus cho inter-service communication
- **API Gateway Pattern**: Single entry point cho external requests
- **Dual Authentication Strategy**: JWT tokens + API keys cho flexible access
- **PostgreSQL Consistency**: Tất cả services sử dụng PostgreSQL

### 3.2 API Key Architecture Principles
- **Security-First Design**: SHA-256 hashing, scope-based permissions, rate limiting
- **Comprehensive Monitoring**: Usage tracking, audit logging, security event monitoring
- **Flexible Integration**: Support cho mobile apps, third-party services, automation tools
- **Scalable Design**: Per-key rate limiting, IP whitelisting, configurable security settings

## 4. Technology Stack

### 4.1 Backend Technologies
- **Framework**: .NET 9, ASP.NET Core
- **Database**: PostgreSQL với Entity Framework Core
- **Message Bus**: RabbitMQ with MassTransit
- **API Gateway**: Ocelot với enhanced authentication middleware
- **Authentication**: 
  - JWT Bearer tokens cho web applications
  - API key authentication cho third-party integrations
  - Social login providers (Google, Facebook, Apple)

### 4.2 Frontend Technologies
- **Framework**: Nuxt 3, Vue 3 Composition API
- **Styling**: TypeScript, Tailwind CSS, VRISTO Admin Template
- **State Management**: Pinia với API key management stores
- **API Integration**: Axios với dual authentication support

### 4.3 Infrastructure & Monitoring
- **Containerization**: Docker, Docker Compose
- **Monitoring**: Grafana, Loki, Prometheus
- **Caching**: Redis cho rate limiting và session management
- **Security**: 
  - HTTPS enforcement
  - CORS configuration
  - Input validation và sanitization
  - Security headers và CSP

## 5. Infrastructure Components

### 5.1 Core Infrastructure
- **API Gateway**: Ocelot (Port 5800) với API key authentication middleware
- **Message Queue**: RabbitMQ cho async communication
- **Database**: PostgreSQL instances với proper indexing cho API key lookups
- **Caching**: Redis cho rate limiting và API key validation caching
- **Monitoring**: Grafana + Loki + Prometheus stack với API key usage metrics

### 5.2 Security Infrastructure
- **Authentication Services**:
  - JWT token validation middleware
  - API key authentication middleware với IP whitelisting
  - Rate limiting per API key với configurable limits
- **Security Monitoring**:
  - API key usage tracking và analytics
  - Security event logging cho suspicious activities
  - Real-time alerting cho rate limit violations

## 6. API Integration Capabilities

### 6.1 Third-Party Integration Support
- **Mobile Applications**: Native iOS/Android apps với API key authentication
- **Financial Tools**: Integration với budgeting và expense tracking apps
- **Investment Platforms**: Portfolio sync với external investment services
- **Automation Scripts**: API access cho personal finance automation
- **Reporting Tools**: External analytics và business intelligence platforms

### 6.2 API Standards Compliance
- **RESTful Design**: Consistent API patterns across all services
- **OpenAPI Specification**: Complete API documentation với examples
- **Error Handling**: Standardized error responses với proper HTTP status codes
- **Versioning Strategy**: API versioning support cho backward compatibility
- **Rate Limiting**: Configurable limits với proper HTTP headers

## 7. Security & Compliance

### 7.1 Authentication Security
- **Multi-Factor Authentication**: Support cho 2FA và biometric authentication
- **Token Security**: Secure JWT handling với proper expiration
- **API Key Security**: 
  - Cryptographically secure key generation
  - SHA-256 hashing cho storage
  - One-time display pattern cho security
  - Automatic key rotation capabilities

### 7.2 Data Protection
- **Encryption**: Data encryption at rest và in transit
- **Privacy Controls**: User data isolation và access controls
- **Audit Logging**: Comprehensive audit trail cho all API access
- **Compliance**: GDPR-ready data handling và user consent management

## 8. Scalability & Performance

### 8.1 Performance Optimization
- **Database Optimization**: Proper indexing cho API key lookups
- **Caching Strategy**: Redis caching cho frequently accessed data
- **Connection Pooling**: Efficient database connection management
- **Async Processing**: Non-blocking API key validation

### 8.2 Scalability Features
- **Horizontal Scaling**: Independent service scaling
- **Load Balancing**: API Gateway load distribution
- **Rate Limiting**: Per-key và per-user rate limiting
- **Circuit Breaker**: Resilience patterns cho external dependencies

## 9. Development & Deployment

### 9.1 Development Environment
- **Local Development**: Docker Compose với all services
- **API Testing**: Comprehensive test suite với API key scenarios
- **Documentation**: Auto-generated API docs với authentication examples
- **Development Tools**: Hot reload, debugging, và profiling tools

### 9.2 Deployment Strategy
- **Containerization**: Docker containers cho all services
- **Orchestration**: Kubernetes deployment với health checks
- **CI/CD Pipeline**: Automated testing và deployment
- **Environment Management**: Development, staging, và production environments

---

*Updated: December 28, 2024 - Added Enhanced API Key Management capabilities*
*Status: Production-Ready Architecture với Comprehensive Third-Party Integration Support*
