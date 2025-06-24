# System Architecture Flowcharts v4

## Overview
This document contains the detailed flowcharts for TiHoMo (Tiny House Money) personal finance management system architecture version 4.

## 1. High-Level System Architecture

```mermaid
graph TB
    UI[Frontend - Nuxt.js] --> GW[API Gateway - Ocelot]
    
    GW --> ID[Identity Service]
    GW --> CF[CoreFinance Service]
    GW --> MM[MoneyManagement Service]
    GW --> PI[PlanningInvestment Service]
    GW --> RI[ReportingIntegration Service]
    GW --> EA[ExcelApi Service]
    
    ID --> IDB[(Identity DB)]
    CF --> CFDB[(CoreFinance DB)]
    MM --> MMDB[(MoneyManagement DB)]
    PI --> PIDB[(PlanningInvestment DB)]
    RI --> RIDB[(Reporting DB)]
    
    CF --> MQ[RabbitMQ]
    MM --> MQ
    PI --> MQ
    RI --> MQ
    EA --> MQ
    
    MQ --> LOG[Loki Logging]
    CF --> PROM[Prometheus Metrics]
    MM --> PROM
    PI --> PROM
    RI --> PROM
    
    PROM --> GRAF[Grafana Dashboard]
    LOG --> GRAF
```

## 2. Authentication & Authorization Flow

```mermaid
sequenceDiagram
    participant U as User
    participant FE as Frontend
    participant GW as API Gateway
    participant ID as Identity Service
    participant S as Services
    
    U->>FE: Login Request
    FE->>GW: Auth Request
    GW->>ID: Validate Credentials
    ID->>ID: Check User & Password
    ID->>GW: JWT Token
    GW->>FE: JWT Token
    FE->>U: Login Success
    
    U->>FE: API Request
    FE->>GW: Request + JWT
    GW->>GW: Validate JWT
    GW->>S: Forward Request
    S->>GW: Response
    GW->>FE: Response
    FE->>U: Display Data
```

## 3. Account Management Flow

```mermaid
flowchart TD
    A[User Creates Account] --> B{Account Type?}
    B -->|Savings| C[Create Savings Account]
    B -->|Checking| D[Create Checking Account]
    B -->|Investment| E[Create Investment Account]
    B -->|Credit| F[Create Credit Account]
    
    C --> G[Store in CoreFinance DB]
    D --> G
    E --> H[Store in PlanningInvestment DB]
    F --> G
    
    G --> I[Publish Account Created Event]
    H --> I
    I --> J[RabbitMQ Message Queue]
    J --> K[Update MoneyManagement Cache]
    J --> L[Log to Loki]
    J --> M[Update Metrics in Prometheus]
```

## 4. Transaction Processing Flow

```mermaid
flowchart TD
    A[Transaction Input] --> B{Input Source?}
    B -->|Manual Entry| C[Frontend Form]
    B -->|Excel Import| D[ExcelApi Service]
    B -->|Recurring| E[Scheduled Job]
    
    C --> F[Validate Transaction Data]
    D --> F
    E --> F
    
    F --> G{Validation OK?}
    G -->|No| H[Return Error]
    G -->|Yes| I[Store Transaction]
    
    I --> J{Account Type?}
    J -->|Regular| K[CoreFinance DB]
    J -->|Investment| L[PlanningInvestment DB]
    
    K --> M[Update Account Balance]
    L --> N[Update Investment Portfolio]
    
    M --> O[Publish Transaction Event]
    N --> O
    
    O --> P[RabbitMQ Queue]
    P --> Q[Update MoneyManagement Summary]
    P --> R[Generate Notifications]
    P --> S[Log Transaction]
    P --> T[Update Metrics]
```

## 5. Excel Import Processing Flow

```mermaid
sequenceDiagram
    participant U as User
    participant FE as Frontend
    participant EA as ExcelApi
    participant MQ as RabbitMQ
    participant CF as CoreFinance
    participant MM as MoneyManagement
    
    U->>FE: Upload Excel File
    FE->>EA: POST /excel/upload
    EA->>EA: Parse Excel File
    EA->>EA: Validate Data Format
    
    loop For Each Transaction
        EA->>MQ: Publish Transaction Message
        MQ->>CF: Process Transaction
        CF->>CF: Store Transaction
        CF->>MQ: Publish Processed Event
        MQ->>MM: Update Summary
    end
    
    EA->>FE: Import Summary
    FE->>U: Display Results
```

## 6. Reporting & Analytics Flow

