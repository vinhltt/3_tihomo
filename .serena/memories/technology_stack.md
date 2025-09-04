# Technology Stack & Architecture Patterns

## Backend Technologies (.NET 9)
- **Framework**: ASP.NET Core 9.0 with Clean Architecture
- **ORM**: Entity Framework Core with PostgreSQL
- **Testing**: xUnit + FluentAssertions + Bogus (fake data)
- **Validation**: FluentValidation for request DTOs
- **Mapping**: AutoMapper for object mapping
- **Authentication**: JWT + OAuth 2.0/OIDC + API Keys
- **Resilience**: Polly v8 for circuit breaker, retry, timeout patterns
- **Observability**: OpenTelemetry, Prometheus metrics, Serilog
- **API Documentation**: Swagger/OpenAPI with comprehensive documentation

## Frontend Technologies (Nuxt 3)
- **Framework**: Nuxt 3 with Vue.js 3 Composition API
- **Language**: TypeScript (prefer types over interfaces, avoid enums)
- **Styling**: Tailwind CSS with VRISTO admin template
- **State Management**: Pinia for application state
- **UI Components**: Comprehensive component library with dark mode support
- **Syntax**: Exclusively use `<script setup>` composition API
- **Authentication**: vue3-google-signin for social login integration
- **API Integration**: Custom composables for API communication

## Architecture Patterns
- **Clean Architecture**: Domain → Application → Infrastructure → API layers
- **Domain-Driven Design**: Bounded contexts with clear separation
- **Repository Pattern**: Base repositories with Unit of Work
- **CQRS-like Pattern**: Separate DTOs for create/update/view operations
- **Microservices**: Independent services with API Gateway
- **Event-Driven**: Potential for future message queue integration

## Database Design
- **PostgreSQL**: Primary database with EF Core
- **Naming Convention**: snake_case with EFCore.NamingConventions
- **Migrations**: EF Core migrations for schema management
- **Audit Fields**: Created/Updated timestamps, CreatedBy/UpdatedBy tracking