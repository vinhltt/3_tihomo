/**
 * General API Proxy (EN)
 * Proxy API tổng quát (VI)
 * 
 * Proxies general API requests to backend services through Ocelot Gateway
 * Chuyển tiếp yêu cầu API tổng quát đến backend services qua Ocelot Gateway
 */

import { createApiProxy } from '~/server/utils/apiProxy'

export default defineEventHandler(async (event) => {
  return await createApiProxy(event, {
    basePath: 'api',
    serviceName: 'API service'
  })
})
