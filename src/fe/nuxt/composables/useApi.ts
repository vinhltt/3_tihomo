import type { ApiError } from '~/types'

export const useApi = () => {
  const config = useRuntimeConfig()
  const baseURL = config.public.apiBase
  const authStore = useAuthStore()

  /**
   * Generic API call wrapper with error handling and authentication
   */
  const apiCall = async <T>(
    endpoint: string,
    options: {
      method?: 'GET' | 'POST' | 'PUT' | 'DELETE'
      body?: any
      query?: Record<string, any>
      isFormData?: boolean
    } = {}
  ): Promise<T> => {
    try {
      const headers: Record<string, string> = {
        'Accept': 'application/json'
      }

      // Add authentication headers if user is authenticated
      if (authStore.isAuthenticated && authStore.token) {
        headers['Authorization'] = `Bearer ${authStore.token}`
      }

      let body = options.body

      // Convert to FormData if needed
      if (options.isFormData && body && typeof body === 'object') {
        const formData = ConvertObjectToFormData(body, new FormData());
        // Debug: Log all FormData entries
        console.log('FormData entries:')
        for (const [key, value] of formData.entries()) {
          console.log(`  ${key}: ${value}`)
        }

        body = formData
        // Don't set Content-Type for FormData, let browser set it with boundary
      } else {
        headers['Content-Type'] = 'application/json'
      }
      console.log('apiCall', endpoint, options, body, headers)

      // Use native fetch for FormData to avoid issues with $fetch
      if (options.isFormData) {
        const url = new URL(`${baseURL}${endpoint}`)
        if (options.query) {
          Object.keys(options.query).forEach(key => {
            url.searchParams.append(key, options.query![key])
          })
        }        const response = await fetch(url.toString(), {
          method: options.method || 'GET',
          body,
          headers: {
            'Accept': 'application/json',
            // Add authentication headers for FormData requests too
            ...(authStore.isAuthenticated && authStore.token ? {
              'Authorization': `Bearer ${authStore.token}`
            } : {})
            // Don't set Content-Type for FormData
          }
        })

        if (!response.ok) {
          // Check for 401 Unauthorized - token expired
          if (response.status === 401) {
            console.log('🔐 401 Unauthorized detected (FormData) - clearing auth and redirecting to login')
            
            // Clear authentication state
            const authStore = useAuthStore()
            await authStore.clearAuthState()
            
            // Redirect to login page
            await navigateTo('/auth/login')
          }
          
          const errorData = await response.json().catch(() => ({}))
          throw {
            message: response.status === 401 ? 'Your session has expired. Please login again.' : (errorData.message || 'An error occurred'),
            statusCode: response.status,
            errors: errorData.errors
          } as ApiError
        }

        return await response.json()
      } else {
        const response = await $fetch<T>(`${baseURL}${endpoint}`, {
          method: options.method || 'GET',
          body,
          query: options.query,
          headers
        })
        return response
      }
    } catch (error: any) {
      // Handle different types of errors
      if (error.response) {
        // Check for 401 Unauthorized - token expired
        if (error.response.status === 401) {
          console.log('🔐 401 Unauthorized detected - clearing auth and redirecting to login')
          
          // Clear authentication state
          const authStore = useAuthStore()
          await authStore.clearAuthState()
          
          // Redirect to login page
          await navigateTo('/auth/login')
          
          // Still throw the error for the calling component to handle if needed
          const apiError: ApiError = {
            message: 'Your session has expired. Please login again.',
            statusCode: 401,
            errors: error.response._data?.errors
          }
          throw apiError
        }
        
        // Server responded with other error status
        const apiError: ApiError = {
          message: error.response._data?.message || 'An error occurred',
          statusCode: error.response.status,
          errors: error.response._data?.errors
        }
        throw apiError
      } else if (error.request) {
        // Network error
        throw {
          message: 'Network error. Please check your connection.',
          statusCode: 0
        } as ApiError
      } else {
        // Other error
        throw {
          message: error.message || 'An unexpected error occurred',
          statusCode: 500
        } as ApiError
      }
    }
  }

  /**
   * GET request
   */
  const get = <T>(endpoint: string, query?: Record<string, any>): Promise<T> => {
    return apiCall<T>(endpoint, { method: 'GET', query })
  }

  /**
   * POST request
   */
  const post = <T>(endpoint: string, body?: any): Promise<T> => {
    return apiCall<T>(endpoint, { method: 'POST', body })
  }

  /**
   * POST request with form data
   */
  const postForm = <T>(endpoint: string, body?: any): Promise<T> => {
    return apiCall<T>(endpoint, { method: 'POST', body, isFormData: true })
  }

  /**
   * PUT request
   */
  const put = <T>(endpoint: string, body?: any): Promise<T> => {
    return apiCall<T>(endpoint, { method: 'PUT', body })
  }

  /**
   * PUT request with form data
   */
  const putForm = <T>(endpoint: string, body?: any): Promise<T> => {
    return apiCall<T>(endpoint, { method: 'PUT', body, isFormData: true })
  }

  /**
   * DELETE request
   */
  const del = <T>(endpoint: string): Promise<T> => {
    return apiCall<T>(endpoint, { method: 'DELETE' })
  }

  return {
    apiCall,
    get,
    post,
    postForm,
    put,
    putForm,
    delete: del
  }
} 
const ConvertObjectToFormData = (
  object: any,
  formData = new FormData(),
  namespace: string | undefined = undefined
): FormData => {
  for (let property in object) {
    if (!object.hasOwnProperty(property) || !object[property]) {
      continue;
    }
    const formKey = namespace ? `${namespace}[${property}]` : property;
    if (object[property] instanceof Date) {
      formData.append(formKey, object[property].toISOString());
    } else if (
      typeof object[property] === 'object' &&
      !(object[property] instanceof File)
    ) {
      ConvertObjectToFormData(object[property], formData, formKey);
    } else {
      formData.append(formKey, object[property]);
    }
  }
  return formData;
};