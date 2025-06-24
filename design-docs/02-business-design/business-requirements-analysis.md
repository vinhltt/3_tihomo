# Business Requirements Analysis - TiHoMo

## 1. Executive Summary

Tài liệu này mô tả thiết kế phân tích nghiệp vụ cho hệ thống Quản lý Tài chính Cá nhân (TiHoMo). Hệ thống được xây dựng theo kiến trúc microservices, nhằm mục đích tự động hóa quy trình, tích hợp linh hoạt các dịch vụ, và cung cấp trải nghiệm quản lý tài chính toàn diện cho người dùng. Mục tiêu chính là giải quyết các vấn đề về kết nối hệ thống, đơn giản hóa quản lý tài chính và tăng hiệu suất.

## 2. Business Context & Background

**Lý do dự án tồn tại:** Nhu cầu tự động hóa các quy trình nghiệp vụ tài chính cá nhân, giảm thiểu lỗi thủ công và tăng hiệu suất quản lý tài chính.

**Vấn đề cần giải quyết:** Khó khăn trong việc tổng hợp dữ liệu từ nhiều nguồn (ngân hàng, ví điện tử), theo dõi chi tiêu, lập ngân sách, đặt mục tiêu tài chính và quản lý các khoản đầu tư/nợ một cách hiệu quả.

## 3. Scope & Objectives

### 3.1 Business Scope
- Xây dựng hệ thống TiHoMo dựa trên kiến trúc microservices
- Bao gồm các bounded context: Identity & Access, Core Finance, Money Management, Planning & Investment, Reporting & Integration
- Tích hợp các dịch vụ bên ngoài (ngân hàng, ví điện tử)
- Cung cấp giao diện người dùng để quản lý tài chính
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

## 4. Functional Requirements

### 4.1 Identity & Access Management
- **FR-IA-1:** User registration with email verification
- **FR-IA-2:** Social login (Google, Facebook, Apple)
- **FR-IA-3:** Password recovery functionality
- **FR-IA-4:** User management capabilities
- **FR-IA-5:** Role-based access control

### 4.2 Core Finance Management
- **FR-CF-1:** Account management (bank, wallet, cash, credit cards)
- **FR-CF-2:** Manual transaction recording (income, expense, transfer)
- **FR-CF-3:** Bank statement import (CSV, Excel formats)
- **FR-CF-4:** Automatic transaction categorization
- **FR-CF-5:** Transaction history viewing with filters

### 4.3 Money Management
- **FR-MM-1:** Budget creation and management by category and period
- **FR-MM-2:** Six Jars financial management model support
- **FR-MM-3:** Shared expense management with groups

### 4.4 Planning & Investment
- **FR-PI-1:** Debt tracking and management
- **FR-PI-2:** Financial goal creation and monitoring
- **FR-PI-3:** Investment portfolio tracking
- **FR-PI-4:** Recurring transaction templates
- **FR-PI-5:** Expected transaction generation
- **FR-PI-6:** Transaction linking capabilities
- **FR-PI-7:** Payment reminder notifications

### 4.5 Reporting & Integration
- **FR-RI-1:** Basic financial reports (asset overview, cash flow, expense categorization)
- **FR-RI-2:** Advanced analytics reports
- **FR-RI-3:** Notification system (due dates, budget alerts, goal achievements)
- **FR-RI-4:** External service integration

## 5. Non-Functional Requirements

### 5.1 Performance Requirements
- **NFR-1:** API response time < 2 seconds for 1000 concurrent users
- **NFR-2:** Statement import (1000 rows) completed within 30 seconds
- **NFR-3:** Real-time dashboard updates

### 5.2 Scalability Requirements
- **NFR-4:** Support for 10x user growth over 2 years
- **NFR-5:** Independent microservice scaling
- **NFR-6:** Database scaling capabilities

### 5.3 Availability Requirements
- **NFR-7:** 99.9% system uptime (excluding scheduled maintenance)
- **NFR-8:** High availability for API Gateway and core services
- **NFR-9:** Graceful degradation under load

