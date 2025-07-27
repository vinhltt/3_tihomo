@echo off
setlocal enabledelayedexpansion

REM ================================
REM TiHoMo Frontend Deployment Fix Script (Windows)
REM ================================

echo 🔧 [FIX] Starting TiHoMo Frontend Deployment Fix...

REM Navigate to project root
cd /d "%~dp0"

REM Configuration
set "DEPLOY_DIR=%CD%"
set "ENV_FILE=%DEPLOY_DIR%\.env"

for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "BACKUP_DIR=%DEPLOY_DIR%\backup_%dt:~0,8%_%dt:~8,6%"

echo [INFO] Working directory: %DEPLOY_DIR%

REM Create backup
echo [BACKUP] Creating backup of current configuration...
if not exist "%BACKUP_DIR%" mkdir "%BACKUP_DIR%"
if exist "%ENV_FILE%" (
    copy "%ENV_FILE%" "%BACKUP_DIR%\.env.backup" >nul
    echo [OK] Backup created at: %BACKUP_DIR%\.env.backup
)

REM Step 1: Create comprehensive .env file
echo [STEP 1] Creating comprehensive .env file...

(
echo # ================================
echo # CORS Configuration
echo # ================================
echo CORS_POLICY_NAME=DefaultCorsPolicy
echo CORS_ALLOWED_ORIGINS=*
echo CORS_ALLOWED_METHODS=*
echo CORS_ALLOWED_HEADERS=*
echo CORS_EXPOSED_HEADERS=Token-Expired
echo CORS_PREFLIGHT_MAX_AGE=10
echo.
echo # ================================
echo # Environment Configuration
echo # ================================
echo ASPNETCORE_ENVIRONMENT=Production
echo.
echo # ================================
echo # Database Configuration
echo # ================================
echo # Identity Database
echo IDENTITY_DB_USERNAME=identity_user
echo IDENTITY_DB_PASSWORD=TiHoMo2024!DB
echo IDENTITY_DB_PORT=5431
echo.
echo # CoreFinance Database
echo COREFINANCE_DB_USERNAME=corefinance_user
echo COREFINANCE_DB_PASSWORD=TiHoMo2024!DB
echo COREFINANCE_DB_PORT=5432
echo.
echo # MoneyManagement Database
echo MONEYMANAGEMENT_DB_USERNAME=money_user  
echo MONEYMANAGEMENT_DB_PASSWORD=TiHoMo2024!DB
echo MONEYMANAGEMENT_DB_PORT=5433
echo.
echo # PlanningInvestment Database
echo PLANNINGINVESTMENT_DB_USERNAME=planning_user
echo PLANNINGINVESTMENT_DB_PASSWORD=TiHoMo2024!DB
echo PLANNINGINVESTMENT_DB_PORT=5434
echo.
echo # Reporting Database
echo REPORTING_DB_USERNAME=reporting_user
echo REPORTING_DB_PASSWORD=TiHoMo2024!DB
echo REPORTING_DB_PORT=5435
echo.
echo # ================================
echo # Authentication Configuration
echo # ================================
echo JWT_SECRET_KEY=TiHoMo-Super-Secret-JWT-Key-2024-Production-Environment-Must-Be-At-Least-64-Characters
echo JWT_ISSUER=TiHoMo
echo JWT_AUDIENCE_IDENTITY_API=TiHoMo.Identity.Api
echo JWT_AUDIENCE_COREFINANCE_API=TiHoMo.CoreFinance.Api
echo JWT_AUDIENCE_EXCEL_API=TiHoMo.Excel.Api
echo JWT_AUDIENCE_FRONTEND=TiHoMo.Frontend
echo.
echo # ================================
echo # External Services Configuration
echo # ================================
echo # Redis
echo REDIS_PASSWORD=TiHoMo2024!Redis
echo.
echo # RabbitMQ
echo RABBITMQ_PASSWORD=TiHoMo2024!Rabbit
echo.
echo # ================================
echo # API Ports Configuration
echo # ================================
echo API_GATEWAY_PORT=8080
echo IDENTITY_API_PORT=5217
echo COREFINANCE_API_PORT=7293
echo EXCEL_API_PORT=5219
echo FRONTEND_PORT=3500
echo.
echo # ================================
echo # Docker Build Configuration
echo # ================================
echo # Nuxt Build Target: production for deployment
echo NUXT_BUILD_TARGET=production
echo # Node Environment: production for deployment
echo NODE_ENV=production
echo # Docker User: 1001:1001 for production security
echo DOCKER_USER=1001:1001
echo # Production-specific settings
echo NUXT_DEV_SSR=false
echo NUXT_DEV_TOOLS=false
echo NUXT_DEBUG=false
echo.
echo # ================================
echo # Frontend Configuration
echo # ================================
echo FRONTEND_BASE_URL=http://localhost:3500
echo APP_PUBLIC_GOOGLE_CLIENT_ID=your-google-client-id
echo.
echo # ================================
echo # Infrastructure Configuration
echo # ================================
echo # pgAdmin
echo PGADMIN_PASSWORD=TiHoMo2024!pgAdmin
echo PGADMIN_PORT=8080
echo.
echo # Loki
echo LOKI_PORT=3100
echo.
echo # Nginx
echo NGINX_HTTP_PORT=80
echo NGINX_HTTPS_PORT=443
echo.
echo # ================================
echo # Docker Compose Project Name
echo # ================================
echo COMPOSE_PROJECT_NAME=tihomo_production
) > "%ENV_FILE%"

echo [OK] .env file created successfully

REM Step 2: Stop any running frontend containers
echo [STEP 2] Stopping existing frontend containers...