```mermaid
flowchart LR
    A[Data Sources] --> B[ReportingIntegration Service]
    
    subgraph A [Data Sources]
        A1[CoreFinance DB]
        A2[MoneyManagement DB]
        A3[PlanningInvestment DB]
    end
    
    B --> C[Data Aggregation]
    C --> D[Report Generation]
    D --> E{Report Type?}
    
    E -->|Financial Summary| F[Monthly/Annual Reports]
    E -->|Investment Analysis| G[Portfolio Performance]
    E -->|Budget Analysis| H[Spending Categories]
    E -->|Custom Reports| I[User-Defined Reports]
    
    F --> J[Store in Reporting DB]
    G --> J
    H --> J
    I --> J
    
    J --> K[Cache Results]
    K --> L[API Response]
    L --> M[Frontend Display]
```

## 7. Message Queue Event Flow

```mermaid
graph TD
    subgraph Publishers
        CF[CoreFinance]
        MM[MoneyManagement]
        PI[PlanningInvestment]
        RI[ReportingIntegration]
        EA[ExcelApi]
    end
    
    subgraph RabbitMQ
        EX1[Account Exchange]
        EX2[Transaction Exchange]
        EX3[Investment Exchange]
        EX4[Notification Exchange]
        
        Q1[Account.Created Queue]
        Q2[Account.Updated Queue]
        Q3[Transaction.Created Queue]
        Q4[Transaction.Updated Queue]
        Q5[Investment.Updated Queue]
        Q6[Notification.Email Queue]
        Q7[Notification.SMS Queue]
    end
    
    subgraph Consumers
        MM2[MoneyManagement Consumer]
        RI2[Reporting Consumer]
        NS[Notification Service]
        LS[Logging Service]
    end
    
    CF --> EX1
    CF --> EX2
    PI --> EX3
    EA --> EX2
    
    EX1 --> Q1
    EX1 --> Q2
    EX2 --> Q3
    EX2 --> Q4
    EX3 --> Q5
    EX4 --> Q6
    EX4 --> Q7
    
    Q1 --> MM2
    Q2 --> MM2
    Q3 --> MM2
    Q3 --> RI2
    Q4 --> RI2
    Q5 --> RI2
    Q6 --> NS
    Q7 --> NS
```

## 8. Monitoring & Observability Flow

```mermaid
flowchart TB
    subgraph Services
        S1[CoreFinance]
        S2[MoneyManagement]
        S3[PlanningInvestment]
        S4[ReportingIntegration]
        S5[ExcelApi]
        S6[Identity]
        S7[API Gateway]
    end
    
    subgraph Logging
        S1 --> L[Loki]
        S2 --> L
        S3 --> L
        S4 --> L
        S5 --> L
        S6 --> L
        S7 --> L
    end
    
    subgraph Metrics
        S1 --> P[Prometheus]
        S2 --> P
        S3 --> P
        S4 --> P
        S5 --> P
        S6 --> P
        S7 --> P
    end
    
    subgraph Visualization
        L --> G[Grafana]
        P --> G
    end
    
    subgraph Alerting
        G --> A1[Performance Alerts]
        G --> A2[Error Rate Alerts]
        G --> A3[Business Metric Alerts]
    end
```

## 9. Development & Deployment Flow

```mermaid
flowchart LR
    A[Developer] --> B[Git Commit]
    B --> C[GitHub Actions]
    C --> D{Tests Pass?}
    D -->|No| E[Build Failed]
    D -->|Yes| F[Build Docker Images]
    F --> G[Push to Registry]
    G --> H[Deploy to Environment]
    
    subgraph Environments
        H1[Development]
        H2[Staging]
        H3[Production]
    end
    
    H --> H1
    H1 --> I{Manual Approval?}
    I -->|Yes| H2
    H2 --> J{Manual Approval?}
    J -->|Yes| H3
    
    H1 --> K[Run Integration Tests]
    H2 --> L[Run E2E Tests]
    H3 --> M[Monitor Production]
```

## 10. Data Synchronization Flow

```mermaid
sequenceDiagram
    participant CF as CoreFinance
    participant MQ as RabbitMQ
    participant MM as MoneyManagement
    participant RI as ReportingIntegration
    participant Cache as Redis Cache
    
    CF->>MQ: Account Balance Updated
    MQ->>MM: Process Balance Update
    MM->>MM: Update Summary Tables
    MM->>Cache: Invalidate Cache
    
    CF->>MQ: Transaction Created
    MQ->>MM: Process Transaction
    MM->>MM: Update Categories Summary
    MQ->>RI: Process for Reporting
    RI->>RI: Update Aggregated Data
    
    MM->>Cache: Store Updated Summary
    RI->>Cache: Store Report Cache
```

## Notes

- All services communicate through the API Gateway for external requests
- Internal service communication happens via RabbitMQ message queues
- Each service has its own database following microservices principles
- Monitoring is implemented using Prometheus for metrics and Loki for logs
- Grafana provides unified dashboards for observability
- Redis is used for caching frequently accessed data
- All flows include proper error handling and retry mechanisms
