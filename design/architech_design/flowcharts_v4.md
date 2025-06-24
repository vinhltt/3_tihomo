# Personal Finance Management System - Flow Charts & Architecture Diagrams

## 1. High-Level Microservices Architecture

```mermaid
flowchart TB
  subgraph "Client Layer"
    FE[Web Frontend<br/>React/Angular<br/>Port 3000]
    Mobile[Mobile App<br/>React Native/Flutter]
    ThirdParty[Third Party Apps<br/>API Integration]
  end

  subgraph "Infrastructure Layer"
    subgraph "API Gateway"
      Gateway[Ocelot Gateway<br/>Load Balancer<br/>Port 5000]
      GW_Auth[JWT Middleware]
      GW_CORS[CORS Policy]
      GW_Rate[Rate Limiting]
      GW_Log[Request Logging]
    end
    
    subgraph "Message Bus"
      RabbitMQ[RabbitMQ<br/>Event Broker<br/>Port 5672]
      EventEx[Event Exchange<br/>tihomo.events]
      DeadLetter[Dead Letter Queue]
    end
    
    subgraph "File Storage"
      MinIO[MinIO<br/>Object Storage<br/>Port 9000]
      Buckets[statements/<br/>reports/<br/>backups/]
    end
  end

  subgraph "Microservices Layer"    subgraph "Identity & Access - Domain"
      IdentityAPI[Identity.Api<br/>Social Auth & User Management<br/>Port 5228]
      IdentityDB[(PostgreSQL<br/>db_identity<br/>Port 5432)]
    end

    subgraph "Core Finance - Domain"
      CoreAPI[CoreFinance.Api<br/>Accounts & Transactions<br/>Port 5001]
      CoreWorker[CoreFinance.Worker<br/>Background Jobs]
      CoreDB[(PostgreSQL<br/>db_finance<br/>Port 5433)]
    end

    subgraph "Money Management - Domain"
      MoneyAPI[MoneyManagement.Api<br/>Budgets & SixJars<br/>Port 5002]
      MoneyWorker[MoneyManagement.Worker<br/>Budget Alerts]
      MoneyDB[(PostgreSQL<br/>db_money<br/>Port 5434)]
    end

    subgraph "Planning & Investment - Domain"
      PlanAPI[PlanningInvestment.Api<br/>Goals & Debts<br/>Port 5003]
      PlanWorker[PlanningInvestment.Worker<br/>Goal Tracking]
      PlanDB[(PostgreSQL<br/>db_planning<br/>Port 5435)]
    end

    subgraph "Reporting & Integration - Domain"
      ReportAPI[Reporting.Api<br/>Analytics & Exports<br/>Port 5004]
      ReportWorker[Reporting.Worker<br/>Data Aggregation]
      ReportDB[(PostgreSQL<br/>db_reporting<br/>Port 5436)]
    end
  end

  %% Client to Gateway
  FE --> Gateway
  Mobile --> Gateway
  ThirdParty --> Gateway

  %% Gateway Middleware Pipeline
  Gateway --> GW_CORS
  GW_CORS --> GW_Auth
  GW_Auth --> GW_Rate
  GW_Rate --> GW_Log
  %% Gateway to Services
  GW_Log --> IdentityAPI
  GW_Log --> CoreAPI
  GW_Log --> MoneyAPI
  GW_Log --> PlanAPI
  GW_Log --> ReportAPI

  %% Services to Databases
  IdentityAPI --> IdentityDB
  CoreAPI --> CoreDB
  MoneyAPI --> MoneyDB
  PlanAPI --> PlanDB
  ReportAPI --> ReportDB

  %% Event-Driven Communication
  IdentityAPI -.->|User Events| RabbitMQ
  CoreAPI -.->|Transaction Events| RabbitMQ
  MoneyAPI -.->|Budget Events| RabbitMQ
  PlanAPI -.->|Goal Events| RabbitMQ
  ReportAPI -.->|Report Events| RabbitMQ

  RabbitMQ -.-> CoreWorker
  RabbitMQ -.-> MoneyWorker
  RabbitMQ -.-> PlanWorker
  RabbitMQ -.-> ReportWorker

  %% File Storage Integration
  CoreAPI --> MinIO
  ReportAPI --> MinIO

  %% Styling
  classDef clientLayer fill:#e1f5fe
  classDef gatewayLayer fill:#f3e5f5
  classDef serviceLayer fill:#e8f5e8
  classDef dataLayer fill:#fff3e0
  classDef messageLayer fill:#fce4ec

  class FE,Mobile,ThirdParty clientLayer
  class Gateway,GW_Auth,GW_CORS,GW_Rate,GW_Log gatewayLayer
  class IdentityAPI,CoreAPI,MoneyAPI,PlanAPI,ReportAPI serviceLayer
  class IdentityDB,CoreDB,MoneyDB,PlanDB,ReportDB,MinIO dataLayer
  class RabbitMQ,EventEx,DeadLetter messageLayer
```

## 2. Ocelot API Gateway - Detailed Configuration & Routing

```mermaid
flowchart TB
  subgraph "Client Requests"
    WebReq[Web App Request<br/>https://frontend.com]
    MobileReq[Mobile App Request<br/>iOS/Android]
    APIReq[Third Party API<br/>API Key Auth]
  end

  subgraph "Ocelot Gateway - Port 5000"
    subgraph "Middleware Pipeline (Order Matters)"
      direction TB
      Middleware1[1. Exception Handling]
      Middleware2[2. HTTPS Redirection]
      Middleware3[3. CORS Policy]
      Middleware4[4. Request Logging]
      Middleware5[5. Rate Limiting]
      Middleware6[6. JWT Authentication]
      Middleware7[7. Authorization Policy]
      Middleware8[8. Request/Response Caching]
      Middleware9[9. Load Balancing]
      
      Middleware1 --> Middleware2
      Middleware2 --> Middleware3
      Middleware3 --> Middleware4
      Middleware4 --> Middleware5
      Middleware5 --> Middleware6
      Middleware6 --> Middleware7
      Middleware7 --> Middleware8
      Middleware8 --> Middleware9
    end
    
    subgraph "Route Configuration (ocelot.json)"
      direction TB
        subgraph "Authentication Routes"
        AuthRoute[/api/auth/* → Identity.Api:5228<br/>Social Login & Token Auth<br/>No Auth Required]
        ApiKeyRoute[/api/apikeys/* → Identity.Api:5228<br/>API Key Management<br/>Requires: Valid JWT]
      end
      
      subgraph "API Routes (JWT Required)"
        UserRoute[/api/users/* → Identity.Api:5228<br/>User Management<br/>Requires: User Role]
        AdminRoute[/api/admin/* → Identity.Api:5228<br/>Admin Operations<br/>Requires: Admin Role]
        CoreRoute[/api/finance/* → CoreFinance.Api:5001<br/>Accounts & Transactions<br/>Requires: Valid JWT]
        MoneyRoute[/api/money/* → MoneyManagement.Api:5002<br/>Budgets & SixJars<br/>Requires: Valid JWT]
        PlanRoute[/api/planning/* → PlanningInvestment.Api:5003<br/>Goals & Investments<br/>Requires: Valid JWT]
        ReportRoute[/api/reporting/* → Reporting.Api:5004<br/>Analytics & Reports<br/>Requires: Valid JWT]
      end
      
      subgraph "Special Routes"
        ApiKeyRoute[/api/external/* → Multiple Services<br/>API Key Authentication<br/>Rate Limited: 1000/hour]
        WebhookRoute[/webhooks/* → Reporting.Api:5004<br/>Bank Integration<br/>IP Whitelist Only]
      end
    end
    
    subgraph "Load Balancing & Health Checks"
      direction TB
      HealthCheck[Health Check Endpoints<br/>/health]
      ServiceDiscovery[Service Discovery<br/>Consul/Manual Config]
      LoadBalancer[Round Robin<br/>Least Connections<br/>Weighted Distribution]
    end
  end
  subgraph "Downstream Services"
    direction TB
    IdentityAPI[Identity.Api:5228]
    CoreAPI[CoreFinance.Api:5001]
    MoneyAPI[MoneyManagement.Api:5002]
    PlanAPI[PlanningInvestment.Api:5003]
    ReportAPI[Reporting.Api:5004]
  end

  %% Request Flow
  WebReq --> Middleware1
  MobileReq --> Middleware1
  APIReq --> Middleware1
    %% Route Mapping
  Middleware9 --> AuthRoute
  Middleware9 --> UserRoute
  Middleware9 --> AdminRoute
  Middleware9 --> CoreRoute
  Middleware9 --> MoneyRoute
  Middleware9 --> PlanRoute
  Middleware9 --> ReportRoute
  Middleware9 --> ApiKeyRoute
  Middleware9 --> WebhookRoute
  
  %% Health & Discovery
  HealthCheck --> ServiceDiscovery
  ServiceDiscovery --> LoadBalancer
  LoadBalancer --> Middleware9
  
  %% Downstream Routing
  AuthRoute --> IdentityAPI
  UserRoute --> IdentityAPI
  AdminRoute --> IdentityAPI
  ApiKeyRoute --> IdentityAPI
  CoreRoute --> CoreAPI
  MoneyRoute --> MoneyAPI
  PlanRoute --> PlanAPI
  ReportRoute --> ReportAPI
  ApiKeyRoute --> IdentityAPI
  ApiKeyRoute --> CoreAPI
  ApiKeyRoute --> MoneyAPI
  WebhookRoute --> ReportAPI

  %% Styling
  classDef clientStyle fill:#e3f2fd
  classDef middlewareStyle fill:#f3e5f5
  classDef routeStyle fill:#e8f5e8
  classDef serviceStyle fill:#fff3e0

  class WebReq,MobileReq,APIReq clientStyle
  class Middleware1,Middleware2,Middleware3,Middleware4,Middleware5,Middleware6,Middleware7,Middleware8,Middleware9 middlewareStyle
  class SSORoute,AuthRoute,UserRoute,AdminRoute,CoreRoute,MoneyRoute,PlanRoute,ReportRoute,ApiKeyRoute,WebhookRoute routeStyle
  class IdentitySSO,IdentityAPI,CoreAPI,MoneyAPI,PlanAPI,ReportAPI serviceStyle
```

