# Investment Portfolio Tracking - Brownfield Addition

**Story ID**: INV-001  
**Created**: August 26, 2025  
**Status**: Draft  
**Estimated Effort**: 3-4 hours  
**Epic**: Planning & Investment Module  
**Type**: Brownfield Enhancement  

## User Story

**As a** TiHoMo user,  
**I want** to track my investment portfolio with capital, profit/loss calculation and manual market price updates,  
**So that** I can monitor my investment performance and make informed financial decisions.

## Story Context

**Existing System Integration:**
- **Integrates with**: PlanningInvestment module existing structure
- **Technology**: .NET 9, EF Core, PostgreSQL, Clean Architecture
- **Follows pattern**: Existing PlanningInvestment service patterns với domain models và application services
- **Touch points**: PlanningInvestmentDbContext, Domain entities, Application services, API controllers

## Acceptance Criteria

### Functional Requirements

1. **Investment Entity Creation**: Create Investment domain entity với properties: symbol, investment_type, purchase_price, quantity, current_market_price, purchase_date
2. **Basic CRUD Operations**: Implement InvestmentService với methods để create, read, update, delete investments  
3. **Profit/Loss Calculation**: Calculate và display current profit/loss based on (current_market_price - purchase_price) * quantity

### Integration Requirements

4. **Database Integration**: Investment entity properly mapped trong PlanningInvestmentDbContext với EF Core migrations
5. **API Endpoints**: Create basic REST endpoints `/api/investments` cho CRUD operations
6. **Clean Architecture**: Follow existing clean architecture pattern với proper service layer separation

### Quality Requirements

7. **Testing Coverage**: Unit tests cho InvestmentService và domain logic sử dụng xUnit + FluentAssertions
8. **Documentation**: XML comments trong bilingual format (English/Vietnamese)  
9. **Data Validation**: Proper input validation và error handling

## Technical Notes

- **Integration Approach**: Extend existing PlanningInvestment module bằng cách thêm Investment entity và related services
- **Existing Pattern Reference**: Follow Debt entity pattern trong cùng module cho consistency
- **Key Constraints**: 
  - Manual market price input (no automatic market data integration yet)
  - Basic portfolio tracking only (no advanced analytics trong story này)
  - Must maintain existing PlanningInvestment module integrity

## Definition of Done

- [ ] Investment domain entity created với proper EF Core mapping
- [ ] InvestmentService implemented với basic CRUD operations
- [ ] Profit/loss calculation logic working correctly
- [ ] EF Core migration created và applied successfully
- [ ] API endpoints functional với proper DTOs
- [ ] Unit tests pass với adequate coverage
- [ ] Code follows existing patterns và coding standards
- [ ] XML documentation complete trong bilingual format
- [ ] No regression trong existing PlanningInvestment functionality

## Risk Assessment

**Primary Risk**: Adding new entity có thể affect existing Debt functionality nếu migrations không properly handled  
**Mitigation**: Use separate migration cho Investment entity, test database operations thoroughly  
**Rollback**: Simple database migration rollback và code revert

## Compatibility Verification

- [x] ✅ No breaking changes to existing APIs (adding new endpoints only)
- [x] ✅ Database changes are additive only (new Investment table)
- [x] ✅ UI changes follow existing design patterns (will be handled in separate frontend story)
- [x] ✅ Performance impact is negligible (simple CRUD operations)

## Implementation Plan

### Phase 1: Domain Layer (1 hour)
- Create Investment entity trong `PlanningInvestment.Domain/Entities/`
- Add InvestmentType enum nếu cần
- Update domain model relationships

### Phase 2: Infrastructure Layer (1 hour)  
- Add Investment DbSet trong PlanningInvestmentDbContext
- Create EF Core migration
- Test database operations

### Phase 3: Application Layer (1 hour)
- Create InvestmentService trong Application layer
- Implement CRUD operations với business logic
- Add profit/loss calculation methods

### Phase 4: API Layer (1 hour)
- Create InvestmentController
- Add DTOs cho requests/responses  
- Implement REST endpoints
- Add proper validation và error handling

## Future Enhancements (Separate Stories)

- **INV-002**: Real-time market data integration
- **INV-003**: Advanced portfolio analytics và performance metrics
- **INV-004**: Investment performance charts và visualizations
- **INV-005**: Tax calculation support cho capital gains
- **INV-006**: Frontend UI components cho investment management
- **INV-007**: Portfolio diversification analysis
- **INV-008**: Investment alerts và notifications

## Related Documents

- [Feature Design: Investment Portfolio Tracking](../design-docs/07-features/feat-04-investment-portfolio-tracking.md)
- [PlanningInvestment Service Design](../design-docs/06-backend-design/service-architecture/planninginvestment-service.md)
- [Project Brief](../memory-bank/projectbrief.md)
- [Active Context](../memory-bank/activeContext.md)

## Notes

- Story follows brownfield-create-story task requirements
- Integration với existing system patterns prioritized
- Minimal risk approach với clear rollback strategy
- Foundation cho advanced investment features trong future stories

---

**Last Updated**: August 26, 2025  
**Next Review**: Before development start
