# API Key Management Endpoints

## 📋 Overview

API endpoints cho Enhanced API Key Management feature trong TiHoMo system. Cho phép users tạo, quản lý và monitor API keys để integrate với third-party applications.

---

## 🔑 Authentication & Authorization

### Base URL
```
Production: https://api.tihomo.com
Development: http://localhost:5800/api
```

### Authentication Methods
1. **JWT Bearer Token** (cho web app users)
```http
Authorization: Bearer {jwt_token}
```

2. **API Key Authentication** (cho third-party integrations)
```http
Authorization: Bearer {api_key}
```

### Required Headers
```http
Content-Type: application/json
Authorization: Bearer {token}
X-Correlation-ID: {correlation_id}
Accept: application/json
```

---

## 📚 API Endpoints

### 1. List API Keys

#### `GET /api/identity/apikeys`

Retrieve list of API keys for the authenticated user.

**Query Parameters:**
```typescript
interface ListApiKeysQuery {
  status?: 'active' | 'revoked' | 'expired'
  scope?: string
  search?: string
  cursor?: string
  limit?: number // default: 20, max: 100
}
```

**Request Example:**
```http
GET /api/identity/apikeys?status=active&limit=10&cursor=abc123
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Example:**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Production API",
      "description": "API key for production mobile app",
      "keyPrefix": "pfm_abc123",
      "scopes": ["read", "transactions:write"],
      "status": "active",
      "rateLimitPerMinute": 100,
      "dailyUsageQuota": 10000,
      "todayUsageCount": 245,
      "usageCount": 15420,
      "ipWhitelist": ["192.168.1.0/24"],
      "securitySettings": {
        "requireHttps": true,
        "allowCorsRequests": false,
        "allowedOrigins": []
      },
      "createdAt": "2024-12-01T10:00:00Z",
      "updatedAt": "2024-12-15T14:30:00Z",
      "expiresAt": "2025-12-01T10:00:00Z",
      "lastUsedAt": "2024-12-28T09:15:00Z"
    }
  ],
  "pagination": {
    "nextCursor": "def456",
    "hasMore": true,
    "limit": 10
  },
  "meta": {
    "timestamp": "2024-12-28T10:00:00Z",
    "correlationId": "req-123-abc"
  }
}
```

**Response Codes:**
- `200 OK`: Success
- `401 Unauthorized`: Invalid or missing token
- `403 Forbidden`: Insufficient permissions
- `429 Too Many Requests`: Rate limit exceeded

---

### 2. Get API Key Details

#### `GET /api/identity/apikeys/{id}`

Retrieve detailed information about a specific API key.

**Path Parameters:**
- `id` (string, required): API key UUID

**Request Example:**
```http
GET /api/identity/apikeys/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Example:**
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Production API",
    "description": "API key for production mobile app",
    "keyPrefix": "pfm_abc123",
    "scopes": ["read", "transactions:write"],
    "status": "active",
    "rateLimitPerMinute": 100,
    "dailyUsageQuota": 10000,
    "todayUsageCount": 245,
    "usageCount": 15420,
    "ipWhitelist": ["192.168.1.0/24"],
    "securitySettings": {
      "requireHttps": true,
      "allowCorsRequests": false,
      "allowedOrigins": []
    },
    "createdAt": "2024-12-01T10:00:00Z",
    "updatedAt": "2024-12-15T14:30:00Z",
    "expiresAt": "2025-12-01T10:00:00Z",
    "lastUsedAt": "2024-12-28T09:15:00Z"
  },
  "meta": {
    "timestamp": "2024-12-28T10:00:00Z",
    "correlationId": "req-123-abc"
  }
}
```

**Response Codes:**
- `200 OK`: Success
- `401 Unauthorized`: Invalid or missing token
- `403 Forbidden`: API key belongs to different user
- `404 Not Found`: API key not found

---

### 3. Create API Key

#### `POST /api/identity/apikeys`

