# System Architecture Flowcharts v4

## Overview
This document contains the detailed flowcharts for TiHoMo (Tiny House Money) personal finance management system architecture version 4. **Enhanced with API Key Management** for comprehensive third-party integration support.

## 1. Enhanced High-Level System Architecture

```mermaid
graph TB
    subgraph Client Layer
        WEB[Web Frontend - Nuxt.js]
        MOBILE[Mobile Apps - Native]
        THIRD[Third-party Applications]
        AUTO[Automation Scripts]
        DEV[Developer Tools]
    end
    
    subgraph API Gateway Layer
        GW[Enhanced API Gateway - Ocelot]
        AUTH_MW[API Key Auth Middleware]
        RATE_LIM[Rate Limiting Service]
        IP_WHITE[IP Whitelisting Service]
    end
    
    subgraph Microservices Layer
        ID[Identity Service + API Key Mgmt]
        CF[CoreFinance Service]
        MM[MoneyManagement Service]
        PI[PlanningInvestment Service]
        RI[ReportingIntegration Service]
        EA[ExcelApi Service]
        WEBHOOK[Webhook Service]
    end
    
    subgraph Data Layer
        IDB[(Identity + API Keys DB)]
        CFDB[(CoreFinance DB)]
        MMDB[(MoneyManagement DB)]
        PIDB[(PlanningInvestment DB)]
        RIDB[(Reporting DB)]
    end
    
    subgraph Infrastructure Layer
        MQ[RabbitMQ Message Queue]
        REDIS[Redis Cache + Rate Limiting]
        STORAGE[MinIO File Storage]
    end
    
    subgraph Monitoring Layer
        LOG[Loki Logging + API Metrics]
        PROM[Prometheus Metrics]
        GRAF[Grafana Dashboard]
    end
    
    %% Client connections
    WEB --> GW
    MOBILE --> GW
    THIRD --> GW
    AUTO --> GW
    DEV --> GW
    
    %% API Gateway flow
    GW --> AUTH_MW
    AUTH_MW --> RATE_LIM
    RATE_LIM --> IP_WHITE
    IP_WHITE --> ID
    IP_WHITE --> CF
    IP_WHITE --> MM
    IP_WHITE --> PI
    IP_WHITE --> RI
    IP_WHITE --> EA
    
    %% API Key Management
    AUTH_MW --> ID
    RATE_LIM --> REDIS
    
    %% Database connections
    ID --> IDB
    CF --> CFDB
    MM --> MMDB
    PI --> PIDB
    RI --> RIDB
    
    %% Infrastructure connections
    CF --> MQ
    MM --> MQ
    PI --> MQ
    RI --> MQ
    EA --> MQ
    WEBHOOK --> MQ
    
    ID --> REDIS
    CF --> REDIS
    EA --> STORAGE
    
    %% Monitoring connections
    GW --> LOG
    ID --> LOG
    CF --> LOG
    MM --> LOG
    PI --> LOG
    RI --> LOG
    
    GW --> PROM
    ID --> PROM
    CF --> PROM
    MM --> PROM
    PI --> PROM
    RI --> PROM
    
    LOG --> GRAF
    PROM --> GRAF
```

## 2. Enhanced Authentication & Authorization Flow

```mermaid
sequenceDiagram
    participant Client as Client App
    participant Gateway as API Gateway
    participant AuthMW as Auth Middleware
    participant ApiKeySvc as API Key Service
    participant Redis as Redis Cache
    participant TargetSvc as Target Service
    participant AuditSvc as Audit Service
    
    Note over Client,AuditSvc: API Key Authentication Flow
    
    Client->>Gateway: API Request + X-API-Key Header
    Gateway->>AuthMW: Validate Request
    
    %% Rate Limiting Check
    AuthMW->>Redis: Check Rate Limit (Key Hash)
    Redis-->>AuthMW: Rate Limit Status
    
    alt Rate Limit Exceeded
        AuthMW-->>Gateway: 429 Too Many Requests
        Gateway-->>Client: 429 + Rate Limit Headers
    else Rate Limit OK
        %% API Key Validation
        AuthMW->>ApiKeySvc: Validate API Key
        ApiKeySvc->>ApiKeySvc: Hash Incoming Key
        ApiKeySvc->>ApiKeySvc: Lookup in Database
        ApiKeySvc->>ApiKeySvc: Check Expiration
        ApiKeySvc->>ApiKeySvc: Verify IP Whitelist
        ApiKeySvc->>ApiKeySvc: Validate Scopes
        
        alt API Key Invalid
            ApiKeySvc-->>AuthMW: Validation Failed
            AuthMW->>AuditSvc: Log Security Event (Async)
            AuthMW-->>Gateway: 401 Unauthorized
            Gateway-->>Client: 401 Unauthorized
        else API Key Valid
            ApiKeySvc-->>AuthMW: Validation Success + User Context
            AuthMW->>TargetSvc: Forward Request + User Context
            TargetSvc->>TargetSvc: Process Business Logic
            TargetSvc-->>AuthMW: Service Response
            
            %% Usage Logging
            AuthMW->>ApiKeySvc: Log Usage (Async)
            AuthMW->>Redis: Update Rate Limit Counter
            
            AuthMW-->>Gateway: Response + API Headers
            Gateway-->>Client: Success Response
        end
    end
```

