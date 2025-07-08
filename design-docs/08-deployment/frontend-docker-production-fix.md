# Frontend Docker Production Issue Resolution

## Overview
This document describes the resolution of a critical issue where the Nuxt frontend Docker container failed to start in production mode due to missing build output files.

## Problem Statement
The frontend container was failing to start in production with the error:
```
Cannot find module '.output/server/index.mjs'
```

### Root Cause Analysis
The issue was caused by the development volume mount in `docker-compose.yml`:
```yaml
volumes:
  - ./src/fe/nuxt:/app  # This overwrites .output directory
```

When starting a container, even in production mode, the volume mount would overwrite the `.output` directory that contains the Nuxt build output, causing the production server to fail.

## Solution Architecture

### 1. Enhanced Docker Entrypoint Script
Created a robust `docker-entrypoint.sh` that:
- **Always builds in production mode** for consistency
- **Checks and installs dependencies** if needed
- **Verifies build output exists** before starting server
- **Provides detailed logging** for debugging
- **Supports both development and production** environments
- **⭐ NEW: Development mode enhancements** to handle Nuxt CLI availability issues

```bash
#!/bin/sh
# Enhanced entrypoint script for Nuxt application with robust development support

echo "Starting Nuxt application..."
echo "NODE_ENV: ${NODE_ENV:-development}"
echo "Working directory: $(pwd)"
echo "User: $(whoami)"

# Ensure we're in the correct directory
cd /app

# Check if package.json exists
if [ ! -f "package.json" ]; then
  echo "ERROR: package.json not found in /app"
  echo "Contents of /app:"
  ls -la /app
  exit 1
fi

echo "Found package.json, checking dependencies..."

# In development mode, always ensure dependencies are properly installed
# This handles cases where volume mounts might affect node_modules
if [ "${NODE_ENV:-development}" != "production" ]; then
  echo "Development mode: Ensuring dependencies are properly installed..."
  
  # Check if node_modules exists and has content
  if [ ! -d "node_modules" ] || [ -z "$(ls -A node_modules 2>/dev/null)" ]; then
    echo "node_modules is missing or empty, installing dependencies..."
    npm install --legacy-peer-deps
  else
    # Even if node_modules exists, check if nuxt CLI is available
    if ! command -v npx nuxt >/dev/null 2>&1 && ! npm list nuxt >/dev/null 2>&1; then
      echo "Nuxt CLI not found, reinstalling dependencies..."
      npm install --legacy-peer-deps
    else
      echo "Dependencies appear to be installed correctly"
    fi
  fi
  
  # Final verification before starting dev server
  echo "Verifying Nuxt CLI availability..."
  if ! npx nuxt --version >/dev/null 2>&1; then
    echo "ERROR: Nuxt CLI still not available after installation"
    echo "Attempting to install Nuxt globally as fallback..."
    npm install -g nuxt@latest
  fi
  
  echo "Starting development server..."
  exec npm run dev
else
  echo "Production mode: Building and starting application..."
  
  # Install dependencies if needed
  if [ ! -d "node_modules" ] || [ -z "$(ls -A node_modules 2>/dev/null)" ]; then
    echo "Installing dependencies..."
    npm install --legacy-peer-deps
  fi
  
  # Always build in production to ensure fresh output
  echo "Building Nuxt application for production..."
  npm run build
  
  # Verify build output exists
  if [ ! -f ".output/server/index.mjs" ]; then
    echo "ERROR: Build failed - .output/server/index.mjs not found"
    echo "Checking .output directory:"
    ls -la .output/ 2>/dev/null || echo "No .output directory found"
    exit 1
  fi
  
  echo "Starting production server..."
  exec node .output/server/index.mjs
fi
```

### 2. Docker Compose Configuration Fixes

#### Development Mode (`docker-compose.yml`)
- **Removed custom command** - rely on entrypoint script
- **Keep volume mounts** for development hot reload
- **Let entrypoint handle** build and start logic

#### Production Mode (`docker-compose.prod.yml`)
- **No source code volume mounts** - prevents overwriting build output
- **Only data volumes** for uploads, logs, node_modules
- **Build from Dockerfile** with production target