Create a new API key for the authenticated user.

**Request Body:**
```typescript
interface CreateApiKeyRequest {
  name: string // required, 1-100 characters
  description?: string // optional, max 500 characters
  scopes: string[] // required, at least one scope
  expiresAt?: string // optional, ISO 8601 datetime
  rateLimitPerMinute?: number // default: 100, max: 1000
  dailyUsageQuota?: number // default: 10000, max: 100000
  ipWhitelist?: string[] // optional, IP addresses or CIDR blocks
  securitySettings?: {
    requireHttps?: boolean // default: true
    allowCorsRequests?: boolean // default: false
    allowedOrigins?: string[] // required if allowCorsRequests is true
  }
}
```

**Request Example:**
```http
POST /api/identity/apikeys
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "name": "Mobile App API",
  "description": "API key for iOS/Android mobile application",
  "scopes": ["read", "transactions:read", "transactions:write"],
  "expiresAt": "2025-12-28T00:00:00Z",
  "rateLimitPerMinute": 200,
  "dailyUsageQuota": 50000,
  "ipWhitelist": ["203.0.113.0/24"],
  "securitySettings": {
    "requireHttps": true,
    "allowCorsRequests": true,
    "allowedOrigins": ["https://app.tihomo.com"]
  }
}
```

**Response Example:**
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "name": "Mobile App API",
    "description": "API key for iOS/Android mobile application",
    "apiKey": "pfm_def456ghi789jkl012mno345pqr678stu901vwx234yz", // ⚠️ ONLY shown once
    "keyPrefix": "pfm_def456",
    "scopes": ["read", "transactions:read", "transactions:write"],
    "status": "active",
    "rateLimitPerMinute": 200,
    "dailyUsageQuota": 50000,
    "todayUsageCount": 0,
    "usageCount": 0,
    "ipWhitelist": ["203.0.113.0/24"],
    "securitySettings": {
      "requireHttps": true,
      "allowCorsRequests": true,
      "allowedOrigins": ["https://app.tihomo.com"]
    },
    "createdAt": "2024-12-28T10:00:00Z",
    "updatedAt": "2024-12-28T10:00:00Z",
    "expiresAt": "2025-12-28T00:00:00Z",
    "lastUsedAt": null
  },
  "meta": {
    "timestamp": "2024-12-28T10:00:00Z",
    "correlationId": "req-123-abc"
  }
}
```

**Response Codes:**
- `201 Created`: API key created successfully
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Invalid or missing token
- `422 Unprocessable Entity`: Validation errors
- `429 Too Many Requests`: Rate limit exceeded

**Validation Errors:**
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Request validation failed",
    "details": [
      {
        "field": "name",
        "message": "Name is required and must be 1-100 characters"
      },
      {
        "field": "scopes",
        "message": "At least one scope is required"
      },
      {
        "field": "ipWhitelist",
        "message": "Invalid IP address: 256.1.1.1"
      }
    ]
  },
  "meta": {
    "timestamp": "2024-12-28T10:00:00Z",
    "correlationId": "req-123-abc"
  }
}
```

---

### 4. Update API Key

#### `PATCH /api/identity/apikeys/{id}`

Update an existing API key's settings (không thể update key value).

**Path Parameters:**
- `id` (string, required): API key UUID

**Request Body:**
```typescript
interface UpdateApiKeyRequest {
  name?: string
  description?: string
  scopes?: string[]
  expiresAt?: string | null // null to remove expiration
  rateLimitPerMinute?: number
  dailyUsageQuota?: number
  ipWhitelist?: string[]
  securitySettings?: {
    requireHttps?: boolean
    allowCorsRequests?: boolean
    allowedOrigins?: string[]
  }
}
```

**Request Example:**
```http
PATCH /api/identity/apikeys/550e8400-e29b-41d4-a716-446655440000
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "description": "Updated description for mobile app",
  "rateLimitPerMinute": 150,
  "ipWhitelist": ["203.0.113.0/24", "198.51.100.0/24"]
}
```

