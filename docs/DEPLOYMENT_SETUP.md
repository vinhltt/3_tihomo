# TiHoMo Deployment Setup Guide

## 📋 Tổng quan

Hướng dẫn này mô tả cách thiết lập GitHub Actions để tự động deploy TiHoMo system lên TrueNAS thông qua Cloudflare Tunnel.

## 🔧 Cấu hình GitHub Repository

### 1. GitHub Secrets (Repository Settings > Secrets and Variables > Actions)

#### Core Authentication
```
JWT_SECRET_KEY: "your-super-secret-jwt-key-at-least-32-characters-long"
```

#### Database Passwords
```
IDENTITY_DB_PASSWORD: "your-strong-password-here"
COREFINANCE_DB_PASSWORD: "your-strong-password-here"
MONEYMANAGEMENT_DB_PASSWORD: "your-strong-password-here"  # Optional, defaults to IDENTITY_DB_PASSWORD
PLANNINGINVESTMENT_DB_PASSWORD: "your-strong-password-here"  # Optional, defaults to IDENTITY_DB_PASSWORD
REPORTING_DB_PASSWORD: "your-strong-password-here"  # Optional, defaults to IDENTITY_DB_PASSWORD
```

#### Database Usernames (Optional)
```
IDENTITY_DB_USERNAME: "identity_user"  # Default value
COREFINANCE_DB_USERNAME: "corefinance_user"  # Default value
MONEYMANAGEMENT_DB_USERNAME: "money_user"  # Default value
PLANNINGINVESTMENT_DB_USERNAME: "planning_user"  # Default value
REPORTING_DB_USERNAME: "reporting_user"  # Default value
```

#### Infrastructure Services
```
REDIS_PASSWORD: "your-redis-password"
RABBITMQ_PASSWORD: "your-rabbitmq-password"
GRAFANA_ADMIN_PASSWORD: "your-grafana-password"  # Optional, defaults to IDENTITY_DB_PASSWORD
PGADMIN_PASSWORD: "your-pgadmin-password"  # Optional, defaults to IDENTITY_DB_PASSWORD
```

#### OAuth Configuration
```
APP_PUBLIC_GOOGLE_CLIENT_ID: "your-google-oauth-client-id"
NUXT_PUBLIC_GOOGLE_CLIENT_SECRET: "your-google-oauth-client-secret"  # Optional for OAuth
```

#### Frontend Configuration
```
FRONTEND_BASE_URL: "http://localhost:3500"  # Base URL for frontend application
```

#### TrueNAS SSH Configuration
```
TRUENAS_SSH_HOSTNAME_THROUGH_CLOUDFLARED: "your-truenas-hostname.cloudflareaccess.com"
TRUENAS_USER: "your-truenas-username"
TRUENAS_SSH_PRIVATE_KEY: "-----BEGIN OPENSSH PRIVATE KEY-----\n...\n-----END OPENSSH PRIVATE KEY-----"
```

#### Discord Notifications
```
DISCORD_WEBHOOK_URL: "https://discord.com/api/webhooks/your-webhook-url"
```

### 2. GitHub Variables (Repository Settings > Secrets and Variables > Actions)

#### Deployment Configuration
```
DEPLOY_PATH_ON_TRUENAS: "/mnt/your-pool/tihomo"  # Base deployment path on TrueNAS
COMPOSE_PROJECT_NAME: "tihomo"  # Docker Compose project name
```

#### Port Configuration
```
GATEWAY_PORT: "5000"
FRONTEND_PORT: "3500"
IDENTITY_DB_PORT: "5831"
COREFINANCE_DB_PORT: "5832"
MONEYMANAGEMENT_DB_PORT: "5835"
PLANNINGINVESTMENT_DB_PORT: "5836"
REPORTING_DB_PORT: "5837"
REDIS_PORT: "6379"
RABBITMQ_PORT: "5672"
RABBITMQ_MANAGEMENT_PORT: "15672"
PROMETHEUS_PORT: "9090"
GRAFANA_PORT: "3002"
LOKI_PORT: "3100"
PGADMIN_PORT: "8081"
MAILHOG_SMTP_PORT: "1025"
MAILHOG_UI_PORT: "8025"
NGINX_HTTP_PORT: "8082"
NGINX_HTTPS_PORT: "8443"
```

