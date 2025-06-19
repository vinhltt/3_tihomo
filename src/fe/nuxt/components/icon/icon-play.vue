<template>
    <svg v-if="fill" width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path
            d="M20.4086 9.35258C22.5305 10.5065 22.5305 13.4935 20.4086 14.6474L8.59662 21.6145C6.53435 22.736 4 21.2763 4 18.9671L4 5.0329C4 2.72368 6.53435 1.26402 8.59661 2.38548L20.4086 9.35258Z"
            fill="currentColor"
        />
    </svg>
    <svg v-else width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
        <path
            d="M20.4086 9.35258C22.5305 10.5065 22.5305 13.4935 20.4086 14.6474L8.59662 21.6145C6.53435 22.736 4 21.2763 4 18.9671L4 5.0329C4 2.72368 6.53435 1.26402 8.59661 2.38548L20.4086 9.35258Z"
            stroke="currentColor"
            stroke-width="1.5"
        />
    </svg>
</template>

<script lang="ts" setup>
/**
 * Icon play component
 * Component icon phát
 */
defineProps({
    fill: {
        type: Boolean,
        default: false
    }
})
</script>