**Response Example:**
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Production API",
    "description": "Updated description for mobile app",
    "keyPrefix": "pfm_abc123",
    "scopes": ["read", "transactions:write"],
    "status": "active",
    "rateLimitPerMinute": 150,
    "dailyUsageQuota": 10000,
    "todayUsageCount": 245,
    "usageCount": 15420,
    "ipWhitelist": ["203.0.113.0/24", "198.51.100.0/24"],
    "securitySettings": {
      "requireHttps": true,
      "allowCorsRequests": false,
      "allowedOrigins": []
    },
    "createdAt": "2024-12-01T10:00:00Z",
    "updatedAt": "2024-12-28T10:05:00Z",
    "expiresAt": "2025-12-01T10:00:00Z",
    "lastUsedAt": "2024-12-28T09:15:00Z"
  },
  "meta": {
    "timestamp": "2024-12-28T10:05:00Z",
    "correlationId": "req-123-def"
  }
}
```

**Response Codes:**
- `200 OK`: Success
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Invalid or missing token
- `403 Forbidden`: API key belongs to different user
- `404 Not Found`: API key not found
- `422 Unprocessable Entity`: Validation errors

---

### 5. Regenerate API Key

#### `POST /api/identity/apikeys/{id}/regenerate`

Generate a new key value cho existing API key (invalidates old key).

**Path Parameters:**
- `id` (string, required): API key UUID

**Request Example:**
```http
POST /api/identity/apikeys/550e8400-e29b-41d4-a716-446655440000/regenerate
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Example:**
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Production API",
    "description": "API key for production mobile app",
    "apiKey": "pfm_xyz789abc012def345ghi678jkl901mno234pqr567stu", // ⚠️ New key, only shown once
    "keyPrefix": "pfm_xyz789",
    "scopes": ["read", "transactions:write"],
    "status": "active",
    "rateLimitPerMinute": 150,
    "dailyUsageQuota": 10000,
    "todayUsageCount": 0, // Reset usage count
    "usageCount": 0, // Reset total usage
    "ipWhitelist": ["203.0.113.0/24", "198.51.100.0/24"],
    "securitySettings": {
      "requireHttps": true,
      "allowCorsRequests": false,
      "allowedOrigins": []
    },
    "createdAt": "2024-12-01T10:00:00Z",
    "updatedAt": "2024-12-28T10:10:00Z",
    "expiresAt": "2025-12-01T10:00:00Z",
    "lastUsedAt": null // Reset last used
  },
  "meta": {
    "timestamp": "2024-12-28T10:10:00Z",
    "correlationId": "req-123-ghi"
  }
}
```

**Response Codes:**
- `200 OK`: Success
- `401 Unauthorized`: Invalid or missing token
- `403 Forbidden`: API key belongs to different user
- `404 Not Found`: API key not found
- `409 Conflict`: API key is already revoked

---

### 6. Revoke API Key

#### `DELETE /api/identity/apikeys/{id}`

Revoke an API key (soft delete - sets status to 'revoked').

**Path Parameters:**
- `id` (string, required): API key UUID

**Request Example:**
```http
DELETE /api/identity/apikeys/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Example:**
```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "status": "revoked",
    "revokedAt": "2024-12-28T10:15:00Z"
  },
  "meta": {
    "timestamp": "2024-12-28T10:15:00Z",
    "correlationId": "req-123-jkl"
  }
}
```

**Response Codes:**
- `200 OK`: Success
- `401 Unauthorized`: Invalid or missing token
- `403 Forbidden`: API key belongs to different user
- `404 Not Found`: API key not found
- `409 Conflict`: API key is already revoked

---

### 7. Get API Key Usage Statistics

#### `GET /api/identity/apikeys/{id}/usage`

Get detailed usage statistics for an API key.

