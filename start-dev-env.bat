@echo off
echo ===================================
echo TiHoMo Development Environment
echo ===================================
echo.

:menu
echo Chọn action:
echo 1. Start tất cả services
echo 2. Start chỉ databases
echo 3. Start chỉ monitoring stack  
echo 4. Start chỉ message queue
echo 5. Stop tất cả services
echo 6. Stop và xóa data (DANGER!)
echo 7. Xem trạng thái services
echo 8. Xem logs
echo 9. Exit
echo.
set /p choice="Nhập lựa chọn (1-9): "

if "%choice%"=="1" goto start_all
if "%choice%"=="2" goto start_db
if "%choice%"=="3" goto start_monitoring
if "%choice%"=="4" goto start_queue
if "%choice%"=="5" goto stop_all
if "%choice%"=="6" goto stop_clean
if "%choice%"=="7" goto status
if "%choice%"=="8" goto logs
if "%choice%"=="9" goto exit
echo Lựa chọn không hợp lệ!
goto menu

:start_all
echo Starting tất cả services...
docker-compose -f docker-compose.yml up -d
echo.
echo ✅ Tất cả services đã được start!
echo.
echo 📊 Service URLs:
echo   - Grafana: http://localhost:3000 (admin/admin123)
echo   - RabbitMQ: http://localhost:15672 (tihomo/tihomo123)
echo   - pgAdmin: http://localhost:8080 (admin@tihomo.local/admin123)
echo   - Mailhog: http://localhost:8025
echo   - Prometheus: http://localhost:9090
echo.
goto menu

:start_db
echo Starting databases...
docker-compose -f docker-compose.yml up -d identity-postgres corefinance-postgres moneymanagement-postgres planninginvestment-postgres reporting-postgres
echo ✅ Databases đã được start!
goto menu

:start_monitoring
echo Starting monitoring stack...
docker-compose -f docker-compose.yml up -d prometheus grafana loki
echo ✅ Monitoring stack đã được start!
goto menu

:start_queue
echo Starting message queue services...
docker-compose -f docker-compose.yml up -d rabbitmq redis
echo ✅ Message queue services đã được start!
goto menu

:stop_all
echo Stopping tất cả services...
docker-compose -f docker-compose.yml down
echo ✅ Tất cả services đã được stop!
goto menu

:stop_clean
echo.
echo ⚠️  CẢNH BÁO: Hành động này sẽ XÓA TẤT CẢ DỮ LIỆU!
set /p confirm="Bạn có chắc chắn? (y/N): "
if /i not "%confirm%"=="y" goto menu
echo Stopping và xóa data...
docker-compose -f docker-compose.yml down -v
echo ✅ Services đã được stop và data đã được xóa!
goto menu

:status
echo Trạng thái services:
docker-compose -f docker-compose.yml ps
echo.
goto menu

:logs
echo.
set /p service="Nhập tên service để xem logs (hoặc Enter để xem tất cả): "
if "%service%"=="" (
    docker-compose -f docker-compose.yml logs --tail=50
) else (
    docker-compose -f docker-compose.yml logs --tail=50 %service%
)
echo.
goto menu

:exit
echo Bye!
pause