## 3. API Key Management Lifecycle Flow

```mermaid
flowchart TD
    A[User Request API Key] --> B[API Key Creation Form]
    B --> C{Validation OK?}
    C -->|No| D[Show Validation Errors]
    D --> B
    C -->|Yes| E[Generate Secure Key]
    
    E --> F[Hash Key SHA-256]
    F --> G[Store Hashed Key + Metadata]
    G --> H[Cache Key Info in Redis]
    H --> I[Return Full Key - One Time Only]
    
    I --> J[User Configures Security Settings]
    J --> K[Set Scopes & Permissions]
    K --> L[Configure Rate Limits]
    L --> M[Set IP Whitelist]
    M --> N[Enable/Disable HTTPS Requirement]
    
    N --> O[API Key Active & Ready]
    O --> P[Monitor Usage & Security]
    
    P --> Q{Security Issue?}
    Q -->|Yes| R[Alert & Potential Auto-Disable]
    Q -->|No| S[Continue Monitoring]
    
    R --> T[User Reviews Security Alert]
    S --> U{User Action?}
    
    U -->|Regenerate| V[Generate New Key]
    U -->|Update Settings| W[Modify Configuration]
    U -->|Revoke| X[Disable Key]
    U -->|View Analytics| Y[Display Usage Stats]
    
    V --> F
    W --> J
    X --> Z[Key Revoked - Audit Log]
    Y --> S
    
    T --> AA{Resolve Issue?}
    AA -->|Yes| BB[Re-enable Key]
    AA -->|No| X
    BB --> S
```

## 4. Enhanced Transaction Processing with API Key Support

```mermaid
flowchart TD
    A[Transaction Input] --> B{Input Source?}
    B -->|Web UI| C[Frontend Form + JWT]
    B -->|Mobile App| D[Native App + API Key]
    B -->|Third-party| E[External App + API Key]
    B -->|Excel Import| F[ExcelApi Service + API Key]
    B -->|Recurring| G[Scheduled Job + Internal Auth]
    
    C --> H[Validate User Session]
    D --> I[Validate API Key + Mobile Scope]
    E --> J[Validate API Key + Transaction Scope]
    F --> K[Validate API Key + Import Scope]
    G --> L[Internal Service Auth]
    
    H --> M[Check User Permissions]
    I --> N[Check API Key Scopes]
    J --> N
    K --> N
    L --> M
    
    M --> O{Authorization OK?}
    N --> O
    O -->|No| P[Return 403 Forbidden]
    O -->|Yes| Q[Validate Transaction Data]
    
    Q --> R{Validation OK?}
    R -->|No| S[Return Validation Errors]
    R -->|Yes| T[Determine Account Type]
    
    T --> U{Account Type?}
    U -->|Regular| V[Store in CoreFinance DB]
    U -->|Investment| W[Store in PlanningInvestment DB]
    
    V --> X[Update Account Balance]
    W --> Y[Update Investment Portfolio]
    
    X --> Z[Publish Transaction Event]
    Y --> Z
    
    Z --> AA[RabbitMQ Message Queue]
    AA --> BB[Update MoneyManagement Summary]
    AA --> CC[Trigger Notifications]
    AA --> DD[Log Transaction Activity]
    AA --> EE[Update API Usage Metrics]
    AA --> FF[Webhook Notifications]
    
    %% API Key specific logging
    I --> GG[Log API Key Usage]
    J --> GG
    K --> GG
    GG --> HH[Update Rate Limit Counters]
    GG --> II[Security Monitoring]
```

