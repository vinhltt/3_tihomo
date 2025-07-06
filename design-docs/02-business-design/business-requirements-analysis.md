# Business Requirements Analysis - TiHoMo

## 1. Executive Summary

Tài liệu này mô tả thiết kế phân tích nghiệp vụ cho hệ thống Quản lý Tài chính Cá nhân (TiHoMo). Hệ thống được xây dựng theo kiến trúc microservices, nhằm mục đích tự động hóa quy trình, tích hợp linh hoạt các dịch vụ, và cung cấp trải nghiệm quản lý tài chính toàn diện cho người dùng. Với Enhanced API Key Management, hệ thống mở rộng khả năng tích hợp với third-party applications, mobile apps, và automation tools. Mục tiêu chính là giải quyết các vấn đề về kết nối hệ thống, đơn giản hóa quản lý tài chính và tăng hiệu suất.

## 2. Business Context & Background

**Lý do dự án tồn tại:** Nhu cầu tự động hóa các quy trình nghiệp vụ tài chính cá nhân, giảm thiểu lỗi thủ công và tăng hiệu suất quản lý tài chính. Đặc biệt, nhu cầu tích hợp với các ứng dụng di động, công cụ tài chính bên ngoài, và automation scripts đòi hỏi một hệ thống API key management mạnh mẽ và bảo mật.

**Vấn đề cần giải quyết:** 
- Khó khăn trong việc tổng hợp dữ liệu từ nhiều nguồn (ngân hàng, ví điện tử)
- Theo dõi chi tiêu, lập ngân sách, đặt mục tiêu tài chính một cách hiệu quả
- **Thiếu khả năng tích hợp với third-party applications**: Users cần access data từ mobile apps, financial tools, automation scripts
- **Không có API access control**: Cần system để manage API keys với proper security và monitoring
- **Thiếu flexibility cho developers**: Cần robust API ecosystem cho third-party integrations

## 3. Scope & Objectives

### 3.1 Business Scope
- Xây dựng hệ thống TiHoMo dựa trên kiến trúc microservices
- Bao gồm các bounded context: Identity & Access, Core Finance, Money Management, Planning & Investment, Reporting & Integration
- **Enhanced API Key Management**: Complete API key lifecycle management cho third-party integrations
- Tích hợp các dịch vụ bên ngoài (ngân hàng, ví điện tử)
- Cung cấp giao diện người dùng để quản lý tài chính
- **API ecosystem**: Enable third-party developers và mobile app integrations
- Hỗ trợ import sao kê ngân hàng
- Cung cấp báo cáo tài chính cơ bản và nâng cao
- Đảm bảo bảo mật, khả năng mở rộng và bảo trì

### 3.2 Business Objectives
- **OBJ-1:** Cung cấp một nền tảng tập trung để quản lý tất cả các tài khoản tài chính
- **OBJ-2:** Tự động hóa việc phân loại giao dịch và tạo báo cáo chi tiêu
- **OBJ-3:** Cho phép người dùng đặt và theo dõi ngân sách (budget) và mục tiêu tài chính (goal)
- **OBJ-4:** Hỗ trợ quản lý nợ và các khoản đầu tư
- **OBJ-5:** Đảm bảo tính bảo mật và riêng tư cho dữ liệu tài chính người dùng
- **OBJ-6:** Cung cấp giao diện người dùng trực quan, dễ sử dụng
- **OBJ-7:** Giảm thời gian quản lý tài chính thủ công > 50%
- **OBJ-8:** Tăng tỷ lệ người dùng đạt được mục tiêu tài chính > 20%
- **OBJ-9:** **Enable third-party ecosystem**: Cung cấp secure API access cho mobile apps và external tools
- **OBJ-10:** **Improve user engagement**: Tăng user retention thông qua mobile app integrations và automation capabilities
- **OBJ-11:** **Expand platform reach**: Attract developers và partners thông qua comprehensive API offerings

## 4. Functional Requirements

