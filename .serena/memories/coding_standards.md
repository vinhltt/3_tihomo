# Coding Standards & Conventions

## Backend Coding Standards (.NET 9)

### Code Organization
- **Clean Architecture**: Follow Domain → Application → Infrastructure → API layer separation
- **Partial Classes**: Use for organizing large test classes by method (e.g., `AccountServiceTests.CreateAsync.cs`)
- **Namespace Conventions**: Follow project structure (e.g., `CoreFinance.Application.Services`)

### Testing Standards (MANDATORY)
- **Testing Framework**: Use xUnit exclusively (no NUnit)
- **Assertions**: Use FluentAssertions for all test assertions
- **Fake Data**: Use Bogus library for generating test data
- **Test Organization**: Organize tests in folders by service name (e.g., `AccountServiceTests/`)
- **Test Helpers**: Extract common test utilities to `TestHelpers.cs`

### Entity Framework & Database
- **Naming Convention**: Use snake_case with EFCore.NamingConventions package
- **Migrations**: Always create migrations with descriptive names
- **Audit Fields**: Include Created/Updated timestamps and user tracking

### API Design
- **RESTful Endpoints**: Follow REST conventions with proper HTTP verbs
- **DTOs**: Separate CreateRequest, UpdateRequest, and ViewModel DTOs
- **Validation**: Use FluentValidation for all request validation
- **Documentation**: Comprehensive XML comments in bilingual format (English/Vietnamese)

## Frontend Coding Standards (Nuxt 3)

### Vue.js Conventions
- **Composition API**: Use `<script setup>` syntax exclusively
- **Component Naming**: PascalCase for components, camelCase for composables
- **File Structure**: Follow Nuxt 3 directory conventions (pages/, components/, composables/)

### TypeScript Standards
- **Types vs Interfaces**: Prefer types over interfaces
- **Enums**: Avoid enums, use union types or const assertions instead
- **API Types**: Generate types from backend OpenAPI specifications

### Styling & UI
- **CSS Framework**: Tailwind CSS with utility-first approach
- **Theme**: VRISTO admin template patterns with dark mode support
- **Responsive Design**: Mobile-first approach with responsive breakpoints
- **Component Library**: Use existing design system components

### State Management
- **Pinia**: Use for application state management
- **Composables**: Create reusable composables for API interactions and business logic
- **Authentication**: Use dedicated auth store with JWT token management

## Code Quality Guidelines

### Documentation Requirements
- **XML Comments**: Bilingual documentation (English/Vietnamese) for all public APIs
- **Code Comments**: Explain complex business logic and algorithmic decisions
- **README Files**: Maintain up-to-date documentation for each service

### Performance Considerations
- **Async/Await**: Use async patterns for all I/O operations
- **Database Queries**: Optimize EF Core queries, avoid N+1 problems
- **Caching**: Implement appropriate caching strategies
- **Resource Management**: Proper disposal of resources and memory management