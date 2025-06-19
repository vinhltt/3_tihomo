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

                <!-- Redirect Message -->
                <div class="text-center">
                    <div class="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary mb-4"></div>
                    <h2 class="text-xl font-semibold text-primary mb-2">Redirecting to SSO</h2>
                    <p class="text-white-dark mb-4">You will be redirected to the secure SSO server for authentication.</p>
                    <p class="text-sm text-white-dark">If you are not redirected automatically, click the button below:</p>
                    
                    <button
                        @click="redirectToSso"
                        class="btn btn-primary mt-4"
                    >
                        Login with SSO
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
const router = useRouter()
const route = useRoute()

/**
 * Redirect to SSO login
 * Chuyển hướng đến SSO login
 */
const redirectToSso = async (): Promise<void> => {
    const returnUrl = route.query.returnUrl as string
    const ssoUrl = `/auth/sso/login${returnUrl ? `?returnUrl=${encodeURIComponent(returnUrl)}` : ''}`
    await router.push(ssoUrl)
}

// Auto redirect on mount
onMounted(() => {
    // Short delay to show the redirect message
    setTimeout(() => {
        redirectToSso()
    }, 1500)
})

// Page metadata
definePageMeta({
    layout: 'auth-layout',
    title: 'Redirecting to SSO'
})
</script>
