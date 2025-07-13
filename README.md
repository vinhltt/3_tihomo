# TiHoMo - Personal Finance Management System

TiHoMo is a comprehensive personal finance management system built with modern web technologies. It provides users with tools to track expenses, manage budgets, plan investments, and monitor their financial health.

## Architecture

The system follows a microservices architecture with the following components:

### Backend Services
- **CoreFinance**: Main financial data management service
- **Identity**: Authentication and user management (Social Login)
- **MoneyManagement**: Budget and expense tracking
- **PlanningInvestment**: Investment portfolio management
- **ReportingIntegration**: Financial reports and analytics
- **Ocelot.Gateway**: API Gateway for routing requests

### Frontend
- **Nuxt 3 SPA**: Modern Vue.js application with server-side rendering
- **Social Authentication**: Google and Facebook OAuth integration
- **Responsive Design**: Mobile-first approach with Tailwind CSS

## Features

### Authentication
- ✅ **Social Login**: Google and Facebook OAuth
- ✅ **JWT Token Management**: Secure token-based authentication
- ✅ **Auto Token Refresh**: Seamless session management
- ❌ **SSO Integration**: Removed in favor of social login

### Financial Management
- 📊 **Expense Tracking**: Categorized expense management
- 💰 **Budget Planning**: Set and monitor budgets
- 📈 **Investment Portfolio**: Track investments and performance
- 📋 **Financial Reports**: Comprehensive financial analytics
- 🔄 **Recurring Transactions**: Automated recurring payments

### User Experience
- 📱 **Responsive Design**: Works on all devices
- 🌙 **Dark/Light Mode**: Theme customization
- 🌍 **Multi-language**: Support for multiple languages
- ⚡ **Fast Performance**: Optimized for speed

## Technology Stack

### Backend
- **.NET 8**: Modern C# web framework
- **Entity Framework Core**: Database ORM
- **JWT Authentication**: Secure token-based auth
- **Swagger/OpenAPI**: API documentation
- **Docker**: Containerization support

### Frontend
- **Nuxt 3**: Vue.js meta-framework
- **TypeScript**: Type-safe JavaScript
- **Tailwind CSS**: Utility-first CSS framework
- **Pinia**: State management
- **vue3-google-signin**: Google OAuth integration

### Infrastructure
- **SQL Server**: Primary database
- **Ocelot**: API Gateway
- **Docker Compose**: Local development environment

## Getting Started

### Prerequisites
- Node.js 18+
- .NET 8 SDK
- SQL Server (LocalDB or Docker)

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd 3_tihomo
   ```

2. **Backend Setup**
   ```bash
   cd src/be
   # Restore NuGet packages
   dotnet restore
   
   # Update database
   cd CoreFinance
   dotnet ef database update
   
   cd ../Identity
   dotnet ef database update
   ```

3. **Frontend Setup**
   ```bash
   cd src/fe/nuxt
   npm install
   
   # Copy environment file
   cp .env.example .env
   # Edit .env with your configuration
   ```

4. **Configure Social Login**
   - Get Google Client ID from Google Cloud Console
   - Get Facebook App ID from Facebook Developers
   - Update `.env` file with your credentials

5. **Start Development**
   ```bash
   # Backend (API Gateway)
   cd src/be/Ocelot.Gateway
   dotnet run
   
   # Frontend
   cd src/fe/nuxt
   npm run dev
   ```

## Project Structure

```
3_tihomo/
├── design/                    # Architecture and design documents
│   ├── architech_design/     # Technical architecture
│   ├── ba_design/           # Business analysis
│   └── screens_design/      # UI/UX designs
├── memory-bank/             # Project context and progress
├── plan/                    # Project planning documents
├── src/
│   ├── be/                  # Backend services
│   │   ├── CoreFinance/     # Core financial service
│   │   ├── Identity/        # Authentication service
│   │   ├── MoneyManagement/ # Budget management
│   │   ├── PlanningInvestment/ # Investment planning
│   │   └── Ocelot.Gateway/  # API Gateway
│   └── fe/                  # Frontend application
│       └── nuxt/           # Nuxt 3 application
└── README.md
```

## Development Workflow

### Phase 1: Foundation ✅
- [x] Microservices architecture setup
- [x] Database design and migrations
- [x] API Gateway configuration
- [x] Basic authentication system

### Phase 2: Social Login Integration ✅
- [x] Google OAuth integration
- [x] Facebook OAuth integration
- [x] JWT token management
- [x] Frontend authentication flow
- [x] SSO migration to social login

### Phase 3: Core Features (In Progress)
- [ ] User profile management
- [ ] Transaction management
- [ ] Budget creation and tracking
- [ ] Investment portfolio

### Phase 4: Advanced Features (Planned)
- [ ] Financial reports and analytics
- [ ] Recurring transactions
- [ ] Goal setting and tracking
- [ ] Notifications and alerts

## Configuration

### Environment Variables

**Frontend (.env)**
```env
NUXT_PUBLIC_IDENTITY_API_BASE=https://localhost:5214
APP_PUBLIC_GOOGLE_CLIENT_ID=your-google-client-id
NUXT_PUBLIC_FACEBOOK_APP_ID=your-facebook-app-id
```

**Backend (appsettings.json)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TiHoMo;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-jwt-secret-key",
    "ExpiryMinutes": 60
  },
  "GoogleAuth": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  },
  "FacebookAuth": {
    "AppId": "your-facebook-app-id",
    "AppSecret": "your-facebook-app-secret"
  }
}
```

## API Documentation

The API documentation is available at:
- **API Gateway**: `https://localhost:7293/swagger`
- **Identity Service**: `https://localhost:5214/swagger`
- **CoreFinance Service**: `https://localhost:5001/swagger`

## Testing

### Frontend
```bash
cd src/fe/nuxt
npm run test
```

### Backend
```bash
cd src/be
dotnet test
```

## Contributing

1. Create a feature branch from `main`
2. Make your changes
3. Add tests for new functionality
4. Update documentation
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE.txt file for details.

## Support

For support and questions:
- Check the documentation in the `design/` folder
- Review the memory bank for project context
- Create an issue for bugs or feature requests

## Changelog

### v0.2.0 (Current)
- ✅ Migrated from SSO to social login
- ✅ Google and Facebook OAuth integration
- ✅ Updated frontend authentication flow
- ✅ Cleaned up SSO-related code

### v0.1.0
- ✅ Initial microservices architecture
- ✅ Basic authentication with SSO
- ✅ Database setup and migrations
- ✅ API Gateway configuration
```bash
docker-compose -f docker-compose.yml build --parallel
docker-compose -f docker-compose.yml up -d
```