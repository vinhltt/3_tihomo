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

# Install curl for health checks
RUN apk add --no-cache curl python3 make g++ && \
    ln -sf python3 /usr/bin/python

# Copy package files
COPY package*.json ./

# Install all dependencies including dev dependencies for development
RUN npm install --legacy-peer-deps

# Create non-root user for security
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nuxt -u 1001

# Change ownership of app directory
RUN chown -R nuxt:nodejs /app

# Switch to non-root user
USER nuxt

# Expose port
EXPOSE 3000

# Set development environment variables
ENV NODE_ENV=development
ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=3000

# Start the application in development mode
CMD ["npm", "run", "dev"]

# Stage 3: Production stage
FROM node:20-alpine AS production

# Set working directory
WORKDIR /app

# Install curl for health checks
RUN apk add --no-cache curl

# Create non-root user for security
RUN addgroup -g 1001 -S nodejs && \
    adduser -S nuxt -u 1001

# Copy built application from builder stage
COPY --from=builder --chown=nuxt:nodejs /app/.output /app/.output

# Copy package.json for runtime dependencies (without package-lock.json to avoid postinstall)
COPY --from=builder --chown=nuxt:nodejs /app/package.json /app/

# Install only production dependencies without running scripts
RUN npm install --only=production --ignore-scripts --legacy-peer-deps && npm cache clean --force

# Switch to non-root user
USER nuxt

# Expose port
EXPOSE 3000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:3000/api/health || curl -f http://localhost:3000/ || exit 1

# Set environment variables
ENV NODE_ENV=production
ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=3000

# Start the application
CMD ["node", ".output/server/index.mjs"]
