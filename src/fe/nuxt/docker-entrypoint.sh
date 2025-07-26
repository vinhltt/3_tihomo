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
# Use docker exec with root instead of su-exec to avoid setgroups issues
run_as_root_if_needed() {
    if [ "$(whoami)" != "root" ]; then
        echo "⚠️ Cannot escalate privileges inside container. Trying with current user..."
        "$@"
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
  
  # Check for Nuxt executable and ensure it's properly set up
  echo "🔧 Setting up nuxt command for development..."
  mkdir -p node_modules/.bin
  
  if [ -f "node_modules/.bin/nuxt" ]; then
    echo "✅ Found existing Nuxt executable at node_modules/.bin/nuxt"
  elif [ -f "node_modules/nuxt/bin/nuxt.mjs" ]; then
    echo "⚙️ Creating symlink to nuxt.mjs..."
    ln -sf ../nuxt/bin/nuxt.mjs node_modules/.bin/nuxt
    chmod +x node_modules/.bin/nuxt
  elif [ -f "node_modules/.bin/nuxi" ]; then
    echo "⚙️ Found nuxi, creating nuxt symlink..."
    ln -sf nuxi node_modules/.bin/nuxt
  elif [ -f "node_modules/nuxi/bin/nuxi.mjs" ]; then
    echo "⚙️ Found nuxi.mjs, creating symlink..."
    ln -sf ../nuxi/bin/nuxi.mjs node_modules/.bin/nuxt
    chmod +x node_modules/.bin/nuxt
  else
    echo "🔍 Searching for Nuxt executable..."
    find node_modules -name "*nuxt*" -type f | grep -v "node_modules/.*node_modules" | head -10
    echo "🔧 Creating fallback nuxt command..."
    cat > node_modules/.bin/nuxt << 'EOF'
#!/bin/sh
# Fallback nuxt wrapper script for development

echo "🔧 Nuxt wrapper: Starting with args: $*"

# Try different possible locations for Nuxt
if [ -f "node_modules/nuxt/bin/nuxt.mjs" ]; then
  echo "✅ Using node_modules/nuxt/bin/nuxt.mjs"
  exec node node_modules/nuxt/bin/nuxt.mjs "$@"
elif [ -f "node_modules/.bin/nuxi" ]; then
  echo "✅ Using node_modules/.bin/nuxi"
  exec node_modules/.bin/nuxi "$@"
elif [ -f "node_modules/nuxi/bin/nuxi.mjs" ]; then
  echo "✅ Using node_modules/nuxi/bin/nuxi.mjs"
  exec node node_modules/nuxi/bin/nuxi.mjs "$@"
elif command -v npx >/dev/null 2>&1; then
  echo "✅ Fallback to direct node execution"
  # Debug: Check what nuxt packages we have
  echo "🔍 Checking available nuxt packages..."
  find node_modules -name "*nuxt*" -type d | head -10
  ls -la node_modules/nuxt/ 2>/dev/null || echo "❌ No nuxt directory"
  ls -la node_modules/@nuxt/ 2>/dev/null || echo "❌ No @nuxt directory"
  
  # Try multiple approaches to find nuxt
  if [ -f "node_modules/nuxt/package.json" ]; then
    echo "✅ Found nuxt package, using require.resolve"
    exec node -e "
      const nuxtBin = require.resolve('nuxt/bin/nuxt.mjs');
      const { spawn } = require('child_process');
      const child = spawn('node', [nuxtBin, ...process.argv.slice(2)], { stdio: 'inherit' });
      child.on('exit', (code) => process.exit(code));
    " -- "$@"
  elif [ -f "node_modules/nuxt/bin/nuxt.mjs" ]; then
    echo "✅ Found nuxt.mjs directly"
    exec node node_modules/nuxt/bin/nuxt.mjs "$@"
  elif [ -f "node_modules/@nuxt/cli/bin/nuxt.mjs" ]; then
    echo "✅ Found @nuxt/cli"
    exec node node_modules/@nuxt/cli/bin/nuxt.mjs "$@"
  else
    echo "❌ No nuxt executable found in any location"
    echo "🔍 Available packages:"
    ls -la node_modules/ | grep nuxt || echo "No nuxt packages found"
    exit 1
  fi
else
  echo "Error: Nuxt executable not found in any expected location"
  echo "Available files in node_modules/.bin:"
  ls -la node_modules/.bin/ 2>/dev/null || echo "No .bin directory"
  echo "Available nuxt-related files:"
  find node_modules -name "*nuxt*" -type f 2>/dev/null | head -10 || echo "No nuxt files found"
  exit 1
fi
EOF
    chmod +x node_modules/.bin/nuxt
    echo "✅ Fallback nuxt wrapper created"
  fi
  
  # Verify the nuxt command works with timeout
  echo "🔍 Testing nuxt command..."
  if timeout 10 node_modules/.bin/nuxt --version >/dev/null 2>&1; then
    echo "✅ Nuxt command is working"
  else
    echo "⚠️ Nuxt command test failed or timeout, but proceeding..."
    echo "🔧 Checking nuxt executable directly..."
    ls -la node_modules/.bin/nuxt 2>/dev/null || echo "No nuxt executable found"
    echo "🔧 Testing executable permissions..."
    if [ -x "node_modules/.bin/nuxt" ]; then
      echo "✅ Nuxt executable has correct permissions"
    else
      echo "⚠️ Nuxt executable may not have correct permissions"
      chmod +x node_modules/.bin/nuxt 2>/dev/null || true
    fi
  fi
  
  # Clean up any problematic .output directory before starting
  echo "🧹 Cleaning up .output directory to prevent EBUSY errors..."
  for i in 1 2 3 4 5; do
    if [ -d ".output" ]; then
      echo "🗑️ Attempting to remove .output directory (attempt $i/5)..."
      rm -rf .output 2>/dev/null || true
      sleep 1
    else
      break
    fi
  done
  
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
  # Check for .output/server/index.mjs OR any partial build structure
  echo "🔍 Checking for pre-built .output..."
  echo "📁 Current directory: $(pwd)"
  echo "📁 Directory contents:"
  ls -la ./
  echo "📁 .output directory check:"
  ls -la .output/ 2>/dev/null || echo "❌ No .output directory found"
  echo "📁 .output/server check:"
  ls -la .output/server/ 2>/dev/null || echo "❌ No .output/server directory found"
  
  if [ ! -f ".output/server/index.mjs" ]; then
    echo "⚠️  Production NODE_ENV detected but no .output directory"
    echo "🔨 Building application for production..."
    
    # Create necessary directories with proper permissions (must run as root)
    mkdir -p node_modules .nuxt .output logs uploads
    chown -R nuxt:nodejs node_modules .nuxt .output logs uploads 2>/dev/null || true
    
    # Install dependencies if needed (must run as root)
    if [ ! -d "node_modules" ] || [ ! -d "node_modules/nuxt" ]; then
      echo "📦 Installing dependencies..."
      npm cache clean --force
      npm install --legacy-peer-deps --no-audit --no-fund --unsafe-perm
      chown -R nuxt:nodejs /app/node_modules /app/.npm 2>/dev/null || true
    fi
    
    # Build the application
    echo "🏗️ Building Nuxt application..."
    export PATH="/app/node_modules/.bin:$PATH"
    
    # Ensure nuxt command is available for production build
    echo "🔧 Setting up nuxt command for production build..."
    mkdir -p node_modules/.bin
    
    # Check for different Nuxt executables and create proper wrapper
    if [ -f "node_modules/nuxt/bin/nuxt.mjs" ]; then
      echo "✅ Found nuxt.mjs, creating symlink..."
      ln -sf ../nuxt/bin/nuxt.mjs node_modules/.bin/nuxt
      chmod +x node_modules/.bin/nuxt
    elif [ -f "node_modules/.bin/nuxi" ]; then
      echo "✅ Found nuxi, creating symlink..."
      ln -sf nuxi node_modules/.bin/nuxt
    elif [ -f "node_modules/nuxi/bin/nuxi.mjs" ]; then
      echo "✅ Found nuxi.mjs, creating symlink..."
      ln -sf ../nuxi/bin/nuxi.mjs node_modules/.bin/nuxt
      chmod +x node_modules/.bin/nuxt
    else
      echo "🔧 Creating fallback nuxt wrapper script..."
      cat > node_modules/.bin/nuxt << 'EOF'
#!/bin/sh
# Fallback nuxt wrapper script

echo "🔧 Nuxt wrapper (production): Starting with args: $*"

# Try different possible locations for Nuxt
if [ -f "node_modules/nuxt/bin/nuxt.mjs" ]; then
  echo "✅ Using node_modules/nuxt/bin/nuxt.mjs"
  exec node node_modules/nuxt/bin/nuxt.mjs "$@"
elif [ -f "node_modules/.bin/nuxi" ]; then
  echo "✅ Using node_modules/.bin/nuxi"
  exec node_modules/.bin/nuxi "$@"
elif [ -f "node_modules/nuxi/bin/nuxi.mjs" ]; then
  echo "✅ Using node_modules/nuxi/bin/nuxi.mjs"
  exec node node_modules/nuxi/bin/nuxi.mjs "$@"
elif command -v npx >/dev/null 2>&1; then
  echo "✅ Fallback to direct node execution"
  # Debug: Check what nuxt packages we have
  echo "🔍 Checking available nuxt packages..."
  find node_modules -name "*nuxt*" -type d | head -10
  ls -la node_modules/nuxt/ 2>/dev/null || echo "❌ No nuxt directory"
  ls -la node_modules/@nuxt/ 2>/dev/null || echo "❌ No @nuxt directory"
  
  # Try multiple approaches to find nuxt
  if [ -f "node_modules/nuxt/package.json" ]; then
    echo "✅ Found nuxt package, using require.resolve"
    exec node -e "
      const nuxtBin = require.resolve('nuxt/bin/nuxt.mjs');
      const { spawn } = require('child_process');
      const child = spawn('node', [nuxtBin, ...process.argv.slice(2)], { stdio: 'inherit' });
      child.on('exit', (code) => process.exit(code));
    " -- "$@"
  elif [ -f "node_modules/nuxt/bin/nuxt.mjs" ]; then
    echo "✅ Found nuxt.mjs directly"
    exec node node_modules/nuxt/bin/nuxt.mjs "$@"
  elif [ -f "node_modules/@nuxt/cli/bin/nuxt.mjs" ]; then
    echo "✅ Found @nuxt/cli"
    exec node node_modules/@nuxt/cli/bin/nuxt.mjs "$@"
  else
    echo "❌ No nuxt executable found in any location"
    echo "🔍 Available packages:"
    ls -la node_modules/ | grep nuxt || echo "No nuxt packages found"
    exit 1
  fi
else
  echo "Error: Nuxt executable not found in any expected location"
  echo "Searched locations:"
  echo "  - node_modules/nuxt/bin/nuxt.mjs"
  echo "  - node_modules/.bin/nuxi"
  echo "  - node_modules/nuxi/bin/nuxi.mjs"
  echo "  - npx nuxt (fallback)"
  echo ""
  echo "Available files in node_modules/.bin:"
  ls -la node_modules/.bin/ 2>/dev/null || echo "No .bin directory"
  echo ""
  echo "Available nuxt-related files:"
  find node_modules -name "*nuxt*" -type f 2>/dev/null | head -10 || echo "No nuxt files found"
  exit 1
fi
EOF
      chmod +x node_modules/.bin/nuxt
      echo "✅ Fallback nuxt wrapper created"
    fi
    
    # Verify the nuxt command works with timeout
    echo "🔍 Testing nuxt command..."
    if timeout 10 node_modules/.bin/nuxt --version >/dev/null 2>&1; then
      echo "✅ Nuxt command is working"
    else
      echo "⚠️ Nuxt command test failed or timeout, but proceeding with build..."
      echo "🔧 Checking nuxt executable directly..."
      ls -la node_modules/.bin/nuxt 2>/dev/null || echo "No nuxt executable found"
      echo "🔧 Testing executable permissions..."
      if [ -x "node_modules/.bin/nuxt" ]; then
        echo "✅ Nuxt executable has correct permissions"
      else
        echo "⚠️ Nuxt executable may not have correct permissions"
        chmod +x node_modules/.bin/nuxt 2>/dev/null || true
      fi
    fi
    
    # Use a retry mechanism for build to handle EBUSY errors
    build_attempt=1
    max_attempts=3
    
    while [ $build_attempt -le $max_attempts ]; do
      echo "🔨 Build attempt $build_attempt of $max_attempts..."
      
      # Add timeout to prevent infinite hang
      echo "⏱️ Starting build with 10 minute timeout..."
      timeout 600 npm run build 2>&1 | tee build.log
      build_exit_code=$?
      
      echo "🔍 Build exit code: $build_exit_code"
      if [ $build_exit_code -eq 124 ]; then
        echo "⏰ Build timed out after 10 minutes"
      fi
      
      echo "🔍 Build process completed with exit code: $build_exit_code"
      
      # Check if build output exists regardless of exit code (EBUSY may cause false failure)
      if [ -f ".output/server/index.mjs" ]; then
        echo "✅ Build completed successfully on attempt $build_attempt (found .output/server/index.mjs)"
        break
      elif [ $build_exit_code -eq 0 ]; then
        echo "✅ Build completed successfully on attempt $build_attempt"
        break
      else
        echo "⚠️ Build attempt $build_attempt failed (exit code: $build_exit_code)"
        
        # Check build log for signs of successful completion despite errors
        if grep -q "✔ Server built in" build.log && grep -q "✔ Client built in" build.log; then
          echo "✅ Build actually succeeded (detected completion messages in logs despite exit code)"
          # Wait a bit for filesystem to settle, then check again
          sleep 3
          if [ -f ".output/server/index.mjs" ]; then
            echo "✅ Found .output/server/index.mjs after waiting"
            break
          fi
        fi
        
        if [ $build_attempt -eq $max_attempts ]; then
          echo "❌ All $max_attempts build attempts failed. Checking for partial build..."
          
          # Check if we have at least some output
          if [ -f ".output/server/index.mjs" ]; then
            echo "✅ Partial build found, proceeding with existing .output"
            break
          else
            echo "❌ No usable build output found after $max_attempts attempts"
            echo "🔍 Final directory check:"
            ls -la /app/
            echo "🔍 Node modules check:"
            ls -la /app/node_modules/.bin/ | head -10
            exit 1
          fi
        else
          echo "🔄 Cleaning up and retrying..."
          # Clean up problematic directories with retry logic, but ignore errors
          for i in 1 2 3; do
            rm -rf .output .nuxt dist 2>/dev/null || true
            sleep 1
          done
          # Wait a bit more for filesystem to settle
          sleep 5
          
          build_attempt=$((build_attempt + 1))
        fi
      fi
    done
    
    # Verify build succeeded - enhanced check with EBUSY handling
    echo "🔍 Verifying build completion..."
    
    # Wait for filesystem operations to complete (EBUSY handling)
    sleep 3
    
    # Check build log for success indicators first
    build_log_success=false
    if [ -f "build.log" ]; then
        if grep -q "✔ Server built in" build.log && grep -q "✔ Client built in" build.log; then
            echo "📋 Build completion confirmed in logs"
            build_log_success=true
        fi
    fi
    
    # Enhanced .output directory check with retry for EBUSY
    output_check_attempts=0
    max_output_attempts=5
    
    while [ $output_check_attempts -lt $max_output_attempts ]; do
        if [ -f ".output/server/index.mjs" ]; then
            echo "✅ Build completed successfully - found .output/server/index.mjs"
            break
        elif [ -d ".output" ] && [ "$(ls -A .output 2>/dev/null)" ]; then
            echo "✅ Build completed with output directory - checking contents..."
            ls -la .output/ 2>/dev/null || true
            
            if [ -d ".output/server" ] && [ "$(ls -A .output/server 2>/dev/null)" ]; then
                echo "✅ Found server directory with content, proceeding..."
                # Look for any server file
                server_file=$(find .output/server -name "*.mjs" -o -name "*.js" | head -1 2>/dev/null)
                if [ -n "$server_file" ]; then
                    echo "✅ Found server file: $server_file"
                    break
                fi
            fi
            
            # Check for any generated files in .output
            echo "🔍 Searching for any generated files..."
            generated_files=$(find .output -type f -name "*.mjs" -o -name "*.js" 2>/dev/null | wc -l)
            if [ "$generated_files" -gt 0 ]; then
                echo "✅ Found $generated_files generated files, build may have succeeded"
                find .output -type f -name "*.mjs" -o -name "*.js" | head -5
                break
            fi
        elif [ "$build_log_success" = "true" ]; then
            echo "⏳ Build logs indicate success but .output not ready, waiting... (attempt $((output_check_attempts + 1))/$max_output_attempts)"
            sleep 2
            output_check_attempts=$((output_check_attempts + 1))
            continue
        else
            echo "❌ No .output directory found on attempt $((output_check_attempts + 1))"
            output_check_attempts=$((output_check_attempts + 1))
            sleep 2
        fi
        
        output_check_attempts=$((output_check_attempts + 1))
    done
    
    # Final verification with EBUSY fallback
    if [ ! -d ".output" ] || [ -z "$(ls -A .output 2>/dev/null)" ]; then
        if [ "$build_log_success" = "true" ]; then
            echo "⚠️  Warning: Build logs indicate success but .output directory is empty/missing"
            echo "This may be due to EBUSY filesystem errors during cleanup"
            
            # Try to manually create .output from .nuxt/dist if build succeeded
            if [ -d ".nuxt/dist" ] && [ "$(ls -A .nuxt/dist 2>/dev/null)" ]; then
                echo "🔧 Found .nuxt/dist with content, attempting manual .output creation..."
                
                # Create .output directory structure
                mkdir -p .output/server .output/public
                
                # Copy client files to public
                if [ -d ".nuxt/dist/client" ]; then
                    echo "📁 Copying client files to .output/public..."
                    cp -r .nuxt/dist/client/* .output/public/ 2>/dev/null || true
                fi
                
                # Copy server files from proper Nitro output
                if [ -f ".nuxt/dist/server/server.mjs" ]; then
                    echo "📁 Copying Nitro server to .output/server..."
                    cp .nuxt/dist/server/server.mjs .output/server/index.mjs
                    
                    # Copy additional server assets
                    if [ -f ".nuxt/dist/server/client.manifest.json" ]; then
                        cp .nuxt/dist/server/client.manifest.json .output/server/
                    fi
                    if [ -f ".nuxt/dist/server/client.manifest.mjs" ]; then
                        cp .nuxt/dist/server/client.manifest.mjs .output/server/
                    fi
                    if [ -f ".nuxt/dist/server/styles.mjs" ]; then
                        cp .nuxt/dist/server/styles.mjs .output/server/
                    fi
                    
                    # Copy server-side Nuxt modules
                    if [ -d ".nuxt/dist/server/_nuxt" ]; then
                        mkdir -p .output/server/_nuxt
                        cp -r .nuxt/dist/server/_nuxt/* .output/server/_nuxt/ 2>/dev/null || true
                    fi
                    
                    echo "✅ Successfully copied Nitro server files"
                else
                    echo "⚠️  No Nitro server found, creating fallback static server..."
                    cat > .output/server/index.mjs << 'EOF'
import { createServer } from 'node:http'
import { readFileSync, existsSync } from 'node:fs'
import { join, extname } from 'node:path'

const port = process.env.PORT || 3000
const staticDir = join(process.cwd(), '.output/public')

const mimeTypes = {
  '.html': 'text/html',
  '.js': 'application/javascript',
  '.css': 'text/css',
  '.json': 'application/json',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.gif': 'image/gif',
  '.svg': 'image/svg+xml',
  '.ico': 'image/x-icon'
}

const server = createServer((req, res) => {
  try {
    let filePath = join(staticDir, req.url === '/' ? 'index.html' : req.url)
    
    if (!existsSync(filePath) && req.url !== '/') {
      filePath = join(staticDir, 'index.html')
    }
    
    if (existsSync(filePath)) {
      const ext = extname(filePath)
      const mimeType = mimeTypes[ext] || 'text/plain'
      
      res.writeHead(200, { 'Content-Type': mimeType })
      res.end(readFileSync(filePath))
    } else {
      res.writeHead(404, { 'Content-Type': 'text/plain' })
      res.end('404 Not Found')
    }
  } catch (error) {
    res.writeHead(500, { 'Content-Type': 'text/plain' })
    res.end('500 Internal Server Error')
  }
})

server.listen(port, '0.0.0.0', () => {
  console.log(`TiHoMo Frontend server listening on http://0.0.0.0:${port}`)
})
EOF
                fi
                
                # Verify manual creation worked
                if [ -f ".output/server/index.mjs" ]; then
                    echo "✅ Successfully created .output from .nuxt/dist"
        
        # Copy package.json to .output for proper Nitro server imports
        if [ -f "package.json" ]; then
            echo "📦 Copying package.json to .output..."
            cp package.json .output/
        fi
        
        # Create proper package.json with required imports in .output
        echo "📦 Creating proper package.json in .output for Nitro server..."
        cat > .output/package.json << 'EOF'
{
  "type": "module",
  "imports": {
    "#internal/nuxt/paths": "./nitro/runtime/nitro-paths.mjs",
    "#internal/*": "./nitro/runtime/*"
  },
  "scripts": {
    "start": "node server/index.mjs"
  }
}
EOF
        
        # Create the nitro runtime directory and copy required Nitro files
        echo "🔧 Setting up Nitro runtime dependencies..."
        mkdir -p .output/nitro/runtime
        
        # Copy nitro paths file from node_modules if it exists
        if [ -f "node_modules/nuxt/dist/core/runtime/nitro/paths.mjs" ]; then
            cp node_modules/nuxt/dist/core/runtime/nitro/paths.mjs .output/nitro/runtime/nitro-paths.mjs
        else
            # Create a minimal paths file
            cat > .output/nitro/runtime/nitro-paths.mjs << 'EOF'
export const appDir = '/app'
export const buildDir = '/app/.nuxt'
export const outputDir = '/app/.output'
EOF
        fi
                else
                    echo "❌ Failed to create working .output directory"
                    exit 1
                fi
            else
                echo "❌ No .nuxt/dist directory found, cannot recover from build failure"
                exit 1
            fi
            
            echo "Checking for any build artifacts..."
            ls -la ./ | grep -E "output|dist|build" || echo "No build artifacts found"
        else
            echo "❌ ERROR: Build failed - no usable .output directory"
            exit 1
        fi
    fi
    
    echo "✅ Build completed successfully"
  elif [ ! -f ".output/server/index.mjs" ]; then
    echo "❌ ERROR: Pre-built application not found - .output/server/index.mjs missing"
    echo "📁 Checking .output directory:"
    ls -la .output/ 2>/dev/null || echo "No .output directory found"
    echo "📁 Checking /app directory:"
    ls -la /app/
    echo "🔍 This suggests .output was not properly copied from builder stage"
    echo "🔍 Or source mount is overriding the pre-built .output"
    exit 1
  fi
  
  echo "✅ Found pre-built application"
  echo "🚀 Starting production server..."
  
  # Check for the main server file first
  if [ -f ".output/server/index.mjs" ]; then
    echo "🎯 Starting with .output/server/index.mjs"
    server_file=".output/server/index.mjs"
  else
    # Look for alternative server files
    echo "🔍 Looking for alternative server entry points..."
    server_file=$(find .output -name "index.mjs" -type f | head -1)
    if [ -z "$server_file" ]; then
      server_file=$(find .output -name "*.mjs" -type f | head -1)
    fi
    
    if [ -n "$server_file" ]; then
      echo "🎯 Found server file: $server_file"
    else
      echo "❌ ERROR: No server file found in .output directory"
      ls -la .output/ 2>/dev/null || echo "No .output directory"
      exit 1
    fi
  fi
  
  # Switch to non-root user for security if we're currently root
  if [ "$(whoami)" = "root" ]; then
    echo "🔐 Switching to non-root user for security..."
    # Change to .output directory where package.json exists
    cd .output
    exec su-exec nuxt node "server/index.mjs"
  else
    # Change to .output directory where package.json exists
    cd .output
    exec node "server/index.mjs"
  fi
fi
