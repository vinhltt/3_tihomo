# Social Login Setup Guide

This guide explains how to configure and use the new social login system for TiHoMo frontend.

## Overview

The TiHoMo frontend has been migrated from SSO authentication to social login using Google and Facebook OAuth. This provides a more streamlined user experience and reduces the complexity of managing a separate SSO server.

## Configuration

### Environment Variables

Add the following environment variables to your `.env` file:

```env
# Identity API
NUXT_PUBLIC_IDENTITY_API_BASE=https://localhost:5214

# Social Login Providers
APP_PUBLIC_GOOGLE_CLIENT_ID=your-google-client-id
NUXT_PUBLIC_FACEBOOK_APP_ID=your-facebook-app-id
```

### Google OAuth Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the Google+ API
4. Create OAuth 2.0 credentials
5. Add your domain to authorized JavaScript origins
6. Add your callback URLs to authorized redirect URIs
7. Copy the Client ID to your `.env` file

### Facebook OAuth Setup

1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Create a new app or select an existing one
3. Add Facebook Login product
4. Configure OAuth redirect URIs
5. Copy the App ID to your `.env` file

## Usage

### Available Login Pages

1. **Simple Login** (`/auth/login`): Social login buttons only
2. **Cover Login** (`/auth/cover-login`): Email/password + social login options

### Programmatic Usage

```typescript
// Using the social auth composable
const { loginWith, isLoading, error } = useSocialAuth()

// Login with Google
await loginWith('Google')

// Login with Facebook
await loginWith('Facebook')
```

### Individual Provider Composables

```typescript
// Google only
const { login, isLoading, error } = useGoogleAuth()
await login()

// Facebook only
const { login, isLoading, error } = useFacebookAuth()
await login()
```

## Architecture

### Key Components

1. **Types** (`types/auth.ts`):
   - `SocialProvider`: 'Google' | 'Facebook'
   - `SocialLoginRequest`: Provider-specific login data
   - `SocialLoginResponse`: Backend response with tokens
   - `UserInfo`: User profile information

2. **Composables**:
   - `useSocialAuth`: Unified interface for all providers
   - `useGoogleAuth`: Google-specific authentication
   - `useFacebookAuth`: Facebook-specific authentication

3. **Store** (`stores/auth.ts`):
   - Updated to support social login mode
   - Handles JWT tokens and user session management
   - Automatic token refresh

### Authentication Flow

1. User clicks social login button
2. Frontend SDK handles OAuth flow
3. Frontend receives OAuth token
4. Frontend sends token to Identity.Api
5. Backend validates token with provider
6. Backend returns JWT tokens
7. Frontend stores tokens and redirects user

## Backend Integration

The frontend expects the following endpoints from Identity.Api:

```
POST /api/auth/social/google
POST /api/auth/social/facebook
POST /api/auth/refresh-token
```

## Migration Notes

### Removed Components

- SSO-related pages (`/auth/sso/*`)
- SSO composable (`composables/useSso.ts`)
- SSO utilities (`utils/sso.ts`)
- SSO types (`types/sso.ts`)

### Updated Components

- Authentication store now supports social login
- Login pages updated with social buttons
- Runtime configuration updated
- Environment variables updated

## Testing

1. Start the development server: `npm run dev`
2. Navigate to `/auth/login` or `/auth/cover-login`
3. Click on Google or Facebook login buttons
4. Verify authentication flow (requires backend to be running)

## Troubleshooting

### Common Issues

1. **"Provider not configured"**: Check environment variables
2. **"Failed to initialize"**: Verify provider credentials
3. **"Network error"**: Ensure Identity.Api is running
4. **"Popup blocked"**: Allow popups for OAuth flow

### Debug Mode

Enable debug logging by setting:
```env
NUXT_DEBUG=1
```

## Security Notes

- All OAuth tokens are exchanged server-side
- JWT tokens are stored in secure HTTP-only cookies
- CSRF protection is implemented
- Token refresh is handled automatically

## Next Steps

1. Configure social provider credentials
2. Test authentication flow
3. Implement user profile management
4. Add additional social providers if needed
