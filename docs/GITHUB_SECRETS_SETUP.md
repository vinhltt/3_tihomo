# GitHub Secrets và Variables Setup Guide

## Overview

Deployment workflows hiện tại sử dụng GitHub Secrets và Variables thay vì hard-coded values để tăng security và flexibility.

## Cách setup trong GitHub

### 1. Truy cập GitHub Settings

1. Vào repository trên GitHub
2. Click **Settings** tab
3. Trong sidebar, click **Secrets and variables** → **Actions**

### 2. Required GitHub Secrets (Tab "Secrets")

Click **"New repository secret"** để thêm từng secret:

#### Authentication & Security
```
JWT_SECRET_KEY=your-super-secret-jwt-key-here-must-be-at-least-64-characters-long
```

#### Database Passwords  
```
IDENTITY_DB_PASSWORD=your-secure-identity-db-password
COREFINANCE_DB_PASSWORD=your-secure-corefinance-db-password
MONEYMANAGEMENT_DB_PASSWORD=your-secure-money-db-password
PLANNINGINVESTMENT_DB_PASSWORD=your-secure-planning-db-password
REPORTING_DB_PASSWORD=your-secure-reporting-db-password
```

#### External Services Passwords
```
REDIS_PASSWORD=your-secure-redis-password
RABBITMQ_PASSWORD=your-secure-rabbitmq-password
GRAFANA_ADMIN_PASSWORD=your-secure-grafana-password
PGADMIN_PASSWORD=your-secure-pgadmin-password
```

#### SSH & Infrastructure (already exist)
```
TRUENAS_SSH_PRIVATE_KEY=your-truenas-ssh-private-key
DISCORD_WEBHOOK_URL=your-discord-webhook-url
```

### 3. Required GitHub Variables (Tab "Variables")

Click **"New repository variable"** để thêm từng variable:

#### Database Configuration
```
IDENTITY_DB_USERNAME=identity_user
IDENTITY_DB_PORT=5431
COREFINANCE_DB_USERNAME=corefinance_user  
COREFINANCE_DB_PORT=5432
MONEYMANAGEMENT_DB_USERNAME=money_user
MONEYMANAGEMENT_DB_PORT=5433
PLANNINGINVESTMENT_DB_USERNAME=planning_user
PLANNINGINVESTMENT_DB_PORT=5434
REPORTING_DB_USERNAME=reporting_user
REPORTING_DB_PORT=5435
```

#### JWT Configuration
```
JWT_ISSUER=TiHoMo
JWT_AUDIENCE_IDENTITY_API=TiHoMo.Identity.Api
JWT_AUDIENCE_COREFINANCE_API=TiHoMo.CoreFinance.Api
JWT_AUDIENCE_EXCEL_API=TiHoMo.Excel.Api
JWT_AUDIENCE_FRONTEND=TiHoMo.Frontend
JWT_AUDIENCE_OCELOT_GATEWAY=TiHoMo.Gateway
```

#### Service Ports
```
REDIS_PORT=6379
RABBITMQ_PORT=5672
RABBITMQ_MANAGEMENT_PORT=15672
PROMETHEUS_PORT=9090
GRAFANA_PORT=3000
MAILHOG_SMTP_PORT=1025
MAILHOG_UI_PORT=8025
GATEWAY_PORT=8080
```

#### API Ports
```
API_GATEWAY_PORT=8080
IDENTITY_API_PORT=5217
COREFINANCE_API_PORT=7293
EXCEL_API_PORT=5219
FRONTEND_PORT=3500
```

#### Infrastructure
```
PGADMIN_PORT=8080
LOKI_PORT=3100
NGINX_HTTP_PORT=80
NGINX_HTTPS_PORT=443
DOCKER_NETWORK_SUBNET=172.20.0.0/16
```

#### Frontend & OAuth
```
FRONTEND_BASE_URL=http://your-truenas-ip:3500
APP_PUBLIC_GOOGLE_CLIENT_ID=your-google-oauth-client-id
```