#### API Internal Ports (Optional - for Ocelot Gateway routing)
```
EXCEL_API_PORT: "8080"  # Internal Excel API port for Ocelot routing
IDENTITY_API_PORT: "8080"  # Internal Identity API port for Ocelot routing
COREFINANCE_API_PORT: "8080"  # Internal CoreFinance API port for Ocelot routing
```

#### Network Configuration
```
DOCKER_NETWORK_SUBNET: "172.20.0.0/16"
```

#### Application Configuration
```
NODE_ENV: "production"
ASPNETCORE_ENVIRONMENT: "Production"
NUXT_BUILD_TARGET: "production"
NUXT_DEV_SSR: "false"
NUXT_DEBUG: "false"
NUXT_DEV_TOOLS: "false"
```

#### JWT Configuration
```
JWT_ISSUER: "http://localhost:5000"
JWT_AUDIENCE_OCELOT_GATEWAY: "TiHoMo.Gateway"
JWT_AUDIENCE_IDENTITY_API: "TiHoMo.Identity"
JWT_AUDIENCE_COREFINANCE_API: "TiHoMo.CoreFinance"
```

#### Build & Deployment Configuration
```
NUXT_BUILD_TARGET: "production"  # Build target for Nuxt (development/production)
NITRO_PRESET: "node-server"  # Nitro preset for Nuxt deployment
NITRO_PORT: "3000"  # Internal Nitro server port
```

#### Timezone & Logging
```
GENERIC_TIMEZONE: "Asia/Ho_Chi_Minh"
TZ: "Asia/Ho_Chi_Minh"
LOG_LEVEL: "info"
LOG_FORMAT: "json"
```

#### Feature Toggles
```
ENABLE_PWA: "false"
ENABLE_ANALYTICS: "false"
```

#### Additional Configuration Variables
```
NUXT_HOST: "0.0.0.0"  # Nuxt server host binding
NUXT_PORT: "3000"  # Nuxt internal port
NUXT_TELEMETRY_DISABLED: "1"  # Disable Nuxt telemetry
UPLOAD_PATH: "/app/uploads"  # File upload path for Excel API
IS_DOCKER: "true"  # Docker environment flag
```

## 🏗️ TrueNAS Setup

### 1. Cloudflare Tunnel Setup

1. **Cài đặt Cloudflared trên TrueNAS**:
```bash
# Download cloudflared
sudo wget -O /usr/local/bin/cloudflared https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64
sudo chmod +x /usr/local/bin/cloudflared
```

2. **Tạo Cloudflare Tunnel**:
```bash
# Login to Cloudflare
cloudflared login

# Create tunnel
cloudflared tunnel create tihomo-tunnel

# Configure tunnel
cloudflared tunnel route dns tihomo-tunnel your-domain.com
```

3. **Cấu hình SSH access**:
```bash
# Add to ~/.ssh/config on TrueNAS
Host *
  ProxyCommand cloudflared access ssh --hostname %h
```

### 2. SSH Key Setup

1. **Tạo SSH key pair**:
```bash
ssh-keygen -t rsa -b 4096 -C "github-actions@tihomo"
```

2. **Thêm public key vào TrueNAS**:
```bash
# Add to ~/.ssh/authorized_keys on TrueNAS
cat ~/.ssh/id_rsa.pub >> ~/.ssh/authorized_keys
```

3. **Thêm private key vào GitHub Secrets** với tên `TRUENAS_SSH_PRIVATE_KEY`

### 3. Docker Setup trên TrueNAS

1. **Cài đặt Docker**:
```bash
# Install Docker
sudo apt update
sudo apt install docker.io docker-compose-plugin

# Add user to docker group
sudo usermod -aG docker $USER
```

