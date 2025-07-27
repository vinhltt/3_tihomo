#!/bin/bash

# ================================
# TiHoMo Frontend Deployment Fix Script
# ================================

set -e

echo "🔧 [FIX] Starting TiHoMo Frontend Deployment Fix..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
DEPLOY_DIR="$(pwd)"
ENV_FILE="$DEPLOY_DIR/.env"
BACKUP_DIR="$DEPLOY_DIR/backup_$(date +%Y%m%d_%H%M%S)"

echo -e "${BLUE}[INFO] Working directory: $DEPLOY_DIR${NC}"

# Create backup
echo -e "${YELLOW}[BACKUP] Creating backup of current configuration...${NC}"
mkdir -p "$BACKUP_DIR"
if [ -f "$ENV_FILE" ]; then
    cp "$ENV_FILE" "$BACKUP_DIR/.env.backup"
    echo -e "${GREEN}[OK] Backup created at: $BACKUP_DIR/.env.backup${NC}"
fi

# Step 1: Create comprehensive .env file
echo -e "${BLUE}[STEP 1] Creating comprehensive .env file...${NC}"

cat > "$ENV_FILE" << 'ENV_EOF'
# ================================
# CORS Configuration
# ================================
CORS_POLICY_NAME=DefaultCorsPolicy
CORS_ALLOWED_ORIGINS=*
CORS_ALLOWED_METHODS=*
CORS_ALLOWED_HEADERS=*
CORS_EXPOSED_HEADERS=Token-Expired  
CORS_PREFLIGHT_MAX_AGE=10

# ================================
# Environment Configuration
# ================================
ASPNETCORE_ENVIRONMENT=Production

# ================================
# Database Configuration
# ================================
# Identity Database
IDENTITY_DB_USERNAME=identity_user
IDENTITY_DB_PASSWORD=TiHoMo2024!DB
IDENTITY_DB_PORT=5431

# CoreFinance Database  
COREFINANCE_DB_USERNAME=corefinance_user
COREFINANCE_DB_PASSWORD=TiHoMo2024!DB
COREFINANCE_DB_PORT=5432

# MoneyManagement Database
MONEYMANAGEMENT_DB_USERNAME=money_user
MONEYMANAGEMENT_DB_PASSWORD=TiHoMo2024!DB
MONEYMANAGEMENT_DB_PORT=5433

# PlanningInvestment Database
PLANNINGINVESTMENT_DB_USERNAME=planning_user
PLANNINGINVESTMENT_DB_PASSWORD=TiHoMo2024!DB
PLANNINGINVESTMENT_DB_PORT=5434

# Reporting Database
REPORTING_DB_USERNAME=reporting_user
REPORTING_DB_PASSWORD=TiHoMo2024!DB
REPORTING_DB_PORT=5435

# ================================
# Authentication Configuration
# ================================
JWT_SECRET_KEY=TiHoMo-Super-Secret-JWT-Key-2024-Production-Environment-Must-Be-At-Least-64-Characters
JWT_ISSUER=TiHoMo
JWT_AUDIENCE_IDENTITY_API=TiHoMo.Identity.Api
JWT_AUDIENCE_COREFINANCE_API=TiHoMo.CoreFinance.Api
JWT_AUDIENCE_EXCEL_API=TiHoMo.Excel.Api
JWT_AUDIENCE_FRONTEND=TiHoMo.Frontend

# ================================
# External Services Configuration
# ================================
# Redis
REDIS_PASSWORD=TiHoMo2024!Redis

# RabbitMQ  
RABBITMQ_PASSWORD=TiHoMo2024!Rabbit

# ================================
# API Ports Configuration
# ================================
API_GATEWAY_PORT=8080
IDENTITY_API_PORT=5217
COREFINANCE_API_PORT=7293
EXCEL_API_PORT=5219
FRONTEND_PORT=3500