**Path Parameters:**
- `id` (string, required): API key UUID

**Query Parameters:**
```typescript
interface UsageStatsQuery {
  period?: 'today' | 'week' | 'month' | 'custom'
  startDate?: string // ISO 8601 date (for custom period)
  endDate?: string // ISO 8601 date (for custom period)
  granularity?: 'hour' | 'day' // default: 'day'
}
```

**Request Example:**
```http
GET /api/identity/apikeys/550e8400-e29b-41d4-a716-446655440000/usage?period=week&granularity=day
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Example:**
```json
{
  "data": {
    "apiKeyId": "550e8400-e29b-41d4-a716-446655440000",
    "period": {
      "startDate": "2024-12-21T00:00:00Z",
      "endDate": "2024-12-28T23:59:59Z",
      "granularity": "day"
    },
    "summary": {
      "totalRequests": 1520,
      "successfulRequests": 1485,
      "failedRequests": 35,
      "successRate": 97.7,
      "averageResponseTime": 245.5,
      "uniqueIpAddresses": 3
    },
    "dailyStats": [
      {
        "date": "2024-12-21",
        "requests": 210,
        "successful": 205,
        "failed": 5,
        "averageResponseTime": 230.2
      },
      {
        "date": "2024-12-22",
        "requests": 185,
        "successful": 180,
        "failed": 5,
        "averageResponseTime": 252.1
      }
      // ... more daily stats
    ],
    "endpointStats": [
      {
        "endpoint": "GET /api/core-finance/transaction",
        "requests": 850,
        "percentage": 55.9
      },
      {
        "endpoint": "POST /api/core-finance/transaction",
        "requests": 420,
        "percentage": 27.6
      }
      // ... more endpoint stats
    ],
    "errorStats": [
      {
        "statusCode": 400,
        "count": 20,
        "percentage": 57.1
      },
      {
        "statusCode": 422,
        "count": 10,
        "percentage": 28.6
      }
      // ... more error stats
    ]
  },
  "meta": {
    "timestamp": "2024-12-28T10:20:00Z",
    "correlationId": "req-123-mno"
  }
}
```

**Response Codes:**
- `200 OK`: Success
- `401 Unauthorized`: Invalid or missing token
- `403 Forbidden`: API key belongs to different user
- `404 Not Found`: API key not found

---

### 8. Get API Key Activity Logs

#### `GET /api/identity/apikeys/{id}/logs`

Get recent activity logs for an API key.

**Path Parameters:**
- `id` (string, required): API key UUID

**Query Parameters:**
```typescript
interface ActivityLogsQuery {
  limit?: number // default: 50, max: 200
  cursor?: string
  startDate?: string // ISO 8601 datetime
  endDate?: string // ISO 8601 datetime
  statusCode?: number // filter by HTTP status code
  endpoint?: string // filter by endpoint pattern
}
```

**Request Example:**
```http
GET /api/identity/apikeys/550e8400-e29b-41d4-a716-446655440000/logs?limit=10&statusCode=200
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response Example:**
```json
{
  "data": [
    {
      "id": "log-123-abc",
      "timestamp": "2024-12-28T09:15:30Z",
      "method": "GET",
      "endpoint": "/api/core-finance/transaction",
      "statusCode": 200,
      "responseTime": 245,
      "ipAddress": "203.0.113.45",
      "userAgent": "TiHomoMobileApp/1.2.0",
      "requestSize": 0,
      "responseSize": 2048
    },
    {
      "id": "log-123-def",
      "timestamp": "2024-12-28T09:14:15Z",
      "method": "POST",
      "endpoint": "/api/core-finance/transaction",
      "statusCode": 201,
      "responseTime": 380,
      "ipAddress": "203.0.113.45",
      "userAgent": "TiHomoMobileApp/1.2.0",
      "requestSize": 512,
      "responseSize": 1024
    }
    // ... more logs
  ],
  "pagination": {
    "nextCursor": "cursor-456-ghi",
    "hasMore": true,
    "limit": 10
  },
  "meta": {
    "timestamp": "2024-12-28T10:25:00Z",
    "correlationId": "req-123-pqr"
  }
}
```

