#!/bin/bash

echo "Setting up TiHoMo development configuration files..."

# Tạo thư mục config
mkdir -p config/nginx/conf.d
mkdir -p config/grafana/provisioning/{dashboards,datasources}
mkdir -p config/grafana/dashboards
mkdir -p config/pgadmin
mkdir -p scripts

echo "📁 Created config directories"

# Tạo Redis config
cat > config/redis.conf << EOF
# Redis configuration for TiHoMo
bind 0.0.0.0
port 6379
requirepass redis123
maxmemory 256mb
maxmemory-policy allkeys-lru
save 900 1
save 300 10
save 60 10000
appendonly yes
appendfsync everysec
EOF

echo "✅ Created Redis config"

# Tạo RabbitMQ config
cat > config/rabbitmq.conf << EOF
# RabbitMQ configuration for TiHoMo
default_user = tihomo
default_pass = tihomo123
default_vhost = /
management.tcp.port = 15672
listeners.tcp.default = 5672
log.file.level = info
EOF

echo "✅ Created RabbitMQ config"

# Tạo Prometheus config
cat > config/prometheus.yml << EOF
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - "alert-rules.yml"

scrape_configs:
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']

  - job_name: 'tihomo-gateway'
    static_configs:
      - targets: ['host.docker.internal:5000']
    scrape_interval: 30s
    metrics_path: '/metrics'

  - job_name: 'tihomo-identity'
    static_configs:
      - targets: ['host.docker.internal:5001']
    scrape_interval: 30s
    metrics_path: '/metrics'

  - job_name: 'tihomo-corefinance'
    static_configs:
      - targets: ['host.docker.internal:5002']
    scrape_interval: 30s
    metrics_path: '/metrics'

  - job_name: 'tihomo-moneymanagement'
    static_configs:
      - targets: ['host.docker.internal:5003']
    scrape_interval: 30s
    metrics_path: '/metrics'

  - job_name: 'tihomo-planninginvestment'
    static_configs:
      - targets: ['host.docker.internal:5004']
    scrape_interval: 30s
    metrics_path: '/metrics'

  - job_name: 'tihomo-excelapi'
    static_configs:
      - targets: ['host.docker.internal:5005']
    scrape_interval: 30s
    metrics_path: '/metrics'
EOF

echo "✅ Created Prometheus config"

# Tạo Loki config
cat > config/loki.yml << EOF
auth_enabled: false

server:
  http_listen_port: 3100
  grpc_listen_port: 9096

common:
  path_prefix: /loki
  storage:
    filesystem:
      chunks_directory: /loki/chunks
      rules_directory: /loki/rules
  replication_factor: 1
  ring:
    instance_addr: 127.0.0.1
    kvstore:
      store: inmemory

query_scheduler:
  max_outstanding_requests_per_tenant: 32768

schema_config:
  configs:
    - from: 2020-10-24
      store: boltdb-shipper
      object_store: filesystem
      schema: v11
      index:
        prefix: index_
        period: 24h

ruler:
  alertmanager_url: http://localhost:9093

analytics:
  reporting_enabled: false
EOF

echo "✅ Created Loki config"

# Tạo Grafana datasource config
cat > config/grafana/provisioning/datasources/datasources.yml << EOF
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://prometheus:9090
    isDefault: true
    
  - name: Loki
    type: loki
    access: proxy
    url: http://loki:3100
EOF

echo "✅ Created Grafana datasource config"

# Tạo Grafana dashboard config
cat > config/grafana/provisioning/dashboards/dashboards.yml << EOF
apiVersion: 1

providers:
  - name: 'TiHoMo Dashboards'
    orgId: 1
    folder: ''
    type: file
    disableDeletion: false
    updateIntervalSeconds: 10
    allowUiUpdates: true
    options:
      path: /var/lib/grafana/dashboards
EOF

echo "✅ Created Grafana dashboard config"