## 5. API Key Security & Monitoring Flow

```mermaid
flowchart LR
    subgraph API Key Security
        A[API Request] --> B[Extract API Key]
        B --> C[Hash Key SHA-256]
        C --> D[Database Lookup]
        D --> E{Key Exists?}
        E -->|No| F[Log Security Event]
        E -->|Yes| G[Check Key Status]
        G --> H{Active?}
        H -->|No| F
        H -->|Yes| I[Validate IP Address]
        I --> J{IP Allowed?}
        J -->|No| F
        J -->|Yes| K[Check Rate Limits]
        K --> L{Within Limits?}
        L -->|No| M[Return 429 Rate Limited]
        L -->|Yes| N[Validate Scopes]
        N --> O{Scope Match?}
        O -->|No| P[Return 403 Forbidden]
        O -->|Yes| Q[Allow Request]
    end
    
    subgraph Monitoring & Analytics
        F --> R[Security Alert System]
        M --> S[Rate Limit Monitoring]
        P --> T[Authorization Monitoring]
        Q --> U[Usage Analytics]
        
        R --> V[Real-time Alerts]
        S --> W[Rate Limit Dashboard]
        T --> X[Security Dashboard]
        U --> Y[Usage Dashboard]
        
        V --> Z[Admin Notifications]
        W --> AA[Performance Metrics]
        X --> BB[Security Metrics]
        Y --> CC[Business Metrics]
    end
    
    subgraph Audit & Compliance
        F --> DD[Audit Log]
        M --> DD
        P --> DD
        Q --> DD
        
        DD --> EE[Immutable Log Storage]
        EE --> FF[Compliance Reporting]
        EE --> GG[Security Analysis]
        EE --> HH[Forensic Investigation]
    end
```

## 6. Enhanced Excel Import Processing with API Key Authentication

```mermaid
sequenceDiagram
    participant Client as API Client/Web UI
    participant Gateway as API Gateway
    participant AuthMW as Auth Middleware
    participant EA as ExcelApi Service
    participant Storage as MinIO Storage
    participant MQ as RabbitMQ
    participant CF as CoreFinance Service
    participant MM as MoneyManagement Service
    participant Webhook as Webhook Service
    participant ApiKey as API Key Service
    
    Note over Client,ApiKey: Enhanced Excel Import with API Key Support
    
    Client->>Gateway: POST /excel/upload + File + API Key
    Gateway->>AuthMW: Validate API Key
    AuthMW->>ApiKey: Check Key + Import Scope
    ApiKey-->>AuthMW: Validation Success
    AuthMW->>EA: Forward Upload Request
    
    EA->>EA: Validate File Format
    EA->>Storage: Store File Temporarily
    EA->>EA: Parse Excel Structure
    EA->>EA: Validate Data Schema
    
    alt Validation Success
        EA->>MQ: Publish Import Job Started
        EA-->>Gateway: Return Job ID + Status
        Gateway-->>Client: 202 Accepted + Job ID
        
        loop For Each Valid Transaction
            EA->>MQ: Publish Transaction Message
            MQ->>CF: Process Transaction
            CF->>CF: Store Transaction
            CF->>MQ: Publish Transaction Created
            MQ->>MM: Update Budget Impact
        end
        
        EA->>MQ: Publish Import Job Completed
        MQ->>Webhook: Trigger Completion Webhook
        
        alt Client Has Webhook URL
            Webhook->>Client: POST Completion Notification
        end
        
        %% API Key Usage Logging
        AuthMW->>ApiKey: Log Import Usage
        CF->>ApiKey: Log Transaction API Usage
        
    else Validation Failed
        EA-->>Gateway: 400 Bad Request + Errors
        Gateway-->>Client: 400 Bad Request
        AuthMW->>ApiKey: Log Failed Request
    end
```

## 7. Comprehensive Reporting & Analytics with API Access