## 3. Identity & Access Services - Simplified Architecture

```mermaid
flowchart TB
  subgraph "Client Applications"
    WebApp[Web Frontend<br/>Nuxt.js SPA<br/>Social Login]
    MobileApp[Mobile App<br/>Native Application<br/>Social Login]
    ThirdPartyApp[Third Party<br/>External Integration<br/>API Key Auth]
    AdminPanel[Admin Dashboard<br/>Management UI<br/>Enhanced Permissions]
  end

  subgraph "Ocelot Gateway :5000"
    Gateway[Token Verification<br/>& Route Mapping]
  end

  subgraph "Identity Service - Simplified"
    subgraph "Identity.Api - Port 5228 (Single Service)"
      direction TB
      
      subgraph "Social Authentication"
        GoogleAuth[Google OAuth2<br/>Token Verification]
        FacebookAuth[Facebook Login<br/>Token Verification]
        AppleAuth[Apple Sign-In<br/>Token Verification]
      end
      
      subgraph "Authentication API"
        LoginAPI[POST /api/auth/login<br/>Traditional Login]
        SocialAPI[POST /api/auth/social<br/>Social Login Verification]
        RefreshAPI[POST /api/auth/refresh<br/>Token Refresh]
        LogoutAPI[POST /api/auth/logout<br/>Token Revocation]
      end
      
      subgraph "User Management API"
        UsersAPI[GET|POST|PUT /api/users<br/>User CRUD Operations]
        ProfileAPI[GET|PUT /api/users/me<br/>Profile Management]
        PasswordAPI[POST /api/users/change-password<br/>Password Change]
      end
      
      subgraph "API Key Management"
        ApiKeysAPI[GET|POST|PUT|DELETE /api/apikeys<br/>API Key Lifecycle]
        VerifyAPI[GET /api/apikeys/verify<br/>Key Verification]
      end
    end

    subgraph "Database Layer"
      IdentityDB[(PostgreSQL<br/>db_identity<br/>Users, ApiKeys, UserLogins)]
    end
  end
        UsersAPI[/api/users/*<br/>CRUD Operations]
        ProfileAPI[/api/users/profile<br/>User Profile]
        PasswordAPI[/api/users/password<br/>Password Management]
        PreferencesAPI[/api/users/preferences<br/>User Settings]
      end
      
      subgraph "Administration API"
        RolesAPI[/api/roles/*<br/>Role Management]
        PermissionsAPI[/api/permissions/*<br/>Permission Control]
        ApiKeysAPI[/api/apikeys/*<br/>API Key Management]
        AuditAPI[/api/audit/*<br/>Security Audit Logs]
      end
    end

    subgraph "Shared Business Logic Layer"
      direction TB
    %% Connection Flow
  WebApp -->|Social Auth Tokens| Gateway
  MobileApp -->|Social Auth Tokens| Gateway
  ThirdPartyApp -->|API Key| Gateway
  AdminPanel -->|JWT Bearer| Gateway

  %% Gateway Routing
  Gateway -->|Token Verification| IdentityAPI
  Gateway -->|/api/auth/*| LoginAPI
  Gateway -->|/api/auth/*| SocialAPI
  Gateway -->|/api/users/*| UsersAPI
  Gateway -->|/api/apikeys/*| ApiKeysAPI

  %% Social Authentication Flow
  WebApp --> GoogleAuth
  WebApp --> FacebookAuth
  WebApp --> AppleAuth
  
  %% Service Dependencies
  GoogleAuth --> IdentityDB
  FacebookAuth --> IdentityDB
  AppleAuth --> IdentityDB
  LoginAPI --> IdentityDB
  SocialAPI --> IdentityDB
  UsersAPI --> IdentityDB
  ApiKeysAPI --> IdentityDB

  %% External Integration
  VerifyAPI -.->|Token Validation| Gateway

  %% Styling
  classDef clientStyle fill:#e3f2fd
  classDef serviceStyle fill:#e8f5e8
  classDef dataStyle fill:#fff3e0

  class WebApp,MobileApp,ThirdPartyApp,AdminPanel clientStyle
  class GoogleAuth,FacebookAuth,AppleAuth,LoginAPI,SocialAPI,UsersAPI,ApiKeysAPI serviceStyle
  class IdentityDB dataStyle
```
  TokenEndpoint --> AuthService
  UserInfoEndpoint --> UserService
  LoginUI --> AuthService
  RegisterUI --> UserService

  %% API Service Dependencies
  LoginAPI --> AuthService
  RefreshAPI --> JwtService
  UsersAPI --> UserService
  RolesAPI --> RoleService
  ApiKeysAPI --> ApiKeyService

  %% Service to Service Communication
  AuthService --> UserService
  AuthService --> JwtService
  AuthService --> SessionService
  UserService --> PasswordService
  RoleService --> PermissionsAPI
  ApiKeyService --> AuditService

  %% Data Access
  AuthService --> IdentityDB
  UserService --> IdentityDB
  RoleService --> IdentityDB
  ApiKeyService --> IdentityDB
  AuditService --> IdentityDB

  %% Database Tables
  IdentityDB --> Users
  IdentityDB --> Roles
  IdentityDB --> Permissions
  IdentityDB --> ApiKeys
  IdentityDB --> RefreshTokens
  IdentityDB --> AuditLogs

  %% External Dependencies
  SessionService --> Redis
  AuthService -.->|User Events| RabbitMQ
  UserService -.->|Profile Events| RabbitMQ
  AuditService -.->|Security Events| RabbitMQ

  %% Styling
  classDef clientStyle fill:#e3f2fd
  classDef ssoStyle fill:#f3e5f5
  classDef apiStyle fill:#e8f5e8
  classDef serviceStyle fill:#fff3e0
  classDef dataStyle fill:#fce4ec

  class WebApp,MobileApp,ThirdPartyApp,AdminPanel clientStyle
  class AuthorizeEndpoint,TokenEndpoint,UserInfoEndpoint,LoginUI,RegisterUI ssoStyle
  class LoginAPI,RefreshAPI,UsersAPI,RolesAPI,ApiKeysAPI apiStyle
  class AuthService,UserService,RoleService,ApiKeyService,JwtService serviceStyle
  class IdentityDB,Users,Roles,Permissions,ApiKeys dataStyle
