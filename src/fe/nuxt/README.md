# CoreFinance Frontend (Nuxt 3)

## Overview

Frontend-only Nuxt 3 application for CoreFinance Personal Finance Management system. This application connects to the .NET Core API backend for all data operations.

## Architecture

- **Frontend**: Nuxt 3 + Vue 3 + TypeScript
- **Backend**: .NET Core API (localhost:7293)
- **UI Framework**: VRISTO Admin Template + Tailwind CSS
- **State Management**: Pinia
- **Charts**: ApexCharts
- **Icons**: Custom icon components
- **Internationalization**: @nuxtjs/i18n

## Setup

1. **Install dependencies**:
   ```bash
   npm install
   ```

2. **Environment Configuration**:
   - Default API endpoint: `https://localhost:7293`
   - To change, set environment variable: `API_BASE_URL=your_api_url`

3. **Start development server**:
   ```bash
   npm run dev
   ```

4. **Access application**:
   - Frontend: http://localhost:3500
   - Backend API should be running on: https://localhost:7293

## Project Structure

```
src/FE/nuxt/
├── components/           # Vue components
│   ├── layout/          # Layout components (Header, Sidebar, Footer)
│   ├── icon/            # Icon components
│   └── AccountModal.vue # Account management modal
├── composables/         # Composable functions
│   ├── useApi.ts        # Generic API client
│   └── useAccounts.ts   # Account-specific API calls
├── pages/               # Application pages
│   └── apps/
│       └── accounts/    # Account management pages
├── types/               # TypeScript type definitions
│   ├── account.ts       # Account-related types
│   ├── api.ts           # API-related types
│   └── index.ts         # Type exports
├── stores/              # Pinia stores
├── assets/              # Static assets
└── nuxt.config.ts       # Nuxt configuration
```

## Features Implemented

### Account Management
- **List View** (`/apps/accounts`):
  - Paginated account list with filtering
  - Search by name, filter by type and currency
  - CRUD operations (Create, Read, Update, Delete)
  - Responsive data table with sorting

- **Detail View** (`/apps/accounts/[id]`):
  - Account overview with balance cards
  - Balance history chart (ApexCharts)
  - Recent transactions table
  - Edit/Delete actions

- **Account Modal**:
  - Create/Edit form with validation
  - Dynamic fields based on account type
  - Currency selection with symbols
  - Card number formatting and masking

### API Integration
- **Centralized API Client** (`useApi`):
  - Error handling with typed responses
  - Automatic base URL configuration
  - HTTP methods: GET, POST, PUT, DELETE

- **Account API** (`useAccounts`):
  - Type-safe API calls
  - Utility functions for formatting
  - Form validation helpers

### UI/UX Features
- **VRISTO Theme Integration**:
  - Dark/Light mode support
  - RTL language support
  - Responsive design (mobile-first)
  - Consistent panel layouts

- **Navigation**:
  - Sidebar menu with Account link
  - Breadcrumb navigation
  - Mobile-friendly collapsible menu

- **Data Visualization**:
  - ApexCharts for balance history
  - Loading states and error handling
  - Client-only rendering for SSR compatibility

## API Endpoints

All API calls are made to the .NET Core backend:

```typescript
// Account endpoints
GET    /api/account/{id}           // Get account by ID
POST   /api/account               // Create new account
PUT    /api/account/{id}          // Update account
DELETE /api/account/{id}          // Delete account (soft delete)
POST   /api/account/filter        // Get paginated accounts with filters
```

## Type Safety

Comprehensive TypeScript interfaces:

```typescript
// Account types
type AccountType = 'Bank' | 'Wallet' | 'CreditCard' | 'DebitCard' | 'Cash'

interface Account {
  id: string
  name?: string
  type: AccountType
  currency?: string
  currentBalance: number
  // ... other properties
}

// API types
interface ApiResponse<T> {
  result: T[]
  pagination: Pagination
}
```

## Development Guidelines

1. **Code Organization**:
   - Types in `types/` directory
   - Composables for reusable logic
   - Components follow PascalCase naming

2. **API Calls**:
   - Use `useApi()` for all HTTP requests
   - Handle errors gracefully with user feedback
   - Implement loading states

3. **Styling**:
   - Use Tailwind CSS classes
   - Follow VRISTO theme patterns
   - Ensure mobile responsiveness

4. **State Management**:
   - Use Pinia for global state
   - Leverage `useAppStore` for UI settings

## Next Steps

1. **CORS Configuration**: Ensure .NET API allows requests from localhost:3500
2. **Authentication**: Implement user authentication flow
3. **Error Boundaries**: Add global error handling
4. **Testing**: Add unit tests for composables and components
5. **Performance**: Implement lazy loading and caching strategies

## Troubleshooting

### Common Issues

1. **API Connection Failed**:
   - Ensure .NET API is running on localhost:7293
   - Check CORS configuration in .NET API
   - Verify `API_BASE_URL` environment variable

2. **TypeScript Errors**:
   - Run `npm run build` to check for type errors
   - Ensure all imports use correct paths (`~/types`, `~/composables`)

3. **Auto-imports Not Working**:
   - Restart development server
   - Check `nuxt.config.ts` imports configuration

### Development Commands

```bash
# Development
npm run dev              # Start dev server
npm run build            # Build for production
npm run preview          # Preview production build
npm run generate         # Generate static site

# Type checking
npm run typecheck        # Run TypeScript type checking
```