```mermaid
flowchart TB
    subgraph Data Sources
        A1[CoreFinance DB]
        A2[MoneyManagement DB]
        A3[PlanningInvestment DB]
        A4[API Usage Logs]
        A5[Security Audit Logs]
    end
    
    subgraph ReportingIntegration Service
        B[Data Aggregation Engine]
        B --> C[Financial Report Generator]
        B --> D[API Usage Analytics]
        B --> E[Security Report Generator]
        B --> F[Custom Report Builder]
    end
    
    subgraph Report Types
        C --> G[Monthly Financial Summary]
        C --> H[Investment Performance]
        C --> I[Budget vs Actual Analysis]
        C --> J[Cash Flow Projections]
        
        D --> K[API Usage Statistics]
        D --> L[Rate Limiting Analysis]
        D --> M[Third-party Integration Metrics]
        
        E --> N[Security Incident Reports]
        E --> O[API Key Audit Reports]
        E --> P[Compliance Reports]
        
        F --> Q[User-defined Reports]
    end
    
    subgraph API Access Layer
        G --> R[Financial Reports API]
        H --> R
        I --> R
        J --> R
        
        K --> S[Analytics API]
        L --> S
        M --> S
        
        N --> T[Security API]
        O --> T
        P --> T
        
        Q --> U[Custom Reports API]
    end
    
    subgraph Client Access
        R --> V[Web Dashboard]
        R --> W[Mobile Apps]
        R --> X[Third-party Tools]
        
        S --> Y[Admin Dashboard]
        S --> Z[Monitoring Tools]
        
        T --> AA[Security Dashboard]
        T --> BB[Compliance Tools]
        
        U --> CC[Business Intelligence]
        U --> DD[Custom Integrations]
    end
    
    %% Data flow
    A1 --> B
    A2 --> B
    A3 --> B
    A4 --> B
    A5 --> B
    
    %% Caching layer
    B --> EE[Redis Cache]
    EE --> R
    EE --> S
    EE --> T
    EE --> U
```

## 8. Enhanced Message Queue Event Flow with API Key Events

```mermaid
graph TD
    subgraph Event Publishers
        CF[CoreFinance Service]
        MM[MoneyManagement Service]
        PI[PlanningInvestment Service]
        RI[ReportingIntegration Service]
        EA[ExcelApi Service]
        ID[Identity Service - API Keys]
        WEBHOOK[Webhook Service]
    end
    
    subgraph RabbitMQ Exchange System
        EX1[Account Exchange]
        EX2[Transaction Exchange]
        EX3[Investment Exchange]
        EX4[Notification Exchange]
        EX5[API Key Exchange - NEW]
        EX6[Security Exchange - NEW]
        EX7[Webhook Exchange - NEW]
    end
    
    subgraph Message Queues
        Q1[Account.Created Queue]
        Q2[Account.Updated Queue]
        Q3[Transaction.Created Queue]
        Q4[Transaction.Updated Queue]
        Q5[Investment.Updated Queue]
        Q6[Notification.Email Queue]
        Q7[Notification.SMS Queue]
        Q8[ApiKey.Created Queue - NEW]
        Q9[ApiKey.Used Queue - NEW]
        Q10[Security.Alert Queue - NEW]
        Q11[Webhook.Trigger Queue - NEW]
    end
    
    subgraph Event Consumers
        MM2[MoneyManagement Consumer]
        RI2[Reporting Consumer]
        NS[Notification Service]
        LS[Logging Service]
        AS[Analytics Service - NEW]
        SS[Security Service - NEW]
        WS[Webhook Service - NEW]
    end
    
    %% Publishers to Exchanges
    CF --> EX1
    CF --> EX2
    PI --> EX3
    EA --> EX2
    ID --> EX5
    ID --> EX6
    WEBHOOK --> EX7
    
    %% Exchanges to Queues
    EX1 --> Q1
    EX1 --> Q2
    EX2 --> Q3
    EX2 --> Q4
    EX3 --> Q5
    EX4 --> Q6
    EX4 --> Q7
    EX5 --> Q8
    EX5 --> Q9
    EX6 --> Q10
    EX7 --> Q11
    
    %% Queues to Consumers
    Q1 --> MM2
    Q2 --> MM2
    Q3 --> MM2
    Q3 --> RI2
    Q4 --> RI2
    Q5 --> RI2
    Q6 --> NS
    Q7 --> NS
    Q8 --> AS
    Q9 --> AS
    Q9 --> LS
    Q10 --> SS
    Q10 --> NS
    Q11 --> WS
    
    %% New API Key specific flows
    Q8 --> LS
    Q9 --> SS
    Q10 --> LS
    Q11 --> LS
```

## 9. Enhanced Monitoring & Observability with API Key Metrics

