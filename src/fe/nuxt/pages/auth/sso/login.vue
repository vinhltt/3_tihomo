<template>
    <div>
        <div class="absolute inset-0">
            <img src="/assets/images/auth/bg-gradient.png" alt="image" class="h-full w-full object-cover" />
        </div>
        <div
            class="relative flex min-h-screen items-center justify-center bg-[url(/assets/images/auth/map.png)] bg-cover bg-center bg-no-repeat px-6 py-10 dark:bg-[#060818] sm:px-16"
        >
            <img src="/assets/images/auth/coming-soon-object1.png" alt="image" class="absolute left-0 top-1/2 h-full max-h-[893px] -translate-y-1/2" />
            <img src="/assets/images/auth/coming-soon-object2.png" alt="image" class="absolute left-24 top-0 h-40 md:left-[30%]" />
            <img src="/assets/images/auth/coming-soon-object3.png" alt="image" class="absolute right-0 top-0 h-[300px]" />
            <img src="/assets/images/auth/polygon-object.svg" alt="image" class="absolute bottom-0 end-[28%]" />
            
            <div class="relative flex w-full max-w-[1502px] flex-col justify-between overflow-hidden rounded-md bg-white/60 backdrop-blur-lg dark:bg-black/50 lg:min-h-[758px] lg:flex-row lg:gap-10 xl:gap-0">
                <!-- Left side - Branding -->
                <div class="relative hidden w-full items-center justify-center bg-[linear-gradient(225deg,rgba(239,18,98,1)_0%,rgba(67,97,238,1)_100%)] p-5 lg:inline-flex lg:max-w-[835px] xl:-ms-28 ltr:xl:skew-x-[14deg] rtl:xl:skew-x-[-14deg]">
                    <div class="absolute inset-y-0 w-8 from-primary/10 via-transparent to-transparent ltr:-right-10 ltr:bg-gradient-to-r rtl:-left-10 rtl:bg-gradient-to-l xl:w-16 ltr:xl:-right-20 rtl:xl:-left-20"></div>
                    <div class="ltr:xl:-skew-x-[14deg] rtl:xl:skew-x-[14deg]">
                        <NuxtLink to="/" class="ms-10 block w-48 lg:w-72">
                            <img src="/assets/images/auth/logo-white.svg" alt="Logo" class="w-full" />
                        </NuxtLink>
                        <div class="mt-24 hidden w-full max-w-[430px] lg:block">
                            <img src="/assets/images/auth/login.svg" alt="Cover Image" class="w-full" />
                        </div>
                    </div>
                </div>

                <!-- Right side - SSO Login -->
                <div class="relative flex w-full flex-col items-center justify-center gap-6 px-4 pb-16 pt-6 sm:px-6 lg:max-w-[667px]">
                    <!-- Mobile Logo -->
                    <NuxtLink to="/" class="block w-8 lg:hidden">
                        <img src="/assets/images/logo.svg" alt="Logo" class="mx-auto w-10" />
                    </NuxtLink>

                    <!-- SSO Login Form -->
                    <div class="w-full max-w-[440px]">
                        <div class="mb-10">
                            <h1 class="text-3xl font-extrabold uppercase !leading-snug text-primary md:text-4xl">
                                Single Sign-On
                            </h1>
                            <p class="text-base font-bold leading-normal text-white-dark">
                                Secure authentication via SSO server
                            </p>
                        </div>

                        <!-- Error Display -->
                        <div v-if="error" class="mb-4 rounded-md bg-danger-light p-4 text-danger">
                            <div class="flex items-center">
                                <svg class="h-5 w-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                                    <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
                                </svg>
                                <span>{{ error }}</span>
                            </div>
                        </div>

                        <!-- Loading State -->
                        <div v-if="isLoading" class="text-center py-8">
                            <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
                            <p class="mt-4 text-white-dark">Redirecting to SSO server...</p>
                        </div>

                        <!-- SSO Login Button -->
                        <div v-else class="space-y-5">
                            <button
                                @click="handleSsoLogin"
                                :disabled="isLoading"
                                class="btn btn-gradient !mt-6 w-full border-0 uppercase shadow-[0_10px_20px_-10px_rgba(67,97,238,0.44)]"
                            >
                                <svg class="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
                                    <path fill-rule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clip-rule="evenodd" />
                                </svg>
                                Login with SSO
                            </button>

                            <!-- Information -->
                            <div class="text-center">
                                <p class="text-white-dark">
                                    You will be redirected to the secure SSO server for authentication.
                                </p>
                            </div>

                            <!-- Development Info -->
                            <div v-if="isDevelopment" class="mt-8 p-4 bg-info-light rounded-md text-sm">
                                <h4 class="font-semibold mb-2 text-info">Development Mode</h4>
                                <div class="space-y-1 text-white-dark">
                                    <p><strong>SSO Server:</strong> {{ ssoConfig.baseUrl }}</p>
                                    <p><strong>Client ID:</strong> {{ ssoConfig.clientId }}</p>
                                    <p><strong>Redirect URI:</strong> {{ ssoConfig.redirectUri }}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { useSso } from '@/composables/useSso'

// SSO composable
const { login, isLoading, error, clearError, state } = useSso()
const route = useRoute()

// Development mode check
const isDevelopment = process.env.NODE_ENV === 'development'

// Get SSO config for display
const config = useRuntimeConfig()
const ssoConfig = {
    baseUrl: config.public.ssoBase as string,
    clientId: config.public.ssoClientId as string,
    redirectUri: config.public.ssoRedirectUri as string
}

/**
 * Handle SSO login initiation
 * Xử lý khởi tạo đăng nhập SSO
 */
const handleSsoLogin = async (): Promise<void> => {
    clearError()
    
    try {
        const returnUrl = route.query.returnUrl as string
        await login(returnUrl)
    } catch (err) {
        console.error('SSO login error:', err)
    }
}

// Page metadata
definePageMeta({
    layout: 'auth-layout',
    title: 'SSO Login'
})
</script>
