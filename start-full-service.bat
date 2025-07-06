@echo off
REM TiHoMo Development Environment Startup Script
REM Script để khởi tạo và chạy toàn bộ development environment

echo 🚀 Starting TiHoMo Development Environment...

REM Check if .env file exists
if not exist .env (
    echo ⚠️  .env file not found. Copying from .env.example...
    copy .env.example .env
    echo 📝 Please edit .env file and fill in the required values before running again.
    echo 💡 Key variables to set:
    echo    - FRONTEND_BASE_URL
    echo    - JWT_SECRET_KEY  
    echo    - Database passwords
    echo    - OAuth credentials (if using)
    pause
    exit /b 1
)

REM Create required directories if they don't exist
echo 📁 Creating required directories...
if not exist logs mkdir logs
if not exist logs\identity mkdir logs\identity
if not exist logs\corefinance mkdir logs\corefinance
if not exist logs\excel mkdir logs\excel
if not exist logs\ocelot mkdir logs\ocelot
if not exist uploads mkdir uploads
if not exist config\grafana\dashboards mkdir config\grafana\dashboards
if not exist config\nginx\conf.d mkdir config\nginx\conf.d
if not exist config\ssl mkdir config\ssl

REM Start all services with proper dependency resolution
echo 🚀 Starting all TiHoMo services...
echo 📋 Docker Compose will handle service dependencies automatically
docker compose up -d

echo.
echo ✅ TiHoMo Development Environment is ready!
echo.
echo 🌐 Access URLs:
echo    Frontend (Nuxt):     http://localhost:%FRONTEND_PORT%
echo    API Gateway:         http://localhost:5800
echo    Identity API:        http://localhost:5801
echo    CoreFinance API:     http://localhost:5802
echo    Excel API:           http://localhost:5805
echo.
echo 🛠️  Development Tools:
echo    Grafana:             http://localhost:3000 (admin/admin123)
echo    Prometheus:          http://localhost:9090
echo    RabbitMQ Management: http://localhost:15672 (tihomo/tihomo123)
echo    pgAdmin:             http://localhost:8080 (admin@tihomo.local/admin123)
echo    Mailhog:             http://localhost:8025
echo.
echo 📊 To check service status: docker compose ps
echo 📝 To view logs: docker compose logs -f [service-name]
echo 🛑 To stop all services: docker compose down

pause
