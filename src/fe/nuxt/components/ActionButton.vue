<template>
  <button 
    @click="$emit('click')" 
    class="btn btn-outline-primary w-full h-24 flex flex-col items-center justify-center gap-2 hover:bg-primary hover:text-white transition-all duration-200"
    :disabled="disabled"
  >
    <div class="w-8 h-8 flex items-center justify-center">
      <Icon :name="icon" size="20" />
    </div>
    <span class="text-sm font-medium">{{ label }}</span>
  </button>
</template>

<script setup lang="ts">
interface Props {
  icon: string
  label: string
  disabled?: boolean
}

defineProps<Props>()
defineEmits<{
  click: []
}>()
</script>
