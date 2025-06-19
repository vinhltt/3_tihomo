import type { SsoConfig, AuthorizationRequest, TokenRequest, SsoError } from '@/types/sso'

/**
 * SSO utility functions for OAuth2/OpenID Connect flows
 * Các hàm tiện ích SSO cho OAuth2/OpenID Connect flows
 */

/**
 * Generate cryptographically secure random string
 * Tạo chuỗi ngẫu nhiên an toàn mã hóa
 */
export function generateRandomString(length: number = 32): string {
  const possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~'
  let text = ''
  
  for (let i = 0; i < length; i++) {
    text += possible.charAt(Math.floor(Math.random() * possible.length))
  }
  
  return text
}

/**
 * Generate PKCE code verifier and challenge
 * Tạo PKCE code verifier và challenge
 */
export async function generatePkceChallenge(): Promise<{ verifier: string, challenge: string }> {
  const verifier = generateRandomString(128)
  
  // Create code challenge using SHA256
  const encoder = new TextEncoder()
  const data = encoder.encode(verifier)
  const digest = await crypto.subtle.digest('SHA-256', data)
  
  // Convert to base64url
  const challenge = btoa(String.fromCharCode(...new Uint8Array(digest)))
    .replace(/=/g, '')
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
  
  return { verifier, challenge }
}

/**
 * Build authorization URL for OAuth2 flow
 * Xây dựng URL authorization cho OAuth2 flow
 */
export function buildAuthorizationUrl(
  config: SsoConfig, 
  params: Partial<AuthorizationRequest> = {},
  codeVerifier?: string
): string {
  const state = generateRandomString()
  
  const authParams = new URLSearchParams({
    client_id: config.clientId,
    redirect_uri: config.redirectUri,
    response_type: config.responseType,
    scope: config.scope,
    state,
    ...params
  })
  
  // Store state in sessionStorage for validation
  if (process.client) {
    sessionStorage.setItem('sso_state', state)
    if (codeVerifier) {
      sessionStorage.setItem('sso_code_verifier', codeVerifier)
    }
  }
  
  return `${config.baseUrl}/connect/authorize?${authParams.toString()}`
}

/**
 * Build logout URL for SSO logout
 * Xây dựng URL logout cho SSO logout
 */
export function buildLogoutUrl(config: SsoConfig, idToken?: string): string {
  const logoutParams = new URLSearchParams({
    post_logout_redirect_uri: config.logoutRedirectUri
  })
  
  if (idToken) {
    logoutParams.set('id_token_hint', idToken)
  }
  
  return `${config.baseUrl}/connect/logout?${logoutParams.toString()}`
}

/**
 * Validate authorization callback parameters
 * Xác thực parameters từ authorization callback
 */
export function validateAuthorizationCallback(searchParams: URLSearchParams): { isValid: boolean, error?: SsoError } {
  const state = searchParams.get('state')
  const code = searchParams.get('code')
  const error = searchParams.get('error')
  
  // Check for OAuth error response
  if (error) {
    return {
      isValid: false,
      error: {
        error,
        error_description: searchParams.get('error_description') || undefined,
        error_uri: searchParams.get('error_uri') || undefined,
        state: state || undefined
      }
    }
  }
  
  // Validate state parameter
  if (process.client) {
    const storedState = sessionStorage.getItem('sso_state')
    if (!state || state !== storedState) {
      return {
        isValid: false,
        error: {
          error: 'invalid_state',
          error_description: 'Invalid or missing state parameter'
        }
      }
    }
  }
  
  // Validate authorization code
  if (!code) {
    return {
      isValid: false,
      error: {
        error: 'invalid_request',
        error_description: 'Authorization code is missing'
      }
    }
  }
  
  return { isValid: true }
}

/**
 * Exchange authorization code for tokens
 * Trao đổi authorization code lấy tokens
 */
export async function exchangeCodeForTokens(config: SsoConfig, code: string): Promise<any> {
  const codeVerifier = process.client ? sessionStorage.getItem('sso_code_verifier') : null
  
  const tokenRequest: TokenRequest = {
    clientId: config.clientId,
    redirectUri: config.redirectUri,
    grantType: config.grantType,
    code,
    codeVerifier: codeVerifier || undefined
  }
  
  const response = await fetch(`${config.baseUrl}/connect/token`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: new URLSearchParams(tokenRequest as any)
  })
  
  if (!response.ok) {
    const errorData = await response.json().catch(() => null)
    throw new Error(errorData?.error_description || `Token exchange failed: ${response.status}`)
  }
  
  return await response.json()
}

/**
 * Clean up SSO session storage
 * Dọn dẹp session storage của SSO
 */
export function cleanupSsoSession(): void {
  if (process.client) {
    sessionStorage.removeItem('sso_state')
    sessionStorage.removeItem('sso_code_verifier')
  }
}
