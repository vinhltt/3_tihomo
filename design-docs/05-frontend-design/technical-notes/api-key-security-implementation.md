# API Key Security Implementation - Frontend

## 📋 Overview

Tài liệu này mô tả cách implement security measures cho API Key Management trong frontend của TiHoMo application, đảm bảo rằng API keys được handle an toàn và tuân thủ security best practices.

---

## 🔐 Core Security Principles

### 1. **Never Store Full API Keys**
```typescript
// ❌ WRONG - Never store full API keys in frontend
const apiKeyStore = {
  keys: [
    {
      id: '1',
      fullKey: 'pfm_abc123def456ghi789...' // NEVER DO THIS
    }
  ]
}

// ✅ CORRECT - Only store metadata
const apiKeyStore = {
  keys: [
    {
      id: '1',
      name: 'Production API',
      keyPrefix: 'pfm_abc123',
      maskedKey: 'pfm_abc123***************************',
      status: 'active',
      scopes: ['read', 'write']
    }
  ]
}
```

### 2. **One-Time Display Pattern**
```typescript
// API Key Creation Flow
const createApiKey = async (request: CreateApiKeyRequest) => {
  try {
    // Call backend API
    const response = await $fetch('/api/apikeys', {
      method: 'POST',
      body: request
    })
    
    // Show full key ONLY ONCE in success modal
    showSuccessModal.value = true
    fullApiKey.value = response.apiKey // Only for immediate display
    
    // Add to store without full key
    apiKeyStore.addApiKey({
      ...response,
      apiKey: undefined // Remove full key from store
    })
    
    // Clear full key after user confirms
    setTimeout(() => {
      fullApiKey.value = null
    }, 30000) // Auto-clear after 30 seconds
    
  } catch (error) {
    handleApiError(error)
  }
}
```

### 3. **Secure Clipboard Operations**
```typescript
// Secure copy-to-clipboard implementation
const copyToClipboard = async (text: string, autoCleanup = true) => {
  try {
    await navigator.clipboard.writeText(text)
    
    // Show success feedback
    copied.value = true
    setTimeout(() => { copied.value = false }, 2000)
    
    // Auto-clear clipboard for security
    if (autoCleanup) {
      setTimeout(async () => {
        try {
          await navigator.clipboard.writeText('')
        } catch (e) {
          // Ignore clipboard clear errors
        }
      }, 30000) // Clear after 30 seconds
    }
    
  } catch (error) {
    // Fallback for older browsers
    fallbackCopyToClipboard(text)
  }
}

// Fallback copy method
const fallbackCopyToClipboard = (text: string) => {
  const textArea = document.createElement('textarea')
  textArea.value = text
  textArea.style.position = 'fixed'
  textArea.style.opacity = '0'
  document.body.appendChild(textArea)
  textArea.focus()
  textArea.select()
  
  try {
    document.execCommand('copy')
  } catch (err) {
    console.error('Fallback copy failed:', err)
  }
  
  document.body.removeChild(textArea)
}
```

---

## 🛡️ Input Validation & Sanitization

### 1. **Client-Side Validation**
```typescript
// useApiKeyValidation.ts
export const useApiKeyValidation = (form: Ref<CreateApiKeyForm>) => {
  const errors = ref<Record<string, string>>({})
  
  const validateName = (name: string): string | null => {
    if (!name || name.trim().length === 0) {
      return 'Name is required'
    }
    if (name.length > 100) {
      return 'Name must be less than 100 characters'
    }
    // Sanitize HTML và script tags
    if (/<script|<iframe|javascript:/i.test(name)) {
      return 'Name contains invalid characters'
    }
    return null
  }
  
  const validateDescription = (description: string): string | null => {
    if (description && description.length > 500) {
      return 'Description must be less than 500 characters'
    }
    // Sanitize HTML
    if (/<script|<iframe|javascript:/i.test(description)) {
      return 'Description contains invalid characters'
    }
    return null
  }
  
  const validateScopes = (scopes: string[]): string | null => {
    if (!scopes || scopes.length === 0) {
      return 'At least one scope is required'
    }
    
    // Validate scope format
    const validScopes = [
      'read', 'write', 'delete', 'admin',
      'accounts:read', 'accounts:write',
      'transactions:read', 'transactions:write'
    ]
    
    for (const scope of scopes) {
      if (!validScopes.includes(scope)) {
        return `Invalid scope: ${scope}`
      }
    }
    
    return null
  }
  
  const validateIpWhitelist = (ipList: string[]): string | null => {
    if (!ipList || ipList.length === 0) return null
    
    for (const ip of ipList) {
      if (!isValidIpAddress(ip) && !isValidCidr(ip)) {
        return `Invalid IP address or CIDR: ${ip}`
      }
    }
    
    return null
  }
  
  const validateForm = (): boolean => {
    errors.value = {}
    
    // Validate all fields
    const nameError = validateName(form.value.name)
    const descError = validateDescription(form.value.description)
    const scopesError = validateScopes(form.value.scopes)
    const ipError = validateIpWhitelist(form.value.ipWhitelist)
    
    if (nameError) errors.value.name = nameError
    if (descError) errors.value.description = descError
    if (scopesError) errors.value.scopes = scopesError
    if (ipError) errors.value.ipWhitelist = ipError
    
    return Object.keys(errors.value).length === 0
  }
  
  return {
    errors: readonly(errors),
    validateForm,
    isFormValid: computed(() => Object.keys(errors.value).length === 0)
  }
}
```

