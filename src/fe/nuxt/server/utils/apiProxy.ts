/**
 * Shared API Proxy Utility (EN)
 * Tiện ích chia sẻ cho API Proxy (VI)
 * 
 * Consolidates common proxy logic to reduce code duplication
 * Tập trung logic proxy chung để giảm trùng lặp code
 */

import type { H3Event } from 'h3'

interface ProxyOptions {
  /** Base path for the target URL (e.g., 'api', 'identity', 'identity/auth') */
  basePath: string
  /** Service name for error messages */
  serviceName: string
}

/**
 * Generic API proxy handler (EN)
 * Xử lý proxy API tổng quát (VI)
 */
export async function createApiProxy(event: H3Event, options: ProxyOptions) {
  const config = useRuntimeConfig()
  const path = getRouterParam(event, 'path')
  const query = getQuery(event)
  
  // Get the API base URL from environment
  const apiBaseUrl = process.env.API_BASE_URL || config.public.apiBase
  console.log('apiBaseUrl', apiBaseUrl, process.env.API_BASE_URL, config.public.apiBase);
  // Construct the target URL
  const targetUrl = `${apiBaseUrl}/${options.basePath}/${path}`
  
  // Prepare headers to forward
  const headers: Record<string, string> = {}
  
  // Forward important headers
  const headersToForward = [
    'authorization',
    'content-type',
    'accept',
    'user-agent',
    'x-correlation-id'
  ]
  
  headersToForward.forEach(headerName => {
    const headerValue = getHeader(event, headerName)
    if (headerValue) {
      headers[headerName] = headerValue
    }
  })
  
  try {
    // Get the request body if it exists
    let body: any = undefined
    if (['POST', 'PUT', 'PATCH'].includes(getMethod(event))) {
      body = await readBody(event)
    }
    console.log('targetUrl', targetUrl);
    // Make the proxy request
    const response = await $fetch(targetUrl, {
      method: getMethod(event),
      headers,
      body,
      query,
      // Don't throw on error status codes, let the client handle them
      ignoreResponseError: true
    })
    
    return response
  } catch (error: any) {
    // Handle network errors or other fetch errors
    console.error(`Proxy error for /${options.basePath}:`, error)
    
    // Return a structured error response
    throw createError({
      statusCode: error.status || 500,
      statusMessage: error.statusText || 'Proxy Error',
      data: {
        message: error.message || `Failed to proxy request to ${options.serviceName}`,
        path: targetUrl
      }
    })
  }
} 