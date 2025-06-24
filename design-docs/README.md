# TiHoMo Design Documentation

This directory contains the comprehensive design documentation for the TiHoMo (Personal Financial Management) system, structured according to the guidelines defined in `main.instructions.md`.

## 📁 Directory Structure

### 01-overview/
High-level system overview and architecture documents
- `system-overview-v4.md` - Complete system architecture overview

### 02-business-design/
Business requirements and domain analysis
- `business-requirements-analysis.md` - Comprehensive business requirements

### 03-architecture/
Technical architecture and infrastructure design
- `api-gateway/` - API Gateway (Ocelot) design documents
- `database-design/` - Database schemas and design patterns
- `microservices/` - Individual service architecture documents

### 04-api-design/
API specifications and standards
- `api-standards.md` - RESTful API design guidelines
- `endpoints/` - Detailed API endpoint specifications
- `data-models/` - API data models and contracts
- `authentication/` - Authentication and authorization patterns

### 05-frontend-design/
Frontend architecture and UI/UX specifications
- `ui-ux-design/design-system.md` - Complete design system specification
- `component-library/` - Reusable component documentation

### 06-backend-design/
Backend service architecture and patterns
- `service-architecture/corefinance-service.md` - Core Finance service design
- `data-layer/` - Data access patterns and repository designs
- `business-logic/` - Domain logic and business rules

### 07-features/
Feature-specific design documents
- Individual feature designs and specifications

### 08-deployment/
Deployment and infrastructure documentation
- `environments/` - Environment-specific configurations

### 09-templates/
Reusable templates and patterns
- Standard templates for new features and services

## 🔄 Migration from Old Structure

This new structure replaces the previous organization:

**Old Structure:**
```
design/
├── architech_design/
├── ba_design/
└── screens_design/
```

**New Structure (Current):**
```
design-docs/
├── 01-overview/
├── 02-business-design/
├── 03-architecture/
├── 04-api-design/
├── 05-frontend-design/
├── 06-backend-design/
├── 07-features/
├── 08-deployment/
└── 09-templates/
```

## 📋 Documentation Standards

### Documentation-First Development Protocol

Following the guidelines in `main.instructions.md`, all development follows this sequence:

1. **Check Memory Bank** (`memory-bank/activeContext.md`)
2. **Check Feature Documentation** (`design-docs/07-features/feature-[name]/`)
3. **Check Architecture Constraints** (`design-docs/03-architecture/`)
4. **Check API/UI Standards** (`design-docs/04-api-design/` or `design-docs/05-frontend-design/`)
5. **Check MCP Memory** for recent insights
6. **Apply Sequential Thinking** if complex problem

### File Naming Conventions

- Use kebab-case for file names: `system-overview-v4.md`
- Include version numbers for major revisions: `v4`, `v2.1`
- Use descriptive names that clearly indicate content
- Group related documents in subdirectories

### Content Standards

- **Bilingual Support**: Comments in code should be English/Vietnamese
- **Memory Bank Integration**: All major decisions should be documented in memory bank
- **Cross-References**: Link between related documents
- **Version Control**: Track major changes and decisions

## 🏗️ Architecture Compliance

### Backend (.NET 9)
- **Reference Documents**: `/design-docs/06-backend-design/` and `/design-docs/04-api-design/`
- **Technologies**: .NET 9, ASP.NET Core, Entity Framework Core, PostgreSQL, xUnit, FluentAssertions
- **Patterns**: Clean Architecture, Domain-Driven Design
- **Key Practices**: XML comments with bilingual format, PostgreSQL with snake_case naming

### Frontend (Nuxt 3)
- **Reference Documents**: `/design-docs/05-frontend-design/` and component library
- **Technologies**: Nuxt 3, Vue 3, TypeScript, Tailwind CSS, Pinia, VRISTO Admin Template
- **Patterns**: Composition API with `<script setup>`, mobile-first responsive design
- **Key Practices**: PascalCase for components, camelCase for composables, types over interfaces

## 📊 Current System Status

### ✅ Completed Components
- **Identity Service**: Advanced implementation with resilience and observability
- **Core Finance**: Account and transaction management
- **Money Management**: Budget and jar services
- **API Gateway**: Ocelot configuration with routing and security

### 🚧 In Progress
- **Message Queue Implementation**: ExcelAPI → RabbitMQ → CoreFinance flow
- **Logging and Monitoring**: Grafana + Loki + Prometheus stack
- **Frontend Integration**: Authentication and API routing fixes

### 📋 Planned Features
- **Planning Investment**: Debt, goal, and investment services
- **Reporting Integration**: Advanced analytics and notifications
- **Mobile Application**: React Native or Flutter implementation

## 🔗 Related Documents

- **Memory Bank**: `/memory-bank/` - Session-based context and insights
- **Project Plans**: `/plan/` - Implementation timelines and strategies
- **Source Code**: `/src/` - Backend and frontend implementation
- **Configuration**: `/config/` - Infrastructure and service configurations

## 📞 Contact & Contribution

This documentation system is maintained according to the comprehensive memory and documentation system defined in `main.instructions.md`. All updates should follow the established patterns and be synchronized across the four integrated layers:

1. **Memory Bank** (Session persistence)
2. **Design Documentation** (Project structure) - This directory
3. **MCP Memory** (Dynamic context)
4. **Sequential Thinking** (Problem-solving)

For questions or contributions, ensure all changes are documented in the appropriate memory systems and follow the established development protocols.

---

## 📦 Migration Status

### ✅ Completed Migrations (June 2025)
All major design documents from the old `design/architech_design/detail_service_design/` structure have been successfully migrated to the new `design-docs/` hierarchy:

- `overview_v4.md` → `01-overview/system-overview-v4.md`
- `ba_design_v1.md` → `02-business-design/business-requirements-analysis.md`
- `apigateway_design.md` → `03-architecture/api-gateway/ocelot-gateway-design.md`
- `corefinance_design.md` → `06-backend-design/service-architecture/corefinance-service.md`
- `moneymanagement_design.md` → `06-backend-design/service-architecture/moneymanagement-service.md`
- `planninginvestment_design.md` → `06-backend-design/service-architecture/planninginvestment-service.md`
- `reportingintegration_design.md` → `06-backend-design/service-architecture/reportingintegration-service.md`
- `identity/` subdirectory → `06-backend-design/service-architecture/identity/`

### ✅ Migration Complete
The migration to the new Documentation-First Development Protocol structure is now **100% complete**. All services now follow the standardized hierarchy and comply with the folder-specific rules defined in `main.instructions.md`.

---

*Last Updated: June 24, 2025*
*System Version: TiHoMo v4*
