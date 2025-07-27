# TiHoMo Frontend Deployment Troubleshooting

## Lỗi rsync code 23 - Giải pháp

### Nguyên nhân chính
- **rsync error code 23**: Một số files/attributes không thể transfer được
- Thường do vấn đề permissions, file locks, hoặc filesystem conflicts
- Trong trường hợp này: conflict giữa Docker bind mounts và build directories

### Giải pháp đã triển khai

#### 1. Tạo .dockerignore
```bash
# Đã tạo src/fe/nuxt/.dockerignore để loại trừ:
- node_modules/
- .nuxt/, .output/, dist/, .cache/
- Build và cache directories
- IDE files, logs, temporary files
```

#### 2. Cập nhật docker-compose.yml
```yaml
volumes:
  # Thêm :delegated flag cho performance
  - ${FRONTEND_SOURCE_MOUNT:-./src/fe/nuxt}:/app:delegated
  # Exclude problematic directories
  - /app/.nuxt
  - /app/.output  
  - /app/dist
  - /app/.cache
```

#### 3. Scripts khắc phục tự động

**Windows**: `fix-frontend-deployment.bat`
**Linux/Mac**: `fix-frontend-deployment.sh`

Các scripts này sẽ:
- Stop và remove problematic containers
- Clean up build directories
- Fix file permissions
- Rebuild với --no-cache
- Restart services

### Cách sử dụng

#### Option A: Chạy script tự động
```bash
# Windows
fix-frontend-deployment.bat

# Linux/Mac  
chmod +x fix-frontend-deployment.sh
./fix-frontend-deployment.sh
```

#### Option B: Thực hiện thủ công
```bash
# 1. Stop service
docker-compose stop frontend-nuxt

# 2. Clean up
docker-compose rm -f frontend-nuxt
docker volume rm 3_tihomo_frontend_node_modules

# 3. Remove build directories
rm -rf src/fe/nuxt/.nuxt src/fe/nuxt/.output src/fe/nuxt/dist src/fe/nuxt/.cache

# 4. Rebuild
docker-compose build --no-cache frontend-nuxt

# 5. Start
docker-compose up -d frontend-nuxt
```

### Kiểm tra kết quả
```bash
# Check service status
docker-compose ps frontend-nuxt

# Check logs
docker-compose logs --tail=20 frontend-nuxt

# Test access
curl http://localhost:3500
```

### Troubleshooting thêm

#### Nếu vẫn gặp lỗi:
1. **Permission issues**: Chạy Docker Desktop với admin privileges
2. **Port conflicts**: Kiểm tra port 3500 có bị chiếm không
3. **Resource limits**: Tăng memory/CPU limit cho Docker Desktop
4. **Windows path issues**: Sử dụng WSL2 backend thay vì Hyper-V

#### Logs quan trọng:
```bash
# Container logs
docker-compose logs frontend-nuxt

# Docker build logs
docker-compose build --no-cache frontend-nuxt

# System resources
docker system df
docker stats
```

### Environment Variables quan trọng
```bash
# Có thể set để debug
FRONTEND_SOURCE_MOUNT=./src/fe/nuxt
NODE_ENV=development
FRONTEND_PORT=3500
NUXT_DEBUG=true
```

### Khi nào cần rebuild complete:
- Thay đổi package.json
- Thay đổi Dockerfile
- Thay đổi docker-compose.yml
- Sau khi clean install dependencies
- Khi chuyển giữa development/production modes