### 4.1 Identity & Access Management (Enhanced)
- **FR-IA-1:** User registration with email verification
- **FR-IA-2:** Social login (Google, Facebook, Apple)
- **FR-IA-3:** Password recovery functionality
- **FR-IA-4:** User management capabilities
- **FR-IA-5:** Role-based access control
- **FR-IA-6:** **API Key Creation**: Users can create API keys với descriptive names và scopes
- **FR-IA-7:** **API Key Management**: Users can view, update, regenerate, và revoke API keys
- **FR-IA-8:** **Scope-based Permissions**: API keys có granular permissions (read, write, transactions:read, etc.)
- **FR-IA-9:** **Rate Limiting Configuration**: Per-key rate limiting với configurable limits
- **FR-IA-10:** **IP Whitelisting**: API keys có thể restrict access by IP addresses/CIDR ranges
- **FR-IA-11:** **Usage Analytics**: Comprehensive usage tracking và statistics cho API keys
- **FR-IA-12:** **Security Monitoring**: Real-time monitoring và alerting cho suspicious API key activities

### 4.2 Core Finance Management (API-Enabled)
- **FR-CF-1:** Account management (bank, wallet, cash, credit cards)
- **FR-CF-2:** Manual transaction recording (income, expense, transfer)
- **FR-CF-3:** Bank statement import (CSV, Excel formats)
- **FR-CF-4:** Automatic transaction categorization
- **FR-CF-5:** Transaction history viewing with filters
- **FR-CF-6:** **API Key Authentication**: All endpoints support API key authentication
- **FR-CF-7:** **Third-party Integration**: Mobile apps có thể access account data via API keys
- **FR-CF-8:** **Bulk Operations**: API endpoints cho batch transaction processing
- **FR-CF-9:** **Real-time Notifications**: API-triggered webhooks cho transaction events

### 4.3 Money Management (API-Accessible)
- **FR-MM-1:** Budget creation and management by category and period
- **FR-MM-2:** Six Jars financial management model support
- **FR-MM-3:** Shared expense management with groups
- **FR-MM-4:** **Budget API Access**: Third-party budgeting tools có thể integrate via API keys
- **FR-MM-5:** **Expense Analysis APIs**: External analytics tools có thể access spending data
- **FR-MM-6:** **Category Management APIs**: Programmatic category creation và management

### 4.4 Planning & Investment (Integration-Ready)
- **FR-PI-1:** Debt tracking and management
- **FR-PI-2:** Financial goal creation and monitoring
- **FR-PI-3:** Investment portfolio tracking
- **FR-PI-4:** Recurring transaction templates
- **FR-PI-5:** Expected transaction generation
- **FR-PI-6:** Transaction linking capabilities
- **FR-PI-7:** Payment reminder notifications
- **FR-PI-8:** **Investment API Integration**: Portfolio sync với external investment platforms
- **FR-PI-9:** **Goal Tracking APIs**: Third-party goal tracking apps có thể monitor progress
- **FR-PI-10:** **Debt Management APIs**: External debt consolidation tools integration

### 4.5 Reporting & Integration (Enhanced API Support)
- **FR-RI-1:** Basic financial reports (asset overview, cash flow, expense categorization)
- **FR-RI-2:** Advanced analytics reports
- **FR-RI-3:** Notification system (due dates, budget alerts, goal achievements)
- **FR-RI-4:** External service integration
- **FR-RI-5:** **Report API Endpoints**: Programmatic access to all report types
- **FR-RI-6:** **Data Export APIs**: Bulk data export cho external analytics tools
- **FR-RI-7:** **Custom Report Generation**: API-driven custom report creation
- **FR-RI-8:** **Webhook Management**: Configurable webhooks cho real-time notifications

### 4.6 API Key Management (New Functional Area)
- **FR-AK-1:** **API Key Lifecycle Management**: Create, read, update, regenerate, revoke API keys
- **FR-AK-2:** **Security Configuration**: Configure HTTPS requirements, CORS settings, IP restrictions
- **FR-AK-3:** **Usage Monitoring**: Real-time usage tracking với detailed analytics
- **FR-AK-4:** **Rate Limit Management**: Configure và monitor per-key rate limits
- **FR-AK-5:** **Audit Logging**: Comprehensive audit trail cho all API key activities
- **FR-AK-6:** **Scope Management**: Granular permission control với predefined scopes
- **FR-AK-7:** **Key Rotation**: Automated và manual key rotation capabilities
- **FR-AK-8:** **Developer Tools**: API documentation, code examples, testing tools

## 5. Non-Functional Requirements

### 5.1 Performance Requirements
- **NFR-1:** API response time < 2 seconds for 1000 concurrent users
- **NFR-2:** Statement import (1000 rows) completed within 30 seconds
- **NFR-3:** Real-time dashboard updates
- **NFR-4:** **API Key Validation**: API key authentication < 100ms response time
- **NFR-5:** **Rate Limiting**: Accurate rate limiting với minimal latency overhead

