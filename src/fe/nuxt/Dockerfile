# Dockerfile for TiHoMo Nuxt Frontend
# Multi-stage build để tối ưu hóa kích thước image và support development mode

# Stage 1: Build stage
FROM node:20-alpine AS builder

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install necessary build tools for native dependencies
RUN apk add --no-cache python3 make g++ curl && \
    ln -sf python3 /usr/bin/python

# Remove potential problematic files and install dependencies
# This addresses the npm optional dependencies bug with rollup and esbuild version conflicts
RUN rm -rf node_modules package-lock.json 2>/dev/null || true && \
    npm cache clean --force && \
    # Set npm to use exact versions for esbuild to avoid conflicts
    npm config set fund false && \
    npm config set audit false && \
    # Install dependencies with platform-specific handling for Alpine Linux
    # Note: We need optional dependencies for native modules like @rollup/rollup-linux-x64-musl
    npm install --legacy-peer-deps && \
    # Ensure rollup native binaries are properly installed for musl
    npm install @rollup/rollup-linux-x64-musl --save-optional && \
    # Clear any esbuild cache that might cause version conflicts
    rm -rf ~/.cache/esbuild 2>/dev/null || true && \
    # Force clean install of esbuild to ensure version consistency
    npm uninstall esbuild && npm install esbuild@0.23.1

# Copy source code
COPY . .

# Build the application
RUN npm run build

# Stage 2: Development stage (for development mode)
FROM node:20-alpine AS development

# Set working directory  
WORKDIR /app

# Install curl for health checks and su-exec for user switching
RUN apk add --no-cache curl python3 make g++ su-exec && \
    ln -sf python3 /usr/bin/python

# Copy package files
COPY package*.json ./

# Install all dependencies including dev dependencies for development
RUN npm install --legacy-peer-deps

# Create non-root user for security
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nuxt -u 1001

# Copy entrypoint script
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Change ownership of app directory
RUN chown -R nuxt:nodejs /app

# Create .nuxt directory with proper permissions
RUN mkdir -p /app/.nuxt /app/.output /app/logs /app/uploads && \
    chown -R nuxt:nodejs /app/.nuxt /app/.output /app/logs /app/uploads && \
    chmod -R 755 /app/.nuxt /app/.output /app/logs /app/uploads

# For development mode, we'll handle user switching in entrypoint
# to avoid permission issues with volume mounts
# USER nuxt

# Expose port
EXPOSE 3000

# Set development environment variables
ENV NODE_ENV=development
ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=3000

# Start the application in development mode using our entrypoint script
ENTRYPOINT ["/docker-entrypoint.sh"]

# Stage 3: Production stage
FROM node:20-alpine AS production

# Set working directory
WORKDIR /app

# Install curl for health checks and su-exec for user switching
RUN apk add --no-cache curl su-exec

# Create non-root user for security
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nuxt -u 1001

# Copy built application from builder stage
COPY --from=builder --chown=nuxt:nodejs /app/.output /app/.output

# Copy package.json and package-lock.json for potential rebuild scenarios
COPY --from=builder --chown=nuxt:nodejs /app/package.json /app/package.json
COPY --from=builder --chown=nuxt:nodejs /app/package-lock.json /app/package-lock.json

# Copy node_modules for fallback rebuild capability
COPY --from=builder --chown=nuxt:nodejs /app/node_modules /app/node_modules

# Copy entrypoint script
COPY --chown=nuxt:nodejs docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Create necessary directories with proper ownership - split for debugging
RUN mkdir -p /app/logs /app/uploads /app/.nuxt

# Debug: Check what we have before ownership change
RUN echo "[DEBUG] Contents before ownership change:" && \
    ls -la /app/ && \
    echo "[DEBUG] node_modules size:" && \
    du -sh /app/node_modules 2>/dev/null || echo "No node_modules found"

# Skip recursive ownership change to avoid hanging - run as root if needed
# Production containers often run as root anyway for simplicity
RUN echo "[INFO] Skipping ownership change to avoid build hang" && \
    chmod 755 /app /app/logs /app/uploads /app/.nuxt 2>/dev/null || true

# Verify build output exists (debugging step)
RUN ls -la /app/.output/ && \
    ls -la /app/.output/server/ && \
    test -f /app/.output/server/index.mjs || \
    (echo "ERROR: index.mjs not found after copy" && exit 1)

# Note: We copy node_modules for fallback rebuild scenarios
# When source is mounted, container may need to rebuild if .output is missing

# Run as root to avoid permission issues during deployment
# Entrypoint will handle user switching if needed

# Expose port
EXPOSE 3000

# Set production environment variables
ENV NODE_ENV=production
ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=3000

# Start the application using our entrypoint script
ENTRYPOINT ["/docker-entrypoint.sh"]