2. **Tạo deployment directory**:
```bash
sudo mkdir -p /mnt/your-pool/tihomo
sudo chown $USER:$USER /mnt/your-pool/tihomo
```

## 🚀 Deployment Process

### 1. Workflow Trigger

Workflow sẽ tự động chạy khi:
- Push code lên branches: `master`, `develop`, `staging`
- Manual trigger từ GitHub Actions tab

### 2. Deployment Steps

1. **Validation**: Kiểm tra secrets và variables
2. **SSH Setup**: Cấu hình Cloudflare tunnel và SSH
3. **File Sync**: Sync project files lên TrueNAS
4. **Environment Setup**: Tạo file `.env` với cấu hình
5. **Docker Operations**: Pull, build, và start containers
6. **Health Checks**: Kiểm tra trạng thái services
7. **Notifications**: Gửi thông báo Discord

### 3. Environment-Specific Deployment

- **master branch** → production environment
- **develop branch** → development environment  
- **staging branch** → staging environment

## 📊 Monitoring & Access

### Service URLs (sau khi deploy)

```
Frontend: http://<TRUENAS_IP>:3500
API Gateway: http://<TRUENAS_IP>:5000
Grafana: http://<TRUENAS_IP>:3002
RabbitMQ: http://<TRUENAS_IP>:15672
pgAdmin: http://<TRUENAS_IP>:8081
Prometheus: http://<TRUENAS_IP>:9090
```

### Default Credentials

- **Grafana**: admin / [GRAFANA_ADMIN_PASSWORD]
- **RabbitMQ**: tihomo / [RABBITMQ_PASSWORD]
- **pgAdmin**: admin@tihomo.com / [PGADMIN_PASSWORD]

## 🔍 Troubleshooting

### 1. SSH Connection Issues

```bash
# Test SSH connection
ssh -F ~/.ssh/config your-truenas-user@your-truenas-hostname.cloudflareaccess.com

# Check cloudflared status
cloudflared tunnel list
```

### 2. Docker Issues

```bash
# Check Docker status
docker --version
docker compose version

# Check running containers
docker compose ps

# View logs
docker compose logs [service-name]
```

### 3. Environment Variables

```bash
# Check .env file
cat .env | grep -E "(JWT|DB|REDIS|RABBITMQ)"

# Verify environment loading
source .env && echo $GATEWAY_PORT
```

## 🛡️ Security Considerations

1. **Secrets Management**: Không bao giờ commit secrets vào code
2. **SSH Keys**: Sử dụng dedicated SSH keys cho GitHub Actions
3. **Network Security**: Cấu hình firewall cho TrueNAS
4. **Database Security**: Sử dụng strong passwords cho databases
5. **JWT Security**: Sử dụng strong JWT secret key (>32 characters)

## 📝 Tổng hợp Variables cần thiết

### GitHub Secrets (Bảo mật - không được public)
```
JWT_SECRET_KEY
IDENTITY_DB_PASSWORD
COREFINANCE_DB_PASSWORD
MONEYMANAGEMENT_DB_PASSWORD
PLANNINGINVESTMENT_DB_PASSWORD
REPORTING_DB_PASSWORD
REDIS_PASSWORD
RABBITMQ_PASSWORD
GRAFANA_ADMIN_PASSWORD
PGADMIN_PASSWORD
NUXT_PUBLIC_GOOGLE_CLIENT_SECRET
TRUENAS_SSH_HOSTNAME_THROUGH_CLOUDFLARED
TRUENAS_USER
TRUENAS_SSH_PRIVATE_KEY
DISCORD_WEBHOOK_URL
```