### 2. **IP Address Validation**
```typescript
// utils/ipValidation.ts
export const isValidIpAddress = (ip: string): boolean => {
  // IPv4 validation
  const ipv4Regex = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/
  
  // IPv6 validation (simplified)
  const ipv6Regex = /^(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$/
  
  return ipv4Regex.test(ip) || ipv6Regex.test(ip)
}

export const isValidCidr = (cidr: string): boolean => {
  const [ip, prefix] = cidr.split('/')
  
  if (!ip || !prefix) return false
  
  const prefixNum = parseInt(prefix, 10)
  
  // IPv4 CIDR
  if (isValidIpAddress(ip)) {
    return prefixNum >= 0 && prefixNum <= 32
  }
  
  // IPv6 CIDR (simplified check)
  if (ip.includes(':')) {
    return prefixNum >= 0 && prefixNum <= 128
  }
  
  return false
}

// Real-time IP validation component
export const useIpValidation = () => {
  const validateIpList = (ipText: string): { valid: string[], invalid: string[] } => {
    const ips = ipText.split('\n')
      .map(ip => ip.trim())
      .filter(ip => ip.length > 0)
    
    const valid: string[] = []
    const invalid: string[] = []
    
    for (const ip of ips) {
      if (isValidIpAddress(ip) || isValidCidr(ip)) {
        valid.push(ip)
      } else {
        invalid.push(ip)
      }
    }
    
    return { valid, invalid }
  }
  
  return { validateIpList }
}
```

---

## 🔒 Secure State Management

### 1. **API Key Store Implementation**
```typescript
// stores/apiKeys.ts
export const useApiKeysStore = defineStore('apiKeys', {
  state: () => ({
    apiKeys: [] as ApiKey[],
    selectedApiKey: null as ApiKey | null,
    isLoading: false,
    // Never store full API keys in state
    tempDisplayKey: null as string | null, // Only for one-time display
    filters: {
      status: '',
      scope: '',
      search: ''
    }
  }),
  
  getters: {
    activeApiKeys: (state) => 
      state.apiKeys.filter(key => key.status === 'active'),
    
    expiringSoonKeys: (state) => {
      const sevenDaysFromNow = new Date()
      sevenDaysFromNow.setDate(sevenDaysFromNow.getDate() + 7)
      
      return state.apiKeys.filter(key => 
        key.expiresAt && 
        new Date(key.expiresAt) <= sevenDaysFromNow &&
        key.status === 'active'
      )
    }
  },
  
  actions: {
    async loadApiKeys() {
      this.isLoading = true
      try {
        const response = await $fetch('/api/apikeys')
        
        // Ensure no full keys in response
        this.apiKeys = response.data.map(key => ({
          ...key,
          apiKey: undefined, // Remove any full key data
          maskedKey: key.keyPrefix + '*'.repeat(32)
        }))
        
      } catch (error) {
        throw new ApiKeyError('Failed to load API keys', error)
      } finally {
        this.isLoading = false
      }
    },
    
    async createApiKey(request: CreateApiKeyRequest): Promise<ApiKey> {
      try {
        const response = await $fetch('/api/apikeys', {
          method: 'POST',
          body: request
        })
        
        // Store temp key for one-time display
        this.tempDisplayKey = response.apiKey
        
        // Add to store without full key
        const newKey = {
          ...response,
          apiKey: undefined,
          maskedKey: response.keyPrefix + '*'.repeat(32)
        }
        
        this.apiKeys.unshift(newKey)
        
        return newKey
        
      } catch (error) {
        throw new ApiKeyError('Failed to create API key', error)
      }
    },
    
    clearTempDisplayKey() {
      this.tempDisplayKey = null
    },
    
    async revokeApiKey(id: string) {
      try {
        await $fetch(`/api/apikeys/${id}`, { method: 'DELETE' })
        
        const index = this.apiKeys.findIndex(key => key.id === id)
        if (index !== -1) {
          this.apiKeys[index].status = 'revoked'
        }
        
      } catch (error) {
        throw new ApiKeyError('Failed to revoke API key', error)
      }
    }
  }
})
```

