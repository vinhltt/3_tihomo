@echo off
echo Setting up TiHoMo development configuration files...

REM Tạo thư mục config
mkdir config\nginx\conf.d 2>nul
mkdir config\grafana\provisioning\dashboards 2>nul
mkdir config\grafana\provisioning\datasources 2>nul
mkdir config\grafana\dashboards 2>nul
mkdir config\pgadmin 2>nul
mkdir scripts 2>nul

echo 📁 Created config directories

REM Tạo Redis config
(
echo # Redis configuration for TiHoMo
echo bind 0.0.0.0
echo port 6379
echo requirepass redis123
echo maxmemory 256mb
echo maxmemory-policy allkeys-lru
echo save 900 1
echo save 300 10
echo save 60 10000
echo appendonly yes
echo appendfsync everysec
) > config\redis.conf

echo ✅ Created Redis config

REM Tạo RabbitMQ config
(
echo # RabbitMQ configuration for TiHoMo
echo default_user = tihomo
echo default_pass = tihomo123
echo default_vhost = /
echo management.tcp.port = 15672
echo listeners.tcp.default = 5672
echo log.file.level = info
) > config\rabbitmq.conf

echo ✅ Created RabbitMQ config

REM Tạo Prometheus config
(
echo global:
echo   scrape_interval: 15s
echo   evaluation_interval: 15s
echo.
echo scrape_configs:
echo   - job_name: 'prometheus'
echo     static_configs:
echo       - targets: ['localhost:9090']
echo.
echo   - job_name: 'tihomo-gateway'
echo     static_configs:
echo       - targets: ['host.docker.internal:5000']
echo     scrape_interval: 30s
echo     metrics_path: '/metrics'
echo.
echo   - job_name: 'tihomo-identity'
echo     static_configs:
echo       - targets: ['host.docker.internal:5001']
echo     scrape_interval: 30s
echo     metrics_path: '/metrics'
) > config\prometheus.yml

echo ✅ Created Prometheus config

REM Tạo Loki config
(
echo auth_enabled: false
echo.
echo server:
echo   http_listen_port: 3100
echo   grpc_listen_port: 9096
echo.
echo common:
echo   path_prefix: /loki
echo   storage:
echo     filesystem:
echo       chunks_directory: /loki/chunks
echo       rules_directory: /loki/rules
echo   replication_factor: 1
echo   ring:
echo     instance_addr: 127.0.0.1
echo     kvstore:
echo       store: inmemory
echo.
echo schema_config:
echo   configs:
echo     - from: 2020-10-24
echo       store: boltdb-shipper
echo       object_store: filesystem
echo       schema: v11
echo       index:
echo         prefix: index_
echo         period: 24h
) > config\loki.yml

echo ✅ Created Loki config

REM Tạo Grafana datasource config
(
echo apiVersion: 1
echo.
echo datasources:
echo   - name: Prometheus
echo     type: prometheus
echo     access: proxy
echo     url: http://prometheus:9090
echo     isDefault: true
echo.    
echo   - name: Loki
echo     type: loki
echo     access: proxy
echo     url: http://loki:3100
) > config\grafana\provisioning\datasources\datasources.yml

echo ✅ Created Grafana datasource config

REM Tạo Identity database init script
(
echo -- TiHoMo Identity Database Initialization
echo CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
echo.
echo -- Users table
echo CREATE TABLE IF NOT EXISTS users ^(
echo     id UUID PRIMARY KEY DEFAULT uuid_generate_v4^(^),
echo     email VARCHAR^(255^) UNIQUE NOT NULL,
echo     username VARCHAR^(100^) UNIQUE NOT NULL,
echo     password_hash VARCHAR^(255^) NOT NULL,
echo     is_active BOOLEAN DEFAULT TRUE,
echo     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
echo     updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
echo ^);
echo.
echo -- Insert default admin user
echo INSERT INTO users ^(email, username, password_hash^) VALUES 
echo ^('admin@tihomo.local', 'admin', '$2a$11$dummy.hash.for.development'^)
echo ON CONFLICT ^(email^) DO NOTHING;
echo.
echo GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO identity_user;
echo GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO identity_user;
) > scripts\init-identity-db.sql

echo ✅ Created Identity database init script

REM Tạo CoreFinance database init script
(
echo -- TiHoMo CoreFinance Database Initialization
echo CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
echo.
echo -- Accounts table
echo CREATE TABLE IF NOT EXISTS accounts ^(
echo     id UUID PRIMARY KEY DEFAULT uuid_generate_v4^(^),
echo     user_id UUID NOT NULL,
echo     name VARCHAR^(255^) NOT NULL,
echo     account_type VARCHAR^(50^) NOT NULL,
echo     balance DECIMAL^(18,2^) DEFAULT 0,
echo     currency VARCHAR^(3^) DEFAULT 'VND',
echo     is_active BOOLEAN DEFAULT TRUE,
echo     created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
echo     updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
echo ^);
echo.
echo GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO corefinance_user;
echo GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO corefinance_user;
) > scripts\init-corefinance-db.sql

echo ✅ Created CoreFinance database init script

echo.
echo 🎉 Configuration setup completed!
echo.
echo Next steps:
echo 1. Run: docker-compose -f docker-compose.dev.yml up -d
echo 2. Access services at the URLs provided in DEVELOPMENT_SETUP.md
echo 3. Start your API services using Visual Studio or dotnet CLI
echo.
pause
