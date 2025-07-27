# Environment Variables Setup for TiHoMo Deployment

## ⚠️ CRITICAL: Required GitHub Variables

Để các workflows hoạt động đúng, bạn cần cấu hình các environment variables sau trong GitHub:

### 1. **Repository Variables** (Settings → Secrets and variables → Actions → Variables)

```bash
# Required Variables:
DEPLOY_PATH_ON_TRUENAS=/mnt/pool1/docker/tihomo

# Optional Variables (with defaults):
FRONTEND_PORT=3500
GATEWAY_PORT=5000
IDENTITY_DB_PORT=5831
COREFINANCE_DB_PORT=5832
MONEYMANAGEMENT_DB_PORT=5835
PLANNINGINVESTMENT_DB_PORT=5836
REPORTING_DB_PORT=5837
REDIS_PORT=6379
RABBITMQ_PORT=5672
RABBITMQ_MANAGEMENT_PORT=15672
PROMETHEUS_PORT=9090
GRAFANA_PORT=3002
LOKI_PORT=3100
PGADMIN_PORT=8081
MAILHOG_SMTP_PORT=1025
MAILHOG_UI_PORT=8025
NGINX_HTTP_PORT=8082
NGINX_HTTPS_PORT=8443

# Environment Variables:
NODE_ENV=production
ASPNETCORE_ENVIRONMENT=Production
NUXT_BUILD_TARGET=production
NUXT_DEV_SSR=false
NUXT_DEV_TOOLS=false
NUXT_DEBUG=false
TZ=Asia/Ho_Chi_Minh
DOCKER_USER=1001:1001
FRONTEND_BASE_URL=http://localhost:3500
JWT_ISSUER=http://localhost:5000
JWT_AUDIENCE_OCELOT_GATEWAY=TiHoMo.Gateway
JWT_AUDIENCE_IDENTITY_API=TiHoMo.Identity
JWT_AUDIENCE_COREFINANCE_API=TiHoMo.CoreFinance
DOCKER_NETWORK_SUBNET=172.20.0.0/16
GENERIC_TIMEZONE=Asia/Ho_Chi_Minh
LOG_LEVEL=info
LOG_FORMAT=json
ENABLE_PWA=false
ENABLE_ANALYTICS=false
```

### 2. **Repository Secrets** (Settings → Secrets and variables → Actions → Secrets)

```bash
# SSH and Server Access:
TRUENAS_SSH_HOSTNAME_THROUGH_CLOUDFLARED=your-truenas-hostname
TRUENAS_USER=your-truenas-user
TRUENAS_SSH_PRIVATE_KEY=your-ssh-private-key

# Database Passwords:
IDENTITY_DB_PASSWORD=your-identity-db-password
COREFINANCE_DB_PASSWORD=your-corefinance-db-password
MONEYMANAGEMENT_DB_PASSWORD=your-money-db-password
PLANNINGINVESTMENT_DB_PASSWORD=your-planning-db-password
REPORTING_DB_PASSWORD=your-reporting-db-password

# Service Passwords:
REDIS_PASSWORD=your-redis-password
RABBITMQ_PASSWORD=your-rabbitmq-password
GRAFANA_ADMIN_PASSWORD=your-grafana-password
PGADMIN_PASSWORD=your-pgadmin-password

# JWT and Authentication:
JWT_SECRET_KEY=your-jwt-secret-key-minimum-32-characters
APP_PUBLIC_GOOGLE_CLIENT_ID=your-google-oauth-client-id

# Optional Database Users (with defaults):
IDENTITY_DB_USERNAME=identity_user
COREFINANCE_DB_USERNAME=corefinance_user
MONEYMANAGEMENT_DB_USERNAME=money_user
PLANNINGINVESTMENT_DB_USERNAME=planning_user
REPORTING_DB_USERNAME=reporting_user

# Notifications:
DISCORD_WEBHOOK_URL=your-discord-webhook-url
```

## 🔧 Setup Instructions

### Step 1: Go to GitHub Repository Settings
```
Your Repo → Settings → Secrets and variables → Actions
```

### Step 2: Add Variables Tab
Click **Variables** and add each variable from the list above.

### Step 3: Add Secrets Tab  
Click **Secrets** and add each secret from the list above.

### Step 4: Test Configuration
Run a simple workflow to verify setup:

```yaml
# Manual test via GitHub Actions
Workflow: TiHoMo Deployment Orchestrator
Options:
  deployment_type: infrastructure-only
  environment: development
  force_rebuild: false
```

## 🚨 Common Issues & Solutions

### Issue 1: "DEPLOY_PATH_ON_TRUENAS not set"
```bash
# Solution: Add to GitHub Variables
DEPLOY_PATH_ON_TRUENAS=/mnt/pool1/docker/tihomo
```

### Issue 2: "no such file or directory: .env"
```bash
# Solution: Workflow will now create directory and .env file automatically
# Make sure DEPLOY_PATH_ON_TRUENAS is correct
```

### Issue 3: "Permission denied" on TrueNAS
```bash
# Solution: Verify SSH key and user permissions
# Make sure TRUENAS_USER has docker access
```

### Issue 4: "Database connection failed"
```bash
# Solution: Check database passwords in secrets
# Ensure passwords are strong (12+ characters)
```

## 📋 Environment Validation Checklist

Before running deployment:

- [ ] ✅ All GitHub Variables configured
- [ ] ✅ All GitHub Secrets configured  
- [ ] ✅ SSH access to TrueNAS working
- [ ] ✅ Docker permissions on TrueNAS correct
- [ ] ✅ Discord webhook URL working (optional)
- [ ] ✅ JWT secret key is 32+ characters
- [ ] ✅ Database passwords are strong
- [ ] ✅ Deploy path exists on TrueNAS

## 🎯 Environment-Specific Settings

### Development Environment
```bash
NODE_ENV=development
ASPNETCORE_ENVIRONMENT=Development
NUXT_DEBUG=true
NUXT_DEV_TOOLS=true
```

### Production Environment
```bash
NODE_ENV=production
ASPNETCORE_ENVIRONMENT=Production
NUXT_DEBUG=false
NUXT_DEV_TOOLS=false
```

### Staging Environment
```bash
NODE_ENV=staging
ASPNETCORE_ENVIRONMENT=Staging
NUXT_DEBUG=false
NUXT_DEV_TOOLS=false
```

---

**Important**: Never commit secrets to git! Always use GitHub Secrets for sensitive information.

**Verification**: After setup, test with infrastructure-only deployment to verify all variables are working correctly.