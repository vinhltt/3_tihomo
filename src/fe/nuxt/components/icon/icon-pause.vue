<template>
    <svg v-if="fill" width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path
            d="M8 5C8.55228 5 9 5.44772 9 6V18C9 18.5523 8.55228 19 8 19C7.44772 19 7 18.5523 7 18V6C7 5.44772 7.44772 5 8 5Z"
            fill="currentColor"
        />
        <path
            d="M16 5C16.5523 5 17 5.44772 17 6V18C17 18.5523 16.5523 19 16 19C15.4477 19 15 18.5523 15 18V6C15 5.44772 15.4477 5 16 5Z"
            fill="currentColor"
        />
    </svg>
    <svg v-else width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path
            d="M8 5C8.55228 5 9 5.44772 9 6V18C9 18.5523 8.55228 19 8 19C7.44772 19 7 18.5523 7 18V6C7 5.44772 7.44772 5 8 5Z"
            stroke="currentColor"
            stroke-width="1.5"
            stroke-linecap="round"
        />
        <path
            d="M16 5C16.5523 5 17 5.44772 17 6V18C17 18.5523 16.5523 19 16 19C15.4477 19 15 18.5523 15 18V6C15 5.44772 15.4477 5 16 5Z"
            stroke="currentColor"
            stroke-width="1.5"
            stroke-linecap="round"
        />
    </svg>
</template>

<script lang="ts" setup>
/**
 * Icon pause component
 * Component icon tạm dừng
 */
defineProps({
    fill: {
        type: Boolean,
        default: false
    }
})
</script>
