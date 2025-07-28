# Task Completion Checklist

## When Completing Any Development Task

### Code Quality Validation
- [ ] **Build Success**: Ensure all projects build without errors
- [ ] **Test Coverage**: Write and run relevant unit tests (xUnit + FluentAssertions)
- [ ] **Code Review**: Verify code follows established patterns and conventions
- [ ] **Documentation**: Update XML comments and relevant documentation files

### Backend Task Completion (.NET)
- [ ] **Entity Framework**: Run and test database migrations if schema changed
- [ ] **API Documentation**: Update Swagger/OpenAPI documentation
- [ ] **Validation**: Ensure FluentValidation rules are in place for new DTOs
- [ ] **Dependencies**: Update Dependency Injection container registrations
- [ ] **Health Checks**: Verify service health endpoints are working
- [ ] **Integration Tests**: Run integration tests if available

### Frontend Task Completion (Nuxt)
- [ ] **Type Safety**: Ensure TypeScript types are properly defined
- [ ] **Responsive Design**: Test on multiple screen sizes and devices
- [ ] **Dark Mode**: Verify dark mode compatibility if UI changes made
- [ ] **Navigation**: Update navigation menus and breadcrumbs if new pages added
- [ ] **API Integration**: Test API calls and error handling
- [ ] **State Management**: Update Pinia stores if state changes required

### Infrastructure & Deployment
- [ ] **Docker**: Update Docker configurations if needed
- [ ] **Environment Variables**: Add new environment variables to deployment configs
- [ ] **CI/CD**: Ensure GitHub Actions workflows still pass
- [ ] **Health Monitoring**: Verify service monitoring and alerting still works

### Documentation Updates
- [ ] **Memory Bank**: Update relevant memory-bank files with changes
- [ ] **Design Docs**: Update design documentation if architecture changed
- [ ] **API Specs**: Update OpenAPI specifications
- [ ] **README**: Update project README files if setup process changed

### Final Validation Commands
```bash
# Backend validation
cd src/be
dotnet build TiHoMo.sln
dotnet test TiHoMo.sln

# Frontend validation  
cd src/fe/nuxt
npm run build
npm run dev # Verify no runtime errors

# Docker validation
docker-compose build
docker-compose up -d
docker-compose ps # Check all services are healthy
```

### Quality Gates
- **Zero Build Errors**: All projects must compile successfully
- **Passing Tests**: All existing tests must continue to pass
- **No Breaking Changes**: Existing APIs must remain backward compatible
- **Security Review**: No security vulnerabilities introduced
- **Performance Check**: No significant performance degradation