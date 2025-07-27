# CORS Configuration với Environment Variables

## Tổng quan

Document này hướng dẫn cách cấu hình CORS (Cross-Origin Resource Sharing) cho TiHoMo CoreFinance API thông qua environment variables, đặc biệt hữu ích cho GitHub Actions deployment.

## Biến môi trường CORS

### Core CORS Settings

| Biến môi trường | Mô tả | Giá trị mặc định | Ví dụ |
|---|---|---|---|
| `CORS_POLICY_NAME` | Tên policy CORS | `DefaultCorsPolicy` | `ProductionCorsPolicy` |
| `CORS_ALLOWED_ORIGINS` | Danh sách origins được phép | `*` | `https://app.tihomo.com,https://admin.tihomo.com` |
| `CORS_ALLOWED_METHODS` | Danh sách HTTP methods được phép | `*` | `GET,POST,PUT,DELETE,OPTIONS` |
| `CORS_ALLOWED_HEADERS` | Danh sách headers được phép | `*` | `Content-Type,Authorization,X-Requested-With` |
| `CORS_EXPOSED_HEADERS` | Headers được expose cho client | `Token-Expired` | `Token-Expired,X-Total-Count` |
| `CORS_PREFLIGHT_MAX_AGE` | Thời gian cache preflight (phút) | `10` | `60` |

## Cấu hình cho các môi trường khác nhau

### Development Environment
```bash
# .env.development
CORS_POLICY_NAME=DevelopmentCorsPolicy
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:3500
CORS_ALLOWED_METHODS=*
CORS_ALLOWED_HEADERS=*
CORS_EXPOSED_HEADERS=Token-Expired
CORS_PREFLIGHT_MAX_AGE=5
```

### Staging Environment
```bash
# .env.staging
CORS_POLICY_NAME=StagingCorsPolicy
CORS_ALLOWED_ORIGINS=https://staging.tihomo.com
CORS_ALLOWED_METHODS=GET,POST,PUT,DELETE,OPTIONS
CORS_ALLOWED_HEADERS=Content-Type,Authorization,X-Requested-With
CORS_EXPOSED_HEADERS=Token-Expired,X-Total-Count
CORS_PREFLIGHT_MAX_AGE=30
```

### Production Environment
```bash
# .env.production
CORS_POLICY_NAME=ProductionCorsPolicy
CORS_ALLOWED_ORIGINS=https://app.tihomo.com,https://admin.tihomo.com
CORS_ALLOWED_METHODS=GET,POST,PUT,DELETE,OPTIONS
CORS_ALLOWED_HEADERS=Content-Type,Authorization,X-Requested-With,X-API-Key
CORS_EXPOSED_HEADERS=Token-Expired,X-Total-Count,X-Rate-Limit-Remaining
CORS_PREFLIGHT_MAX_AGE=60
```

## GitHub Actions Configuration

### Secret Variables
Trong GitHub repository settings, tạo các secrets sau:

```yaml
# Development
CORS_ALLOWED_ORIGINS_DEV: "http://localhost:3000,http://localhost:3500"

# Staging  
CORS_ALLOWED_ORIGINS_STAGING: "https://staging.tihomo.com"

# Production
CORS_ALLOWED_ORIGINS_PROD: "https://app.tihomo.com,https://admin.tihomo.com"
```

### Workflow Configuration