```

## 4. Event-Driven Architecture - RabbitMQ Message Bus

```mermaid
flowchart TB
  subgraph "Message Publishers (Event Sources)"
    direction TB
    IdentityPub[Identity Services<br/>User & Auth Events]
    CorePub[Core Finance<br/>Transaction Events]
    MoneyPub[Money Management<br/>Budget & Jar Events]
    PlanPub[Planning & Investment<br/>Goal & Debt Events]
    ReportPub[Reporting<br/>Analytics Events]
  end

  subgraph "RabbitMQ Event Bus - Port 5672"
    direction TB
    
    subgraph "Exchange Configuration"
      MainExchange[Topic Exchange<br/>tihomo.events<br/>Routing Key Pattern]
      DLX[Dead Letter Exchange<br/>tihomo.dlx<br/>Failed Messages]
    end
    
    subgraph "Event Queues & Routing"
      direction TB
      
      subgraph "Identity Events"
        UserQueue[user.events.*<br/>user.created, user.updated<br/>user.deleted, user.locked]
        AuthQueue[auth.events.*<br/>login.success, login.failed<br/>token.expired, password.changed]
      end
      
      subgraph "Financial Events"
        TxQueue[transaction.events.*<br/>transaction.created, transaction.updated<br/>transaction.deleted, statement.imported]
        AccountQueue[account.events.*<br/>account.created, balance.updated<br/>account.closed, limit.exceeded]
      end
      
      subgraph "Budget & Money Events"
        BudgetQueue[budget.events.*<br/>budget.created, budget.exceeded<br/>budget.warning, budget.reset]
        JarQueue[jar.events.*<br/>jar.allocated, jar.transferred<br/>jar.balance.updated]
      end
      
      subgraph "Planning Events"
        GoalQueue[goal.events.*<br/>goal.created, goal.achieved<br/>goal.progress, goal.deadline]
        DebtQueue[debt.events.*<br/>debt.created, debt.payment<br/>debt.completed]
      end
      
      subgraph "System Events"
        NotificationQueue[notification.events.*<br/>email.sent, push.sent<br/>sms.sent, webhook.called]
        AnalyticsQueue[analytics.events.*<br/>report.generated, export.completed<br/>dashboard.updated]
      end
    end
    
    subgraph "Dead Letter Handling"
      DLQueue[Dead Letter Queue<br/>Failed Processing]
      RetryQueue[Retry Queue<br/>Exponential Backoff]
      PoisonQueue[Poison Queue<br/>Manual Investigation]
    end
  end

  subgraph "Message Consumers (Event Handlers)"
    direction TB
    
    subgraph "Background Workers"
      CoreWorker[CoreFinance.Worker<br/>Transaction Processing]
      MoneyWorker[MoneyManagement.Worker<br/>Budget Monitoring]
      PlanWorker[PlanningInvestment.Worker<br/>Goal Tracking]
      ReportWorker[Reporting.Worker<br/>Data Aggregation]
    end
    
    subgraph "Notification Services"
      EmailSvc[Email Service<br/>SMTP/SendGrid]
      PushSvc[Push Notification<br/>Firebase/APNS]
      WebhookSvc[Webhook Service<br/>External APIs]
    end
    
    subgraph "Analytics & Reporting"
      DashboardSvc[Dashboard Service<br/>Real-time Updates]
      ExportSvc[Export Service<br/>PDF/Excel Generation]
      AuditSvc[Audit Service<br/>Compliance Logging]
    end
  end

  %% Event Publishing
  IdentityPub -->|Publish| MainExchange
  CorePub -->|Publish| MainExchange
  MoneyPub -->|Publish| MainExchange
  PlanPub -->|Publish| MainExchange
  ReportPub -->|Publish| MainExchange

  %% Queue Routing via Exchange
  MainExchange -->|user.*| UserQueue
  MainExchange -->|auth.*| AuthQueue
  MainExchange -->|transaction.*| TxQueue
  MainExchange -->|account.*| AccountQueue
  MainExchange -->|budget.*| BudgetQueue
  MainExchange -->|jar.*| JarQueue
  MainExchange -->|goal.*| GoalQueue
  MainExchange -->|debt.*| DebtQueue
  MainExchange -->|notification.*| NotificationQueue
  MainExchange -->|analytics.*| AnalyticsQueue

  %% Consumer Subscriptions
  UserQueue --> ReportWorker
  AuthQueue --> AuditSvc
  TxQueue --> CoreWorker
  TxQueue --> MoneyWorker
  TxQueue --> ReportWorker
  AccountQueue --> DashboardSvc
  BudgetQueue --> MoneyWorker
  BudgetQueue --> EmailSvc
  JarQueue --> DashboardSvc
  GoalQueue --> PlanWorker
  GoalQueue --> PushSvc
  DebtQueue --> PlanWorker
  NotificationQueue --> EmailSvc
  NotificationQueue --> PushSvc
  NotificationQueue --> WebhookSvc
  AnalyticsQueue --> DashboardSvc
  AnalyticsQueue --> ExportSvc

  %% Dead Letter Handling
  MainExchange -->|Failed| DLX
  DLX --> DLQueue
  DLQueue --> RetryQueue
  RetryQueue -->|Retry| MainExchange
  RetryQueue -->|Max Retries| PoisonQueue

  %% Styling
  classDef publisherStyle fill:#e3f2fd
  classDef exchangeStyle fill:#f3e5f5
  classDef queueStyle fill:#e8f5e8
  classDef consumerStyle fill:#fff3e0
  classDef errorStyle fill:#ffebee

  class IdentityPub,CorePub,MoneyPub,PlanPub,ReportPub publisherStyle
  class MainExchange,DLX exchangeStyle
  class UserQueue,AuthQueue,TxQueue,AccountQueue,BudgetQueue,JarQueue,GoalQueue,DebtQueue,NotificationQueue,AnalyticsQueue queueStyle
  class CoreWorker,MoneyWorker,PlanWorker,ReportWorker,EmailSvc,PushSvc,WebhookSvc,DashboardSvc,ExportSvc,AuditSvc consumerStyle
  class DLQueue,RetryQueue,PoisonQueue errorStyle
```

## 5. Statement Import & Processing Flow - MinIO Integration

```mermaid
flowchart TB
  subgraph "Client Applications"
    WebClient[Web Dashboard<br/>File Upload UI]
    MobileClient[Mobile App<br/>Camera Upload]
    EmailGateway[Email Gateway<br/>Forwarded Statements]
  end

  subgraph "API Gateway & Load Balancer"
    Gateway[Ocelot Gateway<br/>:5000<br/>File Upload Routing]
  end

  subgraph "Core Finance Service"
    CoreAPI[CoreFinance.Api<br/>:5001]
    
    subgraph "Upload Controllers"
      UploadController[StatementUploadController<br/>/api/statements/upload]
      BatchController[BatchUploadController<br/>/api/statements/batch]
    end
    
    subgraph "Core Services"
      StatementSvc[StatementService<br/>File Management]
      ValidationSvc[ValidationService<br/>File Validation]
      TransactionSvc[TransactionService<br/>Transaction Creation]
    end
  end

  subgraph "Reporting & Processing Service"
    ReportAPI[Reporting.Api<br/>:5004]
    
    subgraph "Processing Services"
      ParserSvc[StatementParserService<br/>PDF/CSV/Excel Parser]
      NLPSvc[NLPService<br/>Transaction Categorization]
      DeduplicationSvc[DeduplicationService<br/>Duplicate Detection]
    end
    
    subgraph "Background Workers"
      ProcessingWorker[Processing.Worker<br/>Async File Processing]
      CategorizationWorker[Categorization.Worker<br/>ML-based Categorization]
    end
  end

  subgraph "Storage & Message Infrastructure"
    subgraph "MinIO Object Storage - Port 9000"
      direction TB
      
      subgraph "Storage Buckets"
        RawBucket[raw-statements/<br/>Original Files]
        ProcessedBucket[processed-statements/<br/>Parsed Data]
        ArchiveBucket[archive/<br/>Historical Files]
        TempBucket[temp/<br/>Processing Files]
      end
      
      subgraph "File Metadata"
        FileIndex[File Index<br/>Metadata & Status]
        Versioning[Version Control<br/>File History]
      end
    end
    
    RabbitMQ[RabbitMQ Event Bus<br/>:5672]
  end

  subgraph "Database Layer"
    CoreDB[(CoreFinance DB<br/>PostgreSQL :5433)]
    ReportDB[(Reporting DB<br/>PostgreSQL :5436)]
  end

  %% Upload Flow
  WebClient -->|1. Upload File| Gateway
  MobileClient -->|1. Upload Photo| Gateway
  EmailGateway -->|1. Forward Statement| Gateway
  
  Gateway -->|2. Route to Core| CoreAPI
  CoreAPI -->|3. Validate Request| UploadController
  UploadController -->|4. File Validation| ValidationSvc
  ValidationSvc -->|5. Store Original| RawBucket
  ValidationSvc -->|6. Log Metadata| FileIndex
  ValidationSvc -->|7. Publish Event| RabbitMQ
  
  %% Processing Pipeline
  RabbitMQ -->|8. statement.uploaded| ProcessingWorker
  ProcessingWorker -->|9. Retrieve File| RawBucket
  ProcessingWorker -->|10. Parse Content| ParserSvc
  ParserSvc -->|11. Extract Data| TempBucket
  ParserSvc -->|12. Categorize| NLPSvc
  NLPSvc -->|13. ML Processing| CategorizationWorker
  
  %% Transaction Creation
  CategorizationWorker -->|14. Publish Transactions| RabbitMQ
  RabbitMQ -->|15. transactions.parsed| TransactionSvc
  TransactionSvc -->|16. Deduplicate| DeduplicationSvc
  DeduplicationSvc -->|17. Save Transactions| CoreDB
  TransactionSvc -->|18. Store Processed| ProcessedBucket
  TransactionSvc -->|19. Archive Original| ArchiveBucket
  
  %% Completion & Notification
  TransactionSvc -->|20. Publish Complete| RabbitMQ
  RabbitMQ -->|21. statement.processed| ReportAPI
  ReportAPI -->|22. Update Analytics| ReportDB
  ReportAPI -->|23. Notify Client| Gateway
  Gateway -->|24. Real-time Update| WebClient

  %% Error Handling
  ValidationSvc -.->|Validation Failed| RabbitMQ
  ParserSvc -.->|Parse Error| RabbitMQ
  TransactionSvc -.->|Save Error| RabbitMQ

  %% Styling
  classDef clientStyle fill:#e3f2fd
  classDef gatewayStyle fill:#f3e5f5
  classDef serviceStyle fill:#e8f5e8
  classDef storageStyle fill:#fff3e0
  classDef dbStyle fill:#fce4ec

  class WebClient,MobileClient,EmailGateway clientStyle
  class Gateway gatewayStyle
  class CoreAPI,ReportAPI,UploadController,StatementSvc,ParserSvc serviceStyle
  class RawBucket,ProcessedBucket,ArchiveBucket,TempBucket,MinIO storageStyle
  class CoreDB,ReportDB dbStyle