### 2. **Secure Session Management**
```typescript
// composables/useSecureSession.ts
export const useSecureSession = () => {
  const sessionTimeoutMinutes = 30
  let sessionTimer: NodeJS.Timeout | null = null
  
  const startSessionTimer = () => {
    clearSessionTimer()
    
    sessionTimer = setTimeout(() => {
      // Clear sensitive data on timeout
      clearSensitiveData()
      
      // Redirect to login
      navigateTo('/login')
    }, sessionTimeoutMinutes * 60 * 1000)
  }
  
  const clearSessionTimer = () => {
    if (sessionTimer) {
      clearTimeout(sessionTimer)
      sessionTimer = null
    }
  }
  
  const clearSensitiveData = () => {
    // Clear API key store
    const apiKeyStore = useApiKeysStore()
    apiKeyStore.clearTempDisplayKey()
    
    // Clear localStorage
    localStorage.removeItem('api-key-preferences')
    
    // Clear sessionStorage
    sessionStorage.clear()
  }
  
  const resetSessionTimer = () => {
    startSessionTimer()
  }
  
  // Auto-start session timer
  onMounted(() => {
    startSessionTimer()
    
    // Reset timer on user activity
    document.addEventListener('click', resetSessionTimer)
    document.addEventListener('keypress', resetSessionTimer)
  })
  
  onUnmounted(() => {
    clearSessionTimer()
    document.removeEventListener('click', resetSessionTimer)
    document.removeEventListener('keypress', resetSessionTimer)
  })
  
  return {
    clearSensitiveData,
    resetSessionTimer
  }
}
```

---

## 🌐 HTTP Security Headers

### 1. **API Request Security**
```typescript
// composables/useSecureApi.ts
export const useSecureApi = () => {
  const makeSecureRequest = async (url: string, options: RequestInit = {}) => {
    const secureOptions: RequestInit = {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest',
        'Cache-Control': 'no-cache, no-store, must-revalidate',
        'Pragma': 'no-cache',
        ...options.headers
      },
      credentials: 'same-origin' // Prevent CSRF
    }
    
    // Add CSRF token if available
    const csrfToken = getCsrfToken()
    if (csrfToken) {
      secureOptions.headers['X-CSRF-Token'] = csrfToken
    }
    
    try {
      const response = await $fetch(url, secureOptions)
      return response
    } catch (error) {
      // Log security-related errors
      if (error.status === 401 || error.status === 403) {
        console.warn('Authentication/Authorization error:', error)
        // Redirect to login
        await navigateTo('/login')
      }
      throw error
    }
  }
  
  const getCsrfToken = (): string | null => {
    const metaTag = document.querySelector('meta[name="csrf-token"]')
    return metaTag?.getAttribute('content') || null
  }
  
  return {
    makeSecureRequest
  }
}
```

### 2. **Content Security Policy**
```typescript
// nuxt.config.ts
export default defineNuxtConfig({
  nitro: {
    routeRules: {
      '/settings/api-keys/**': {
        headers: {
          'Content-Security-Policy': [
            "default-src 'self'",
            "script-src 'self' 'unsafe-inline'",
            "style-src 'self' 'unsafe-inline'",
            "img-src 'self' data: https:",
            "connect-src 'self'",
            "font-src 'self'",
            "object-src 'none'",
            "base-uri 'self'",
            "form-action 'self'"
          ].join('; '),
          'X-Frame-Options': 'DENY',
          'X-Content-Type-Options': 'nosniff',
          'Referrer-Policy': 'strict-origin-when-cross-origin'
        }
      }
    }
  }
})
```

---

## 🔍 Security Monitoring

