/**
 * SSO (Single Sign-On) type definitions for OAuth2/OpenID Connect integration
 * Định nghĩa types cho tích hợp SSO OAuth2/OpenID Connect
 */

export type SsoConfig = {
  baseUrl: string
  clientId: string
  redirectUri: string
  logoutRedirectUri: string
  scope: string
  responseType: string
  grantType: string
}

export type AuthorizationRequest = {
  clientId: string
  redirectUri: string
  responseType: string
  scope: string
  state: string
  codeChallenge?: string
  codeChallengeMethod?: string
}

export type TokenRequest = {
  clientId: string
  redirectUri: string
  grantType: string
  code: string
  codeVerifier?: string
}

export type TokenResponse = {
  access_token: string
  token_type: string
  expires_in: number
  refresh_token?: string
  scope: string
  id_token?: string
}

export type SsoUserInfo = {
  sub: string
  name: string
  email: string
  roles: string[]
  email_verified?: boolean
}

export type SsoState = {
  isLoading: boolean
  isAuthenticated: boolean
  user: SsoUserInfo | null
  accessToken: string | null
  refreshToken: string | null
  error: string | null
}

export type SsoError = {
  error: string
  error_description?: string
  error_uri?: string
  state?: string
}
