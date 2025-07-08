# TiHoMo Development Environment Makefile
# Provides convenient commands for managing the development environment

.PHONY: help setup up down restart logs build clean status health

# Default target
help: ## Show this help message
	@echo "TiHoMo Development Environment Commands:"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-20s\033[0m %s\n", $$1, $$2}'
	@echo ""

setup: ## Setup the development environment (first time setup)
	@echo "🔧 Setting up TiHoMo development environment..."
	@if [ ! -f .env ]; then cp .env.example .env; echo "📝 .env file created. Please edit it with your values."; fi
	@mkdir -p logs/{identity,corefinance,excel,ocelot}
	@mkdir -p uploads
	@mkdir -p config/{grafana/dashboards,nginx/conf.d,ssl}
	@echo "✅ Setup complete!"

up: ## Start all services
	@echo "🚀 Starting TiHoMo development environment..."
	@docker compose -f docker-compose.yml up -d
	@echo "✅ All services started!"

up-infra: ## Start only infrastructure services (databases, cache, messaging)
	@echo "🏗️ Starting infrastructure services..."
	@docker compose -f docker-compose.yml up -d identity-postgres corefinance-postgres moneymanagement-postgres planninginvestment-postgres reporting-postgres redis rabbitmq
	@echo "✅ Infrastructure services started!"

up-apis: ## Start API services
	@echo "🔨 Starting API services..."
	@docker compose -f docker-compose.yml up -d identity-api corefinance-api excel-api ocelot-gateway
	@echo "✅ API services started!"

up-frontend: ## Start frontend service
	@echo "🎨 Starting frontend service..."
	@docker compose -f docker-compose.yml up -d frontend-nuxt
	@echo "✅ Frontend service started!"

up-monitoring: ## Start monitoring services
	@echo "📊 Starting monitoring services..."
	@docker compose -f docker-compose.yml up -d prometheus grafana loki
	@echo "✅ Monitoring services started!"

up-tools: ## Start development tools
	@echo "🛠️ Starting development tools..."
	@docker compose -f docker-compose.yml up -d pgadmin mailhog
	@echo "✅ Development tools started!"

down: ## Stop all services
	@echo "🛑 Stopping all services..."
	@docker compose -f docker-compose.yml down
	@echo "✅ All services stopped!"

restart: down up ## Restart all services

logs: ## Show logs for all services
	@docker compose -f docker-compose.yml logs -f

logs-api: ## Show logs for API services
	@docker compose -f docker-compose.yml logs -f identity-api corefinance-api excel-api ocelot-gateway

logs-frontend: ## Show logs for frontend
	@docker compose -f docker-compose.yml logs -f frontend-nuxt

build: ## Build all Docker images
	@echo "🔨 Building all Docker images..."
	@docker compose -f docker-compose.yml build
	@echo "✅ All images built!"

build-frontend: ## Build frontend Docker image
	@echo "🎨 Building frontend image..."
	@docker compose -f docker-compose.yml build frontend-nuxt
	@echo "✅ Frontend image built!"

build-apis: ## Build API Docker images
	@echo "🔨 Building API images..."
	@docker compose -f docker-compose.yml build identity-api corefinance-api excel-api ocelot-gateway
	@echo "✅ API images built!"

clean: ## Clean up Docker resources
	@echo "🧹 Cleaning up Docker resources..."
	@docker compose -f docker-compose.yml down -v --remove-orphans
	@docker system prune -f
	@echo "✅ Cleanup complete!"

status: ## Show status of all services
	@echo "📊 Service Status:"
	@docker compose -f docker-compose.yml ps

health: ## Check health of all services
	@echo "🏥 Health Check:"
	@echo "Testing API endpoints..."
	@curl -f http://localhost:5800/health 2>/dev/null && echo "✅ API Gateway: Healthy" || echo "❌ API Gateway: Unhealthy"
	@curl -f http://localhost:5801/health 2>/dev/null && echo "✅ Identity API: Healthy" || echo "❌ Identity API: Unhealthy"
	@curl -f http://localhost:5802/health 2>/dev/null && echo "✅ CoreFinance API: Healthy" || echo "❌ CoreFinance API: Unhealthy"
	@curl -f http://localhost:5805/health 2>/dev/null && echo "✅ Excel API: Healthy" || echo "❌ Excel API: Unhealthy"
	@curl -f http://localhost:3500 2>/dev/null && echo "✅ Frontend: Healthy" || echo "❌ Frontend: Unhealthy"

db-migrate: ## Run database migrations
	@echo "🗃️ Running database migrations..."
	@docker compose -f docker-compose.yml exec identity-api dotnet ef database update
	@docker compose -f docker-compose.yml exec corefinance-api dotnet ef database update
	@echo "✅ Database migrations completed!"

db-reset: ## Reset all databases (WARNING: This will delete all data!)
	@echo "⚠️ This will delete all database data! Are you sure? [y/N]" && read ans && [ $${ans:-N} = y ]
	@echo "🗃️ Resetting databases..."
	@docker compose -f docker-compose.yml down
	@docker volume rm tihomo_identity_pgdata tihomo_corefinance_pgdata tihomo_moneymanagement_pgdata tihomo_planninginvestment_pgdata tihomo_reporting_pgdata 2>/dev/null || true
	@make up-infra
	@sleep 30
	@make db-migrate
	@echo "✅ Databases reset complete!"

# Quick access commands
dev: setup up ## Full development setup and start
urls: ## Show all service URLs
	@echo "🌐 Service URLs:"
	@echo "   Frontend (Nuxt):     http://localhost:3500"
	@echo "   API Gateway:         http://localhost:5800"
	@echo "   Identity API:        http://localhost:5801"
	@echo "   CoreFinance API:     http://localhost:5802"
	@echo "   Excel API:           http://localhost:5805"
	@echo ""
	@echo "🛠️ Development Tools:"
	@echo "   Grafana:             http://localhost:3000 (admin/admin123)"
	@echo "   Prometheus:          http://localhost:9090"
	@echo "   RabbitMQ Management: http://localhost:15672 (tihomo/tihomo123)"
	@echo "   pgAdmin:             http://localhost:8080 (admin@tihomo.local/admin123)"
	@echo "   Mailhog:             http://localhost:8025"