```

## 6. Sequence Diagrams - Detailed Service Interactions

### 6.1 Social Login Flow (Simplified)

```mermaid
sequenceDiagram
    participant Browser as Web Browser
    participant Frontend as Nuxt.js SPA
    participant Gateway as Ocelot Gateway
    participant Identity as Identity.Api
    participant Google as Google OAuth2
    participant DB as PostgreSQL

    Note over Browser,DB: Simplified Social Authentication Flow

    Browser->>Frontend: Click "Login with Google"
    Frontend->>Google: Initiate Google OAuth2 flow
    Google->>Browser: Show Google login dialog
    Browser->>Google: Enter credentials
    Google-->>Frontend: Return ID Token (JWT)
    
    Frontend->>Gateway: POST /api/auth/social {provider: "google", token: "jwt_token"}
    Gateway->>Identity: Forward social login request
    Identity->>Google: Verify token with Google API
    Google-->>Identity: Token validation result + user profile
    
    Identity->>DB: SELECT user WHERE provider_id = ? AND provider = 'google'
    DB-->>Identity: User record (if exists)
    
    alt User exists
        Identity->>DB: UPDATE last_login_at = NOW()
    else User not found
        Identity->>DB: INSERT new user (email, name, provider, provider_id)
    end
    
    Identity->>Identity: Generate JWT access token
    Identity->>DB: INSERT refresh_token
    Identity-->>Gateway: {access_token, refresh_token, user_profile}
    Gateway-->>Frontend: Authentication response
    
    Frontend->>Frontend: Store tokens in secure storage
    Frontend->>Browser: Redirect to dashboard
    
    Note over Browser,DB: Subsequent API calls use JWT Bearer token
    AuthSvc->>JwtSvc: CreateAccessTokenAsync(user, scopes)
    JwtSvc->>JwtSvc: Sign JWT with RS256
    AuthSvc->>DB: Store refresh token (hashed)
    JwtSvc-->>AuthSvc: Signed access token
    AuthSvc-->>SSO: Token response
    SSO-->>Gateway: {access_token, refresh_token, id_token, expires_in}
    Gateway-->>Browser: Token response
    
    Browser->>Frontend: Store tokens securely
    Frontend->>Gateway: GET /api/users/profile (with Bearer token)
    Gateway->>Gateway: Validate JWT signature
    Gateway-->>Frontend: User profile data
```

### 6.2 API Key Authentication Flow

```mermaid
sequenceDiagram
    participant ThirdParty as Third Party System
    participant Gateway as Ocelot Gateway
    participant Identity as Identity.Api
    participant DB as PostgreSQL

    Note over ThirdParty,DB: API Key Authentication Flow

    ThirdParty->>Gateway: GET /api/core-finance/transaction
    Note right of ThirdParty: Authorization: ApiKey abc123...
    
    Gateway->>Gateway: Extract API Key from header
    Gateway->>Identity: GET /api/apikeys/verify/abc123...
    Identity->>DB: SELECT api_key WHERE key_hash = ? AND is_active = true
    DB-->>Identity: API key record + user_id + scopes
    
    alt API Key Valid
        Identity-->>Gateway: {valid: true, user_id: "123", scopes: ["transactions:read"]}
        Gateway->>Gateway: Set user claims from API key
        Gateway->>CoreFinance: Forward request with user context
        CoreFinance-->>Gateway: Transaction data
        Gateway-->>ThirdParty: API response
    else API Key Invalid/Expired
        Identity-->>Gateway: {valid: false, error: "Invalid API key"}
        Gateway-->>ThirdParty: 401 Unauthorized
    end
    
    Note over Identity,DB: API key usage tracked for analytics
    
    AuthSvc->>DeviceDB: Register/update device info
    AuthSvc->>JwtSvc: GenerateTokensAsync(user, device)
    JwtSvc->>JwtSvc: Create access token (30min) + refresh token (30 days)
    JwtSvc->>DB: Store refresh token with device binding
    JwtSvc-->>AuthSvc: Token pair
    
    AuthSvc->>DB: Log successful login + device info
    AuthSvc-->>IdentityAPI: LoginResponse
    IdentityAPI-->>Gateway: {access_token, refresh_token, user_profile, permissions}
    Gateway-->>Mobile: Authentication successful
    
    Mobile->>Mobile: Store tokens in secure keychain
    Mobile->>Mobile: Set up token refresh timer
```

### 6.3 API Key Authentication & Validation Flow

```mermaid
sequenceDiagram
    participant ThirdParty as Third Party System
    participant Gateway as Ocelot Gateway
    participant IdentityAPI as Identity.Api
    participant ApiKeySvc as ApiKeyService
    participant RateLimit as Rate Limiter
    participant DB as PostgreSQL
    participant AuditSvc as AuditService

    Note over ThirdParty,AuditSvc: API Key Authentication with Rate Limiting & Audit

    ThirdParty->>Gateway: GET /api/finance/accounts
    Note right of ThirdParty: Header: Authorization: ApiKey ak_live_1234567890abcdef
    
    Gateway->>Gateway: Extract API key from Authorization header
    Gateway->>RateLimit: Check rate limits for API key
    RateLimit->>DB: Get current usage counters
    DB-->>RateLimit: Current rate limit status
    
    alt Rate Limit Exceeded
        RateLimit-->>Gateway: Rate limit exceeded (429)
        Gateway-->>ThirdParty: HTTP 429 Too Many Requests
    else Rate Limit OK
        Gateway->>IdentityAPI: GET /api/apikeys/verify
        Note right of Gateway: Internal call with API key
        
        IdentityAPI->>ApiKeySvc: VerifyApiKeyAsync(hashedKey)
        ApiKeySvc->>DB: SELECT api_key WHERE key_hash = ? AND active = true
        DB-->>ApiKeySvc: API key record + user + scopes
        
        ApiKeySvc->>ApiKeySvc: Check key expiration & permissions
        ApiKeySvc->>DB: UPDATE last_used_at, usage_count
        ApiKeySvc->>AuditSvc: Log API key usage
        AuditSvc->>DB: INSERT audit_log (api_key_used)
        
        ApiKeySvc-->>IdentityAPI: Verification result
        IdentityAPI-->>Gateway: {valid: true, user_id, scopes: ["finance:read"]}
        
        Gateway->>Gateway: Set user context for downstream services
        Gateway->>CoreFinance: GET /api/accounts (with user context)
        CoreFinance-->>Gateway: Account data
        Gateway-->>ThirdParty: HTTP 200 + account data
        
        RateLimit->>DB: Increment usage counters
    end
