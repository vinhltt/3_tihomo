# Manual Deployment Workflow Guide

## 🎯 Overview

TiHoMo deployment system hiện tại được thiết kế để **chỉ chạy manual trigger**. Không có auto deployment khi push code để đảm bảo kiểm soát tốt hơn việc deploy production.

## 🚀 Deployment Workflows

### 1. **Main Orchestrator** (Recommended)

**Workflow:** `deploy-orchestrator.yml`

**Cách chạy:**
```bash
# Via GitHub CLI
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=full-system \
  -f environment=production \
  -f force_rebuild=false

# Via GitHub Web UI
Actions → TiHoMo Deployment Orchestrator → Run workflow
```

**Options:**
- `deployment_type`: 
  - `full-system` - Deploy tất cả (Infrastructure → Backend → Frontend)
  - `infrastructure-only` - Chỉ databases, Redis, RabbitMQ
  - `backend-only` - Chỉ APIs và Gateway
  - `frontend-only` - Chỉ Nuxt frontend
  - `specific-service` - Services cụ thể
- `environment`: `development|staging|production`
- `specific_services`: Nếu chọn specific-service (e.g., "identity-api,frontend-nuxt")
- `force_rebuild`: `true|false`
- `skip_health_checks`: `true|false`

### 2. **Component-Specific Workflows**

#### A. Infrastructure Only
```bash
gh workflow run deploy-infrastructure.yml \
  -f environment=production \
  -f force_rebuild=false
```

#### B. Backend Services
```bash
gh workflow run deploy-backend-services.yml \
  -f environment=production \
  -f services="identity-api,corefinance-api" \
  -f force_rebuild=false
```

#### C. Frontend Only
```bash
gh workflow run deploy-frontend-ghcr.yml \
  -f environment=production \
  -f force_rebuild=false
```

## 📋 Deployment Scenarios

### Scenario 1: **Full Production Deployment**
```bash
# Complete system deployment
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=full-system \
  -f environment=production \
  -f force_rebuild=false \
  -f skip_health_checks=false
```

**Timeline:**
1. ✅ Infrastructure (5-8 phút)
2. ✅ Backend APIs (8-12 phút)  
3. ✅ Frontend (5-8 phút)
4. ✅ Health Check (2-3 phút)
5. 📨 Discord notification

**Total: ~20-30 phút**

### Scenario 2: **Quick Backend Fix**
```bash
# Fix bug trong Identity API
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=specific-service \
  -f specific_services="identity-api" \
  -f environment=production \
  -f force_rebuild=true
```

**Timeline:**
1. ✅ Stop dependents (corefinance-api, gateway, frontend)
2. ✅ Deploy identity-api (3-5 phút)
3. ✅ Restart dependents (2-3 phút)
4. 📨 Discord notification

**Total: ~5-8 phút**

### Scenario 3: **Frontend Update**
```bash
# Update UI changes
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=frontend-only \
  -f environment=production \
  -f force_rebuild=true
```

**Timeline:**
1. ✅ Check backend dependencies
2. ✅ Stop frontend gracefully
3. ✅ Build & deploy new frontend (5-8 phút)
4. ✅ Health check
5. 📨 Discord notification

**Total: ~6-10 phút**

### Scenario 4: **Database Migration**
```bash
# Step 1: Deploy infrastructure with new schema
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=infrastructure-only \
  -f environment=production \
  -f force_rebuild=true

# Step 2: Deploy backend with new code
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=backend-only \
  -f environment=production \
  -f force_rebuild=true

# Step 3: Deploy frontend if needed
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=frontend-only \
  -f environment=production
```

## 🔄 Typical Development Workflow

### 1. **Development Environment**
```bash
# Test changes on development first
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=full-system \
  -f environment=development \
  -f force_rebuild=true
```

### 2. **Staging Environment** (if available)
```bash
# Deploy to staging for final testing
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=full-system \
  -f environment=staging \
  -f force_rebuild=false
```

