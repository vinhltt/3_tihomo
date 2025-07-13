#!/bin/bash

echo "==================================="
echo "🚀 Starting TiHoMo Development Environment"
echo "==================================="

# Set required environment variables
export GATEWAY_PORT=5000
export FRONTEND_PORT=3500
export JWT_SECRET_KEY="your-super-secret-key-here-at-least-32-characters-long"

# Set database environment variables  
export IDENTITY_DB_USERNAME=identity_user
export IDENTITY_DB_PASSWORD=Admin@123
export IDENTITY_DB_PORT=5831

export COREFINANCE_DB_USERNAME=corefinance_user  
export COREFINANCE_DB_PASSWORD=Admin@123
export COREFINANCE_DB_PORT=5832

export MONEYMANAGEMENT_DB_USERNAME=money_user
export MONEYMANAGEMENT_DB_PASSWORD=Admin@123
export MONEYMANAGEMENT_DB_PORT=5835

export PLANNINGINVESTMENT_DB_USERNAME=planning_user
export PLANNINGINVESTMENT_DB_PASSWORD=Admin@123
export PLANNINGINVESTMENT_DB_PORT=5836

export REPORTING_DB_PASSWORD=Admin@123
export REPORTING_DB_PORT=5837

# Set other service environment variables
export REDIS_PORT=6379
export REDIS_PASSWORD=Admin@123
export RABBITMQ_PORT=5672
export RABBITMQ_MANAGEMENT_PORT=15672
export RABBITMQ_PASSWORD=Admin@123
export PROMETHEUS_PORT=9090
export GRAFANA_PORT=3002
export GRAFANA_ADMIN_PASSWORD=Admin@123
export LOKI_PORT=3100
export PGADMIN_PORT=8081
export PGADMIN_PASSWORD=Admin@123
export MAILHOG_SMTP_PORT=1025
export MAILHOG_UI_PORT=8025
export NGINX_HTTP_PORT=8082
export NGINX_HTTPS_PORT=8443

# Network configuration
export DOCKER_NETWORK_SUBNET=172.20.0.0/16

# Node environment
export NODE_ENV=development
export ASPNETCORE_ENVIRONMENT=Development

# OAuth configuration
export APP_PUBLIC_GOOGLE_CLIENT_ID=your-google-client-id

echo "📊 Environment Variables Set:"
echo "  - Gateway Port: $GATEWAY_PORT"
echo "  - Frontend Port: $FRONTEND_PORT"
echo "  - JWT Secret: ****"
echo

echo "🐳 Starting Docker Compose..."
docker compose down
docker compose up -d

echo
echo "✅ TiHoMo services started successfully!"
echo
echo "📊 Service URLs:"
echo "  - 🌐 Frontend: http://localhost:$FRONTEND_PORT"
echo "  - 🚪 Gateway: http://localhost:$GATEWAY_PORT"
echo "  - 📊 Grafana: http://localhost:$GRAFANA_PORT (admin/Admin@123)"
echo "  - 🐰 RabbitMQ: http://localhost:$RABBITMQ_MANAGEMENT_PORT (tihomo/Admin@123)"
echo "  - 🗄️ pgAdmin: http://localhost:$PGADMIN_PORT (admin@tihomo.com/Admin@123)"
echo "  - 📧 MailHog: http://localhost:$MAILHOG_UI_PORT"
echo "  - 📈 Prometheus: http://localhost:$PROMETHEUS_PORT"
echo
echo "🔧 For logs: docker compose logs [service-name]"
echo "🛑 To stop: docker compose down" 