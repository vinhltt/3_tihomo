/**
 * Debug utilities for authentication state
 * Tiện ích debug cho trạng thái xác thực
 */

export function debugAuthState(context: string) {
  const tokenCookie = useCookie('auth-token', { default: () => null })
  const refreshCookie = useCookie('refresh-token', { default: () => null })
  
  console.group(`🔍 Auth Debug - ${context}`)
  console.log('Process:', process.server ? 'SERVER' : 'CLIENT')
  console.log('Token Cookie:', tokenCookie.value ? '✅ Present' : '❌ Missing')
  console.log('Refresh Cookie:', refreshCookie.value ? '✅ Present' : '❌ Missing')
  
  if (process.client) {
    const authStore = useAuthStore()
    console.log('Store Authenticated:', authStore.isAuthenticated)
    console.log('Store Loading:', authStore.isLoading)
    console.log('Store Error:', authStore.error)
  }
  
  console.groupEnd()
}

export function logAuthFlow(step: string, details?: any) {
  console.log(`🔐 Auth Flow: ${step}`, details || '')
}
