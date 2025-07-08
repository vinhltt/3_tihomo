<template>
  <span 
    :class="badgeClasses"
    :title="scopeDescription"
  >
    <!-- Icon -->
    <component :is="scopeIcon" v-if="showIcon" class="w-3 h-3 mr-1" />
    
    <!-- Scope text -->
    <span class="text-xs font-medium">{{ displayText }}</span>
    
    <!-- Admin indicator -->
    <span v-if="isAdminScope" class="ml-1 text-xs">👑</span>
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ApiKeyScope } from '~/types/api-key'

// Props
interface Props {
  scope: string
  showIcon?: boolean
  size?: 'sm' | 'md' | 'lg'
  variant?: 'solid' | 'outline' | 'subtle'
}

const props = withDefaults(defineProps<Props>(), {
  showIcon: true,
  size: 'sm',
  variant: 'solid'
})

// Scope mappings
const scopeConfig = {
  [ApiKeyScope.Read]: {
    label: 'Read',
    color: 'blue',
    icon: 'IconEye',
    description: 'View data only'
  },
  [ApiKeyScope.Write]: {
    label: 'Write',
    color: 'green',
    icon: 'IconEdit',
    description: 'Create and modify data'
  },
  [ApiKeyScope.Delete]: {
    label: 'Delete',
    color: 'red',
    icon: 'IconTrash',
    description: 'Delete data'
  },
  [ApiKeyScope.Admin]: {
    label: 'Admin',
    color: 'purple',
    icon: 'IconCrown',
    description: 'Full administrative access'
  },
  [ApiKeyScope.TransactionsRead]: {
    label: 'Txn Read',
    color: 'cyan',
    icon: 'IconCreditCard',
    description: 'View transaction data'
  },
  [ApiKeyScope.TransactionsWrite]: {
    label: 'Txn Write',
    color: 'teal',
    icon: 'IconCreditCard',
    description: 'Create and modify transactions'
  },
  [ApiKeyScope.AccountsRead]: {
    label: 'Acc Read',
    color: 'indigo',
    icon: 'IconBank',
    description: 'View account data'
  },
  [ApiKeyScope.AccountsWrite]: {
    label: 'Acc Write',
    color: 'violet',
    icon: 'IconBank',
    description: 'Create and modify accounts'
  }
}

// Computed properties
const scopeInfo = computed(() => {
  return scopeConfig[props.scope as ApiKeyScope] || {
    label: props.scope,
    color: 'gray',
    icon: 'IconKey',
    description: 'Custom scope'
  }
})

const displayText = computed(() => scopeInfo.value.label)

const scopeDescription = computed(() => 
  `${scopeInfo.value.label}: ${scopeInfo.value.description}`
)

const isAdminScope = computed(() => props.scope === ApiKeyScope.Admin)

const scopeIcon = computed(() => {
  // Return icon component name or null
  if (!props.showIcon) return null
  
  // For now, using simple Unicode icons until we integrate proper icon system
  const iconMap: Record<string, string> = {
    'IconEye': '👁️',
    'IconEdit': '✏️', 
    'IconTrash': '🗑️',
    'IconCrown': '👑',
    'IconCreditCard': '💳',
    'IconBank': '🏦',
    'IconKey': '🔑'
  }
  
  return iconMap[scopeInfo.value.icon] || '🔑'
})