### 1. **Client-Side Security Logging**
```typescript
// composables/useSecurityLogger.ts
export const useSecurityLogger = () => {
  const logSecurityEvent = (event: SecurityEvent) => {
    const logEntry = {
      timestamp: new Date().toISOString(),
      event: event.type,
      details: event.details,
      userAgent: navigator.userAgent,
      url: window.location.href,
      sessionId: getSessionId()
    }
    
    // Send to backend security monitoring
    $fetch('/api/security/log', {
      method: 'POST',
      body: logEntry
    }).catch(error => {
      console.error('Failed to log security event:', error)
    })
  }
  
  const logApiKeyCreation = (apiKeyId: string) => {
    logSecurityEvent({
      type: 'API_KEY_CREATED',
      details: { apiKeyId }
    })
  }
  
  const logApiKeyRevocation = (apiKeyId: string) => {
    logSecurityEvent({
      type: 'API_KEY_REVOKED',
      details: { apiKeyId }
    })
  }
  
  const logSuspiciousActivity = (activity: string, details: any) => {
    logSecurityEvent({
      type: 'SUSPICIOUS_ACTIVITY',
      details: { activity, ...details }
    })
  }
  
  return {
    logApiKeyCreation,
    logApiKeyRevocation,
    logSuspiciousActivity
  }
}

interface SecurityEvent {
  type: string
  details: Record<string, any>
}
```

### 2. **Rate Limiting Detection**
```typescript
// composables/useRateLimitDetection.ts
export const useRateLimitDetection = () => {
  const requestCounts = new Map<string, number>()
  const requestTimestamps = new Map<string, number[]>()
  
  const checkRateLimit = (endpoint: string, maxRequests = 10, windowMs = 60000): boolean => {
    const now = Date.now()
    const timestamps = requestTimestamps.get(endpoint) || []
    
    // Remove old timestamps outside window
    const validTimestamps = timestamps.filter(ts => now - ts < windowMs)
    
    if (validTimestamps.length >= maxRequests) {
      // Rate limit exceeded
      const securityLogger = useSecurityLogger()
      securityLogger.logSuspiciousActivity('RATE_LIMIT_EXCEEDED', {
        endpoint,
        requestCount: validTimestamps.length,
        windowMs
      })
      
      return false
    }
    
    // Add current timestamp
    validTimestamps.push(now)
    requestTimestamps.set(endpoint, validTimestamps)
    
    return true
  }
  
  return {
    checkRateLimit
  }
}
```

---

## 🧪 Security Testing

### 1. **Unit Tests for Security Functions**
```typescript
// tests/security/apiKeyValidation.test.ts
import { describe, it, expect } from 'vitest'
import { isValidIpAddress, isValidCidr } from '@/utils/ipValidation'

describe('IP Validation', () => {
  it('validates IPv4 addresses correctly', () => {
    expect(isValidIpAddress('192.168.1.1')).toBe(true)
    expect(isValidIpAddress('255.255.255.255')).toBe(true)
    expect(isValidIpAddress('256.1.1.1')).toBe(false)
    expect(isValidIpAddress('192.168.1')).toBe(false)
  })
  
  it('validates CIDR notation correctly', () => {
    expect(isValidCidr('192.168.1.0/24')).toBe(true)
    expect(isValidCidr('10.0.0.0/8')).toBe(true)
    expect(isValidCidr('192.168.1.0/33')).toBe(false)
    expect(isValidCidr('192.168.1.0')).toBe(false)
  })
  
  it('prevents XSS in input validation', () => {
    const { validateName } = useApiKeyValidation(ref({
      name: '<script>alert("xss")</script>',
      description: '',
      scopes: []
    }))
    
    expect(validateName('<script>alert("xss")</script>')).toContain('invalid characters')
  })
})
```

