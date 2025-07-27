# syntax=docker/dockerfile:1
# Multi-stage Dockerfile for TiHoMo Nuxt Frontend

# Build arguments for metadata
ARG BUILDTIME
ARG VERSION  
ARG COMMIT_SHA

# Base stage with common dependencies
FROM node:20-alpine AS base

WORKDIR /app

# Install ALL system dependencies once (combined để reduce layers)
RUN apk add --no-cache \
    python3 \
    make \
    g++ \
    curl \
    dumb-init \
    && ln -sf python3 /usr/bin/python \
    && rm -rf /var/cache/apk/*

# Copy package files first for better caching
COPY package*.json ./

# Install ALL dependencies once với better caching
RUN --mount=type=cache,target=/root/.npm \
    --mount=type=cache,target=/tmp/npm-cache \
    npm ci --legacy-peer-deps

# Copy source code
COPY . .

# Create non-root user
RUN addgroup -g 1001 -S nuxt && \
    adduser -S nuxt -u 1001 -G nuxt

# Development stage
FROM base AS development

ENV NODE_ENV=development \
    NUXT_DEV_SSR=true \
    NUXT_DEV_TOOLS=true \
    NUXT_DEBUG=true

# Set ownership for development
RUN chown -R nuxt:nuxt /app

USER nuxt

EXPOSE 3000

# Development command - run dev server
CMD ["npm", "run", "dev"]

# Production build stage
FROM base AS build

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

# Build application
RUN npm run build

# Remove dev dependencies và clean up để reduce image size
RUN npm prune --omit=dev && \
    npm cache clean --force && \
    rm -rf /tmp/* /var/tmp/* /root/.npm

# Production stage
FROM base AS production

# Copy built application from build stage
COPY --from=build /app/.output /app/.output
COPY --from=build /app/package*.json /app/

# Install only production dependencies
RUN npm ci --omit=dev --legacy-peer-deps && \
    npm cache clean --force

# Set ownership
RUN chown -R nuxt:nuxt /app

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
