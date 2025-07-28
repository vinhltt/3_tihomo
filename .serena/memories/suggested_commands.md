# Development Commands & Scripts

## Backend Development Commands (Windows/PowerShell)

### Solution Management
```bash
# Navigate to backend root
cd src/be

# Build entire solution
dotnet build TiHoMo.sln

# Run specific service (from service directory)
cd CoreFinance/CoreFinance.Api
dotnet run

# Run Identity service
cd Identity/Identity.Api  
dotnet run

# Run API Gateway (main entry point)
cd Ocelot.Gateway
dotnet run
```

### Database Management
```bash
# Entity Framework migrations (from service project directory)
dotnet ef migrations add InitialCreate
dotnet ef database update

# Example for CoreFinance
cd src/be/CoreFinance/CoreFinance.Api
dotnet ef database update -c CoreFinanceDbContext
```

### Testing Commands
```bash
# Run all tests in solution
dotnet test TiHoMo.sln

# Run tests for specific project
dotnet test CoreFinance/CoreFinance.Application.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Frontend Development Commands

### Nuxt Development
```bash
# Navigate to frontend
cd src/fe/nuxt

# Install dependencies
npm install

# Development server (runs on http://localhost:3500)
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Generate static site
npm run generate
```

### Docker Commands
```bash
# Build and run entire system
docker-compose up -d

# Build specific service
docker-compose build frontend

# View logs
docker-compose logs -f [service-name]

# Stop services
docker-compose down
```

## Development Workflow Scripts

### Batch Scripts (Windows)
- `start-dev-env.bat` - Start development environment
- `start-full-service.bat` - Start all services
- `rebuild-frontend.bat` - Rebuild frontend container
- `deploy-frontend-fix.bat` - Quick frontend deployment fix

### Shell Scripts (Linux/WSL)
- `start-dev-env.sh` - Start development environment  
- `start-full-service.sh` - Start all services
- `rebuild-frontend.sh` - Rebuild frontend container