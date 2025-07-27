#!/bin/bash

# Single Service Deployment Script for TiHoMo
# Usage: ./deploy-single-service.sh <service-name> [environment] [force-rebuild]
# Examples:
#   ./deploy-single-service.sh identity-api
#   ./deploy-single-service.sh frontend-nuxt production true
#   ./deploy-single-service.sh corefinance-api development false

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
DEPLOY_DIR="${DEPLOY_PATH_ON_TRUENAS:-/mnt/pool1/docker/tihomo}/deploy_$(git branch --show-current)"

# Service categories and dependencies
declare -A SERVICE_CATEGORIES
SERVICE_CATEGORIES["infrastructure"]="identity-postgres corefinance-postgres moneymanagement-postgres planninginvestment-postgres reporting-postgres redis rabbitmq"
SERVICE_CATEGORIES["backend"]="identity-api corefinance-api excel-api ocelot-gateway"
SERVICE_CATEGORIES["frontend"]="frontend-nuxt"
SERVICE_CATEGORIES["monitoring"]="prometheus grafana loki"
SERVICE_CATEGORIES["utility"]="pgadmin mailhog nginx"

declare -A SERVICE_DEPENDENCIES
SERVICE_DEPENDENCIES["identity-api"]="identity-postgres redis"
SERVICE_DEPENDENCIES["corefinance-api"]="corefinance-postgres identity-api redis"
SERVICE_DEPENDENCIES["excel-api"]="corefinance-api"
SERVICE_DEPENDENCIES["ocelot-gateway"]="identity-api corefinance-api"
SERVICE_DEPENDENCIES["frontend-nuxt"]="ocelot-gateway corefinance-api"

declare -A SERVICE_DEPENDENTS
SERVICE_DEPENDENTS["identity-api"]="corefinance-api excel-api ocelot-gateway frontend-nuxt"
SERVICE_DEPENDENTS["corefinance-api"]="excel-api ocelot-gateway frontend-nuxt"
SERVICE_DEPENDENTS["ocelot-gateway"]="frontend-nuxt"

# Functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

show_help() {
    echo "TiHoMo Single Service Deployment Script"
    echo ""
    echo "Usage: $0 <service-name> [environment] [force-rebuild]"
    echo ""
    echo "Services:"
    echo "  Infrastructure: ${SERVICE_CATEGORIES[infrastructure]}"
    echo "  Backend APIs:   ${SERVICE_CATEGORIES[backend]}"
    echo "  Frontend:       ${SERVICE_CATEGORIES[frontend]}"
    echo "  Monitoring:     ${SERVICE_CATEGORIES[monitoring]}"
    echo "  Utilities:      ${SERVICE_CATEGORIES[utility]}"
    echo ""
    echo "Arguments:"
    echo "  service-name    Name of the service to deploy"
    echo "  environment     Deployment environment (development|staging|production) [default: development]"
    echo "  force-rebuild   Force rebuild image (true|false) [default: false]"
    echo ""
    echo "Examples:"
    echo "  $0 identity-api"
    echo "  $0 frontend-nuxt production true"
    echo "  $0 corefinance-api development false"
    echo ""
}

check_prerequisites() {
    log_info "Checking prerequisites..."
    
    # Check if we're in the right directory
    if [ ! -f "$PROJECT_ROOT/docker-compose.yml" ]; then
        log_error "docker-compose.yml not found. Please run from project root."
        exit 1
    fi
    
    # Check if service exists in compose file
    if ! docker compose config --services | grep -q "^$SERVICE_NAME$"; then
        log_error "Service '$SERVICE_NAME' not found in docker-compose.yml"
        log_info "Available services:"
        docker compose config --services | sed 's/^/  - /'
        exit 1
    fi
    
    log_success "Prerequisites check passed"
}

get_service_category() {
    local service=$1
    for category in "${!SERVICE_CATEGORIES[@]}"; do
        if [[ " ${SERVICE_CATEGORIES[$category]} " =~ " $service " ]]; then
            echo "$category"
            return 0
        fi
    done
    echo "unknown"
}

