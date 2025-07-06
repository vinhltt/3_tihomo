/**
 * Auth API Proxy (EN)
 * Proxy API cho xác thực (VI)
 * 
 * Proxies authentication requests to the Identity API through Ocelot Gateway
 * Chuyển tiếp yêu cầu xác thực đến Identity API thông qua Ocelot Gateway
 */

import { createApiProxy } from '~/server/utils/apiProxy'

export default defineEventHandler(async (event) => {
  return await createApiProxy(event, {
    basePath: 'identity/auth',
    serviceName: 'auth service'
  })
})
