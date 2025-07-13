#!/bin/bash

# TiHoMo Deployment Configuration Validator
# Validates GitHub Secrets and Variables for TrueNAS deployment

echo "🔍 TiHoMo Deployment Configuration Validator"
echo "============================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Counters
MISSING_SECRETS=0
MISSING_VARIABLES=0
WARNINGS=0

# Function to check if a value is set
check_secret() {
    local name=$1
    local value=$2
    local required=${3:-true}
    
    if [ -z "$value" ]; then
        if [ "$required" = true ]; then
            echo -e "${RED}❌ Missing required secret: $name${NC}"
            ((MISSING_SECRETS++))
        else
            echo -e "${YELLOW}⚠️  Optional secret not set: $name${NC}"
            ((WARNINGS++))
        fi
    else
        echo -e "${GREEN}✅ Secret configured: $name${NC}"
        
        # Additional validation for specific secrets
        case $name in
            "JWT_SECRET_KEY")
                if [ ${#value} -lt 32 ]; then
                    echo -e "${YELLOW}⚠️  JWT_SECRET_KEY should be at least 32 characters${NC}"
                    ((WARNINGS++))
                fi
                ;;
            "TRUENAS_SSH_PRIVATE_KEY")
                if [[ ! "$value" =~ ^-----BEGIN ]]; then
                    echo -e "${YELLOW}⚠️  SSH private key format may be incorrect${NC}"
                    ((WARNINGS++))
                fi
                ;;
            "DISCORD_WEBHOOK_URL")
                if [[ ! "$value" =~ ^https://discord\.com/api/webhooks/ ]]; then
                    echo -e "${YELLOW}⚠️  Discord webhook URL format may be incorrect${NC}"
                    ((WARNINGS++))
                fi
                ;;
        esac
    fi
}

check_variable() {
    local name=$1
    local value=$2
    local required=${3:-true}
    
    if [ -z "$value" ]; then
        if [ "$required" = true ]; then
            echo -e "${RED}❌ Missing required variable: $name${NC}"
            ((MISSING_VARIABLES++))
        else
            echo -e "${YELLOW}⚠️  Optional variable not set: $name (will use default)${NC}"
            ((WARNINGS++))
        fi
    else
        echo -e "${GREEN}✅ Variable configured: $name = $value${NC}"
        
        # Additional validation for specific variables
        case $name in
            "*_PORT")
                if ! [[ "$value" =~ ^[0-9]+$ ]] || [ "$value" -lt 1 ] || [ "$value" -gt 65535 ]; then
                    echo -e "${YELLOW}⚠️  Port $name should be between 1-65535${NC}"
                    ((WARNINGS++))
                fi
                ;;
            "DOCKER_NETWORK_SUBNET")
                if [[ ! "$value" =~ ^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+/[0-9]+$ ]]; then
                    echo -e "${YELLOW}⚠️  Network subnet format may be incorrect${NC}"
                    ((WARNINGS++))
                fi
                ;;
        esac
    fi
}

echo
echo -e "${BLUE}📋 Checking GitHub Secrets...${NC}"
echo "================================"

# Core Authentication
check_secret "JWT_SECRET_KEY" "$JWT_SECRET_KEY" true

# Database Passwords
check_secret "IDENTITY_DB_PASSWORD" "$IDENTITY_DB_PASSWORD" true
check_secret "COREFINANCE_DB_PASSWORD" "$COREFINANCE_DB_PASSWORD" true
check_secret "MONEYMANAGEMENT_DB_PASSWORD" "$MONEYMANAGEMENT_DB_PASSWORD" false
check_secret "PLANNINGINVESTMENT_DB_PASSWORD" "$PLANNINGINVESTMENT_DB_PASSWORD" false
check_secret "REPORTING_DB_PASSWORD" "$REPORTING_DB_PASSWORD" false

# Database Usernames (Optional)
check_secret "IDENTITY_DB_USERNAME" "$IDENTITY_DB_USERNAME" false
check_secret "COREFINANCE_DB_USERNAME" "$COREFINANCE_DB_USERNAME" false
check_secret "MONEYMANAGEMENT_DB_USERNAME" "$MONEYMANAGEMENT_DB_USERNAME" false
check_secret "PLANNINGINVESTMENT_DB_USERNAME" "$PLANNINGINVESTMENT_DB_USERNAME" false

# Infrastructure Services
check_secret "REDIS_PASSWORD" "$REDIS_PASSWORD" true
check_secret "RABBITMQ_PASSWORD" "$RABBITMQ_PASSWORD" true
check_secret "GRAFANA_ADMIN_PASSWORD" "$GRAFANA_ADMIN_PASSWORD" false
check_secret "PGADMIN_PASSWORD" "$PGADMIN_PASSWORD" false

# OAuth Configuration
check_secret "APP_PUBLIC_GOOGLE_CLIENT_ID" "$APP_PUBLIC_GOOGLE_CLIENT_ID" false

# TrueNAS SSH Configuration
check_secret "TRUENAS_SSH_HOSTNAME_THROUGH_CLOUDFLARED" "$TRUENAS_SSH_HOSTNAME_THROUGH_CLOUDFLARED" true
check_secret "TRUENAS_USER" "$TRUENAS_USER" true
check_secret "TRUENAS_SSH_PRIVATE_KEY" "$TRUENAS_SSH_PRIVATE_KEY" true