check_dependencies() {
    local service=$1
    local deps="${SERVICE_DEPENDENCIES[$service]}"
    
    if [ -z "$deps" ]; then
        log_info "No dependencies for $service"
        return 0
    fi
    
    log_info "Checking dependencies for $service: $deps"
    
    local missing_deps=()
    for dep in $deps; do
        if ! docker compose ps "$dep" | grep -q "Up"; then
            missing_deps+=("$dep")
        fi
    done
    
    if [ ${#missing_deps[@]} -gt 0 ]; then
        log_error "Missing dependencies: ${missing_deps[*]}"
        log_info "Please deploy dependencies first or use the orchestrator workflow"
        return 1
    fi
    
    log_success "All dependencies are running"
    return 0
}

stop_dependents() {
    local service=$1
    local dependents="${SERVICE_DEPENDENTS[$service]}"
    
    if [ -z "$dependents" ]; then
        log_info "No dependents to stop for $service"
        return 0
    fi
    
    log_warning "Stopping dependent services: $dependents"
    
    local stopped_services=()
    for dependent in $dependents; do
        if docker compose ps "$dependent" | grep -q "Up"; then
            log_info "Stopping dependent service: $dependent"
            docker compose stop "$dependent"
            stopped_services+=("$dependent")
        fi
    done
    
    # Save stopped services for later restart
    echo "${stopped_services[*]}" > "/tmp/tihomo_stopped_services_$$"
    
    if [ ${#stopped_services[@]} -gt 0 ]; then
        log_warning "Stopped ${#stopped_services[@]} dependent services"
    fi
}

restart_dependents() {
    local stopped_services_file="/tmp/tihomo_stopped_services_$$"
    
    if [ ! -f "$stopped_services_file" ]; then
        log_info "No dependent services to restart"
        return 0
    fi
    
    local stopped_services=$(cat "$stopped_services_file")
    rm -f "$stopped_services_file"
    
    if [ -z "$stopped_services" ]; then
        log_info "No dependent services to restart"
        return 0
    fi
    
    log_info "Restarting dependent services: $stopped_services"
    
    for service in $stopped_services; do
        log_info "Starting dependent service: $service"
        docker compose start "$service"
        
        # Wait a moment for service to be ready
        sleep 5
        
        # Basic health check
        local max_attempts=6
        local attempt=1
        while [ $attempt -le $max_attempts ]; do
            if docker compose ps "$service" | grep -q "Up"; then
                log_success "Dependent service $service is running"
                break
            else
                log_info "Waiting for $service to be ready (attempt $attempt/$max_attempts)"
                sleep 10
                ((attempt++))
            fi
        done
        
        if [ $attempt -gt $max_attempts ]; then
            log_warning "Dependent service $service may not be fully ready"
        fi
    done
}

deploy_service() {
    local service=$1
    local force_rebuild=$2
    
    log_info "Deploying service: $service"
    
    # Build image if needed
    if [ "$force_rebuild" = "true" ]; then
        log_info "Force rebuilding image for $service..."
        docker compose build --no-cache "$service"
    else
        log_info "Building image for $service (with cache)..."
        docker compose build "$service"
    fi
    
    if [ $? -ne 0 ]; then
        log_error "Build failed for $service"
        return 1
    fi
    
    # Deploy the service
    log_info "Starting service: $service"
    docker compose up -d --no-deps --force-recreate "$service"
    
    # Wait for service to be ready
    local max_attempts=20
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if docker compose ps "$service" | grep -q "Up"; then
            log_success "Service $service is running"
            
            # Additional health checks based on service type
            case "$service" in
                *-api|*-gateway)
                    log_info "Performing API health check for $service..."
                    sleep 10
                    if docker compose exec -T "$service" curl -f http://localhost:8080/health >/dev/null 2>&1; then
                        log_success "$service health check passed"
                    else
                        log_warning "$service health check failed, but container is running"
                    fi
                    ;;
                frontend-nuxt)
                    log_info "Performing frontend health check..."
                    sleep 15
                    if docker compose exec -T "$service" curl -f http://localhost:3000/ >/dev/null 2>&1; then
                        log_success "Frontend health check passed"
                    else
                        log_warning "Frontend health check failed, but container is running"
                    fi
                    ;;
                redis)
                    sleep 5
                    if docker compose exec -T "$service" redis-cli ping | grep -q "PONG"; then
                        log_success "Redis health check passed"
                    else
                        log_warning "Redis health check failed"
                    fi
                    ;;
                rabbitmq)
                    sleep 10
                    if docker compose exec -T "$service" rabbitmq-diagnostics ping >/dev/null 2>&1; then
                        log_success "RabbitMQ health check passed"
                    else
                        log_warning "RabbitMQ health check failed"
                    fi
                    ;;
                *-postgres)
                    log_info "Database is starting, waiting for initialization..."
                    sleep 20
                    ;;
            esac
            
            return 0
        else
            log_info "Service $service not ready yet (attempt $attempt/$max_attempts)"
            sleep 10
            ((attempt++))
        fi
    done
    
    log_error "Service $service failed to start after $max_attempts attempts"
    docker compose logs --tail=20 "$service"
    return 1
}

# Main script
if [ $# -eq 0 ] || [ "$1" = "-h" ] || [ "$1" = "--help" ]; then
    show_help
    exit 0
fi

# Parse arguments
SERVICE_NAME="$1"
ENVIRONMENT="${2:-development}"
FORCE_REBUILD="${3:-false}"

log_info "TiHoMo Single Service Deployment"
log_info "Service: $SERVICE_NAME"
log_info "Environment: $ENVIRONMENT"
log_info "Force Rebuild: $FORCE_REBUILD"

# Validate arguments
if [[ ! "$ENVIRONMENT" =~ ^(development|staging|production)$ ]]; then
    log_error "Invalid environment: $ENVIRONMENT. Must be development, staging, or production."
    exit 1
fi

if [[ ! "$FORCE_REBUILD" =~ ^(true|false)$ ]]; then
    log_error "Invalid force_rebuild value: $FORCE_REBUILD. Must be true or false."
    exit 1
fi

# Check prerequisites
check_prerequisites

# Get service category
SERVICE_CATEGORY=$(get_service_category "$SERVICE_NAME")
log_info "Service category: $SERVICE_CATEGORY"

# Load environment if .env exists
if [ -f "$PROJECT_ROOT/.env" ]; then
    log_info "Loading environment variables from .env"
    set -a
    source "$PROJECT_ROOT/.env"
    set +a
fi

# Set project name
export COMPOSE_PROJECT_NAME="tihomo_$ENVIRONMENT"

# Check dependencies
if ! check_dependencies "$SERVICE_NAME"; then
    exit 1
fi

# Stop dependent services
stop_dependents "$SERVICE_NAME"

# Deploy the service
if ! deploy_service "$SERVICE_NAME" "$FORCE_REBUILD"; then
    log_error "Deployment failed for $SERVICE_NAME"
    
    # Try to restart dependents anyway
    restart_dependents
    exit 1
fi

# Restart dependent services
restart_dependents

# Final status check
log_info "Final deployment status:"
docker compose ps "$SERVICE_NAME"

# Show recent logs
log_info "Recent logs for $SERVICE_NAME:"
docker compose logs --tail=10 "$SERVICE_NAME"

log_success "Single service deployment completed successfully!"
log_info "Service $SERVICE_NAME is ready in $ENVIRONMENT environment"