# ================================
# Docker Build Configuration
# ================================
# Nuxt Build Target: production for deployment
NUXT_BUILD_TARGET=production
# Node Environment: production for deployment
NODE_ENV=production
# Docker User: 1001:1001 for production security
DOCKER_USER=1001:1001
# Production-specific settings
NUXT_DEV_SSR=false
NUXT_DEV_TOOLS=false
NUXT_DEBUG=false

# ================================
# Frontend Configuration
# ================================
FRONTEND_BASE_URL=http://localhost:3500
APP_PUBLIC_GOOGLE_CLIENT_ID=your-google-client-id

# ================================
# Infrastructure Configuration
# ================================
# pgAdmin
PGADMIN_PASSWORD=TiHoMo2024!pgAdmin
PGADMIN_PORT=8080

# Loki
LOKI_PORT=3100

# Nginx
NGINX_HTTP_PORT=80
NGINX_HTTPS_PORT=443

# ================================
# Docker Compose Project Name
# ================================
COMPOSE_PROJECT_NAME=tihomo_production
ENV_EOF

echo -e "${GREEN}[OK] .env file created successfully${NC}"

# Step 2: Stop any running frontend containers
echo -e "${BLUE}[STEP 2] Stopping existing frontend containers...${NC}"

# Detect if we need sudo for docker
if docker ps >/dev/null 2>&1; then
    USE_SUDO=""
    echo -e "${GREEN}[OK] Docker available without sudo${NC}"
else
    USE_SUDO="sudo"
    echo -e "${YELLOW}[INFO] Using sudo for docker commands${NC}"
fi

# Stop frontend containers
if $USE_SUDO docker compose ps frontend-nuxt | grep -q "Up"; then
    echo -e "${YELLOW}[INFO] Stopping running frontend container...${NC}"
    $USE_SUDO docker compose stop frontend-nuxt
    sleep 3
fi

# Remove stopped containers
echo -e "${YELLOW}[INFO] Removing stopped containers...${NC}"
$USE_SUDO docker compose rm -f frontend-nuxt 2>/dev/null || true

# Step 3: Clean up any orphaned containers
echo -e "${BLUE}[STEP 3] Cleaning up Docker environment...${NC}"
$USE_SUDO docker system prune -f --volumes 2>/dev/null || true

# Step 4: Rebuild frontend image
echo -e "${BLUE}[STEP 4] Rebuilding frontend image...${NC}"
$USE_SUDO docker compose build --no-cache frontend-nuxt

if [ $? -ne 0 ]; then
    echo -e "${RED}[ERROR] Frontend image build failed${NC}"
    echo -e "${YELLOW}[DEBUG] Checking build context...${NC}"
    
    echo -e "${BLUE}[DEBUG] Current directory contents:${NC}"
    ls -la
    
    echo -e "${BLUE}[DEBUG] Frontend directory structure:${NC}"
    if [ -d "src/fe/nuxt" ]; then
        ls -la src/fe/nuxt/
        echo -e "${BLUE}[DEBUG] Dockerfile exists: $(test -f src/fe/nuxt/Dockerfile && echo 'YES' || echo 'NO')${NC}"
        echo -e "${BLUE}[DEBUG] Package.json exists: $(test -f src/fe/nuxt/package.json && echo 'YES' || echo 'NO')${NC}"
    else
        echo -e "${RED}[ERROR] src/fe/nuxt directory not found!${NC}"
    fi
    
    exit 1
fi

echo -e "${GREEN}[OK] Frontend image built successfully${NC}"

# Step 5: Start frontend service
echo -e "${BLUE}[STEP 5] Starting frontend service...${NC}"
$USE_SUDO docker compose up -d --no-deps --force-recreate frontend-nuxt

# Step 6: Wait for frontend to be ready
echo -e "${BLUE}[STEP 6] Waiting for frontend to be ready...${NC}"
max_attempts=30
attempt=1