### 3. Rebuild Scripts Enhancement
Updated both `rebuild-frontend.sh` and `rebuild-frontend.bat`:
- **Fixed compose file selection** for production mode
- **Use only production compose file** when `prod` parameter is passed
- **Proper environment variable setting**

## Implementation Details

### File Changes Summary
1. **`src/fe/nuxt/docker-entrypoint.sh`** - Complete rewrite with robust logic
2. **`docker-compose.yml`** - Removed custom command override
3. **`docker-compose.prod.yml`** - Removed obsolete version declaration
4. **`rebuild-frontend.sh`** - Fixed production mode compose file usage
5. **`rebuild-frontend.bat`** - Fixed production mode compose file usage

### Key Design Decisions

#### Always Build in Production Mode
- **Rationale**: Ensures consistent, optimized output regardless of environment
- **Benefit**: Eliminates development vs production build differences
- **Trade-off**: Slightly longer startup time in development

#### Volume Mount Strategy
- **Development**: Mount source code for hot reload capability
- **Production**: No source mounts to protect build output
- **Shared**: Data volumes for uploads, logs, dependencies

#### Single Entrypoint Script
- **Rationale**: Centralized startup logic reduces maintenance
- **Benefit**: Same script works for both dev and production
- **Implementation**: Environment detection for appropriate behavior

## Testing Instructions

### Development Mode Testing
```bash
# Build and start development
./rebuild-frontend.sh

# Or using Docker Compose directly
docker-compose -f docker-compose.yml up --build frontend-nuxt
```

### Production Mode Testing
```bash
# Build and start production
./rebuild-frontend.sh prod

# Or using Docker Compose directly
docker-compose -f docker-compose.prod.yml up --build frontend-nuxt
```

### Verification Steps
1. **Check container logs** for build success messages
2. **Verify .output directory** exists in container
3. **Test frontend accessibility** on configured port
4. **Confirm hot reload** works in development mode
5. **Validate production optimizations** in production mode

## Troubleshooting Guide

### Common Issues and Solutions

#### "Cannot find module '.output/server/index.mjs'"
- **Cause**: Build failed or output was overwritten
- **Solution**: Check entrypoint logs for build errors
- **Verification**: `docker exec <container> ls -la .output/server/`

#### "sh: nuxt: not found" in Development Mode
- **Cause**: Nuxt CLI not available due to volume mount timing issues
- **Solution**: Enhanced entrypoint script automatically handles this
- **Manual Fix**: `docker exec <container> npm install --legacy-peer-deps`
- **Debug**: Check if node_modules volume is properly mounted

#### Container exits immediately
- **Cause**: Entrypoint script error or missing dependencies
- **Solution**: Check container logs for specific error
- **Debug**: Run container interactively with `docker run -it <image> bash`

#### Development hot reload not working
- **Cause**: Volume mount issues or dev server not starting
- **Solution**: Verify volume mounts in docker-compose.yml
- **Check**: Ensure `npm run dev` is executed for development mode

## Benefits Achieved

### Reliability
- **Consistent builds** across all environments
- **Robust error handling** with clear error messages
- **Dependency validation** prevents runtime failures

### Maintainability
- **Single entrypoint script** reduces complexity
- **Clear separation** between dev and production modes
- **Standardized rebuild process** with helper scripts

### Developer Experience
- **Hot reload** preserved in development mode
- **Fast rebuilds** with intelligent dependency checking
- **Clear logging** for debugging issues

## Future Enhancements

### Potential Improvements
1. **Multi-stage health checks** for container readiness
2. **Graceful shutdown handling** for production deployments
3. **Build caching optimization** for faster rebuilds
4. **Environment-specific optimizations** for different deployment targets

### Monitoring Considerations
1. **Container startup time** metrics
2. **Build success/failure** tracking
3. **Frontend accessibility** health checks
4. **Resource utilization** monitoring

## Conclusion
This solution provides a robust, maintainable approach to frontend Docker deployments that:
- **Eliminates production startup failures**
- **Preserves development workflow**
- **Provides clear debugging capabilities**
- **Follows Docker best practices**

The implementation ensures that both development and production environments work reliably while maintaining the developer experience for hot reload and rapid iteration.