### 2. **E2E Security Tests**
```typescript
// tests/e2e/apiKeySecurity.spec.ts
import { test, expect } from '@playwright/test'

test.describe('API Key Security', () => {
  test('should not expose full API key in DOM', async ({ page }) => {
    await page.goto('/settings/api-keys')
    
    // Create API key
    await page.click('[data-testid="create-api-key-btn"]')
    await page.fill('[data-testid="name-input"]', 'Test Key')
    await page.click('[data-testid="submit-btn"]')
    
    // Check success modal shows key
    const successModal = page.locator('[data-testid="success-modal"]')
    await expect(successModal).toBeVisible()
    
    const apiKeyValue = await page.locator('[data-testid="api-key-value"]').textContent()
    expect(apiKeyValue).toMatch(/^pfm_[a-zA-Z0-9]{32}$/)
    
    // Close modal
    await page.click('[data-testid="confirm-saved-btn"]')
    
    // Verify key is not in DOM anymore
    const pageContent = await page.content()
    expect(pageContent).not.toContain(apiKeyValue)
  })
  
  test('should clear clipboard after timeout', async ({ page }) => {
    // Mock clipboard API
    await page.addInitScript(() => {
      let clipboardContent = ''
      Object.defineProperty(navigator, 'clipboard', {
        value: {
          writeText: (text: string) => {
            clipboardContent = text
            return Promise.resolve()
          },
          readText: () => Promise.resolve(clipboardContent)
        }
      })
    })
    
    await page.goto('/settings/api-keys')
    
    // Create and copy API key
    await page.click('[data-testid="create-api-key-btn"]')
    await page.fill('[data-testid="name-input"]', 'Test Key')
    await page.click('[data-testid="submit-btn"]')
    await page.click('[data-testid="copy-key-btn"]')
    
    // Check clipboard has content
    const clipboardContent = await page.evaluate(() => navigator.clipboard.readText())
    expect(clipboardContent).toMatch(/^pfm_[a-zA-Z0-9]{32}$/)
    
    // Wait for auto-clear timeout
    await page.waitForTimeout(31000)
    
    // Check clipboard is cleared
    const clearedContent = await page.evaluate(() => navigator.clipboard.readText())
    expect(clearedContent).toBe('')
  })
})
```

---

## 🚨 Security Incident Response

### 1. **Incident Detection**
```typescript
// composables/useSecurityIncidentDetection.ts
export const useSecurityIncidentDetection = () => {
  const detectAnomalousActivity = () => {
    // Monitor for suspicious patterns
    const suspiciousPatterns = [
      'Multiple failed API key creations',
      'Rapid API key creation/deletion',
      'Unusual IP address patterns',
      'Suspicious user agent strings'
    ]
    
    // Implementation would monitor these patterns
    // và trigger alerts khi detected
  }
  
  const handleSecurityIncident = (incident: SecurityIncident) => {
    // Log incident
    console.error('Security incident detected:', incident)
    
    // Clear sensitive data
    const secureSession = useSecureSession()
    secureSession.clearSensitiveData()
    
    // Notify backend
    $fetch('/api/security/incident', {
      method: 'POST',
      body: incident
    })
    
    // Redirect to safe page
    navigateTo('/security-notice')
  }
  
  return {
    detectAnomalousActivity,
    handleSecurityIncident
  }
}

interface SecurityIncident {
  type: string
  severity: 'low' | 'medium' | 'high' | 'critical'
  details: Record<string, any>
  timestamp: string
}
```

### 2. **Emergency Response**
```typescript
// Emergency security response procedures
export const useEmergencyResponse = () => {
  const emergencyLockdown = () => {
    // Clear all sensitive data
    localStorage.clear()
    sessionStorage.clear()
    
    // Clear API key store
    const apiKeyStore = useApiKeysStore()
    apiKeyStore.$reset()
    
    // Disable all API key operations
    const router = useRouter()
    router.push('/security-lockdown')
  }
  
  const reportSecurityBreach = (details: any) => {
    // Immediate reporting to security team
    $fetch('/api/security/breach', {
      method: 'POST',
      body: {
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent,
        url: window.location.href,
        details
      }
    })
  }
  
  return {
    emergencyLockdown,
    reportSecurityBreach
  }
}
```

---

## 📝 Security Checklist

### Pre-Deployment Security Checklist:
- [ ] No full API keys stored in frontend state
- [ ] One-time display pattern implemented
- [ ] Secure clipboard operations với auto-clear
- [ ] Input validation và sanitization
- [ ] XSS prevention measures
- [ ] CSRF protection enabled
- [ ] Content Security Policy configured
- [ ] Rate limiting detection
- [ ] Security logging implemented
- [ ] Session timeout configured
- [ ] Emergency response procedures
- [ ] Security tests passing

### Runtime Security Monitoring:
- [ ] Monitor for suspicious API key creation patterns
- [ ] Track failed authentication attempts
- [ ] Log all security-relevant events
- [ ] Monitor for XSS attempts
- [ ] Check for unusual user behavior
- [ ] Validate all user inputs
- [ ] Ensure HTTPS enforcement
- [ ] Monitor clipboard operations

---

*Last updated: December 28, 2024*
*Implementation: API Key Security Measures* 