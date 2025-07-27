# Migration Checklist - Chuyển từ Deploy cũ sang Deploy mới

## ✅ Pre-Migration Checklist

### 1. **Backup current deployment**
- [ ] File cũ: `deploy-to-truenas.yml` → rename thành `.backup`
- [ ] Test deployment hiện tại hoạt động bình thường
- [ ] Backup database trước khi migration
- [ ] Document current service status

### 2. **Verify new workflows**
- [ ] Test `deploy-infrastructure.yml` trên development
- [ ] Test `deploy-backend-services.yml` trên development  
- [ ] Test `deploy-frontend-ghcr.yml` trên development
- [ ] Test `deploy-orchestrator.yml` full-system trên development

### 3. **Environment variables check**
- [ ] All GitHub Secrets are properly configured
- [ ] All GitHub Variables are properly configured
- [ ] Discord webhook URL is working
- [ ] SSH keys and Cloudflared access working

## 🚀 Migration Steps

### Step 1: Rename old workflow
```bash
cd .github/workflows/
mv deploy-to-truenas.yml deploy-to-truenas.yml.backup
git add .
git commit -m "backup: archive old deployment workflow"
```

### Step 2: Test new system on development
```bash
# Test infrastructure only
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=infrastructure-only \
  -f environment=development \
  -f force_rebuild=false

# Test backend only  
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=backend-only \
  -f environment=development \
  -f force_rebuild=false

# Test frontend only
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=frontend-only \
  -f environment=development \
  -f force_rebuild=false

# Test full system
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=full-system \
  -f environment=development \
  -f force_rebuild=false
```

### Step 3: Verify all components working
- [ ] All databases accessible
- [ ] All APIs responding to health checks
- [ ] Frontend loading and connecting to backend
- [ ] Discord notifications received
- [ ] Logs are properly generated

### Step 4: Production migration
```bash
# Production deployment with new system
gh workflow run deploy-orchestrator.yml \
  -f deployment_type=full-system \
  -f environment=production \
  -f force_rebuild=false \
  -f skip_health_checks=false
```

### Step 5: Monitor and verify
- [ ] All production services running
- [ ] Health checks passing
- [ ] Users can access the application
- [ ] No errors in logs
- [ ] Performance is normal

## 🔄 Rollback Plan (if needed)

If new system fails:

```bash
# 1. Restore old workflow
mv deploy-to-truenas.yml.backup deploy-to-truenas.yml

# 2. Deploy with old system  
gh workflow run deploy-to-truenas.yml \
  -f environment=production \
  -f force_rebuild=false

# 3. Remove new workflows temporarily
mv deploy-orchestrator.yml deploy-orchestrator.yml.temp
mv deploy-infrastructure.yml deploy-infrastructure.yml.temp
mv deploy-backend-services.yml deploy-backend-services.yml.temp  
mv deploy-frontend-ghcr.yml deploy-frontend-ghcr.yml.temp
```

## 📊 Comparison: Old vs New

### Old System Issues:
- ❌ Deploy toàn bộ hệ thống mỗi lần
- ❌ Không có dependency management
- ❌ Thời gian deploy lâu (30+ phút)
- ❌ Một service lỗi → block tất cả
- ❌ Khó debug khi có vấn đề
- ❌ Không flexible

### New System Benefits:
- ✅ Deploy riêng từng service/component
- ✅ Automatic dependency management  
- ✅ Thời gian deploy nhanh (5-15 phút)
- ✅ Service isolation - không ảnh hưởng lẫn nhau
- ✅ Better error handling và debugging
- ✅ Multiple deployment strategies
- ✅ Better monitoring và notifications

## 🎯 Post-Migration Tasks

### 1. **Clean up (after 1 week successful operation)**
```bash
# Remove backup file
rm .github/workflows/deploy-to-truenas.yml.backup

# Update documentation
# Update team training materials
```

### 2. **Team training**
- [ ] Train team on new deployment workflows
- [ ] Update deployment procedures documentation
- [ ] Create troubleshooting guides
- [ ] Share deployment guide with team

### 3. **Monitoring setup**
- [ ] Set up alerts for deployment failures
- [ ] Monitor deployment times
- [ ] Track deployment frequency by service
- [ ] Monitor system health after deployments

## 🚨 Emergency Contacts

**If migration fails:**
1. Check GitHub Actions logs
2. Check Discord notifications  
3. Check TrueNAS server status
4. Follow rollback plan above
5. Contact system administrator

**Success Criteria:**
- [ ] All services deployed successfully
- [ ] Health checks passing
- [ ] Users can access application
- [ ] No service downtime
- [ ] Team trained on new process

---

**Migration Date:** ___________
**Performed by:** ___________  
**Rollback tested:** [ ] Yes [ ] No
**Team notified:** [ ] Yes [ ] No