# Tạo pgAdmin servers config
cat > config/pgadmin/servers.json << EOF
{
  "Servers": {
    "1": {
      "Name": "TiHoMo Identity",
      "Group": "TiHoMo Services",
      "Host": "identity-postgres",
      "Port": 5432,
      "MaintenanceDB": "identity",
      "Username": "identity_user",
      "SSLMode": "prefer",
      "SSLCert": "<STORAGE_DIR>/.postgresql/postgresql.crt",
      "SSLKey": "<STORAGE_DIR>/.postgresql/postgresql.key",
      "SSLCompression": 0,
      "Timeout": 10,
      "UseSSHTunnel": 0,
      "TunnelPort": "22",
      "TunnelAuthentication": 0
    },
    "2": {
      "Name": "TiHoMo CoreFinance",
      "Group": "TiHoMo Services",
      "Host": "corefinance-postgres",
      "Port": 5432,
      "MaintenanceDB": "corefinance",
      "Username": "corefinance_user",
      "SSLMode": "prefer"
    },
    "3": {
      "Name": "TiHoMo MoneyManagement",
      "Group": "TiHoMo Services",
      "Host": "moneymanagement-postgres",
      "Port": 5432,
      "MaintenanceDB": "db_money",
      "Username": "money_user",
      "SSLMode": "prefer"
    },
    "4": {
      "Name": "TiHoMo PlanningInvestment",
      "Group": "TiHoMo Services",
      "Host": "planninginvestment-postgres",
      "Port": 5432,
      "MaintenanceDB": "db_planning",
      "Username": "planning_user",
      "SSLMode": "prefer"
    },
    "5": {
      "Name": "TiHoMo Reporting",
      "Group": "TiHoMo Services",
      "Host": "reporting-postgres",
      "Port": 5432,
      "MaintenanceDB": "db_reporting",
      "Username": "reporting_user",
      "SSLMode": "prefer"
    }
  }
}
EOF

echo "✅ Created pgAdmin servers config"

# Tạo Nginx config
cat > config/nginx/nginx.conf << EOF
events {
    worker_connections 1024;
}

http {
    upstream grafana {
        server grafana:3000;
    }
    
    upstream rabbitmq {
        server rabbitmq:15672;
    }
    
    server {
        listen 80;
        server_name localhost;
        
        location /grafana/ {
            proxy_pass http://grafana/;
            proxy_set_header Host \$host;
            proxy_set_header X-Real-IP \$remote_addr;
            proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto \$scheme;
        }
        
        location /rabbitmq/ {
            proxy_pass http://rabbitmq/;
            proxy_set_header Host \$host;
            proxy_set_header X-Real-IP \$remote_addr;
            proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto \$scheme;
        }
        
        location / {
            return 200 'TiHoMo Development Environment';
            add_header Content-Type text/plain;
        }
    }
}
EOF

echo "✅ Created Nginx config"

# Tạo database init scripts
cat > scripts/init-identity-db.sql << EOF
-- TiHoMo Identity Database Initialization
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Users table
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Roles table
CREATE TABLE IF NOT EXISTS roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- User roles junction table
CREATE TABLE IF NOT EXISTS user_roles (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    role_id UUID REFERENCES roles(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);

-- Insert default roles
INSERT INTO roles (name, description) VALUES 
('Admin', 'System Administrator'),
('User', 'Regular User')
ON CONFLICT (name) DO NOTHING;

GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO identity_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO identity_user;
EOF

echo "✅ Created Identity database init script"

cat > scripts/init-corefinance-db.sql << EOF
-- TiHoMo CoreFinance Database Initialization
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Accounts table
CREATE TABLE IF NOT EXISTS accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    account_type VARCHAR(50) NOT NULL,
    balance DECIMAL(18,2) DEFAULT 0,
    currency VARCHAR(3) DEFAULT 'VND',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Transactions table
CREATE TABLE IF NOT EXISTS transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL,
    account_id UUID REFERENCES accounts(id),
    amount DECIMAL(18,2) NOT NULL,
    transaction_type VARCHAR(50) NOT NULL,
    category VARCHAR(100),
    description TEXT,
    transaction_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Categories table
CREATE TABLE IF NOT EXISTS categories (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL, -- income, expense
    color VARCHAR(7), -- hex color
    icon VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO corefinance_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO corefinance_user;
EOF

echo "✅ Created CoreFinance database init script"

# Tạo script cho các database khác
for service in moneymanagement planninginvestment reporting; do
    cat > scripts/init-${service}-db.sql << EOF
-- TiHoMo ${service^} Database Initialization
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Service specific tables will be created by Entity Framework migrations
-- This script just ensures the database is ready

CREATE TABLE IF NOT EXISTS _migration_history (
    id SERIAL PRIMARY KEY,
    service_name VARCHAR(100) NOT NULL,
    migration_name VARCHAR(255) NOT NULL,
    applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO _migration_history (service_name, migration_name) 
VALUES ('${service}', 'initial_setup')
ON CONFLICT DO NOTHING;
EOF
done

echo "✅ Created additional database init scripts"

echo
echo "🎉 Configuration setup completed!"
echo
echo "Next steps:"
echo "1. Run: docker-compose -f docker-compose.yml up -d"
echo "2. Access services at the URLs provided in DEVELOPMENT_SETUP.md"
echo "3. Start your API services using Visual Studio or dotnet CLI"
