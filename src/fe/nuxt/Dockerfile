# syntax=docker/dockerfile:1
# Optimized Dockerfile for TiHoMo Nuxt Frontend with GHCR best practices

# Build arguments for metadata
ARG BUILDTIME
ARG VERSION  
ARG COMMIT_SHA

# Stage 1: Dependencies
FROM node:20-alpine AS dependencies

WORKDIR /app

# Install system dependencies for native modules
RUN apk add --no-cache \
    python3 \
    make \
    g++ \
    curl \
    && ln -sf python3 /usr/bin/python

# Copy package files
COPY package*.json ./

# Install production dependencies with cache mount
RUN --mount=type=cache,target=/root/.npm \
    --mount=type=cache,target=/tmp/npm-cache-prod \
    npm ci --only=production --legacy-peer-deps

# Stage 2: Build
FROM node:20-alpine AS build

WORKDIR /app

# Install system dependencies
RUN apk add --no-cache \
    python3 \
    make \
    g++ \
    && ln -sf python3 /usr/bin/python

# Copy package files
COPY package*.json ./

# Install all dependencies with cache mount
RUN --mount=type=cache,target=/root/.npm \
    --mount=type=cache,target=/tmp/npm-cache \
    npm ci --legacy-peer-deps

# Copy source code
COPY . .

# Build application with build arguments
ARG BUILDTIME
ARG VERSION
ARG COMMIT_SHA

ENV NUXT_BUILD_TARGET=production \
    NODE_ENV=production \
    NUXT_DEV_SSR=false \
    NUXT_DEV_TOOLS=false \
    NUXT_DEBUG=false

# Add build metadata as environment variables
ENV BUILD_TIME=$BUILDTIME \
    BUILD_VERSION=$VERSION \
    BUILD_COMMIT=$COMMIT_SHA

RUN npm run build

# Stage 3: Runtime
FROM node:20-alpine AS runtime

# Create non-root user
RUN addgroup -g 1001 -S nuxt && \
    adduser -S nuxt -u 1001 -G nuxt

# Install runtime dependencies
RUN apk add --no-cache \
    curl \
    dumb-init \
    && rm -rf /var/cache/apk/*

WORKDIR /app

# Copy production dependencies from dependencies stage
COPY --from=dependencies --chown=nuxt:nuxt /app/node_modules ./node_modules

# Copy built application from build stage
COPY --from=build --chown=nuxt:nuxt /app/.output ./.output

# Copy necessary configuration files
COPY --from=build --chown=nuxt:nuxt /app/package.json ./package.json

# Add build metadata labels
ARG BUILDTIME
ARG VERSION
ARG COMMIT_SHA

LABEL org.opencontainers.image.title="TiHoMo Frontend" \
      org.opencontainers.image.description="Nuxt 3 Frontend for TiHoMo Application" \
      org.opencontainers.image.version=${VERSION} \
      org.opencontainers.image.created=${BUILDTIME} \
      org.opencontainers.image.revision=${COMMIT_SHA} \
      org.opencontainers.image.vendor="TiHoMo Team"

# Switch to non-root user
USER nuxt

# Expose port
EXPOSE 3000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:3000/health || exit 1

# Use dumb-init to handle signals properly
ENTRYPOINT ["dumb-init", "--"]

# Start the application
CMD ["node", ".output/server/index.mjs"]