**Response Codes:**
- `200 OK`: Success
- `401 Unauthorized`: Invalid or missing token
- `403 Forbidden`: API key belongs to different user
- `404 Not Found`: API key not found

---

## 🔧 Implementation Status

### ✅ Already Implemented (Existing in Identity Service)
- **Core API Key Entity**: `ApiKey` domain model with essential properties
- **API Key Service**: `ApiKeyService` với basic CRUD operations
- **API Key Repository**: Database operations với EF Core
- **API Key Hasher**: Secure key generation và hashing
- **Basic Authentication**: Gateway middleware cho API key validation

### 🚧 Needs Enhancement (Current Implementation Gaps)
- **API Controllers**: REST endpoints chưa được exposed
- **Enhanced DTOs**: Request/Response models cần bổ sung
- **Usage Tracking**: Statistics và logging chưa implement
- **Rate Limiting**: Per-key rate limiting chưa có
- **IP Whitelisting**: Security validation chưa implement
- **Scope Validation**: Fine-grained permission checking

### 🔮 Planned Implementation (New Features)
- **Usage Analytics API**: Detailed statistics và reporting
- **Activity Logs API**: Comprehensive audit trail
- **Bulk Operations**: Batch create/update/revoke operations
- **API Key Templates**: Pre-configured key templates
- **Webhook Management**: Event notifications cho key events

---

## 🛡️ Security Considerations

### Rate Limiting
- **Per-User Limits**: 100 requests/minute cho API key management endpoints
- **Per-API-Key Limits**: Configurable limits cho each API key
- **Burst Protection**: Allow temporary spikes với exponential backoff

### Input Validation
- **Scope Validation**: Only allow valid, predefined scopes
- **IP Address Validation**: Validate CIDR notation và IP ranges
- **Name Sanitization**: Prevent XSS attacks trong API key names
- **Description Limits**: Prevent abuse với reasonable length limits

### Audit Logging
- **All Operations**: Log create, update, delete, regenerate operations
- **Usage Tracking**: Track all API calls made với each key
- **Security Events**: Log suspicious activities và rate limit violations
- **Data Retention**: Keep logs for compliance và security analysis

### Access Control
- **User Isolation**: Users can only access their own API keys
- **Admin Override**: System administrators can view/manage all keys
- **Service-to-Service**: Internal services can validate keys without user context

---

## 📊 Error Handling

### Common Error Responses

#### Validation Error (422)
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Request validation failed",
    "details": [
      {
        "field": "scopes",
        "message": "Invalid scope: invalid_scope"
      }
    ]
  }
}
```

#### Rate Limit Exceeded (429)
```json
{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests",
    "retryAfter": 60
  }
}
```

#### API Key Not Found (404)
```json
{
  "error": {
    "code": "API_KEY_NOT_FOUND",
    "message": "API key not found or access denied"
  }
}
```

#### Insufficient Permissions (403)
```json
{
  "error": {
    "code": "INSUFFICIENT_PERMISSIONS",
    "message": "You don't have permission to access this API key"
  }
}
```

---

## 🧪 Testing Guidelines

### Unit Tests
- Test all endpoint controllers
- Validate request/response serialization
- Test error handling scenarios
- Mock external dependencies

### Integration Tests
- Test complete API workflows
- Validate database interactions
- Test authentication/authorization
- Test rate limiting behavior

### Security Tests
- Test API key validation
- Test scope enforcement
- Test IP whitelisting
- Test rate limiting effectiveness

### Performance Tests
- Load test critical endpoints
- Test database query performance
- Test caching effectiveness
- Monitor memory usage

---

*Last updated: December 28, 2024*
*Status: Design Complete, Implementation Planned* 