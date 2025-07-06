#!/bin/bash

# TiHoMo Development Environment Startup Script
# Script để khởi tạo và chạy toàn bộ development environment

set -e

echo "🚀 Starting TiHoMo Development Environment..."

# Load environment variables if .env exists
if [ -f .env ]; then
    echo "📋 Loading environment variables from .env..."
    set -a  # automatically export all variables
    source .env
    set +a  # disable automatic export
else
    echo "⚠️  .env file not found. Copying from .env.example..."
    cp .env.example .env
    echo "📝 Please edit .env file and fill in the required values before running again."
    echo "💡 Key variables to set:"
    echo "   - FRONTEND_BASE_URL"
    echo "   - JWT_SECRET_KEY"
    echo "   - Database passwords"
    echo "   - OAuth credentials (if using)"
    exit 1
fi

# Check for required environment variables
required_vars=("FRONTEND_BASE_URL" "JWT_SECRET_KEY" "IDENTITY_DB_PASSWORD" "COREFINANCE_DB_PASSWORD")
missing_vars=()

for var in "${required_vars[@]}"; do
    if [ -z "${!var}" ]; then
        missing_vars+=("$var")
    fi
done

if [ ${#missing_vars[@]} -ne 0 ]; then
    echo "❌ Missing required environment variables:"
    printf '   - %s\n' "${missing_vars[@]}"
    echo "📝 Please set these variables in your .env file"
    exit 1
fi

# Create required directories if they don't exist
echo "📁 Creating required directories..."
mkdir -p logs/{identity,corefinance,excel,ocelot}
mkdir -p uploads
mkdir -p config/{grafana/dashboards,nginx/conf.d,ssl}

# Start all services with proper dependency resolution
echo "🚀 Starting all TiHoMo services..."
echo "📋 Docker Compose will handle service dependencies automatically"
docker compose up -d

echo ""
echo "✅ TiHoMo Development Environment is ready!"
echo ""
echo "🌐 Access URLs:"
echo "   Frontend (Nuxt):     http://localhost:${FRONTEND_PORT:-3001}"
echo "   API Gateway:         http://localhost:${GATEWAY_PORT:-5800}"
echo "   Identity API:        http://localhost:${IDENTITY_API_PORT:-5801}"
echo "   CoreFinance API:     http://localhost:${COREFINANCE_API_PORT:-5802}"
echo "   Excel API:           http://localhost:${EXCEL_API_PORT:-5805}"
echo ""
echo "🛠️  Development Tools:"
echo "   Grafana:             http://localhost:${GRAFANA_PORT:-3000} (admin/\${GRAFANA_ADMIN_PASSWORD})"
echo "   Prometheus:          http://localhost:${PROMETHEUS_PORT:-9090}"
echo "   RabbitMQ Management: http://localhost:${RABBITMQ_MANAGEMENT_PORT:-15672} (tihomo/\${RABBITMQ_PASSWORD})"
echo "   pgAdmin:             http://localhost:${PGADMIN_PORT:-8080} (admin@tihomo.com/\${PGADMIN_PASSWORD})"
echo "   Mailhog:             http://localhost:${MAILHOG_UI_PORT:-8025}"
echo ""
echo "📊 To check service status: docker compose ps"
echo "📝 To view logs: docker compose logs -f [service-name]"
echo "🛑 To stop all services: docker compose down"
