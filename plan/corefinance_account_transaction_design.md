# Core Finance – Account & Transaction Design

## 1. Business Overview
- **Account**: Represents financial accounts (bank, wallet, credit card, debit card, cash, etc.), used to classify and manage transactions.
- **Transaction**: Represents each transaction on accounts, including income, expense, transfer, refund, etc.

---

## 2. Data Model Design

### 2.1. Account Table
| Field            | Type         | Description                        |
|------------------|--------------|------------------------------------|
| AccountId        | UUID (PK)    | Primary key                        |
| UserId           | UUID (FK)    | Account owner                      |
| AccountName      | string       | Account name (e.g., Techcombank VISA)|
| AccountType      | enum         | Type (Bank, Wallet, CreditCard, DebitCard, Cash, etc.)|
| CardNumber       | string       | Card number (if any)               |
| Currency         | string       | Currency (VND, USD, ...)           |
| InitialBalance   | decimal      | Initial balance                    |
| CurrentBalance   | decimal      | Current balance                    |
| AvailableLimit   | decimal      | Available limit (credit card)      |
| CreatedAt        | datetime     | Created date                       |
| UpdatedAt        | datetime     | Last updated                       |
| IsActive         | bool         | Active flag                        |

### 2.2. Transaction Table
| Field                | Type         | Description                                  |
|----------------------|--------------|----------------------------------------------|
| TransactionId        | UUID (PK)    | Primary key                                   |
| AccountId            | UUID (FK)    | Linked to Account                             |
| UserId               | UUID (FK)    | Transaction owner                             |
| TransactionDate      | datetime     | Transaction date                              |
| RevenueAmount        | decimal      | Income amount                                 |
| SpentAmount          | decimal      | Expense amount                                |
| Description          | string       | Transaction description                       |
| Balance              | decimal      | Balance after transaction                     |
| BalanceCompare       | decimal      | Balance compared to app notification          |
| AvailableLimit       | decimal      | Available limit (credit card)                 |
| AvailableLimitCompare| decimal      | Available limit compared to app (credit card) |
| TransactionCode      | string       | Transaction code                              |
| SyncMisa             | bool         | Synced with MISA                              |
| SyncSms              | bool         | Synced with SMS                               |
| Vn                   | bool         | Mark for VN transaction                       |
| CategorySummary      | string       | Category summary                              |
| Note                 | string       | Note                                          |
| ImportFrom           | string       | Import source (file, sms, api, ...)           |
| IncreaseCreditLimit  | decimal      | Credit limit increase (credit card)           |
| UsedPercent          | decimal      | Used percent (credit card)                    |
| CategoryType         | enum         | Transaction type (Income, Expense, Transfer, Fee, ...) |
| Group                | string       | Group (statement period, batch import, ...)   |
| CreatedAt            | datetime     | Created date                                  |
| UpdatedAt            | datetime     | Last updated                                  |

---

## 3. Entity & DTO (C#)

### 3.1. Account Entity
```csharp
public enum AccountType
{
    Bank,
    Wallet,
    CreditCard,
    DebitCard,
    Cash
}

public class Account
{
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public string AccountName { get; set; }
    public AccountType AccountType { get; set; }
    public string CardNumber { get; set; }
    public string Currency { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal? AvailableLimit { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}
```

### 3.2. Transaction Entity
```csharp
public enum CategoryType
{
    Income,
    Expense,
    Transfer,
    Fee,
    Other
}

public class Transaction
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal RevenueAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public string Description { get; set; }
    public decimal Balance { get; set; }
    public decimal? BalanceCompare { get; set; }
    public decimal? AvailableLimit { get; set; }
    public decimal? AvailableLimitCompare { get; set; }
    public string TransactionCode { get; set; }
    public bool SyncMisa { get; set; }
    public bool SyncSms { get; set; }
    public bool Vn { get; set; }
    public string CategorySummary { get; set; }
    public string Note { get; set; }
    public string ImportFrom { get; set; }
    public decimal? IncreaseCreditLimit { get; set; }
    public decimal? UsedPercent { get; set; }
    public CategoryType CategoryType { get; set; }
    public string Group { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Account Account { get; set; }
}
```

---

## 4. Database Migration (EF Core)
- Use code-first migration to create Account and Transaction tables with foreign key relationship.
- Use EFCore.NamingConventions for snake_case mapping in PostgreSQL.

---

## 5. API Design (RESTful)
### 5.1. Account API
- `GET /api/accounts` – List accounts
- `GET /api/accounts/{id}` – Account details
- `POST /api/accounts` – Create account
- `PUT /api/accounts/{id}` – Update account
- `DELETE /api/accounts/{id}` – Delete account

### 5.2. Transaction API
- `GET /api/core-finance/transaction` – List transactions (filter by account, date, type, ...)
- `GET /api/core-finance/transaction/{id}` – Transaction details
- `POST /api/core-finance/transaction` – Create transaction
- `PUT /api/core-finance/transaction/{id}` – Update transaction
- `DELETE /api/core-finance/transaction/{id}` – Delete transaction

---

## 6. Development Notes
- Apply FluentValidation for DTOs.
- Use AutoMapper for mapping between Entity and DTO.
- Standardize input/output data.
- Add sample migration for PostgreSQL.
- Write unit tests for service and controller.

---

*This design is ready for .NET + PostgreSQL development and can be extended as needed.* 