#### TrueNAS SSH (already exist)
```
DEPLOY_PATH_ON_TRUENAS=/mnt/pool/containers/tihomo
TRUENAS_SSH_HOSTNAME_THROUGH_CLOUDFLARED=your-cloudflare-tunnel-hostname
TRUENAS_USER=root
```

## Default Values

Nếu không set GitHub Variables, workflow sẽ dùng default values:
- Database ports: 5431, 5432, 5433, 5434, 5435
- Service ports: Redis 6379, RabbitMQ 5672, Grafana 3000...
- Usernames: identity_user, corefinance_user, money_user...

**Lưu ý**: Passwords KHÔNG có default values - phải set trong Secrets.

## Environment-specific Setup

### Development Environment
- Có thể dùng default values
- Passwords có thể đơn giản hơn

### Production Environment  
- **PHẢI** set tất cả secrets với passwords mạnh
- **PHẢI** set APP_PUBLIC_GOOGLE_CLIENT_ID
- **PHẢI** set FRONTEND_BASE_URL với IP/domain thật

## Security Best Practices

### Secrets (Sensitive Data)
- ✅ Passwords, API keys, tokens
- ✅ SSH private keys
- ✅ OAuth client secrets
- ❌ Không được log hoặc expose

### Variables (Non-sensitive)
- ✅ Usernames, ports, URLs
- ✅ Configuration values
- ✅ Có thể thấy trong logs

## Verification

Sau khi setup, run deployment workflow và check:

1. **No missing variable warnings**:
```
# Before (bad)
level=warning msg="The \"REDIS_PASSWORD\" variable is not set"

# After (good) 
No warnings about missing variables
```

2. **Environment file created correctly**:
```
[VERIFY] .env file content (first 20 lines):
# ================================
# CORS Configuration
# ================================
CORS_POLICY_NAME=DefaultCorsPolicy
```

## Troubleshooting

### Common Issues

1. **Secret not set**: Workflow fails với "variable is not set"
   - **Fix**: Add missing secret trong GitHub Settings

2. **Variable not set**: Workflow dùng default value
   - **Fix**: Add variable hoặc để default (nếu OK)

3. **Wrong environment file**: Services không start được
   - **Fix**: Check logs để tìm missing/wrong variables

### Debug Steps

1. Check workflow logs cho "variable is not set" warnings
2. Verify secrets/variables tồn tại trong GitHub Settings  
3. Ensure correct naming (case-sensitive)
4. Test với minimal setup trước

## Migration từ Hard-coded

Nếu đang migrate từ hard-coded values:

1. ✅ Backup current .env files
2. ✅ Setup GitHub Secrets/Variables
3. ✅ Deploy workflow mới
4. ✅ Verify services work correctly
5. ✅ Remove hard-coded values từ code

## Example Setup Script

```bash
# Local script để generate GitHub CLI commands
cat << 'EOF' > setup-github-vars.sh
#!/bin/bash

# GitHub Secrets
gh secret set JWT_SECRET_KEY --body "your-64-char-jwt-secret-key"
gh secret set IDENTITY_DB_PASSWORD --body "YourSecurePassword123!"
gh secret set REDIS_PASSWORD --body "YourSecureRedisPass123!"
# ... add more secrets

# GitHub Variables  
gh variable set JWT_ISSUER --value "TiHoMo"
gh variable set IDENTITY_DB_USERNAME --value "identity_user"
gh variable set REDIS_PORT --value "6379"
# ... add more variables

echo "✅ GitHub Secrets and Variables setup completed!"
EOF

chmod +x setup-github-vars.sh
# Review and run: ./setup-github-vars.sh
```

---

**⚠️ Important**: Đừng commit passwords hoặc sensitive data vào repository. Chỉ sử dụng GitHub Secrets cho sensitive information.