REM Check if docker works without error
docker ps >nul 2>&1
if !errorlevel! equ 0 (
    echo [OK] Docker is available
) else (
    echo [ERROR] Docker is not available or not running
    pause
    exit /b 1
)

REM Stop frontend containers
docker compose ps frontend-nuxt 2>nul | findstr /C:"Up" >nul
if !errorlevel! equ 0 (
    echo [INFO] Stopping running frontend container...
    docker compose stop frontend-nuxt
    timeout /t 3 /nobreak >nul
)

REM Remove stopped containers
echo [INFO] Removing stopped containers...
docker compose rm -f frontend-nuxt 2>nul

REM Step 3: Clean up Docker environment
echo [STEP 3] Cleaning up Docker environment...
docker system prune -f --volumes 2>nul

REM Step 4: Rebuild frontend image
echo [STEP 4] Rebuilding frontend image...
docker compose build --no-cache frontend-nuxt

if !errorlevel! neq 0 (
    echo [ERROR] Frontend image build failed
    echo [DEBUG] Checking build context...
    
    echo [DEBUG] Current directory contents:
    dir
    
    echo [DEBUG] Frontend directory structure:
    if exist "src\fe\nuxt" (
        dir "src\fe\nuxt"
        if exist "src\fe\nuxt\Dockerfile" (
            echo [DEBUG] Dockerfile exists: YES
        ) else (
            echo [DEBUG] Dockerfile exists: NO
        )
        if exist "src\fe\nuxt\package.json" (
            echo [DEBUG] Package.json exists: YES
        ) else (
            echo [DEBUG] Package.json exists: NO
        )
    ) else (
        echo [ERROR] src\fe\nuxt directory not found!
    )
    
    pause
    exit /b 1
)

echo [OK] Frontend image built successfully

REM Step 5: Start frontend service
echo [STEP 5] Starting frontend service...
docker compose up -d --no-deps --force-recreate frontend-nuxt

REM Step 6: Wait for frontend to be ready
echo [STEP 6] Waiting for frontend to be ready...
set /a max_attempts=30
set /a attempt=1

:wait_loop
if !attempt! gtr !max_attempts! goto :failed_start

docker compose ps frontend-nuxt 2>nul | findstr /C:"Up" >nul
if !errorlevel! equ 0 (
    echo [OK] Frontend container is running
    
    REM Wait for Nuxt to initialize
    echo [INFO] Waiting for Nuxt application to initialize...
    timeout /t 15 /nobreak >nul
    
    REM Test if frontend is responding
    docker compose exec -T frontend-nuxt curl -f http://localhost:3000/ >nul 2>&1
    if !errorlevel! equ 0 (
        echo [SUCCESS] Frontend is responding to HTTP requests
        goto :health_check
    ) else (
        echo [WAIT] Frontend not ready yet (attempt !attempt!/!max_attempts!)
        if !attempt! equ 10 (
            echo [DEBUG] Frontend logs at attempt !attempt!:
            docker compose logs --tail=10 frontend-nuxt
        )
        if !attempt! equ 20 (
            echo [DEBUG] Frontend logs at attempt !attempt!:
            docker compose logs --tail=10 frontend-nuxt
        )
        timeout /t 10 /nobreak >nul
        set /a attempt=!attempt!+1
        goto :wait_loop
    )
) else (
    echo [WAIT] Frontend container not up yet (attempt !attempt!/!max_attempts!)
    timeout /t 5 /nobreak >nul
    set /a attempt=!attempt!+1
    goto :wait_loop
)

:failed_start
echo [ERROR] Frontend failed to start after !max_attempts! attempts
echo [DEBUG] Frontend logs:
docker compose logs --tail=50 frontend-nuxt
echo [DEBUG] Container status:
docker compose ps frontend-nuxt
pause
exit /b 1

:health_check
REM Step 7: Final health check
echo [STEP 7] Final health check...

REM Check container status
docker compose ps frontend-nuxt 2>nul | findstr /C:"Up" >nul
if !errorlevel! equ 0 (
    echo [OK] Frontend container is running
    
    REM Check HTTP response
    docker compose exec -T frontend-nuxt curl -f http://localhost:3000/ >nul 2>&1
    if !errorlevel! equ 0 (
        echo [OK] Frontend is serving HTTP requests
        
        REM Check if API gateway is accessible
        docker compose exec -T frontend-nuxt curl -f http://ocelot-gateway:8080/health >nul 2>&1
        if !errorlevel! equ 0 (
            echo [OK] Frontend can connect to backend gateway
        ) else (
            echo [WARNING] Frontend cannot connect to backend gateway
        )
        
        echo [SUCCESS] ✅ Frontend deployment completed successfully!
    ) else (
        echo [ERROR] Frontend HTTP check failed
        docker compose logs --tail=20 frontend-nuxt
        pause
        exit /b 1
    )
) else (
    echo [ERROR] Frontend container is not running
    docker compose ps frontend-nuxt
    pause
    exit /b 1
)

REM Step 8: Show final status
echo [FINAL STATUS] System overview:
docker compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"

echo.
echo ==================================
echo 🎉 FRONTEND DEPLOYMENT SUCCESS! 🎉
echo ==================================
echo.
echo 📊 Deployment Summary:
echo    • Frontend URL: http://localhost:3500
echo    • Environment: Production
echo    • Status: ✅ Running
echo    • Health Check: ✅ Passed
echo.
echo 💡 Next Steps:
echo    • Access frontend at: http://localhost:3500
echo    • Monitor logs: docker compose logs -f frontend-nuxt
echo    • Check status: docker compose ps
echo.
echo ✅ Frontend is ready for use!

pause