### 3. **Production Deployment**
```bash
# Deploy to production after testing
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=full-system \
  -f environment=production \
  -f force_rebuild=false
```

## ⚠️ Pre-Deployment Checklist

### Before Any Production Deployment:

- [ ] ✅ Code đã được test trên development environment
- [ ] ✅ All tests passing locally
- [ ] 🔍 Review code changes carefully
- [ ] 💾 Database backup completed (auto trong workflow)
- [ ] 📱 Team được thông báo về deployment
- [ ] 🕐 Deploy trong giờ làm việc (tránh cuối tuần/đêm)
- [ ] 👥 Có người monitor sau deployment

### Environment Variables Check:
- [ ] GitHub Secrets được cập nhật
- [ ] GitHub Variables đúng cho environment
- [ ] Discord webhook hoạt động
- [ ] SSH keys và Cloudflared access OK

## 📊 Monitoring & Notifications

### Discord Notifications Include:
- ✅/❌ Overall deployment status
- 📊 Individual component status (Infrastructure, Backend, Frontend)
- 🔗 Service access URLs
- 📋 Link to detailed GitHub Actions logs
- ⏱️ Deployment duration

### Post-Deployment Monitoring:
1. **Health Check URLs:**
   - Frontend: `http://<TRUENAS_IP>:3500`
   - API Gateway: `http://<TRUENAS_IP>:5000/health`
   - Identity API: `http://<TRUENAS_IP>:5801/health`
   - CoreFinance API: `http://<TRUENAS_IP>:5802/health`

2. **Grafana Dashboard:** `http://<TRUENAS_IP>:3002`
3. **RabbitMQ Management:** `http://<TRUENAS_IP>:15672`

## 🚨 Emergency Procedures

### 1. **Rollback Strategy**
```bash
# Option 1: Deploy previous commit
git log --oneline -10  # Find previous good commit
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=full-system \
  -f environment=production

# Option 2: Restore from backup (manually on server)
# Option 3: Deploy specific service only
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=specific-service \
  -f specific_services="identity-api"
```

### 2. **Common Issues & Solutions**

**Issue: Workflow fails on infrastructure**
```bash
# Solution: Deploy infrastructure separately
gh workflow run deploy-infrastructure.yml \
  -f environment=production \
  -f force_rebuild=true
```

**Issue: Backend service won't start**
```bash
# Solution: Check dependencies first
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=infrastructure-only

# Then deploy backend
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=backend-only
```

**Issue: Frontend can't connect to backend**
```bash
# Solution: Verify backend health, then redeploy frontend
gh workflow run deploy-frontend-ghcr.yml \
  -f environment=production \
  -f skip_backend_check=false
```

## 🎯 Best Practices

### 1. **Deployment Frequency**
- **Development:** Deploy often (daily/multiple times per day)
- **Production:** Deploy during business hours, max 1-2 times per day
- **Emergency fixes:** Can deploy anytime with team notification

### 2. **Change Management**
- Small, incremental changes
- Test thoroughly on development first
- Document changes in commit messages
- Communicate with team before production deployments

### 3. **Resource Management**
- Use `force_rebuild=false` for normal deployments (faster)
- Use `force_rebuild=true` only when needed (slower but ensures fresh build)
- Monitor resource usage after deployments

### 4. **Security**
- All deployments logged và tracked
- Environment variables secured in GitHub Secrets
- Access controlled via GitHub permissions
- Discord notifications provide audit trail

---

## 📞 Support & Troubleshooting

**For deployment issues:**
1. Check GitHub Actions logs
2. Check Discord notifications
3. Monitor Grafana dashboard
4. Check individual service logs on TrueNAS
5. Contact system administrator if needed

**Deployment successful when:**
- ✅ All workflows complete successfully
- ✅ Health checks pass
- ✅ Services accessible via URLs
- ✅ No errors in logs
- ✅ Discord notification shows success

Remember: **Manual deployment = Better control = More reliable production deployments** 🎯