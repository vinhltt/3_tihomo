#!/bin/sh
set -e

# Print startup message with detailed debugging info
echo "🚀 Starting TiHoMo Frontend (Nuxt) container..."
echo "NODE_ENV: ${NODE_ENV:-development}"
echo "Working directory: $(pwd)"
echo "User: $(whoami)"
echo "Node version: $(node -v)"
echo "NPM version: $(npm -v)"

# Set environment variable to ensure proper terminal output with Docker
export TERM=xterm-256color

# Ensure we're in the correct directory
cd /app

# Fix permissions for development mode
if [ "${NODE_ENV:-development}" != "production" ]; then
  echo "🔧 Fixing permissions for development mode..."
  
  # Ensure the nuxt user owns the app directory
  chown -R nuxt:nodejs /app 2>/dev/null || echo "⚠️ Could not change ownership (running as non-root?)"
  
  # Make sure directories are writable
  chmod -R 755 /app 2>/dev/null || echo "⚠️ Could not change permissions"
  
  # Ensure node_modules directory exists and is writable
  if [ ! -d "node_modules" ]; then
    mkdir -p node_modules
    chown nuxt:nodejs node_modules 2>/dev/null || echo "⚠️ Could not change node_modules ownership"
  fi
  
  # Ensure .nuxt cache directory is writable
  if [ -d ".nuxt" ]; then
    echo "🧹 Cleaning .nuxt cache directory..."
    rm -rf .nuxt
  fi
  mkdir -p .nuxt
  chown -R nuxt:nodejs .nuxt 2>/dev/null || echo "⚠️ Could not change .nuxt ownership"
  
  # Ensure other Nuxt directories are writable
  for dir in ".output" ".nitro" "dist"; do
    if [ -d "$dir" ]; then
      rm -rf "$dir"
    fi
    mkdir -p "$dir"
    chown -R nuxt:nodejs "$dir" 2>/dev/null || echo "⚠️ Could not change $dir ownership"
  done
fi

# Check if package.json exists
if [ ! -f "package.json" ]; then
  echo "❌ ERROR: package.json not found in /app"
  echo "Contents of /app:"
  ls -la /app
  exit 1
fi

echo "✅ Found package.json, proceeding with setup..."

# Handle different environments
if [ "${NODE_ENV:-development}" != "production" ]; then
  echo "🧪 Development mode: Setting up environment..."
  
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
  
  # Switch to nuxt user for running the application (if we're root)
  if [ "$(whoami)" = "root" ]; then
    echo "🔄 Switching to nuxt user for application execution..."
    # Try different methods to start Nuxt as nuxt user
    if [ -x "node_modules/.bin/nuxt" ]; then
      echo "✅ Running with node_modules/.bin/nuxt as nuxt user"
      exec su-exec nuxt node_modules/.bin/nuxt dev --host 0.0.0.0 --port 3000
    elif [ -f "node_modules/nuxt/bin/nuxt.mjs" ]; then
      echo "✅ Running with node directly as nuxt user"
      exec su-exec nuxt node node_modules/nuxt/bin/nuxt.mjs dev --host 0.0.0.0 --port 3000
    elif command -v npx >/dev/null 2>&1; then
      echo "✅ Running with npx as nuxt user"
      exec su-exec nuxt npx nuxt dev --host 0.0.0.0 --port 3000
    else
      echo "✅ Falling back to npm run dev as nuxt user"
      exec su-exec nuxt npm run dev
    fi
  else
    # Already running as non-root user
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
  fi
else
  echo "🏭 Production mode: Building and starting application..."
  
  # Install dependencies if needed
  if [ ! -d "node_modules" ] || [ -z "$(ls -A node_modules 2>/dev/null)" ]; then
    echo "📥 Installing dependencies..."
    npm ci --legacy-peer-deps
  fi
  
  # Always build in production to ensure fresh output
  echo "🛠️ Building Nuxt application for production..."
  npm run build
  
  # Verify build output exists
  if [ ! -f ".output/server/index.mjs" ]; then
    echo "❌ ERROR: Build failed - .output/server/index.mjs not found"
    echo "Checking .output directory:"
    ls -la .output/ 2>/dev/null || echo "No .output directory found"
    exit 1
  fi
  
  echo "🚀 Starting production server..."
  exec node .output/server/index.mjs
fi