while [ $attempt -le $max_attempts ]; do
    if $USE_SUDO docker compose ps frontend-nuxt | grep -q "Up"; then
        echo -e "${GREEN}[OK] Frontend container is running${NC}"
        
        # Wait for Nuxt to initialize
        echo -e "${YELLOW}[INFO] Waiting for Nuxt application to initialize...${NC}"
        sleep 15
        
        # Test if frontend is responding
        if $USE_SUDO docker compose exec -T frontend-nuxt curl -f http://localhost:3000/ >/dev/null 2>&1; then
            echo -e "${GREEN}[SUCCESS] Frontend is responding to HTTP requests${NC}"
            break
        else
            echo -e "${YELLOW}[WAIT] Frontend not ready yet (attempt $attempt/$max_attempts)${NC}"
            if [ $attempt -eq 10 ] || [ $attempt -eq 20 ]; then
                echo -e "${BLUE}[DEBUG] Frontend logs at attempt $attempt:${NC}"
                $USE_SUDO docker compose logs --tail=10 frontend-nuxt
            fi
            sleep 10
            ((attempt++))
        fi
    else
        echo -e "${YELLOW}[WAIT] Frontend container not up yet (attempt $attempt/$max_attempts)${NC}"
        sleep 5
        ((attempt++))
    fi
done

if [ $attempt -gt $max_attempts ]; then
    echo -e "${RED}[ERROR] Frontend failed to start after $max_attempts attempts${NC}"
    echo -e "${RED}[DEBUG] Frontend logs:${NC}"
    $USE_SUDO docker compose logs --tail=50 frontend-nuxt
    echo -e "${RED}[DEBUG] Container status:${NC}"
    $USE_SUDO docker compose ps frontend-nuxt
    exit 1
fi

# Step 7: Final health check
echo -e "${BLUE}[STEP 7] Final health check...${NC}"

# Check container status
if $USE_SUDO docker compose ps frontend-nuxt | grep -q "Up"; then
    echo -e "${GREEN}[OK] Frontend container is running${NC}"
    
    # Check HTTP response
    if $USE_SUDO docker compose exec -T frontend-nuxt curl -f http://localhost:3000/ >/dev/null 2>&1; then
        echo -e "${GREEN}[OK] Frontend is serving HTTP requests${NC}"
        
        # Check if API gateway is accessible
        if $USE_SUDO docker compose exec -T frontend-nuxt curl -f http://ocelot-gateway:8080/health >/dev/null 2>&1; then
            echo -e "${GREEN}[OK] Frontend can connect to backend gateway${NC}"
        else
            echo -e "${YELLOW}[WARNING] Frontend cannot connect to backend gateway${NC}"
        fi
        
        echo -e "${GREEN}[SUCCESS] ✅ Frontend deployment completed successfully!${NC}"
    else
        echo -e "${RED}[ERROR] Frontend HTTP check failed${NC}"
        $USE_SUDO docker compose logs --tail=20 frontend-nuxt
        exit 1
    fi
else
    echo -e "${RED}[ERROR] Frontend container is not running${NC}"
    $USE_SUDO docker compose ps frontend-nuxt
    exit 1
fi

# Step 8: Show final status
echo -e "${BLUE}[FINAL STATUS] System overview:${NC}"
$USE_SUDO docker compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"

echo -e "${GREEN}"
echo "=================================="
echo "🎉 FRONTEND DEPLOYMENT SUCCESS! 🎉"
echo "=================================="
echo -e "${NC}"
echo -e "${BLUE}📊 Deployment Summary:${NC}"
echo -e "   • Frontend URL: http://localhost:3500"
echo -e "   • Environment: Production"
echo -e "   • Status: ✅ Running"
echo -e "   • Health Check: ✅ Passed"
echo ""
echo -e "${YELLOW}💡 Next Steps:${NC}"
echo -e "   • Access frontend at: http://localhost:3500"
echo -e "   • Monitor logs: docker compose logs -f frontend-nuxt"
echo -e "   • Check status: docker compose ps"
echo ""
echo -e "${GREEN}✅ Frontend is ready for use!${NC}"