### 5.2 Scalability Requirements
- **NFR-6:** Support for 10x user growth over 2 years
- **NFR-7:** Independent microservice scaling
- **NFR-8:** Database scaling capabilities
- **NFR-9:** **API Key Scaling**: Support for 100,000+ active API keys per user
- **NFR-10:** **Third-party Integration Scaling**: Handle 1M+ API requests per day

### 5.3 Availability Requirements
- **NFR-11:** 99.9% system uptime (excluding scheduled maintenance)
- **NFR-12:** High availability for API Gateway and core services
- **NFR-13:** Graceful degradation under load
- **NFR-14:** **API Key Service Availability**: 99.95% uptime cho API key validation

### 5.4 Security Requirements
- **NFR-15:** OWASP Top 10 compliance
- **NFR-16:** Encryption of sensitive data (at rest and in transit)
- **NFR-17:** Strong authentication and authorization (JWT, RBAC)
- **NFR-18:** Protection against common attacks (XSS, CSRF, SQL Injection)
- **NFR-19:** Security monitoring and anomaly detection
- **NFR-20:** **API Key Security**: SHA-256 hashing, secure key generation, one-time display
- **NFR-21:** **Rate Limiting Security**: DDoS protection via per-key rate limiting
- **NFR-22:** **IP Security**: IP whitelisting với CIDR notation support
- **NFR-23:** **Audit Security**: Immutable audit logs cho API key activities

### 5.5 Maintainability Requirements
- **NFR-24:** Clean Architecture implementation
- **NFR-25:** SOLID principles compliance
- **NFR-26:** Test coverage > 80%
- **NFR-27:** Comprehensive documentation
- **NFR-28:** **API Documentation**: Auto-generated OpenAPI specs với examples
- **NFR-29:** **Developer Experience**: Clear error messages, consistent API patterns

### 5.6 Usability Requirements
- **NFR-30:** Intuitive user interface
- **NFR-31:** Maximum 3 steps for primary tasks
- **NFR-32:** Mobile-responsive design
- **NFR-33:** Accessibility compliance
- **NFR-34:** **API Key Management UX**: Simple API key creation và management workflow
- **NFR-35:** **Developer Portal**: Self-service API key management với clear documentation

### 5.7 Reliability Requirements
- **NFR-36:** Data integrity and consistency
- **NFR-37:** Transaction support
- **NFR-38:** Event-driven synchronization
- **NFR-39:** Automated backup and recovery
- **NFR-40:** **API Key Reliability**: Consistent API key validation across all services
- **NFR-41:** **Failover Support**: Graceful handling of API key service failures

## 6. Business Process Flows

### 6.1 User Onboarding Process
1. User registration with email verification
2. Social authentication setup
3. Initial financial account configuration
4. Basic preferences and goals setup
5. **Optional API key setup** cho mobile app access

### 6.2 Daily Financial Management
1. Transaction recording (manual or automated)
2. Real-time balance updates
3. Category assignment and validation
4. Budget impact assessment
5. **Third-party app sync** via API keys

### 6.3 Monthly Financial Review
1. Budget vs actual spending analysis
2. Goal progress evaluation
3. Expense pattern insights
4. Financial health assessment
5. **External reporting tool integration** via APIs

### 6.4 Long-term Planning
1. Goal setting and milestone creation
2. Investment portfolio review
3. Debt management planning
4. Financial forecast generation
5. **Investment platform synchronization** via API integrations

### 6.5 API Key Management Process (New)
1. **API Key Creation**: User creates API key với specific scopes
2. **Security Configuration**: Set up IP whitelisting, rate limits, CORS settings
3. **Integration Setup**: Use API key trong third-party applications
4. **Usage Monitoring**: Track API key usage và performance
5. **Security Management**: Monitor cho suspicious activities, rotate keys as needed
6. **Lifecycle Management**: Update, regenerate, or revoke keys as required

## 7. User Personas & Use Cases

### 7.1 Primary Personas
- **Young Professional:** Tech-savvy, goal-oriented, needs automation và mobile access
- **Family Manager:** Budget-conscious, multiple accounts, shared expenses, needs third-party integrations
- **Investment Enthusiast:** Portfolio tracking, advanced analytics, external tool integrations
- **Debt Manager:** Debt consolidation focus, payment planning, automation tools
- **Developer/Power User:** API access, automation scripts, custom integrations