### 5.4 Security Requirements
- **NFR-10:** OWASP Top 10 compliance
- **NFR-11:** Encryption of sensitive data (at rest and in transit)
- **NFR-12:** Strong authentication and authorization (JWT, RBAC)
- **NFR-13:** Protection against common attacks (XSS, CSRF, SQL Injection)
- **NFR-14:** Security monitoring and anomaly detection

### 5.5 Maintainability Requirements
- **NFR-15:** Clean Architecture implementation
- **NFR-16:** SOLID principles compliance
- **NFR-17:** Test coverage > 80%
- **NFR-18:** Comprehensive documentation

### 5.6 Usability Requirements
- **NFR-19:** Intuitive user interface
- **NFR-20:** Maximum 3 steps for primary tasks
- **NFR-21:** Mobile-responsive design
- **NFR-22:** Accessibility compliance

### 5.7 Reliability Requirements
- **NFR-23:** Data integrity and consistency
- **NFR-24:** Transaction support
- **NFR-25:** Event-driven synchronization
- **NFR-26:** Automated backup and recovery

## 6. Business Process Flows

### 6.1 User Onboarding Process
1. User registration with email verification
2. Social authentication setup
3. Initial financial account configuration
4. Basic preferences and goals setup

### 6.2 Daily Financial Management
1. Transaction recording (manual or automated)
2. Real-time balance updates
3. Category assignment and validation
4. Budget impact assessment

### 6.3 Monthly Financial Review
1. Budget vs actual spending analysis
2. Goal progress evaluation
3. Expense pattern insights
4. Financial health assessment

### 6.4 Long-term Planning
1. Goal setting and milestone creation
2. Investment portfolio review
3. Debt management planning
4. Financial forecast generation

## 7. User Personas & Use Cases

### 7.1 Primary Personas
- **Young Professional:** Tech-savvy, goal-oriented, needs automation
- **Family Manager:** Budget-conscious, multiple accounts, shared expenses
- **Investment Enthusiast:** Portfolio tracking, advanced analytics
- **Debt Manager:** Debt consolidation focus, payment planning

### 7.2 Key Use Cases
- Daily expense tracking
- Budget management
- Goal achievement tracking
- Investment monitoring
- Debt payment planning
- Financial report generation

## 8. Success Metrics & KPIs

### 8.1 User Engagement
- Daily/Monthly active users
- Feature adoption rates
- Session duration and frequency

### 8.2 Financial Impact
- Average time saved per user
- Goal achievement rate
- Budget adherence improvement

### 8.3 System Performance
- API response times
- System uptime
- Error rates
- User satisfaction scores

## 9. Risks & Mitigation Strategies

### 9.1 Business Risks
- **RISK-1:** User adoption challenges → Mitigation: User-centric design, onboarding optimization
- **RISK-2:** Competition from established players → Mitigation: Unique value proposition, rapid iteration
- **RISK-3:** Regulatory compliance → Mitigation: Legal consultation, privacy-by-design

### 9.2 Technical Risks
- **RISK-4:** Integration complexity → Mitigation: Phased implementation, manual fallbacks
- **RISK-5:** Performance issues → Mitigation: Load testing, scalability planning
- **RISK-6:** Security vulnerabilities → Mitigation: Security audits, best practices

## 10. Implementation Strategy

### 10.1 Phased Approach
- **Phase 1:** Core functionality (accounts, transactions, basic reporting)
- **Phase 2:** Advanced features (budgets, goals, analytics)
- **Phase 3:** Integrations and automation
- **Phase 4:** Advanced analytics and AI features

### 10.2 Success Criteria
- User adoption targets by phase
- Feature completion milestones
- Performance benchmarks
- Security compliance checkpoints

---
*This document serves as the foundation for the TiHoMo system development and should be reviewed and updated regularly based on stakeholder feedback and market requirements.*
