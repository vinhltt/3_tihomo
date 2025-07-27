# TiHoMo Docker Compose Architecture

## Overview

TiHoMo sử dụng dual Docker Compose architecture để tách biệt môi trường development và production:

- **docker-compose.yml**: Dành cho local development
- **docker-compose.ghcr.yml**: Dành cho GitHub Actions và production deployment

## Architecture Diagram

```
Local Development:
├── docker-compose.yml
│   ├── Build from source (./src/fe/nuxt)
│   ├── Source code mounting for hot reload
│   ├── Development environment variables
│   └── Dev tools enabled

Production Deployment:
├── docker-compose.ghcr.yml
│   ├── Pull from GHCR registry
│   ├── Pre-built optimized images
│   ├── Production environment variables
│   └── No source code dependencies

GitHub Actions Pipeline:
├── build-frontend.yml
│   ├── Build from Dockerfile
│   ├── Push to GHCR
│   └── Tag with branch/commit info
└── deploy-frontend.yml
    ├── Sync docker-compose.ghcr.yml to TrueNAS
    ├── Pull latest image from GHCR
    └── Deploy with docker-compose
```

## Files Description

### docker-compose.yml (Local Development)
- **Purpose**: Local development environment
- **Frontend**: Build from source using `./src/fe/nuxt` context
- **Features**:
  - Source code mounting for hot reload
  - Development environment variables
  - Dev tools (pgAdmin, Mailhog) enabled
  - NUXT_BUILD_TARGET=development

### docker-compose.ghcr.yml (Production)
- **Purpose**: Production deployment via GitHub Actions
- **Frontend**: Pull pre-built image from GHCR
- **Features**:
  - GHCR image: `ghcr.io/thevinh19/3_tihomo/frontend-nuxt:${FRONTEND_IMAGE_TAG}`
  - Production environment variables
  - Optimized settings (NODE_ENV=production, NUXT_DEV_SSR=false)
  - No source code dependencies
  - Faster startup (start_period: 120s)

## Usage

### Local Development
```bash
# Start development environment
docker-compose up

# Build and start with fresh build
docker-compose up --build

# Start specific services
docker-compose up frontend-nuxt redis postgres
```

### Production Deployment
Production deployment được handle tự động bởi GitHub Actions:

1. **build-frontend.yml**: Builds và pushes image tới GHCR
2. **deploy-frontend.yml**: Deploys từ GHCR tới TrueNAS

```bash
# Manual production deployment (if needed)
docker-compose -f docker-compose.ghcr.yml up
```

## Environment Variables

### Common Variables
- `COMPOSE_PROJECT_NAME`: Project name prefix
- `NODE_ENV`: development/production
- `FRONTEND_PORT`: Frontend port mapping

### Development Specific
- `NUXT_BUILD_TARGET`: development
- `NUXT_DEV_SSR`: true
- `NUXT_DEV_TOOLS`: true
- `ENABLE_DEV_TOOLS`: development

### Production Specific
- `FRONTEND_IMAGE_TAG`: GHCR image tag (latest/branch-name)
- `NODE_ENV`: production
- `NUXT_DEV_SSR`: false
- `NUXT_DEV_TOOLS`: false

## GitHub Actions Integration

### Build Process (build-frontend.yml)
1. Checkout source code
2. Build Docker image using Dockerfile
3. Tag with branch/commit info
4. Push to GHCR registry
5. Run security scan

### Deployment Process (deploy-frontend.yml)
1. Sync docker-compose.ghcr.yml to TrueNAS as docker-compose.yml
2. Set FRONTEND_IMAGE_TAG based on branch
3. Pull latest image from GHCR
4. Deploy using docker-compose
5. Health check and verification

## Benefits

### Development
- ⚡ Fast iteration with hot reload
- 🔧 Full development tools available
- 📁 Direct source code access
- 🐛 Easy debugging

### Production
- 🚀 Fast deployment (pre-built images)
- 📦 Consistent environments
- 🔒 Immutable deployments
- ⚖️ Resource optimization

## Migration from Single File

Previously, TiHoMo used a single docker-compose.yml file with conditional logic. The new architecture provides:

- **Clearer separation of concerns**
- **Environment-specific optimizations**
- **Reduced complexity in each file**
- **Better maintainability**

## Troubleshooting

### Development Issues
- Ensure source code is properly mounted
- Check if ports are available
- Verify build context in `./src/fe/nuxt`

### Production Issues
- Verify GHCR image exists and is accessible
- Check FRONTEND_IMAGE_TAG environment variable
- Ensure GitHub Actions build completed successfully
- Verify network connectivity to GHCR

## Related Files
- `.github/workflows/build-frontend.yml`: Build and push to GHCR
- `.github/workflows/deploy-frontend.yml`: Deploy from GHCR
- `src/fe/nuxt/Dockerfile`: Frontend build configuration
- `.env`: Environment variables (not committed)