```mermaid
flowchart TB
    subgraph Service Metrics
        S1[CoreFinance + API Metrics]
        S2[MoneyManagement + API Metrics]
        S3[PlanningInvestment + API Metrics]
        S4[ReportingIntegration + API Metrics]
        S5[ExcelApi + API Metrics]
        S6[Identity + API Key Metrics]
        S7[Enhanced API Gateway]
    end
    
    subgraph API Key Specific Metrics
        AK1[API Key Usage Counters]
        AK2[Rate Limiting Metrics]
        AK3[Security Event Counters]
        AK4[Authentication Success/Failure Rates]
        AK5[Scope Validation Metrics]
        AK6[IP Whitelisting Metrics]
    end
    
    subgraph Logging Infrastructure
        S1 --> L[Enhanced Loki]
        S2 --> L
        S3 --> L
        S4 --> L
        S5 --> L
        S6 --> L
        S7 --> L
        
        AK1 --> L
        AK2 --> L
        AK3 --> L
        AK4 --> L
        AK5 --> L
        AK6 --> L
    end
    
    subgraph Metrics Collection
        S1 --> P[Enhanced Prometheus]
        S2 --> P
        S3 --> P
        S4 --> P
        S5 --> P
        S6 --> P
        S7 --> P
        
        AK1 --> P
        AK2 --> P
        AK3 --> P
        AK4 --> P
        AK5 --> P
        AK6 --> P
    end
    
    subgraph Visualization & Alerting
        L --> G[Enhanced Grafana]
        P --> G
        
        G --> A1[Performance Alerts]
        G --> A2[Error Rate Alerts]
        G --> A3[Business Metric Alerts]
        G --> A4[API Key Security Alerts - NEW]
        G --> A5[Rate Limiting Alerts - NEW]
        G --> A6[Third-party Integration Alerts - NEW]
    end
    
    subgraph Dashboards
        G --> D1[System Performance Dashboard]
        G --> D2[Business Metrics Dashboard]
        G --> D3[API Usage Dashboard - NEW]
        G --> D4[Security Monitoring Dashboard - NEW]
        G --> D5[Third-party Integration Dashboard - NEW]
        G --> D6[Developer Analytics Dashboard - NEW]
    end
```

## 10. API Key Integration Security Architecture

```mermaid
flowchart LR
    subgraph Security Layers
        A[Request Ingress] --> B[HTTPS Termination]
        B --> C[Rate Limiting]
        C --> D[API Key Validation]
        D --> E[IP Whitelisting]
        E --> F[Scope Authorization]
        F --> G[Request Processing]
    end
    
    subgraph Security Services
        H[Key Generation Service]
        I[Hash Validation Service]
        J[Rate Limit Service]
        K[IP Validation Service]
        L[Scope Validation Service]
        M[Audit Logging Service]
        N[Security Monitoring Service]
    end
    
    subgraph Security Storage
        O[Hashed Keys Database]
        P[Security Settings Database]
        Q[Rate Limit Cache]
        R[Audit Log Storage]
        S[Security Event Storage]
    end
    
    subgraph Security Monitoring
        T[Real-time Alerts]
        U[Anomaly Detection]
        V[Threat Intelligence]
        W[Compliance Reporting]
        X[Forensic Analysis]
    end
    
    %% Security flow connections
    D --> I
    C --> J
    E --> K
    F --> L
    G --> M
    
    I --> O
    J --> Q
    K --> P
    L --> P
    M --> R
    
    %% Monitoring connections
    M --> N
    N --> T
    N --> U
    N --> V
    N --> W
    N --> X
    
    %% Storage connections
    N --> S
    R --> W
    S --> X
```

## Notes

- **Enhanced Authentication**: Dual authentication strategy with JWT tokens for web users and API keys for third-party integrations
- **Comprehensive Security**: Multi-layer security with rate limiting, IP whitelisting, scope-based authorization, and comprehensive audit logging
- **API-First Architecture**: All services designed with API access in mind, supporting web, mobile, and third-party integrations
- **Real-time Monitoring**: Enhanced monitoring with API-specific metrics, security event tracking, and comprehensive alerting
- **Developer Experience**: Complete developer portal with documentation, testing tools, and analytics
- **Scalable Design**: Redis-based caching and rate limiting for high-performance API key validation
- **Compliance Ready**: Comprehensive audit logging and compliance reporting for financial data protection regulations

*Updated: December 28, 2024 - Enhanced with comprehensive API Key Management architecture*
