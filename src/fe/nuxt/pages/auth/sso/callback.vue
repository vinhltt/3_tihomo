<template>
    <div>
        <div class="absolute inset-0">
            <img src="/assets/images/auth/bg-gradient.png" alt="image" class="h-full w-full object-cover" />
        </div>
        <div
            class="relative flex min-h-screen items-center justify-center bg-[url(/assets/images/auth/map.png)] bg-cover bg-center bg-no-repeat px-6 py-10 dark:bg-[#060818] sm:px-16"
        >
            <div class="relative flex w-full max-w-[440px] flex-col items-center justify-center gap-6 rounded-md bg-white/60 backdrop-blur-lg p-8 dark:bg-black/50">
                <!-- Logo -->
                <NuxtLink to="/" class="block">
                    <img src="/assets/images/logo.svg" alt="Logo" class="mx-auto w-16" />
                </NuxtLink>

                <!-- Processing State -->
                <div v-if="isProcessing" class="text-center">
                    <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary mb-4"></div>
                    <h2 class="text-xl font-semibold text-primary mb-2">Processing Authentication</h2>
                    <p class="text-white-dark">Please wait while we complete your login...</p>
                </div>

                <!-- Success State -->
                <div v-else-if="isSuccess" class="text-center">
                    <div class="inline-block rounded-full bg-success p-3 mb-4">
                        <svg class="w-8 h-8 text-white" fill="currentColor" viewBox="0 0 20 20">
                            <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
                        </svg>
                    </div>
                    <h2 class="text-xl font-semibold text-success mb-2">Login Successful</h2>
                    <p class="text-white-dark">Redirecting to dashboard...</p>
                </div>

                <!-- Error State -->
                <div v-else-if="error" class="text-center">
                    <div class="inline-block rounded-full bg-danger p-3 mb-4">
                        <svg class="w-8 h-8 text-white" fill="currentColor" viewBox="0 0 20 20">
                            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
                        </svg>
                    </div>
                    <h2 class="text-xl font-semibold text-danger mb-2">Authentication Failed</h2>
                    <p class="text-white-dark mb-4">{{ error }}</p>
                    
                    <!-- Retry Button -->
                    <button
                        @click="retryLogin"
                        class="btn btn-primary"
                    >
                        Try Again
                    </button>
                </div>

                <!-- Development Debug Info -->
                <div v-if="isDevelopment && debugInfo" class="mt-6 p-4 bg-gray-100 rounded-md text-xs w-full">
                    <h4 class="font-semibold mb-2">Debug Info:</h4>
                    <pre class="text-gray-600 whitespace-pre-wrap">{{ debugInfo }}</pre>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { useSso } from '@/composables/useSso'

// SSO composable
const { handleCallback, isLoading, isAuthenticated, error, clearError } = useSso()
const router = useRouter()

// Component state
const isProcessing = ref(true)
const isSuccess = ref(false)
const debugInfo = ref('')

// Development mode check
const isDevelopment = process.env.NODE_ENV === 'development'

/**
 * Retry login on error
 * Thử lại đăng nhập khi có lỗi
 */
const retryLogin = async (): Promise<void> => {
    await router.push('/auth/sso/login')
}

/**
 * Process SSO callback
 * Xử lý callback SSO
 */
const processCallback = async (): Promise<void> => {
    try {
        if (isDevelopment) {
            debugInfo.value = JSON.stringify({
                url: window.location.href,
                search: window.location.search,
                params: Object.fromEntries(new URLSearchParams(window.location.search))
            }, null, 2)
        }

        await handleCallback()
        
        // If we get here, authentication was successful
        isSuccess.value = true
        isProcessing.value = false
        
        // The redirect will be handled by the composable
    } catch (err: any) {
        console.error('SSO callback processing error:', err)
        isProcessing.value = false
        
        if (isDevelopment) {
            debugInfo.value += '\n\nError Details:\n' + JSON.stringify({
                message: err.message,
                stack: err.stack
            }, null, 2)
        }
    }
}

// Process callback on mount
onMounted(() => {
    // Clear any previous errors
    clearError()
    
    // Process the callback
    processCallback()
})

// Watch for changes in authentication state
watch(isAuthenticated, (newValue) => {
    if (newValue) {
        isSuccess.value = true
        isProcessing.value = false
    }
})

// Page metadata
definePageMeta({
    layout: 'auth-layout',
    title: 'SSO Callback'
})
</script>
