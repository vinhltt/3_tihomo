# Project Plan & Timeline – TiHoMo System

## 1. Overview
This document outlines the development plan and weekly timeline for the Personal Finance Management (TiHoMo) system, based on the BA Design document. The plan follows Agile Scrum, with each sprint lasting one week and focusing on a specific functional group.

---

## 2. Weekly Breakdown

| Week | Functional Group           | Key Deliverables                                              |
|------|---------------------------|---------------------------------------------------------------|
| 1    | Core Finance              | Account, Transaction, Statement APIs; Import; History; Categorization |
| 2    | Identity & Access         | Registration, Login, User Management, Role-based Access        |
| 3    | Money Management          | Budget, SixJars Model, Shared Expense                         |
| 4    | Planning & Investment     | Debt, Financial Goals, Investment                             |
| 5    | Reporting & Integration   | Basic & Advanced Reports, Notifications, External Integration  |
| 6    | NFR, Testing, UI/UX, Hardening | Performance, Security, Testing, UI/UX, Release Prep         |

---

## 3. Detailed Weekly Plan

### Week 1: Core Finance
- Build Account, Transaction, Statement APIs (CRUD)
- Implement manual statement import (CSV/Excel upload)
- Basic transaction categorization
- Transaction history view

### Week 2: Identity & Access
- User registration, login, password recovery
- User management (CRUD, lock/unlock)
- Role-based access control (User, Admin)

### Week 3: Money Management
- Budget management (create/edit/delete)
- SixJars model support
- Shared expense/group management

### Week 4: Planning & Investment
- Debt management (loans, repayments, reminders)
- Financial goal management
- Investment tracking (stocks, funds, real estate)
- Recurring transactions management (subscriptions, scheduled payments, future cash flow projection)

### Week 5: Reporting & Integration
- Basic and advanced financial reports
- User notifications (in-app, email, push)
- Prepare for external service integration (Open Banking, Aggregator)

### Week 6: NFR, Testing, UI/UX, Hardening
- Performance, security, backup/restore, logging, monitoring
- Unit, integration, and performance testing
- UI/UX finalization and user experience testing
- Code review, optimization, release preparation

---

## 4. Notes & Recommendations
- Adjust timeline as needed based on team velocity and complexity
- Hold daily standups and weekly reviews to adapt the plan
- Prioritize completion of each functional group before moving on
- Ensure thorough documentation (Memory Bank, API docs, test cases)

---

*This plan is based on the BA Design and may be refined as the project progresses.* 