<template>
  <div>
    <!-- Auto redirect to dashboard -->
  </div>
</template>

<script setup lang="ts">
// Page meta
definePageMeta({
  title: 'TiHoMo - Quản lý tài chính cá nhân'
  // Bỏ middleware auth tạm thời để test UI
  // middleware: 'auth'
})

// Redirect to dashboard
await navigateTo('/dashboard')
</script>