```

### 6.4 Transaction Creation with Balance Update Flow

```mermaid
sequenceDiagram
    participant Client as Frontend Client
    participant Gateway as Ocelot Gateway
    participant CoreAPI as CoreFinance.Api
    participant TxSvc as TransactionService
    participant AccSvc as AccountService
    participant ValidatorSvc as ValidationService
    participant RabbitMQ as Message Bus
    participant CoreDB as Core Database
    participant MoneyWorker as Money.Worker
    participant ReportWorker as Report.Worker

    Note over Client,ReportWorker: End-to-End Transaction Processing with Event Propagation

    Client->>Gateway: POST /api/finance/transactions
    Note right of Client: {accountId, amount: -50.00, description: "Grocery Store", category: "Food"}
    
    Gateway->>Gateway: Validate JWT + extract user claims
    Gateway->>CoreAPI: Forward request with user context
    CoreAPI->>TxSvc: CreateTransactionAsync(request, userId)
    
    TxSvc->>AccSvc: ValidateAccountOwnershipAsync(accountId, userId)
    AccSvc->>CoreDB: Check account ownership
    CoreDB-->>AccSvc: Ownership confirmed
    AccSvc-->>TxSvc: Validation passed
    
    TxSvc->>ValidatorSvc: ValidateTransactionRules(transaction)
    ValidatorSvc->>ValidatorSvc: Check business rules (daily limits, account balance)
    ValidatorSvc->>CoreDB: Get account current balance
    CoreDB-->>ValidatorSvc: Current balance: $1,250.00
    ValidatorSvc->>ValidatorSvc: Validate sufficient funds ($1,250 - $50 = $1,200 ✓)
    ValidatorSvc-->>TxSvc: Validation successful
    
    TxSvc->>CoreDB: BEGIN TRANSACTION
    TxSvc->>CoreDB: INSERT transaction (id, account_id, amount, description, category, created_at)
    TxSvc->>AccSvc: UpdateAccountBalanceAsync(accountId, -50.00)
    AccSvc->>CoreDB: UPDATE accounts SET balance = balance - 50.00 WHERE id = ?
    TxSvc->>CoreDB: COMMIT TRANSACTION
    
    TxSvc->>RabbitMQ: Publish TransactionCreated event
    Note right of RabbitMQ: {transactionId, accountId, amount, category, userId, timestamp}
    
    TxSvc-->>CoreAPI: Transaction created successfully
    CoreAPI-->>Gateway: {transactionId: "tx_12345", newBalance: 1200.00}
    Gateway-->>Client: Transaction response
    
    par Parallel Event Processing
        RabbitMQ->>MoneyWorker: Process budget impact
        MoneyWorker->>MoneyWorker: Update category budget spending
        MoneyWorker->>MoneyWorker: Check budget thresholds
        MoneyWorker->>RabbitMQ: Publish BudgetUpdated event
    and
        RabbitMQ->>ReportWorker: Update analytics
        ReportWorker->>ReportWorker: Aggregate spending data
        ReportWorker->>ReportWorker: Update dashboard metrics
        ReportWorker->>RabbitMQ: Publish AnalyticsUpdated event
    end
```

### 6.5 SixJars Money Allocation & Budget Tracking Flow

```mermaid
sequenceDiagram
    participant Client as Mobile App
    participant Gateway as Ocelot Gateway
    participant MoneyAPI as MoneyManagement.Api
    participant JarSvc as JarService
    participant BudgetSvc as BudgetService
    participant TxSvc as TransactionService
    participant RabbitMQ as Message Bus
    participant MoneyDB as Money Database
    participant NotificationSvc as Notification Service

    Note over Client,NotificationSvc: SixJars Allocation with Automated Budget Monitoring

    Client->>Gateway: POST /api/money/jars/allocate
    Note right of Client: {income: 3000.00, customRatios: {necessity: 0.55, play: 0.10, education: 0.10, longTermSaving: 0.10, give: 0.05, financialFreedom: 0.10}}
    
    Gateway->>MoneyAPI: Forward allocation request
    MoneyAPI->>JarSvc: AllocateIncomeAsync(request, userId)
    
    JarSvc->>JarSvc: Calculate jar amounts based on ratios
    Note right of JarSvc: Necessity: $1,650, Play: $300, Education: $300, LTSS: $300, Give: $150, FFA: $300
    
    JarSvc->>MoneyDB: Get current jar balances
    MoneyDB-->>JarSvc: Current jar balances
    JarSvc->>MoneyDB: BEGIN TRANSACTION
    
    loop For each jar (6 iterations)
        JarSvc->>MoneyDB: UPDATE jar_balances SET amount = amount + allocation
        JarSvc->>TxSvc: CreateJarTransactionAsync(jarId, amount, "Income Allocation")
    end
    
    JarSvc->>MoneyDB: COMMIT TRANSACTION
    JarSvc->>RabbitMQ: Publish JarAllocationCompleted event
    Note right of RabbitMQ: {userId, totalAmount: 3000, jarAllocations: {...}, timestamp}
    
    JarSvc-->>MoneyAPI: Allocation successful
    MoneyAPI-->>Gateway: Updated jar balances
    Gateway-->>Client: Allocation completed
    
    %% Background Budget Monitoring
    RabbitMQ->>BudgetSvc: Monitor spending against jars
    BudgetSvc->>MoneyDB: Get user's active budgets
    MoneyDB-->>BudgetSvc: Budget limits per category
    
    BudgetSvc->>BudgetSvc: Calculate spending rate predictions
    BudgetSvc->>MoneyDB: Get recent transaction patterns
    MoneyDB-->>BudgetSvc: Spending history
    
    alt Projected Overspending Detected
        BudgetSvc->>RabbitMQ: Publish BudgetWarningEvent
        RabbitMQ->>NotificationSvc: Send proactive warning
        NotificationSvc->>Client: Push notification: "You're on track to exceed your Food budget by $50 this month"
    end
    
    BudgetSvc->>MoneyDB: Update budget tracking metrics
```

### 6.6 Shared Expense Management Flow

```mermaid
sequenceDiagram
    participant Creator as Expense Creator
    participant Participants as Other Participants
    participant Gateway as Ocelot Gateway
    participant MoneyAPI as MoneyManagement.Api
    participant SharedSvc as SharedExpenseService
    participant NotiSvc as NotificationService
    participant EmailSvc as EmailService
    participant RabbitMQ as Message Bus
    participant MoneyDB as Money Database

    Note over Creator,MoneyDB: Collaborative Expense Sharing with Automated Notifications

    Creator->>Gateway: POST /api/money/shared-expenses
    Note right of Creator: {description: "Team Dinner", totalAmount: 120.00, participants: ["user2", "user3"], splitType: "equal"}
    
    Gateway->>MoneyAPI: Forward expense creation
    MoneyAPI->>SharedSvc: CreateSharedExpenseAsync(request, creatorId)
    
    SharedSvc->>SharedSvc: Validate participant permissions
    SharedSvc->>MoneyDB: Check if participants exist and are friends/colleagues
    MoneyDB-->>SharedSvc: Participant validation results
    
    SharedSvc->>SharedSvc: Calculate split amounts
    Note right of SharedSvc: Equal split: $40.00 per person (3 participants)
    
    SharedSvc->>MoneyDB: BEGIN TRANSACTION
    SharedSvc->>MoneyDB: INSERT shared_expense (id, creator_id, total_amount, description, status)
    
    loop For each participant
        SharedSvc->>MoneyDB: INSERT expense_share (expense_id, user_id, amount_owed, status: 'pending')
    end
    
    SharedSvc->>MoneyDB: COMMIT TRANSACTION
    SharedSvc->>RabbitMQ: Publish SharedExpenseCreated event
    Note right of RabbitMQ: {expenseId, creatorId, participants[], totalAmount, individualAmounts[]}
    
    SharedSvc-->>MoneyAPI: Expense created successfully
    MoneyAPI-->>Gateway: {expenseId: "exp_12345", shareDetails: [...]}
    Gateway-->>Creator: Expense sharing initiated
    
    %% Notification Flow
    RabbitMQ->>NotiSvc: Process expense notifications
    NotiSvc->>MoneyDB: Get participant contact preferences
    MoneyDB-->>NotiSvc: Email addresses + notification settings
    
    par Notify All Participants
        NotiSvc->>EmailSvc: Send expense notification emails
        EmailSvc->>Participants: Email: "You owe $40.00 for Team Dinner"
        
        NotiSvc->>NotiSvc: Create in-app notifications
        NotiSvc->>Participants: Push notification: "New shared expense"
    end
    
    %% Payment Flow (Subsequent Actions)
    Participants->>Gateway: POST /api/money/shared-expenses/{id}/pay
    Note right of Participants: {amount: 40.00, paymentMethod: "jar_necessity"}
    
    Gateway->>MoneyAPI: Process payment
    MoneyAPI->>SharedSvc: ProcessPaymentAsync(expenseId, userId, amount)
    SharedSvc->>MoneyDB: UPDATE expense_share SET status = 'paid', paid_at = NOW()
    SharedSvc->>RabbitMQ: Publish ExpensePaymentReceived event
    
    alt All Participants Paid
        SharedSvc->>RabbitMQ: Publish SharedExpenseCompleted event
        RabbitMQ->>NotiSvc: Send completion notification
        NotiSvc->>Creator: "All participants have paid for Team Dinner"
    end