### 7.2 Key Use Cases
- Daily expense tracking via mobile apps
- Budget management với third-party tools
- Goal achievement tracking với external platforms
- Investment monitoring và portfolio sync
- Debt payment planning với automation
- Financial report generation cho external analytics
- **API-driven automation** cho recurring tasks
- **Mobile app integration** cho real-time financial data
- **Third-party tool connectivity** cho comprehensive financial management

### 7.3 API Key Use Cases (New)
- **Mobile App Integration**: Native iOS/Android apps accessing user financial data
- **Budgeting Tool Integration**: Third-party budgeting apps syncing với TiHoMo data
- **Investment Platform Sync**: Portfolio tracking tools accessing investment data
- **Automation Scripts**: Personal finance automation với API access
- **Business Intelligence**: External analytics tools accessing financial reports
- **Webhook Integration**: Real-time notifications to external systems

## 8. Success Metrics & KPIs

### 8.1 User Engagement
- Daily/Monthly active users
- Feature adoption rates
- Session duration and frequency
- **API usage metrics**: Active API keys, requests per day
- **Third-party integration adoption**: Percentage of users using API features

### 8.2 Financial Impact
- Average time saved per user
- Goal achievement rate
- Budget adherence improvement
- **Automation efficiency**: Time saved through API-driven automation
- **Integration value**: Financial insights gained through third-party tools

### 8.3 System Performance
- API response times
- System uptime
- Error rates
- User satisfaction scores
- **API Key Performance**: API key validation latency, rate limiting accuracy
- **Security Metrics**: API key security incidents, audit compliance

### 8.4 Developer Experience (New)
- **API adoption rate**: Number of active third-party integrations
- **Developer satisfaction**: API documentation quality, ease of integration
- **API reliability**: Uptime, error rates, consistency
- **Security compliance**: Audit trail completeness, security incident response

## 9. Risks & Mitigation Strategies

### 9.1 Business Risks
- **RISK-1:** User adoption challenges → Mitigation: User-centric design, onboarding optimization
- **RISK-2:** Competition from established players → Mitigation: Unique value proposition, rapid iteration
- **RISK-3:** Regulatory compliance → Mitigation: Legal consultation, privacy-by-design
- **RISK-4:** **API security vulnerabilities** → Mitigation: Comprehensive security testing, regular audits
- **RISK-5:** **Third-party integration complexity** → Mitigation: Clear API documentation, developer support

### 9.2 Technical Risks
- **RISK-6:** Integration complexity → Mitigation: Phased implementation, manual fallbacks
- **RISK-7:** Performance issues → Mitigation: Load testing, scalability planning
- **RISK-8:** Security vulnerabilities → Mitigation: Security audits, best practices
- **RISK-9:** **API key management complexity** → Mitigation: Simple UX design, automated processes
- **RISK-10:** **Rate limiting accuracy** → Mitigation: Proper testing, monitoring, graceful degradation

### 9.3 Security Risks (Enhanced)
- **RISK-11:** **API key compromise** → Mitigation: Secure generation, hashing, rotation capabilities
- **RISK-12:** **Unauthorized API access** → Mitigation: IP whitelisting, scope-based permissions
- **RISK-13:** **DDoS via API keys** → Mitigation: Rate limiting, circuit breakers, monitoring
- **RISK-14:** **Data exposure via APIs** → Mitigation: Proper authorization, audit logging

## 10. Implementation Strategy

### 10.1 Phased Approach
- **Phase 1:** Core functionality (accounts, transactions, basic reporting)
- **Phase 2:** Advanced features (budgets, goals, analytics)
- **Phase 3:** **API Key Management implementation** (authentication, basic CRUD)
- **Phase 4:** **Enhanced API features** (usage analytics, advanced security)
- **Phase 5:** **Third-party ecosystem** (developer portal, integrations, automation)

### 10.2 Success Criteria
- User adoption targets by phase
- Feature completion milestones
- Performance benchmarks
- Security compliance checkpoints
- **API adoption milestones**: Number of active API keys, third-party integrations
- **Developer satisfaction metrics**: API documentation quality, integration success rate

---
*This document serves as the foundation for the TiHoMo system development and should be reviewed and updated regularly based on stakeholder feedback and market requirements.*

*Updated: December 28, 2024 - Enhanced with API Key Management business requirements*
