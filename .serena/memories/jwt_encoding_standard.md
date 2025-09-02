# JWT Encoding Standard - UTF-8 Migration

## Problem Solved
Fixed JWT authentication issue where:
- Identity service used ASCII encoding for JWT signing
- CoreFinance service used UTF-8 encoding for JWT validation
- This caused signature mismatch → authentication failed

## Solution Implemented
Migrated ALL services to UTF-8 encoding for JWT secret key processing:

### Files Updated:
1. **Identity.Api/Program.cs**: ASCII → UTF-8
2. **Identity.Api/Services/JwtService.cs**: ASCII → UTF-8 (2 places)
3. **Identity.Infrastructure/Services/JwtTokenService.cs**: ASCII → UTF-8 (2 places)

### Verification:
- ✅ Gateway: Already using UTF-8
- ✅ CoreFinance: Already using UTF-8

## Best Practice Established
**STANDARD: All JWT secret key processing MUST use UTF-8 encoding**

### Reasons:
1. Industry standard for modern applications
2. Microsoft/.NET official recommendation
3. Better security with higher entropy potential
4. Forward compatibility for international characters
5. Consistency with ASP.NET Core defaults

### Implementation:
```csharp
// CORRECT - Use UTF-8
var key = Encoding.UTF8.GetBytes(secretKey);

// INCORRECT - Don't use ASCII
var key = Encoding.ASCII.GetBytes(secretKey);
```

## Testing Notes
- Current secret key contains only ASCII characters
- UTF-8 encoding produces same result as ASCII for this key
- No breaking change for existing tokens
- Future secret keys with special characters will work correctly

Date: 2025-08-15
Status: COMPLETED