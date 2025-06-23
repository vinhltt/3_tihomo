<template>
    <!-- Redirect to cover-login for better UI -->
</template>

<script setup lang="ts">
// Automatically redirect to cover-login page
const route = useRoute()
const router = useRouter()

// Preserve returnUrl if exists
const redirectPath = route.query.returnUrl 
    ? `/auth/cover-login?returnUrl=${encodeURIComponent(route.query.returnUrl as string)}`
    : '/auth/cover-login'

// Redirect immediately
await router.replace(redirectPath)

// Page metadata
definePageMeta({
    layout: 'auth-layout',
    title: 'Redirecting to Login',
    auth: false // Disable auth middleware for this page
})
</script>