```

### 6.7 Financial Goal Achievement Tracking Flow

```mermaid
sequenceDiagram
    participant Client as Mobile App
    participant Gateway as Ocelot Gateway
    participant PlanAPI as PlanningInvestment.Api
    participant GoalSvc as GoalService
    participant TxSvc as TransactionService
    participant AnalyticsSvc as AnalyticsService
    participant RabbitMQ as Message Bus
    participant PlanDB as Planning Database
    participant NotificationSvc as NotificationService

    Note over Client,NotificationSvc: Goal Creation, Progress Tracking & Achievement Recognition

    Client->>Gateway: POST /api/planning/goals
    Note right of Client: {name: "Emergency Fund", targetAmount: 10000.00, deadline: "2025-12-31", category: "savings", priority: "high"}
    
    Gateway->>PlanAPI: Forward goal creation
    PlanAPI->>GoalSvc: CreateGoalAsync(request, userId)
    
    GoalSvc->>GoalSvc: Validate goal parameters (realistic timeline, amount)
    GoalSvc->>PlanDB: Check for conflicting goals
    PlanDB-->>GoalSvc: No conflicts found
    
    GoalSvc->>GoalSvc: Calculate milestone targets
    Note right of GoalSvc: 25%: $2,500, 50%: $5,000, 75%: $7,500, 100%: $10,000
    
    GoalSvc->>PlanDB: INSERT financial_goal + INSERT goal_milestones
    GoalSvc->>RabbitMQ: Publish GoalCreated event
    Note right of RabbitMQ: {goalId, userId, targetAmount, deadline, milestones[]}
    
    GoalSvc-->>PlanAPI: Goal created successfully
    PlanAPI-->>Gateway: {goalId: "goal_12345", milestones: [...]}
    Gateway-->>Client: Goal tracking initiated
    
    %% Progress Tracking (Triggered by Transactions)
    Note over TxSvc,NotificationSvc: Automatic Progress Updates via Transaction Events
    
    TxSvc->>RabbitMQ: Publish TransactionCreated event
    Note right of RabbitMQ: {transactionId, accountId, amount: 500.00, category: "savings", userId}
    
    RabbitMQ->>GoalSvc: Process savings transaction
    GoalSvc->>PlanDB: Get active goals for user + category
    PlanDB-->>GoalSvc: Emergency Fund goal (current: $2,200, target: $10,000)
    
    GoalSvc->>GoalSvc: Update goal progress
    Note right of GoalSvc: New progress: $2,700 (27% complete)
    
    GoalSvc->>PlanDB: UPDATE goal_progress SET current_amount = 2700, updated_at = NOW()
    
    alt Milestone Achieved (25% reached)
        GoalSvc->>PlanDB: UPDATE goal_milestones SET achieved = true, achieved_at = NOW()
        GoalSvc->>RabbitMQ: Publish GoalMilestoneAchieved event
        RabbitMQ->>NotificationSvc: Send congratulations
        NotificationSvc->>Client: "🎉 You've reached 25% of your Emergency Fund goal!"
        
        GoalSvc->>AnalyticsSvc: Track milestone achievement
        AnalyticsSvc->>AnalyticsSvc: Update user success metrics
    end
    
    GoalSvc->>RabbitMQ: Publish GoalProgressUpdated event
    RabbitMQ->>Client: Real-time progress update in dashboard
    
    %% Goal Completion Flow
    alt Goal Fully Achieved (100%)
        GoalSvc->>RabbitMQ: Publish GoalCompleted event
        RabbitMQ->>NotificationSvc: Send achievement notification
        NotificationSvc->>Client: "🏆 Congratulations! You've completed your Emergency Fund goal!"
        
        GoalSvc->>GoalSvc: Suggest next financial goal
        GoalSvc->>RabbitMQ: Publish GoalSuggestion event
        RabbitMQ->>Client: "Ready for your next challenge? Consider starting an Investment Goal!"
    end
```

### 6.8 Recurring Transaction Automation Flow

```mermaid
sequenceDiagram
    participant Scheduler as Background Scheduler
    participant RecTxWorker as RecurringTx.Worker
    participant RecTxSvc as RecurringTransactionService
    participant TxSvc as TransactionService
    participant ValidatorSvc as ValidationService
    participant RabbitMQ as Message Bus
    participant PlanDB as Planning Database
    participant CoreDB as Core Database
    participant NotificationSvc as NotificationService

    Note over Scheduler,NotificationSvc: Automated Recurring Transaction Processing

    Scheduler->>RecTxWorker: Daily cron trigger (00:00 UTC)
    RecTxWorker->>RecTxSvc: ProcessDueTransactionsAsync()
    
    RecTxSvc->>PlanDB: SELECT recurring_transactions WHERE next_execution <= NOW() AND active = true
    PlanDB-->>RecTxSvc: List of due transactions (rent, subscriptions, savings transfers)
    
    loop For each due recurring transaction
        RecTxSvc->>RecTxSvc: Check business rules
        Note right of RecTxSvc: Validate account status, sufficient funds, execution window
        
        RecTxSvc->>ValidatorSvc: ValidateRecurringExecution(recurringTx)
        ValidatorSvc->>CoreDB: Check source account balance
        CoreDB-->>ValidatorSvc: Account balance: $2,500
        ValidatorSvc->>ValidatorSvc: Validate sufficient funds for $1,200 rent payment
        
        alt Validation Successful
            ValidatorSvc-->>RecTxSvc: Validation passed
            RecTxSvc->>TxSvc: CreateTransactionAsync(transaction)
            TxSvc->>CoreDB: Execute transaction (rent payment)
            TxSvc->>RabbitMQ: Publish TransactionCreated event
            
            RecTxSvc->>RecTxSvc: Calculate next execution date
            Note right of RecTxSvc: Next monthly rent: 2025-07-01
            
            RecTxSvc->>PlanDB: UPDATE recurring_transactions SET next_execution = ?, last_execution = NOW()
            RecTxSvc->>RabbitMQ: Publish RecurringTransactionExecuted event
            
        else Validation Failed (Insufficient Funds)
            ValidatorSvc-->>RecTxSvc: Insufficient funds
            RecTxSvc->>PlanDB: UPDATE recurring_transactions SET failed_attempts = failed_attempts + 1
            RecTxSvc->>RabbitMQ: Publish RecurringTransactionFailed event
            
            alt Max Retries Reached (3 attempts)
                RecTxSvc->>PlanDB: UPDATE recurring_transactions SET status = 'suspended'
                RecTxSvc->>RabbitMQ: Publish RecurringTransactionSuspended event
                RabbitMQ->>NotificationSvc: Send urgent notification
                NotificationSvc->>NotificationSvc: "⚠️ Recurring payment suspended: Rent - Insufficient funds"
            else Retry Later
                RecTxSvc->>RecTxSvc: Schedule retry in 24 hours
                RecTxSvc->>PlanDB: UPDATE next_execution = NOW() + INTERVAL '1 day'
                RabbitMQ->>NotificationSvc: Send retry notification
                NotificationSvc->>NotificationSvc: "Recurring payment failed, will retry tomorrow: Rent"
            end
        end
    end
    
    RecTxSvc->>RabbitMQ: Publish RecurringTransactionBatchProcessed event
    Note right of RabbitMQ: {processedCount: 15, successCount: 14, failedCount: 1, suspendedCount: 0}
    
    RabbitMQ->>NotificationSvc: Send daily summary (if any issues)
    NotificationSvc->>NotificationSvc: "Daily recurring transactions processed: 14 successful, 1 retry scheduled"
