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

                <!-- Login Section -->
                <div class="w-full text-center">
                    <h2 class="text-2xl font-semibold text-primary mb-2">Welcome to TiHoMo</h2>
                    <p class="text-white-dark mb-6">Sign in to your account to continue</p>
                    
                    <!-- Google Login Button -->
                    <button
                        @click="handleGoogleLogin"
                        :disabled="socialLoading"
                        class="flex w-full items-center justify-center gap-3 rounded-lg border border-white-dark/30 bg-white px-4 py-3 text-black hover:bg-gray-50 hover:border-primary focus:border-primary focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed dark:bg-black dark:text-white dark:hover:bg-gray-800 transition-all duration-200"
                    >
                        <span v-if="socialLoading" class="mr-2">
                            <svg class="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                            </svg>
                        </span>
                        <icon-google class="h-5 w-5" />
                        <span class="font-medium">
                            {{ socialLoading ? 'Signing in with Google...' : 'Continue with Google' }}
                        </span>
                    </button>

                    <!-- Error Message -->
                    <div v-if="socialError" class="mt-4 rounded-md bg-red-50 p-3 dark:bg-red-900/10">
                        <div class="text-sm text-red-800 dark:text-red-200">
                            <p>{{ socialError }}</p>
                        </div>
                    </div>

                    <!-- Alternative Login -->
                    <div class="mt-6 text-center text-sm text-white-dark">
                        Need help? 
                        <NuxtLink to="/auth/cover-login" class="text-primary hover:underline">
                            Try alternative login
                        </NuxtLink>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useSocialAuth } from '@/composables/useSocialAuth'

// Composables
const { loginWith, isLoading: socialLoading, error: socialError, initializeProviders } = useSocialAuth()

/**
 * Handle Google login
 * Xử lý đăng nhập Google
 */
const handleGoogleLogin = async (): Promise<void> => {
    try {
        await loginWith('Google')
        console.log('Google login successful')
        // Navigation will be handled by the auth store
    } catch (err) {
        console.error('Google login error:', err)
    }
}

// Initialize social providers on mount
onMounted(async () => {
    try {
        await initializeProviders()
    } catch (error) {
        console.warn('Failed to initialize social providers:', error)
    }
})

// Page metadata
definePageMeta({
    layout: 'auth-layout',
    title: 'Login with Google',
    auth: false // Disable auth middleware for this page
})
</script>