### GitHub Variables (Có thể public)
```
DEPLOY_PATH_ON_TRUENAS
COMPOSE_PROJECT_NAME
GATEWAY_PORT
FRONTEND_PORT
IDENTITY_DB_PORT
COREFINANCE_DB_PORT
MONEYMANAGEMENT_DB_PORT
PLANNINGINVESTMENT_DB_PORT
REPORTING_DB_PORT
REDIS_PORT
RABBITMQ_PORT
RABBITMQ_MANAGEMENT_PORT
PROMETHEUS_PORT
GRAFANA_PORT
LOKI_PORT
PGADMIN_PORT
MAILHOG_SMTP_PORT
MAILHOG_UI_PORT
NGINX_HTTP_PORT
NGINX_HTTPS_PORT
EXCEL_API_PORT
IDENTITY_API_PORT
COREFINANCE_API_PORT
DOCKER_NETWORK_SUBNET
NODE_ENV
ASPNETCORE_ENVIRONMENT
NUXT_BUILD_TARGET
NUXT_DEV_SSR
NUXT_DEBUG
NUXT_DEV_TOOLS
JWT_ISSUER
JWT_AUDIENCE_OCELOT_GATEWAY
JWT_AUDIENCE_IDENTITY_API
JWT_AUDIENCE_COREFINANCE_API
GENERIC_TIMEZONE
TZ
LOG_LEVEL
LOG_FORMAT
ENABLE_PWA
ENABLE_ANALYTICS
NUXT_HOST
NUXT_PORT
NUXT_TELEMETRY_DISABLED
UPLOAD_PATH
IS_DOCKER
NITRO_PRESET
NITRO_PORT
IDENTITY_DB_USERNAME
COREFINANCE_DB_USERNAME
MONEYMANAGEMENT_DB_USERNAME
PLANNINGINVESTMENT_DB_USERNAME
REPORTING_DB_USERNAME
APP_PUBLIC_GOOGLE_CLIENT_ID
FRONTEND_BASE_URL
```

## 📝 Environment Files

### Production Environment (.env example)
```bash
# Core Configuration
GATEWAY_PORT=5000
FRONTEND_PORT=3500
JWT_SECRET_KEY=your-super-secret-jwt-key-at-least-32-characters-long

# Database Configuration
IDENTITY_DB_PASSWORD=your-strong-password
COREFINANCE_DB_PASSWORD=your-strong-password
MONEYMANAGEMENT_DB_PASSWORD=your-strong-password
PLANNINGINVESTMENT_DB_PASSWORD=your-strong-password
REPORTING_DB_PASSWORD=your-strong-password
REDIS_PASSWORD=your-redis-password
RABBITMQ_PASSWORD=your-rabbitmq-password

# OAuth Configuration
APP_PUBLIC_GOOGLE_CLIENT_ID=your-google-oauth-client-id
FRONTEND_BASE_URL=http://localhost:3500

# Application Environment
NODE_ENV=production
ASPNETCORE_ENVIRONMENT=Production
NUXT_BUILD_TARGET=production
NUXT_DEV_SSR=false
NUXT_DEBUG=false
NUXT_DEV_TOOLS=false

# JWT Configuration
JWT_ISSUER=http://localhost:5000
JWT_AUDIENCE_OCELOT_GATEWAY=TiHoMo.Gateway
JWT_AUDIENCE_IDENTITY_API=TiHoMo.Identity
JWT_AUDIENCE_COREFINANCE_API=TiHoMo.CoreFinance

# Network Configuration
DOCKER_NETWORK_SUBNET=172.20.0.0/16

# Timezone & Logging
GENERIC_TIMEZONE=Asia/Ho_Chi_Minh
TZ=Asia/Ho_Chi_Minh
LOG_LEVEL=info
LOG_FORMAT=json
```

## 🔄 Deployment Workflow

```mermaid
graph TD
    A[Push to GitHub] --> B[Trigger Workflow]
    B --> C[Validate Secrets]
    C --> D[Setup SSH Tunnel]
    D --> E[Sync Files to TrueNAS]
    E --> F[Create .env File]
    F --> G[Docker Operations]
    G --> H[Health Checks]
    H --> I[Send Notifications]
    I --> J[Deployment Complete]
```

## 📞 Support

Nếu gặp vấn đề trong quá trình deployment:

1. Kiểm tra GitHub Actions logs
2. Kiểm tra Discord notifications
3. SSH vào TrueNAS để debug
4. Kiểm tra Docker container logs
5. Verify network connectivity

---

*Cập nhật lần cuối: January 2025* 