```

### 6.9 Real-time Analytics & Dashboard Updates Flow

```mermaid
sequenceDiagram
    participant Client as Dashboard UI
    participant Gateway as Ocelot Gateway
    participant ReportAPI as Reporting.Api
    participant AnalyticsSvc as AnalyticsService
    participant CacheSvc as CacheService
    participant RabbitMQ as Message Bus
    participant FinanceDB as Finance Database
    participant MoneyDB as Money Database
    participant PlanDB as Planning Database
    participant ReportDB as Reporting Database
    participant Redis as Redis Cache

    Note over Client,Redis: Real-time Dashboard with Cross-Service Data Aggregation

    Client->>Gateway: GET /api/reporting/dashboard?period=last_30_days
    Gateway->>ReportAPI: Forward dashboard request
    ReportAPI->>AnalyticsSvc: GenerateDashboardAsync(userId, period)
    
    AnalyticsSvc->>CacheSvc: Check cached dashboard data
    CacheSvc->>Redis: GET dashboard:user123:30days
    Redis-->>CacheSvc: Cache miss (data expired)
    CacheSvc-->>AnalyticsSvc: No cached data available
    
    %% Multi-Database Aggregation
    par Parallel Data Collection
        AnalyticsSvc->>FinanceDB: Get transaction summary (income, expenses, balance)
        FinanceDB-->>AnalyticsSvc: Transaction data (last 30 days)
    and
        AnalyticsSvc->>MoneyDB: Get budget utilization + jar balances
        MoneyDB-->>AnalyticsSvc: Budget & jar data
    and
        AnalyticsSvc->>PlanDB: Get goal progress + recurring transaction status
        PlanDB-->>AnalyticsSvc: Goals & automation data
    and
        AnalyticsSvc->>ReportDB: Get cached analytics + trends
        ReportDB-->>AnalyticsSvc: Historical trends & insights
    end
    
    AnalyticsSvc->>AnalyticsSvc: Calculate key metrics
    Note right of AnalyticsSvc: Income: $4,500, Expenses: $3,200<br/>Savings Rate: 28.9%, Budget Utilization: 78%<br/>Active Goals: 3, On-track: 2
    
    AnalyticsSvc->>AnalyticsSvc: Generate insights & recommendations
    Note right of AnalyticsSvc: "You're $200 under budget this month!"<br/>"Emergency Fund goal is 15% ahead of schedule"
    
    AnalyticsSvc->>CacheSvc: Cache dashboard data (TTL: 15 minutes)
    CacheSvc->>Redis: SET dashboard:user123:30days {dashboard_data}
    
    AnalyticsSvc-->>ReportAPI: Dashboard response
    ReportAPI-->>Gateway: Complete dashboard data
    Gateway-->>Client: {metrics, charts, insights, recommendations}
    
    %% Real-time Updates via WebSocket/SSE
    Note over RabbitMQ,Client: Live Updates for Dynamic Dashboard
    
    RabbitMQ->>AnalyticsSvc: New TransactionCreated event
    AnalyticsSvc->>AnalyticsSvc: Update real-time metrics
    AnalyticsSvc->>Redis: Update cached data incrementally
    AnalyticsSvc->>Client: Send WebSocket update
    Note right of Client: Live balance update: $1,850 → $1,800
    
    RabbitMQ->>AnalyticsSvc: BudgetExceeded event
    AnalyticsSvc->>Client: Send real-time alert
    Note right of Client: "⚠️ Food budget exceeded this month"
```

### 6.10 Cross-Service Communication - Complete Transaction Lifecycle

```mermaid
sequenceDiagram
    participant Client as Mobile App
    participant Gateway as Ocelot Gateway
    participant CoreAPI as CoreFinance.Api
    participant TxSvc as TransactionService
    participant RabbitMQ as Message Bus
    participant MoneyWorker as Money.Worker
    participant PlanWorker as Plan.Worker
    participant ReportWorker as Report.Worker
    participant NotificationSvc as Notification.Service
    participant MoneyDB as Money Database
    participant PlanDB as Planning Database
    participant ReportDB as Reporting Database

    Note over Client,ReportDB: End-to-End Transaction Processing with Cross-Service Event Propagation

    Client->>Gateway: POST /api/finance/transactions
    Note right of Client: Expense: $75.00 for "Restaurant" (Food & Dining category)
    
    Gateway->>CoreAPI: Forward transaction request
    CoreAPI->>TxSvc: CreateTransactionAsync(transaction, userId)
    TxSvc->>TxSvc: Validate & save transaction
    TxSvc->>RabbitMQ: Publish TransactionCreated event
    Note right of RabbitMQ: Event: {txId, userId, amount: -75.00, category: "food_dining", timestamp}
    
    TxSvc-->>CoreAPI: Transaction successful
    CoreAPI-->>Gateway: {transactionId, newBalance}
    Gateway-->>Client: Transaction confirmed
    
    %% Parallel Event Processing Across Multiple Domains
    par Budget Impact Processing (Money Domain)
        RabbitMQ->>MoneyWorker: Consume TransactionCreated
        MoneyWorker->>MoneyDB: Get user's Food & Dining budget
        MoneyDB-->>MoneyWorker: Budget: $300/month, Current spent: $225
        MoneyWorker->>MoneyWorker: Update budget: $225 + $75 = $300 (100% utilized)
        MoneyWorker->>MoneyDB: UPDATE budget_tracking SET spent_amount = 300
        MoneyWorker->>RabbitMQ: Publish BudgetExceeded event
        Note right of RabbitMQ: Alert: Food budget reached 100%
        
    and Goal Impact Processing (Planning Domain)
        RabbitMQ->>PlanWorker: Consume TransactionCreated
        PlanWorker->>PlanDB: Get active savings goals
        PlanDB-->>PlanWorker: Emergency Fund: $4,200/$10,000 (42%)
        PlanWorker->>PlanWorker: Analyze spending impact on savings rate
        PlanWorker->>PlanDB: UPDATE goal_analytics SET projected_completion = recalculated_date
        PlanWorker->>RabbitMQ: Publish GoalProgressImpacted event
        Note right of RabbitMQ: Update: Emergency Fund goal timeline affected
        
    and Analytics Processing (Reporting Domain)
        RabbitMQ->>ReportWorker: Consume TransactionCreated
        ReportWorker->>ReportDB: Update real-time spending metrics
        ReportWorker->>ReportWorker: Recalculate category trends & insights
        ReportWorker->>ReportDB: INSERT daily_spending_summary (if new day)
        ReportWorker->>RabbitMQ: Publish AnalyticsUpdated event
        Note right of RabbitMQ: Update: Real-time dashboard metrics
    end
    
    %% Notification & Alert Processing
    RabbitMQ->>NotificationSvc: Consume BudgetExceeded event
    NotificationSvc->>NotificationSvc: Generate budget alert
    NotificationSvc->>Client: Push notification: "🚨 Food budget fully utilized this month"
    
    RabbitMQ->>NotificationSvc: Consume GoalProgressImpacted event
    NotificationSvc->>NotificationSvc: Generate goal insight
    NotificationSvc->>Client: In-app notification: "💡 Tip: Reduce dining out by $100/month to reach Emergency Fund 2 months earlier"
    
    %% Real-time Dashboard Updates
    RabbitMQ->>ReportWorker: Consume multiple events (budget, goal, analytics)
    ReportWorker->>ReportWorker: Aggregate cross-domain insights
    ReportWorker->>Client: WebSocket update: Dashboard refresh with new metrics
    Note right of Client: Live updates: Balance, Budget %, Goal timeline
