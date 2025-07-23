#!/bin/sh
set -e

# Print startup message with detailed debugging info
echo "🚀 Starting TiHoMo Frontend (Nuxt) container..."
echo "NODE_ENV: ${NODE_ENV:-development}"
echo "Working directory: $(pwd)"
echo "User: $(whoami)"
echo "Node version: $(node -v)"
echo "NPM version: $(npm -v)"

# Function to run command as root if needed for permissions
run_as_root_if_needed() {
    if [ "$(whoami)" != "root" ] && [ -x "$(command -v su-exec)" ]; then
        su-exec root "$@"
    elif [ "$(whoami)" != "root" ] && [ -x "$(command -v sudo)" ]; then
        sudo "$@"
    else
        "$@"
    fi
}

# Set environment variable to ensure proper terminal output with Docker
export TERM=xterm-256color

# Ensure we're in the correct directory
cd /app

# Handle different environments
if [ "${NODE_ENV:-development}" != "production" ]; then
  echo "🧪 Development mode: Setting up environment..."
  
  # Check if package.json exists for development mode
  if [ ! -f "package.json" ]; then
    echo "❌ ERROR: package.json not found in /app for development mode"
    echo "Contents of /app:"
    ls -la /app
    exit 1
  fi
  
  echo "✅ Found package.json, proceeding with development setup..."
  
  # Check node_modules directory and named volume setup
  if [ ! -d "node_modules" ] || [ ! -d "node_modules/nuxt" ] || [ ! -f "node_modules/.bin/nuxt" ]; then
    echo "📥 Installing or rebuilding dependencies..."
    
    # Make sure node_modules exists and is writable
    mkdir -p node_modules
    
    # Clear any partial installations
    rm -rf node_modules/.bin
    
    # Install dependencies with debugging output
    echo "📦 Running npm install with --legacy-peer-deps flag..."
    npm cache clean --force
    npm install --legacy-peer-deps --no-audit --no-fund --verbose || { 
      echo "⚠️ First npm install attempt failed, trying with force flag..."
      npm install --legacy-peer-deps --no-audit --no-fund --force
    }
  else
    echo "✅ Node modules directory exists with Nuxt dependency"
    
    # Check if package.json is newer than node_modules
    if [ "package.json" -nt "node_modules" ]; then
      echo "📝 Package.json has been updated, reinstalling dependencies..."
      npm install --legacy-peer-deps --no-audit --no-fund || echo "⚠️ Warning: npm update failed, but continuing..."
    fi
  fi
  
  # Add node_modules/.bin to PATH and ensure we can find nuxt
  echo "🔄 Adding node_modules/.bin to PATH..."
  export PATH="/app/node_modules/.bin:$PATH"
  
  # List what's in the .bin directory for debugging
  echo "Contents of node_modules/.bin:"
  ls -la node_modules/.bin 2>/dev/null || echo "No .bin directory found"
  
  # Check for Nuxt executable
  if [ -f "node_modules/.bin/nuxt" ]; then
    echo "✅ Found Nuxt executable at node_modules/.bin/nuxt"
  elif [ -f "node_modules/nuxt/bin/nuxt.mjs" ]; then
    echo "⚙️ Creating symlink to Nuxt executable..."
    mkdir -p node_modules/.bin
    ln -sf ../nuxt/bin/nuxt.mjs node_modules/.bin/nuxt
    chmod +x node_modules/.bin/nuxt
  else
    echo "🔍 Searching for Nuxt executable..."
    find node_modules -name "nuxt*" | grep -v "node_modules/.*node_modules"
  fi
  
  # Start Nuxt with multiple fallback options
  echo "🚀 Starting Nuxt development server..."
  
  # Try different methods to start Nuxt
  if [ -x "node_modules/.bin/nuxt" ]; then
    echo "✅ Running with node_modules/.bin/nuxt"
    exec node_modules/.bin/nuxt dev --host 0.0.0.0 --port 3000
  elif [ -f "node_modules/nuxt/bin/nuxt.mjs" ]; then
    echo "✅ Running with node directly"
    exec node node_modules/nuxt/bin/nuxt.mjs dev --host 0.0.0.0 --port 3000
  elif command -v npx >/dev/null 2>&1; then
    echo "✅ Running with npx"
    exec npx nuxt dev --host 0.0.0.0 --port 3000
  else
    echo "✅ Falling back to npm run dev"
    exec npm run dev
  fi
else
  echo "🏭 Production mode: Starting pre-built application..."
  
  # In production, if source is mounted (compose file), we need to check for .output
  # If no .output exists, this means we're in development with NODE_ENV=production
  # In true production (Docker build), .output should exist
  
  # Check if we're in a development container with production NODE_ENV
  if [ -f "package.json" ] && [ ! -f ".output/server/index.mjs" ]; then
    echo "⚠️  Production NODE_ENV detected but no .output directory"
    echo "🔨 Building application for production..."
    
    # Create necessary directories with proper permissions
    if [ "$(whoami)" = "root" ]; then
      mkdir -p node_modules .nuxt .output logs uploads
      chown -R nuxt:nodejs node_modules .nuxt .output logs uploads 2>/dev/null || true
    else
      run_as_root_if_needed sh -c "
        mkdir -p node_modules .nuxt .output logs uploads && 
        chown -R nuxt:nodejs node_modules .nuxt .output logs uploads
      " 2>/dev/null || true
    fi
    
    # Install dependencies if needed
    if [ ! -d "node_modules" ] || [ ! -d "node_modules/nuxt" ]; then
      echo "📦 Installing dependencies..."
      # Ensure proper permissions for npm operations
      if [ "$(whoami)" != "root" ]; then
        echo "🔐 Need root permissions for npm install, using su-exec..."
        run_as_root_if_needed sh -c "
          npm cache clean --force && \
          npm install --legacy-peer-deps --no-audit --no-fund --unsafe-perm && \
          chown -R nuxt:nodejs /app/node_modules /app/.npm 2>/dev/null || true
        "
      else
        npm cache clean --force
        npm install --legacy-peer-deps --no-audit --no-fund --unsafe-perm
      fi
    fi
    
    # Build the application
    echo "🏗️ Building Nuxt application..."
    export PATH="/app/node_modules/.bin:$PATH"
    npm run build
    
    # Verify build succeeded
    if [ ! -f ".output/server/index.mjs" ]; then
      echo "❌ ERROR: Build failed - .output/server/index.mjs not created"
      echo "Checking .output directory:"
      ls -la .output/ 2>/dev/null || echo "No .output directory found"
      exit 1
    fi
    
    echo "✅ Build completed successfully"
  elif [ ! -f ".output/server/index.mjs" ]; then
    echo "❌ ERROR: Pre-built application not found - .output/server/index.mjs missing"
    echo "Checking .output directory:"
    ls -la .output/ 2>/dev/null || echo "No .output directory found"
    echo "Checking /app directory:"
    ls -la /app/
    exit 1
  fi
  
  echo "✅ Found pre-built application at .output/server/index.mjs"
  echo "🚀 Starting production server..."
  
  # Switch to non-root user for security if we're currently root
  if [ "$(whoami)" = "root" ]; then
    echo "🔐 Switching to non-root user for security..."
    exec su-exec nuxt node .output/server/index.mjs
  else
    exec node .output/server/index.mjs
  fi
fi
