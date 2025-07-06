/**
 * Identity API Proxy (EN)
 * Proxy API cho Identity service (VI)
 * 
 * Proxies identity requests to the Identity API through Ocelot Gateway
 * Chuyển tiếp yêu cầu identity đến Identity API qua Ocelot Gateway
 */

import { createApiProxy } from '~/server/utils/apiProxy'

export default defineEventHandler(async (event) => {
  return await createApiProxy(event, {
    basePath: 'identity',
    serviceName: 'identity service'
  })
})
