<template>
  <nav class="breadcrumb" aria-label="Breadcrumb navigation">
    <ol class="flex items-center space-x-2 text-sm">
      <li 
        v-for="(item, index) in breadcrumbItems" 
        :key="`breadcrumb-${index}`"
        class="flex items-center"
      >
        <!-- Breadcrumb item -->
        <component 
          :is="getBreadcrumbComponent(item, index)"
          :to="item.path"
          :class="getBreadcrumbItemClass(item, index)"
          @click="handleBreadcrumbClick(item, index)"
        >
          {{ item.name }}
        </component>
        
        <!-- Separator -->
        <span 
          v-if="index < breadcrumbItems.length - 1" 
          class="ml-2 text-gray-400 select-none"
          aria-hidden="true"
        >
          {{ separator }}
        </span>
      </li>
    </ol>
  </nav>
</template>

<script setup lang="ts">
// Types cho NavigationBreadcrumb component
type BreadcrumbItem = {
  name: string
  path?: string
  params?: Record<string, string>
  query?: Record<string, string>
  disabled?: boolean
  external?: boolean
}

type NavigationBreadcrumbProps = {
  items: BreadcrumbItem[]
  separator?: string
  maxItems?: number
  showEllipsis?: boolean
}

type NavigationBreadcrumbEmits = {
  'item-click': [item: BreadcrumbItem, index: number]
}

// Props với default values
const props = withDefaults(defineProps<NavigationBreadcrumbProps>(), {
  separator: '>',
  maxItems: 5,
  showEllipsis: true
})

// Emits
const emit = defineEmits<NavigationBreadcrumbEmits>()

// Router composable
const router = useRouter()

// Computed properties
const breadcrumbItems = computed(() => {
  let items = props.items

  // Truncate nếu quá maxItems và showEllipsis = true
  if (props.showEllipsis && items.length > props.maxItems) {
    const firstItem = items[0]
    const lastItems = items.slice(-(props.maxItems - 2))
    const ellipsisItem: BreadcrumbItem = { 
      name: '...', 
      disabled: true 
    }
    
    items = [firstItem, ellipsisItem, ...lastItems]
  }

  return items
})

// Methods
function getBreadcrumbComponent(item: BreadcrumbItem, index: number): string {
  // Last item or disabled items render as span
  if (index === breadcrumbItems.value.length - 1 || item.disabled || !item.path) {
    return 'span'
  }
  
  // External links render as 'a'
  if (item.external) {
    return 'a'
  }
  
  // Internal navigation uses NuxtLink
  return 'NuxtLink'
}

function getBreadcrumbItemClass(item: BreadcrumbItem, index: number): string {
  const baseClasses = 'transition-colors duration-200'
  
  // Last item (current page)
  if (index === breadcrumbItems.value.length - 1) {
    return `${baseClasses} text-gray-500 dark:text-gray-400 cursor-default`
  }
  
  // Disabled items
  if (item.disabled) {
    return `${baseClasses} text-gray-400 dark:text-gray-500 cursor-default`
  }
  
  // Clickable items
  if (item.path) {
    return `${baseClasses} text-primary hover:text-primary/80 dark:text-blue-400 dark:hover:text-blue-300 cursor-pointer hover:underline`
  }
  
  // Default
  return `${baseClasses} text-gray-600 dark:text-gray-300`
}

function handleBreadcrumbClick(item: BreadcrumbItem, index: number) {
  // Don't handle clicks on disabled items or current page
  if (item.disabled || index === breadcrumbItems.value.length - 1) {
    return
  }
  
  // Emit click event
  emit('item-click', item, index)
  
  // Handle navigation for internal links
  if (item.path && !item.external) {
    const routeParams = {
      path: item.path,
      ...(item.params && { params: item.params }),
      ...(item.query && { query: item.query })
    }
    
    router.push(routeParams)
  }
}

/**
 * Utility functions để tạo breadcrumb items
 */

// Generate breadcrumb từ current route
function generateFromRoute(route: any): BreadcrumbItem[] {
  // Implementation would depend on routing structure
  return []
}

// Generate breadcrumb cho Account → Transaction navigation
function generateAccountTransactionBreadcrumb(
  accountId: string, 
  accountName: string
): BreadcrumbItem[] {
  return [
    { name: 'Dashboard', path: '/' },
    { name: 'Accounts', path: '/accounts' },
    { 
      name: accountName, 
      path: '/accounts', 
      query: { highlight: accountId }
    },
    { name: 'Transactions' } // Current page - no path
  ]
}

// Generate breadcrumb cho Direct menu navigation
function generateDirectNavigationBreadcrumb(): BreadcrumbItem[] {
  return [
    { name: 'Dashboard', path: '/' },
    { name: 'Transactions' } // Current page - no path
  ]
}

// Expose utility functions
defineExpose({
  generateFromRoute,
  generateAccountTransactionBreadcrumb,
  generateDirectNavigationBreadcrumb
})
</script>

<style scoped>
/* Component-specific styles */
.breadcrumb ol {
  list-style: none;
  margin: 0;
  padding: 0;
}

/* Hover effects */
.breadcrumb a:hover,
.breadcrumb .cursor-pointer:hover {
  text-decoration: underline;
}

/* Focus styles for accessibility */
.breadcrumb a:focus,
.breadcrumb .cursor-pointer:focus {
  outline: 2px solid #3b82f6;
  outline-offset: 2px;
  border-radius: 2px;
}

/* Ellipsis item styling */
.breadcrumb .cursor-default {
  user-select: none;
}
</style>
