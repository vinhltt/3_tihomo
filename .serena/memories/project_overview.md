# TiHoMo - Personal Finance Management System Overview

## Project Purpose
TiHoMo is a comprehensive personal finance management system built with modern web technologies. It provides users with tools to track expenses, manage budgets, plan investments, and monitor their financial health through automated workflows and forecasting capabilities.

## Architecture Overview
- **Microservices Architecture**: Clean separation of bounded contexts
- **Backend**: .NET 9 with Clean Architecture pattern
- **Frontend**: Nuxt 3 (Vue.js) with TypeScript and Tailwind CSS
- **Database**: PostgreSQL with Entity Framework Core (snake_case naming)
- **API Gateway**: Ocelot Gateway for request routing
- **Authentication**: Social login (Google/Facebook) + JWT + API Keys
- **Infrastructure**: Docker Compose, GitHub Actions CI/CD, TrueNAS deployment

## Bounded Contexts
1. **Identity & Access**: ✅ COMPLETED - Authentication, user management, API keys
2. **Core Finance**: ✅ COMPLETED - Accounts, transactions, recurring transactions
3. **Money Management**: ✅ COMPLETED - Budgets, jars (6-jar system), shared expenses
4. **Planning & Investment**: 🚧 IN PROGRESS - Investment tracking, goals, debt management
5. **Reporting & Integration**: 📋 PLANNED - Analytics, reports, external integrations
6. **Excel API**: ✅ COMPLETED - Excel file processing, transaction imports

## Key Features Completed
- Social authentication (Google/Facebook OAuth)
- Account and transaction management
- Recurring transactions with forecasting
- Budget management with 6-jar allocation system
- API key management with enhanced security
- Health monitoring and observability
- Excel import/export capabilities
- Comprehensive CI/CD pipeline with TrueNAS deployment