# Discord Notifications
check_secret "DISCORD_WEBHOOK_URL" "$DISCORD_WEBHOOK_URL" false

echo
echo -e "${BLUE}📋 Checking GitHub Variables...${NC}"
echo "================================="

# Deployment Configuration
check_variable "DEPLOY_PATH_ON_TRUENAS" "$DEPLOY_PATH_ON_TRUENAS" true
check_variable "COMPOSE_PROJECT_NAME" "$COMPOSE_PROJECT_NAME" false

# Port Configuration
check_variable "GATEWAY_PORT" "$GATEWAY_PORT" false
check_variable "FRONTEND_PORT" "$FRONTEND_PORT" false
check_variable "IDENTITY_DB_PORT" "$IDENTITY_DB_PORT" false
check_variable "COREFINANCE_DB_PORT" "$COREFINANCE_DB_PORT" false
check_variable "MONEYMANAGEMENT_DB_PORT" "$MONEYMANAGEMENT_DB_PORT" false
check_variable "PLANNINGINVESTMENT_DB_PORT" "$PLANNINGINVESTMENT_DB_PORT" false
check_variable "REPORTING_DB_PORT" "$REPORTING_DB_PORT" false
check_variable "REDIS_PORT" "$REDIS_PORT" false
check_variable "RABBITMQ_PORT" "$RABBITMQ_PORT" false
check_variable "RABBITMQ_MANAGEMENT_PORT" "$RABBITMQ_MANAGEMENT_PORT" false
check_variable "PROMETHEUS_PORT" "$PROMETHEUS_PORT" false
check_variable "GRAFANA_PORT" "$GRAFANA_PORT" false
check_variable "LOKI_PORT" "$LOKI_PORT" false
check_variable "PGADMIN_PORT" "$PGADMIN_PORT" false
check_variable "MAILHOG_SMTP_PORT" "$MAILHOG_SMTP_PORT" false
check_variable "MAILHOG_UI_PORT" "$MAILHOG_UI_PORT" false
check_variable "NGINX_HTTP_PORT" "$NGINX_HTTP_PORT" false
check_variable "NGINX_HTTPS_PORT" "$NGINX_HTTPS_PORT" false

# Network Configuration
check_variable "DOCKER_NETWORK_SUBNET" "$DOCKER_NETWORK_SUBNET" false

# Application Configuration
check_variable "NODE_ENV" "$NODE_ENV" false
check_variable "ASPNETCORE_ENVIRONMENT" "$ASPNETCORE_ENVIRONMENT" false
check_variable "NUXT_BUILD_TARGET" "$NUXT_BUILD_TARGET" false

# JWT Configuration
check_variable "JWT_ISSUER" "$JWT_ISSUER" false
check_variable "JWT_AUDIENCE_OCELOT_GATEWAY" "$JWT_AUDIENCE_OCELOT_GATEWAY" false
check_variable "JWT_AUDIENCE_IDENTITY_API" "$JWT_AUDIENCE_IDENTITY_API" false
check_variable "JWT_AUDIENCE_COREFINANCE_API" "$JWT_AUDIENCE_COREFINANCE_API" false

# Timezone & Logging
check_variable "GENERIC_TIMEZONE" "$GENERIC_TIMEZONE" false
check_variable "TZ" "$TZ" false
check_variable "LOG_LEVEL" "$LOG_LEVEL" false
check_variable "LOG_FORMAT" "$LOG_FORMAT" false

echo
echo -e "${BLUE}📊 Validation Summary${NC}"
echo "===================="

if [ $MISSING_SECRETS -eq 0 ] && [ $MISSING_VARIABLES -eq 0 ]; then
    echo -e "${GREEN}✅ All required secrets and variables are configured!${NC}"
    
    if [ $WARNINGS -gt 0 ]; then
        echo -e "${YELLOW}⚠️  $WARNINGS warnings found (see above)${NC}"
    fi
    
    echo
    echo -e "${GREEN}🚀 Ready for deployment!${NC}"
    echo
    echo "Next steps:"
    echo "1. Push your code to master/develop/staging branch"
    echo "2. Or trigger manual deployment from GitHub Actions"
    echo "3. Monitor deployment progress in GitHub Actions"
    echo "4. Check Discord notifications for deployment status"
    
    exit 0
else
    echo -e "${RED}❌ Configuration validation failed!${NC}"
    echo
    echo "Issues found:"
    if [ $MISSING_SECRETS -gt 0 ]; then
        echo -e "${RED}  • $MISSING_SECRETS missing required secrets${NC}"
    fi
    if [ $MISSING_VARIABLES -gt 0 ]; then
        echo -e "${RED}  • $MISSING_VARIABLES missing required variables${NC}"
    fi
    if [ $WARNINGS -gt 0 ]; then
        echo -e "${YELLOW}  • $WARNINGS warnings${NC}"
    fi
    
    echo
    echo "Please fix the issues above before deployment."
    echo "Refer to docs/DEPLOYMENT_SETUP.md for configuration details."
    
    exit 1
fi 