const badgeClasses = computed(() => {
  const color = scopeInfo.value.color
  const { size, variant } = props
  
  const baseClasses = [
    'inline-flex',
    'items-center',
    'rounded-full',
    'font-medium',
    'transition-all',
    'duration-200',
    'cursor-default'
  ]
  
  // Size classes
  const sizeClasses = {
    sm: ['px-2', 'py-1', 'text-xs'],
    md: ['px-3', 'py-1.5', 'text-sm'],
    lg: ['px-4', 'py-2', 'text-base']
  }
  
  // Color and variant classes
  const colorVariantClasses = {
    solid: {
      blue: ['bg-blue-500', 'text-white', 'hover:bg-blue-600'],
      green: ['bg-green-500', 'text-white', 'hover:bg-green-600'],
      red: ['bg-red-500', 'text-white', 'hover:bg-red-600'],
      purple: ['bg-purple-500', 'text-white', 'hover:bg-purple-600'],
      cyan: ['bg-cyan-500', 'text-white', 'hover:bg-cyan-600'],
      teal: ['bg-teal-500', 'text-white', 'hover:bg-teal-600'],
      indigo: ['bg-indigo-500', 'text-white', 'hover:bg-indigo-600'],
      violet: ['bg-violet-500', 'text-white', 'hover:bg-violet-600'],
      gray: ['bg-gray-500', 'text-white', 'hover:bg-gray-600']
    },
    outline: {
      blue: ['border', 'border-blue-500', 'text-blue-600', 'hover:bg-blue-50', 'dark:text-blue-400', 'dark:hover:bg-blue-900/20'],
      green: ['border', 'border-green-500', 'text-green-600', 'hover:bg-green-50', 'dark:text-green-400', 'dark:hover:bg-green-900/20'],
      red: ['border', 'border-red-500', 'text-red-600', 'hover:bg-red-50', 'dark:text-red-400', 'dark:hover:bg-red-900/20'],
      purple: ['border', 'border-purple-500', 'text-purple-600', 'hover:bg-purple-50', 'dark:text-purple-400', 'dark:hover:bg-purple-900/20'],
      cyan: ['border', 'border-cyan-500', 'text-cyan-600', 'hover:bg-cyan-50', 'dark:text-cyan-400', 'dark:hover:bg-cyan-900/20'],
      teal: ['border', 'border-teal-500', 'text-teal-600', 'hover:bg-teal-50', 'dark:text-teal-400', 'dark:hover:bg-teal-900/20'],
      indigo: ['border', 'border-indigo-500', 'text-indigo-600', 'hover:bg-indigo-50', 'dark:text-indigo-400', 'dark:hover:bg-indigo-900/20'],
      violet: ['border', 'border-violet-500', 'text-violet-600', 'hover:bg-violet-50', 'dark:text-violet-400', 'dark:hover:bg-violet-900/20'],
      gray: ['border', 'border-gray-500', 'text-gray-600', 'hover:bg-gray-50', 'dark:text-gray-400', 'dark:hover:bg-gray-900/20']
    },
    subtle: {
      blue: ['bg-blue-100', 'text-blue-800', 'hover:bg-blue-200', 'dark:bg-blue-900/30', 'dark:text-blue-300'],
      green: ['bg-green-100', 'text-green-800', 'hover:bg-green-200', 'dark:bg-green-900/30', 'dark:text-green-300'],
      red: ['bg-red-100', 'text-red-800', 'hover:bg-red-200', 'dark:bg-red-900/30', 'dark:text-red-300'],
      purple: ['bg-purple-100', 'text-purple-800', 'hover:bg-purple-200', 'dark:bg-purple-900/30', 'dark:text-purple-300'],
      cyan: ['bg-cyan-100', 'text-cyan-800', 'hover:bg-cyan-200', 'dark:bg-cyan-900/30', 'dark:text-cyan-300'],
      teal: ['bg-teal-100', 'text-teal-800', 'hover:bg-teal-200', 'dark:bg-teal-900/30', 'dark:text-teal-300'],
      indigo: ['bg-indigo-100', 'text-indigo-800', 'hover:bg-indigo-200', 'dark:bg-indigo-900/30', 'dark:text-indigo-300'],
      violet: ['bg-violet-100', 'text-violet-800', 'hover:bg-violet-200', 'dark:bg-violet-900/30', 'dark:text-violet-300'],
      gray: ['bg-gray-100', 'text-gray-800', 'hover:bg-gray-200', 'dark:bg-gray-900/30', 'dark:text-gray-300']
    }
  }
  
  return [
    ...baseClasses,
    ...sizeClasses[size],
    ...colorVariantClasses[variant][color as keyof typeof colorVariantClasses[typeof variant]]
  ]
})
</script>

<style scoped>
/* Additional custom styles if needed for VRISTO theme integration */
.scope-badge {
  /* Custom animations or effects */
  animation: fadeIn 0.2s ease-in-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: scale(0.95);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}
</style> 