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

                <!-- Logout Success -->
                <div class="text-center">
                    <div class="inline-block rounded-full bg-success p-3 mb-4">
                        <svg class="w-8 h-8 text-white" fill="currentColor" viewBox="0 0 20 20">
                            <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
                        </svg>
                    </div>
                    <h2 class="text-xl font-semibold text-success mb-2">Successfully Logged Out</h2>
                    <p class="text-white-dark mb-6">You have been safely logged out from all systems.</p>
                    
                    <!-- Login Again Button -->
                    <div class="space-y-3">
                        <button
                            @click="loginAgain"
                            class="btn btn-primary w-full"
                        >
                            Login Again
                        </button>
                        
                        <button
                            @click="goHome"
                            class="btn btn-outline-primary w-full"
                        >
                            Go to Homepage
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
const router = useRouter()

/**
 * Redirect to SSO login
 * Chuyển hướng đến SSO login
 */
const loginAgain = async (): Promise<void> => {
    await router.push('/auth/sso/login')
}

/**
 * Go to homepage
 * Đi đến trang chủ
 */
const goHome = async (): Promise<void> => {
    await router.push('/')
}

// Page metadata
definePageMeta({
    layout: 'auth-layout',
    title: 'Logged Out'
})
</script>
