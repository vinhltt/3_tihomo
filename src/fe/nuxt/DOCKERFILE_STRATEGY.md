# Docker Strategy for TiHoMo Frontend

## Current Setup

### Primary: `Dockerfile` (Optimized Single-Stage)
- **Use Case**: CI/CD, Development, Standard Production
- **Performance**: 4m8s build time (85% improvement)
- **Architecture**: Single-stage optimized
- **Platform**: linux/amd64 only
- **Image Size**: ~500MB (includes build tools)

### Alternative: `Dockerfile.multistage` (Multi-Stage)
- **Use Case**: Production (size-optimized), Multi-platform
- **Performance**: ~27m build time (multi-stage overhead)
- **Architecture**: 3-stage (dependencies → build → runtime)
- **Platform**: linux/amd64,linux/arm64
- **Image Size**: ~200MB (runtime only)

## When to Use Which

### Use `Dockerfile` for:
- ✅ **CI/CD workflows** (GitHub Actions)
- ✅ **Development builds**
- ✅ **Standard production** (single platform)
- ✅ **Fast iteration** cycles
- ✅ **Default choice** (recommended)

### Use `Dockerfile.multistage` for:
- 📦 **Production** (size-critical environments)
- 📦 **Multi-platform** deployments (ARM64 support)
- 📦 **Security-hardened** environments
- 📦 **Resource-constrained** deployments
- 📦 **Enterprise compliance** requirements

## Switching Strategy

### To Multi-Stage Dockerfile:
```bash
# Update workflow file
sed -i 's/Dockerfile/Dockerfile.multistage/g' .github/workflows/build-frontend.yml

# Add multi-platform support
platforms: linux/amd64,linux/arm64
```

### Performance Comparison:
```
Dockerfile:           4m8s  → ⭐⭐⭐⭐⭐ (Recommended)
Dockerfile.multistage: 27m → ⭐⭐ (Use when size matters)
```

## Maintenance

### Keep Both Files:
- **Primary**: `Dockerfile` (optimized single-stage)
- **Backup**: `Dockerfile.multistage` (production alternative)
- **Legacy**: `Dockerfile.fast` (can be removed after verification)
- **Sync**: Keep both files updated with dependencies

### Review Schedule:
- **Monthly**: Compare image sizes và performance
- **Quarterly**: Evaluate if multi-platform is needed
- **Release**: Consider switching to `Dockerfile.multistage` for production releases

## Current Recommendation

**✅ Continue using `Dockerfile`** for all current workflows.

**📦 Keep `Dockerfile.multistage`** as tested alternative for future production needs.