```

## 7. Error Handling & Resilience Patterns

### 7.1 Circuit Breaker & Retry Pattern Implementation

```mermaid
flowchart TB
  subgraph "Client Request Flow"
    Client[Client Application]
    Gateway[Ocelot Gateway]
  end

  subgraph "Service Resilience Layer"
    subgraph "Circuit Breaker States"
      Closed[Closed State<br/>Normal Operation<br/>Success Rate > 80%]
      Open[Open State<br/>Failing Fast<br/>Failure Rate > 50%]
      HalfOpen[Half-Open State<br/>Testing Recovery<br/>Limited Requests]
    end
    
    subgraph "Retry Mechanisms"
      ExponentialBackoff[Exponential Backoff<br/>1s, 2s, 4s, 8s]
      MaxRetries[Max Retries: 3<br/>Then Circuit Break]
      JitterStrategy[Jitter Strategy<br/>Avoid Thundering Herd]
    end
  end

  subgraph "Service Mesh"
    ServiceA[Identity.Api<br/>:5228]
    ServiceB[CoreFinance.Api<br/>:5001]
    ServiceC[MoneyManagement.Api<br/>:5002]
  end

  subgraph "Fallback Strategies"
    CacheLayer[Redis Cache<br/>Stale Data Fallback]
    DefaultResponse[Default Response<br/>Empty/Cached Data]
    QueueRequest[Queue for Retry<br/>Async Processing]
  end

  subgraph "Monitoring & Alerting"
    HealthChecks[Health Check Endpoints<br/>/health]
    Metrics[Prometheus Metrics<br/>Success/Failure Rates]
    AlertManager[Alert Manager<br/>PagerDuty Integration]
  end

  %% Normal Flow
  Client --> Gateway
  Gateway --> Closed
  Closed --> ServiceA
  Closed --> ServiceB
  Closed --> ServiceC

  %% Failure Detection
  ServiceA -.->|Failures| Open
  ServiceB -.->|Failures| Open
  ServiceC -.->|Failures| Open

  %% Circuit Breaker States
  Closed -->|Failure Rate > 50%| Open
  Open -->|After Timeout| HalfOpen
  HalfOpen -->|Success| Closed
  HalfOpen -->|Failure| Open

  %% Retry Logic
  Open --> ExponentialBackoff
  ExponentialBackoff --> MaxRetries
  MaxRetries --> JitterStrategy

  %% Fallback Strategies
  Open --> CacheLayer
  Open --> DefaultResponse
  Open --> QueueRequest

  %% Monitoring
  ServiceA --> HealthChecks
  ServiceB --> HealthChecks
  ServiceC --> HealthChecks
  HealthChecks --> Metrics
  Metrics --> AlertManager

  %% Styling
  classDef normalState fill:#e8f5e8
  classDef failureState fill:#ffebee
  classDef recoveryState fill:#fff3e0
  classDef fallbackStyle fill:#f3e5f5

  class Closed,ServiceA,ServiceB,ServiceC normalState
  class Open,MaxRetries failureState
  class HalfOpen,ExponentialBackoff recoveryState
  class CacheLayer,DefaultResponse,QueueRequest fallbackStyle
```

### 7.2 Data Consistency & Transaction Management

```mermaid
flowchart TB
  subgraph "Transaction Patterns"
    subgraph "ACID Transactions (Single Service)"
      LocalTx[Local Database Transaction<br/>BEGIN -> COMMIT/ROLLBACK]
      IsolationLevel[Read Committed<br/>Consistent Reads]
    end
    
    subgraph "Distributed Transactions (Cross-Service)"
      SagaPattern[Saga Pattern<br/>Compensating Actions]
      EventSourcing[Event Sourcing<br/>Append-Only Events]
      CQRS[CQRS Pattern<br/>Command/Query Separation]
    end
  end

  subgraph "Consistency Levels"
    StrongConsistency[Strong Consistency<br/>Financial Transactions<br/>Account Balances]
    EventualConsistency[Eventual Consistency<br/>Analytics & Reporting<br/>Non-Critical Data]
    CausalConsistency[Causal Consistency<br/>User Actions<br/>Related Events]
  end

  subgraph "Compensation Strategies"
    subgraph "Saga Compensations"
      TransactionCompensation[Transaction Rollback<br/>Reverse Amount]
      BudgetCompensation[Budget Adjustment<br/>Restore Allocation]
      NotificationCompensation[Cancel Notifications<br/>Send Reversal Alert]
    end
  end

  subgraph "Event Store & Processing"
    EventStore[(Event Store<br/>Immutable Log)]
    EventProcessor[Event Processor<br/>Guaranteed Delivery]
    DeadLetterQueue[Dead Letter Queue<br/>Failed Events]
  end

  subgraph "Monitoring & Reconciliation"
    ConsistencyChecks[Daily Consistency Checks<br/>Cross-Service Validation]
    ReconciliationJob[Reconciliation Jobs<br/>Data Drift Detection]
    AuditTrail[Audit Trail<br/>All State Changes]
  end

  %% Flow Connections
  LocalTx --> StrongConsistency
  SagaPattern --> EventualConsistency
  EventSourcing --> CausalConsistency
  
  SagaPattern --> TransactionCompensation
  SagaPattern --> BudgetCompensation
  SagaPattern --> NotificationCompensation
  
  EventSourcing --> EventStore
  EventStore --> EventProcessor
  EventProcessor --> DeadLetterQueue
  
  StrongConsistency --> ConsistencyChecks
  EventualConsistency --> ReconciliationJob
  CausalConsistency --> AuditTrail

  %% Styling
  classDef strongConsistency fill:#e8f5e8
  classDef eventualConsistency fill:#fff3e0
  classDef compensationStyle fill:#ffebee
  classDef monitoringStyle fill:#f3e5f5

  class LocalTx,StrongConsistency,TransactionCompensation strongConsistency
  class SagaPattern,EventualConsistency,EventSourcing eventualConsistency
  class BudgetCompensation,NotificationCompensation,DeadLetterQueue compensationStyle
  class ConsistencyChecks,ReconciliationJob,AuditTrail monitoringStyle
```

## 8. Security Architecture & Data Flow

```mermaid
flowchart TB
  subgraph "Security Layers"
    subgraph "Perimeter Security"
      WAF[Web Application Firewall<br/>DDoS Protection<br/>Rate Limiting]
      LoadBalancer[Load Balancer<br/>SSL Termination<br/>Health Checks]
    end
    
    subgraph "Application Security"
      APIGateway[API Gateway<br/>Authentication<br/>Authorization<br/>Request Validation]
      ServiceMesh[Service Mesh<br/>mTLS Communication<br/>Traffic Encryption]
    end
    
    subgraph "Data Security"
      Encryption[Data Encryption<br/>AES-256 at Rest<br/>TLS 1.3 in Transit]
      KeyManagement[Key Management<br/>Azure Key Vault<br/>Key Rotation]
      DataMasking[Data Masking<br/>PII Protection<br/>Audit Logging]
    end
  end

  subgraph "Identity & Access Control"
    subgraph "Authentication Methods"
      OAuth2[OAuth2/OIDC<br/>Web Applications]
      JWT[JWT Tokens<br/>Mobile/API Access]
      ApiKeys[API Keys<br/>System Integration]
      MFA[Multi-Factor Auth<br/>TOTP/SMS]
    end
    
    subgraph "Authorization Patterns"
      RBAC[Role-Based Access<br/>Admin/User/ReadOnly]
      ABAC[Attribute-Based Access<br/>Resource-Level Control]
      ScopeBasedAuth[Scope-Based Auth<br/>API Permissions]
    end
  end

  subgraph "Compliance & Audit"
    subgraph "Financial Compliance"
      PCI_DSS[PCI DSS<br/>Payment Card Security]
      GDPR[GDPR<br/>Data Privacy Rights]
      SOX[SOX Compliance<br/>Financial Reporting]
    end
    
    subgraph "Security Monitoring"
      SIEM[SIEM System<br/>Security Analytics]
      ThreatDetection[Threat Detection<br/>Anomaly Monitoring]
      IncidentResponse[Incident Response<br/>Automated Remediation]
    end
  end

  subgraph "Secure Communication Flow"
    Client[Client Apps<br/>Web/Mobile]
    Gateway[Ocelot Gateway<br/>TLS 1.3]
    Services[Microservices<br/>mTLS]
    Databases[Databases<br/>Encrypted Storage]
  end

  %% Security Flow
  Client --> WAF
  WAF --> LoadBalancer
  LoadBalancer --> APIGateway
  APIGateway --> ServiceMesh
  ServiceMesh --> Services
  Services --> Databases

  %% Authentication Flow
  Client --> OAuth2
  Client --> JWT
  Client --> ApiKeys
  OAuth2 --> MFA
  JWT --> RBAC
  ApiKeys --> ScopeBasedAuth

  %% Data Protection
  Services --> Encryption
  Encryption --> KeyManagement
  KeyManagement --> DataMasking

  %% Compliance Monitoring
  Services --> SIEM
  SIEM --> ThreatDetection
  ThreatDetection --> IncidentResponse
  
  PCI_DSS --> SIEM
  GDPR --> DataMasking
  SOX --> IncidentResponse

  %% Styling
  classDef securityLayer fill:#ffebee
  classDef authLayer fill:#e8f5e8
  classDef complianceLayer fill:#fff3e0
  classDef dataFlow fill:#f3e5f5

  class WAF,LoadBalancer,APIGateway,ServiceMesh securityLayer
  class OAuth2,JWT,ApiKeys,MFA,RBAC,ABAC authLayer
  class PCI_DSS,GDPR,SOX,SIEM complianceLayer
  class Client,Gateway,Services,Databases dataFlow
```