```yaml
# .github/workflows/deploy.yml
name: Deploy TiHoMo CoreFinance API

on:
  push:
    branches: [main, develop, staging]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Set environment variables
        run: |
          if [[ "${{ github.ref }}" == "refs/heads/main" ]]; then
            echo "CORS_ALLOWED_ORIGINS=${{ secrets.CORS_ALLOWED_ORIGINS_PROD }}" >> $GITHUB_ENV
            echo "CORS_POLICY_NAME=ProductionCorsPolicy" >> $GITHUB_ENV
            echo "CORS_PREFLIGHT_MAX_AGE=60" >> $GITHUB_ENV
          elif [[ "${{ github.ref }}" == "refs/heads/staging" ]]; then
            echo "CORS_ALLOWED_ORIGINS=${{ secrets.CORS_ALLOWED_ORIGINS_STAGING }}" >> $GITHUB_ENV
            echo "CORS_POLICY_NAME=StagingCorsPolicy" >> $GITHUB_ENV
            echo "CORS_PREFLIGHT_MAX_AGE=30" >> $GITHUB_ENV
          else
            echo "CORS_ALLOWED_ORIGINS=${{ secrets.CORS_ALLOWED_ORIGINS_DEV }}" >> $GITHUB_ENV
            echo "CORS_POLICY_NAME=DevelopmentCorsPolicy" >> $GITHUB_ENV
            echo "CORS_PREFLIGHT_MAX_AGE=5" >> $GITHUB_ENV
          fi
          
      - name: Deploy with Docker Compose
        run: |
          docker-compose up -d corefinance-api
        env:
          CORS_ALLOWED_ORIGINS: ${{ env.CORS_ALLOWED_ORIGINS }}
          CORS_POLICY_NAME: ${{ env.CORS_POLICY_NAME }}
          CORS_ALLOWED_METHODS: "GET,POST,PUT,DELETE,OPTIONS"
          CORS_ALLOWED_HEADERS: "Content-Type,Authorization,X-Requested-With"
          CORS_EXPOSED_HEADERS: "Token-Expired,X-Total-Count"
          CORS_PREFLIGHT_MAX_AGE: ${{ env.CORS_PREFLIGHT_MAX_AGE }}
```

## Multiple Origins Configuration

### Sử dụng nhiều origins
```bash
# Cách 1: Sử dụng dấu phẩy phân cách
CORS_ALLOWED_ORIGINS=https://app.tihomo.com,https://admin.tihomo.com,https://mobile.tihomo.com

# Cách 2: Sử dụng array indices (cho docker-compose)
CORS_ALLOWED_ORIGINS__0=https://app.tihomo.com
CORS_ALLOWED_ORIGINS__1=https://admin.tihomo.com  
CORS_ALLOWED_ORIGINS__2=https://mobile.tihomo.com
```

## Testing CORS Configuration

### Kiểm tra CORS response headers
```bash
# Test preflight request
curl -X OPTIONS \
  -H "Origin: https://app.tihomo.com" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: Content-Type,Authorization" \
  http://localhost:8080/api/accounts

# Expected response headers:
# Access-Control-Allow-Origin: https://app.tihomo.com
# Access-Control-Allow-Methods: GET,POST,PUT,DELETE,OPTIONS
# Access-Control-Allow-Headers: Content-Type,Authorization
# Access-Control-Expose-Headers: Token-Expired
# Access-Control-Max-Age: 3600
```

## Troubleshooting

### Common Issues

1. **CORS policy không hoạt động**
   - Kiểm tra environment variables đã được set đúng chưa
   - Verify container restart sau khi thay đổi env vars
   - Check logs: `docker logs tihomo-corefinance-api`

2. **Multiple origins không hoạt động**
   - Đảm bảo sử dụng đúng format với dấu phẩy
   - Không có spaces sau dấu phẩy
   - Check case-sensitive domains

3. **GitHub Actions deployment fails**
   - Verify secrets được set trong repository settings
   - Check workflow syntax
   - Ensure environment variables được export đúng

### Debug Commands

```bash
# Check current environment variables in container
docker exec tihomo-corefinance-api env | grep CORS

# Check application logs for CORS errors
docker logs tihomo-corefinance-api | grep -i cors

# Test CORS from browser console
fetch('http://localhost:8080/api/health', {
  method: 'GET',
  headers: { 'Content-Type': 'application/json' }
}).then(r => console.log(r.headers))
```

## Security Considerations

1. **Production Environment**
   - Không sử dụng wildcard (`*`) cho origins
   - Chỉ định cụ thể các domains được phép
   - Giới hạn methods và headers cần thiết

2. **Staging Environment**  
   - Chỉ cho phép staging domain
   - Log CORS requests để debug

3. **Development Environment**
   - Có thể sử dụng wildcard cho convenience
   - Enable verbose logging

## Related Documentation

- [ASP.NET Core CORS Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/cors)
- [Docker Environment Variables](https://docs.docker.com/compose/environment-variables/)
- [GitHub Actions Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
