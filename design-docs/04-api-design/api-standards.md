# API Standards & Guidelines

## 1. RESTful API Design Principles

### 1.1 Resource Naming Conventions
- Use nouns for resource names (not verbs)
- Use plural forms for collections: `/accounts`, `/transactions`
- Use kebab-case for multi-word resources: `/recurring-transactions`
- Use nested resources for hierarchical relationships: `/accounts/{id}/transactions`

### 1.2 HTTP Methods
- **GET**: Retrieve resources (idempotent)
- **POST**: Create new resources
- **PUT**: Update entire resources (idempotent)
- **PATCH**: Partial resource updates
- **DELETE**: Remove resources (idempotent)

### 1.3 Status Codes
- **200 OK**: Successful GET, PUT, PATCH
- **201 Created**: Successful POST
- **204 No Content**: Successful DELETE
- **400 Bad Request**: Invalid request syntax
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict
- **422 Unprocessable Entity**: Validation errors
- **500 Internal Server Error**: Server errors

## 2. Request/Response Format

### 2.1 Content Type
- Use `application/json` for request and response bodies
- Support `application/x-www-form-urlencoded` for form submissions
- Use `multipart/form-data` for file uploads

### 2.2 Request Headers
```http
Content-Type: application/json
Authorization: Bearer {jwt_token}
X-Correlation-ID: {correlation_id}
Accept: application/json
```

### 2.3 Response Format
```json
{
  "data": {
    // Resource data
  },
  "meta": {
    "timestamp": "2025-06-24T10:00:00Z",
    "correlationId": "abc-123-def"
  }
}
```

### 2.4 Error Response Format
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Request validation failed",
    "details": [
      {
        "field": "amount",
        "message": "Amount must be greater than 0"
      }
    ]
  },
  "meta": {
    "timestamp": "2025-06-24T10:00:00Z",
    "correlationId": "abc-123-def"
  }
}
```

## 3. Pagination

### 3.1 Cursor-Based Pagination (Recommended)
```http
GET /api/transactions?cursor=abc123&limit=20
```

Response:
```json
{
  "data": [...],
  "pagination": {
    "nextCursor": "def456",
    "hasMore": true,
    "limit": 20
  }
}
```

### 3.2 Offset-Based Pagination (Alternative)
```http
GET /api/transactions?page=2&limit=20
```

Response:
```json
{
  "data": [...],
  "pagination": {
    "page": 2,
    "limit": 20,
    "total": 150,
    "hasMore": true
  }
}
```

## 4. Filtering & Sorting

### 4.1 Query Parameters
```http
GET /api/transactions?accountId=123&type=expense&startDate=2025-01-01&endDate=2025-01-31&sort=date&order=desc
```

### 4.2 Supported Operators
- **eq**: equals (default)
- **ne**: not equals
- **gt**: greater than
- **gte**: greater than or equal
- **lt**: less than
- **lte**: less than or equal
- **in**: in array
- **like**: contains (for strings)

## 5. Versioning

### 5.1 URL Versioning (Current Approach)
```http
GET /api/v1/accounts
GET /api/v2/accounts
```

### 5.2 Header Versioning (Future Consideration)
```http
GET /api/accounts
API-Version: 2.0
```

## 6. Security Standards

### 6.1 Authentication
- JWT Bearer tokens for API access
- Social login integration (Google, Facebook, Apple)
- Token refresh mechanism

### 6.2 Authorization
- Resource-based access control
- User can only access their own data
- Admin roles for system management

### 6.3 Input Validation
- Validate all input parameters
- Sanitize user input
- Use parameterized queries
- Implement rate limiting

## 7. Performance Guidelines

### 7.1 Response Optimization
- Use compression (gzip/brotli)
- Implement caching headers
- Minimize payload size
- Support conditional requests (ETags)

### 7.2 Database Optimization
- Use database indexing
- Implement connection pooling
- Optimize query performance
- Use read replicas for heavy read operations

## 8. Documentation Standards

### 8.1 OpenAPI Specification
- Maintain up-to-date OpenAPI specs
- Include request/response examples
- Document all error scenarios
- Provide clear parameter descriptions

### 8.2 Endpoint Documentation
```yaml
/api/accounts:
  post:
    summary: Create a new financial account
    description: Creates a new financial account for the authenticated user
    requestBody:
      required: true
      content:
        application/json:
          schema:
            $ref: '#/components/schemas/CreateAccountRequest'
    responses:
      '201':
        description: Account created successfully
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/AccountResponse'
```

## 9. Testing Guidelines

### 9.1 API Testing Requirements
- Unit tests for all endpoints
- Integration tests for complex workflows
- Performance testing for critical paths
- Security testing for authentication/authorization

### 9.2 Test Data Management
- Use test databases for testing
- Implement data seeding for consistent tests
- Clean up test data after test execution

## 10. Monitoring & Observability

### 10.1 Logging Standards
- Log all API requests/responses
- Include correlation IDs
- Log performance metrics
- Structured logging format (JSON)

### 10.2 Metrics Collection
- Request count and duration
- Error rates by endpoint
- Authentication success/failure rates
- Business metrics (transactions created, accounts registered)

## 11. Backward Compatibility

### 11.1 Breaking Changes
- Avoid breaking changes in existing API versions
- Deprecate old endpoints before removal
- Provide migration guides for major version updates
- Maintain at least one previous version

### 11.2 Non-Breaking Changes
- Adding new optional fields
- Adding new endpoints
- Adding new query parameters
- Extending enum values (with default handling)

## 12. Rate Limiting

### 12.1 Rate Limit Headers
```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1640995200
```

### 12.2 Rate Limit Strategies
- Per-user rate limiting
- Per-endpoint rate limiting
- Burst rate limiting for peak usage
- Different limits for different user tiers

---

This document establishes the foundation for consistent API development across all TiHoMo services and should be followed by all development teams.
