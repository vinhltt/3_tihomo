/**
 * Social Login API Proxy (EN)
 * Proxy API cho đăng nhập xã hội (VI)
 * 
 * Proxies social login requests to the Identity API through Ocelot Gateway
 * Chuyển tiếp yêu cầu đăng nhập xã hội đến Identity API thông qua Ocelot Gateway
 */

export default defineEventHandler(async (event): Promise<any> => {
  const config = useRuntimeConfig()
  const body: any = await readBody(event)

  try {
    // Proxy request to Identity API through Gateway (identity route)
    const response: any = await $fetch('/identity/auth/social-login', {
      method: 'POST',
      baseURL: config.public.identityApiBase,
      headers: {
        'Content-Type': 'application/json',
      },
      body
    })

    return response
  } catch (error: any) {
    // Handle and forward the error
    console.error('Social login proxy error:', error)
    
    throw createError({
      statusCode: error.status || error.statusCode || 500,
      statusMessage: error.message || 'Social login failed'
    })
  }
})
