#!/bin/sh
set -e

# Print startup message with detailed debugging info
echo "🚀 Starting TiHoMo Frontend (Nuxt) container..."
echo "NODE_ENV: ${NODE_ENV:-development}"
echo "Working directory: $(pwd)"
echo "User: $(whoami)"
echo "User ID: $(id -u)"
echo "Group ID: $(id -g)"
echo "Node version: $(node -v)"
echo "NPM version: $(npm -v)"

# Set environment variable to ensure proper terminal output with Docker
export TERM=xterm-256color

# Ensure we're in the correct directory
cd /app

# Fix permissions for development mode
if [ "${NODE_ENV:-development}" != "production" ]; then
  echo "🔧 Fixing permissions for development mode..."
  
  # Get current user info
  CURRENT_USER=$(whoami)
  echo "Current user: $CURRENT_USER"
  
  # If running as root, fix ownership and switch to nuxt user
  if [ "$CURRENT_USER" = "root" ]; then
    echo "🔄 Running as root, fixing permissions and switching to nuxt user..."
    
    # Ensure nuxt user exists
    if ! id -u nuxt >/dev/null 2>&1; then
      echo "Creating nuxt user..."
      addgroup -g 1001 -S nodejs && adduser -S nuxt -u 1001 -G nodejs
    fi
    
    # Fix ownership of entire app directory
    echo "Fixing ownership of /app directory..."
    chown -R nuxt:nodejs /app
    
    # Ensure critical directories exist and have correct permissions
    for dir in ".nuxt" ".output" ".nitro" "dist" "node_modules" "logs" "uploads"; do
      if [ ! -d "$dir" ]; then
        echo "Creating directory: $dir"
        mkdir -p "$dir"
      fi
      echo "Setting permissions for: $dir"
      chown -R nuxt:nodejs "$dir"
      chmod -R 755 "$dir"
    done
    
    # Make sure package files are readable
    chown nuxt:nodejs package*.json 2>/dev/null || true
    
    echo "✅ Permissions fixed, switching to nuxt user..."
    # Re-run this script as nuxt user
    exec su-exec nuxt "$0" "$@"
  fi
  
  # If we're here, we're running as nuxt user or non-root
  echo "🔧 Running as non-root user: $CURRENT_USER"
  
  # Ensure critical directories exist and are writable
  for dir in ".nuxt" ".output" ".nitro" "dist"; do
    if [ ! -d "$dir" ]; then
      echo "Creating directory: $dir"
      mkdir -p "$dir"
    elif [ -d "$dir" ]; then
      echo "Cleaning existing directory: $dir"
      rm -rf "$dir"
      mkdir -p "$dir"
    fi
    chmod 755 "$dir" 2>/dev/null || echo "⚠️ Could not set permissions for $dir"
  done
  
  # Handle logs directory separately (might be volume mounted)
  if [ ! -d "logs" ]; then
    echo "Creating logs directory"
    mkdir -p "logs"
  fi
  chmod 755 "logs" 2>/dev/null || echo "⚠️ Could not set permissions for logs"
  
  # Special handling for node_modules (might be a named volume)
  if [ ! -d "node_modules" ]; then
    echo "Creating node_modules directory..."
    mkdir -p node_modules
  fi
  
  # Test write permissions
  echo "🧪 Testing write permissions..."
  if ! touch .nuxt/test-write 2>/dev/null; then
    echo "❌ Cannot write to .nuxt directory"
    ls -la .nuxt/
    exit 1
  else
    echo "✅ Write permissions OK"
    rm -f .nuxt/test-write
  fi
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
    rm -rf node_modules/.bin 2>/dev/null || true
    
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
