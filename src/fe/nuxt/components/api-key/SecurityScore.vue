<template>
  <div :class="containerClasses">
    <!-- Security Score Header (Tiêu đề điểm bảo mật) -->
    <div v-if="showHeader" class="flex items-center justify-between mb-3">
      <div class="flex items-center space-x-2">
        <icon-shield-check 
          :class="[
            'h-5 w-5',
            scoreColor
          ]" 
        />
        <h3 :class="titleClasses">
          {{ $t('apiKey.security.securityScore') }}
        </h3>
      </div>
      <div :class="scoreTextClasses">
        {{ score }}/100
      </div>
    </div>

    <!-- Security Score Progress Bar (Thanh tiến trình điểm bảo mật) -->
    <div class="relative">
      <!-- Background Bar -->
      <div :class="[
        'w-full rounded-full overflow-hidden',
        size === 'sm' ? 'h-2' : size === 'lg' ? 'h-4' : 'h-3',
        'bg-gray-200 dark:bg-gray-700'
      ]">
        <!-- Progress Fill -->
        <div 
          :class="[
            'h-full transition-all duration-500 ease-out',
            progressColorClasses,
            animated && 'animate-pulse'
          ]"
          :style="{ width: `${score}%` }"
        />
      </div>

      <!-- Score Label Overlay (Nhãn điểm trên thanh) -->
      <div 
        v-if="showScoreOnBar && size !== 'sm'" 
        class="absolute inset-0 flex items-center justify-center"
      >
        <span :class="[
          'text-xs font-medium',
          score > 50 ? 'text-white' : 'text-gray-700 dark:text-gray-300'
        ]">
          {{ score }}%
        </span>
      </div>
    </div>

    <!-- Security Level Text (Mức độ bảo mật) -->
    <div v-if="showLevel" class="mt-2 flex items-center justify-between">
      <span :class="levelTextClasses">
        {{ securityLevel }}
      </span>
      <span v-if="!showHeader" :class="scoreTextClasses">
        {{ score }}/100
      </span>
    </div>

    <!-- Security Warnings (Cảnh báo bảo mật) -->
    <div v-if="showWarnings && warnings.length > 0" class="mt-3">
      <div class="flex items-center space-x-2 mb-2">
        <icon-exclamation-triangle class="h-4 w-4 text-warning" />
        <span class="text-sm font-medium text-gray-700 dark:text-gray-300">
          {{ $t('apiKey.security.securityWarnings') }}
        </span>
      </div>
      <ul class="space-y-1">
        <li 
          v-for="(warning, index) in warnings" 
          :key="index"
          class="flex items-start space-x-2 text-sm text-gray-600 dark:text-gray-400"
        >
          <icon-minus class="h-3 w-3 mt-1 text-warning flex-shrink-0" />
          <span>{{ warning }}</span>
        </li>
      </ul>
    </div>

    <!-- Security Improvements Suggestions (Gợi ý cải thiện bảo mật) -->
    <div v-if="showSuggestions && score < 90" class="mt-3">
      <div class="flex items-center space-x-2 mb-2">
        <icon-light-bulb class="h-4 w-4 text-info" />
        <span class="text-sm font-medium text-gray-700 dark:text-gray-300">
          {{ $t('apiKey.security.improvements') }}
        </span>
      </div>
      <ul class="space-y-1">
        <li 
          v-for="(suggestion, index) in securitySuggestions" 
          :key="index"
          class="flex items-start space-x-2 text-sm text-gray-600 dark:text-gray-400"
        >
          <icon-plus class="h-3 w-3 mt-1 text-info flex-shrink-0" />
          <span>{{ suggestion }}</span>
        </li>
      </ul>
    </div>

    <!-- Security Factors Breakdown (Phân tích các yếu tố bảo mật) -->
    <div v-if="showBreakdown && variant === 'detailed'" class="mt-4">
      <div class="flex items-center space-x-2 mb-3">
        <icon-chart-bar class="h-4 w-4 text-gray-500" />
        <span class="text-sm font-medium text-gray-700 dark:text-gray-300">
          {{ $t('apiKey.security.breakdown') }}
        </span>
      </div>
      
      <div class="space-y-2">
        <div 
          v-for="factor in securityFactors" 
          :key="factor.name"
          class="flex items-center justify-between"
        >
          <div class="flex items-center space-x-2">
            <component 
              :is="factor.icon" 
              :class="[
                'h-4 w-4',
                factor.achieved ? 'text-success' : 'text-gray-400'
              ]" 
            />
            <span :class="[
              'text-sm',
              factor.achieved ? 'text-gray-700 dark:text-gray-300' : 'text-gray-500'
            ]">
              {{ factor.name }}
            </span>
          </div>
          <div class="flex items-center space-x-2">
            <span :class="[
              'text-sm font-medium',
              factor.achieved ? 'text-success' : 'text-gray-400'
            ]">
              +{{ factor.points }}
            </span>
            <icon-check-circle 
              v-if="factor.achieved" 
              class="h-4 w-4 text-success" 
            />
            <icon-x-circle 
              v-else 
              class="h-4 w-4 text-gray-400" 
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ApiKeySecuritySettings } from '~/types/api-key'

interface SecurityFactor {
  name: string
  points: number
  achieved: boolean
  icon: string
}

interface Props {
  score: number
  warnings?: string[]
  securitySettings?: ApiKeySecuritySettings
  scopes?: string[]
  hasExpiration?: boolean
  variant?: 'compact' | 'detailed' | 'card'
  size?: 'sm' | 'md' | 'lg'
  showHeader?: boolean
  showLevel?: boolean
  showWarnings?: boolean
  showSuggestions?: boolean
  showBreakdown?: boolean
  showScoreOnBar?: boolean
  animated?: boolean
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  score: 0,
  warnings: () => [],
  variant: 'compact',
  size: 'md',
  showHeader: true,
  showLevel: true,
  showWarnings: true,
  showSuggestions: false,
  showBreakdown: false,
  showScoreOnBar: false,
  animated: false,
  class: ''
})

/**
 * Computed properties for styling (Thuộc tính computed cho styling)
 */
const containerClasses = computed(() => [
  props.class,
  {
    'compact': props.variant === 'compact',
    'panel p-4': props.variant === 'card',
    'detailed': props.variant === 'detailed'
  }
])

const titleClasses = computed(() => [
  'font-semibold',
  {
    'text-sm': props.size === 'sm',
    'text-base': props.size === 'md',
    'text-lg': props.size === 'lg'
  },
  'text-gray-700 dark:text-gray-300'
])

const scoreTextClasses = computed(() => [
  'font-bold',
  {
    'text-sm': props.size === 'sm',
    'text-base': props.size === 'md',
    'text-lg': props.size === 'lg'
  },
  scoreColor.value
])

const levelTextClasses = computed(() => [
  'text-sm font-medium',
  scoreColor.value
])

/**
 * Security score color based on value (Màu điểm bảo mật dựa trên giá trị)
 */
const scoreColor = computed(() => {
  if (props.score >= 80) return 'text-success'
  if (props.score >= 60) return 'text-warning'
  if (props.score >= 40) return 'text-info'
  return 'text-danger'
})

/**
 * Progress bar color classes (Lớp màu thanh tiến trình)
 */
const progressColorClasses = computed(() => {
  if (props.score >= 80) return 'bg-success'
  if (props.score >= 60) return 'bg-warning'
  if (props.score >= 40) return 'bg-info'
  return 'bg-danger'
})

/**
 * Security level text (Văn bản mức độ bảo mật)
 */
const securityLevel = computed(() => {
  if (props.score >= 90) return 'Excellent'
  if (props.score >= 80) return 'Very Good'
  if (props.score >= 60) return 'Good'
  if (props.score >= 40) return 'Fair'
  if (props.score >= 20) return 'Poor'
  return 'Very Poor'
})

/**
 * Security improvement suggestions (Gợi ý cải thiện bảo mật)
 */
const securitySuggestions = computed(() => {
  const suggestions: string[] = []
  
  if (!props.securitySettings?.requireHttps) {
    suggestions.push('Enable HTTPS requirement for better security')
  }
  
  if (!props.securitySettings?.enableIpValidation) {
    suggestions.push('Add IP whitelist to restrict access')
  }
  
  if (!props.securitySettings?.enableRateLimiting) {
    suggestions.push('Enable rate limiting to prevent abuse')
  }
  
  if (!props.hasExpiration) {
    suggestions.push('Set an expiration date for the API key')
  }
  
  if (!props.securitySettings?.enableUsageAnalytics) {
    suggestions.push('Enable usage analytics to monitor key activity')
  }
  
  if (props.scopes?.includes('admin')) {
    suggestions.push('Review if admin scope is really necessary')
  }
  
  return suggestions
})

/**
 * Security factors breakdown (Phân tích các yếu tố bảo mật)
 */
const securityFactors = computed((): SecurityFactor[] => [
  {
    name: 'HTTPS Required',
    points: 20,
    achieved: !!props.securitySettings?.requireHttps,
    icon: 'icon-shield-check'
  },
  {
    name: 'IP Validation',
    points: 25,
    achieved: !!props.securitySettings?.enableIpValidation,
    icon: 'icon-globe-alt'
  },
  {
    name: 'Rate Limiting',
    points: 15,
    achieved: !!props.securitySettings?.enableRateLimiting,
    icon: 'icon-clock'
  },
  {
    name: 'Expiration Set',
    points: 15,
    achieved: !!props.hasExpiration,
    icon: 'icon-calendar'
  },
  {
    name: 'Limited Scopes',
    points: props.scopes?.includes('admin') ? 5 : 10,
    achieved: !!props.scopes && props.scopes.length > 0,
    icon: 'icon-key'
  },
  {
    name: 'Usage Analytics',
    points: 10,
    achieved: !!props.securitySettings?.enableUsageAnalytics,
    icon: 'icon-chart-bar'
  }
])
</script>

<style scoped>
/* Custom animations for score changes (Hoạt ảnh tùy chỉnh cho thay đổi điểm) */
.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: .5;
  }
}

/* Responsive adjustments (Điều chỉnh responsive) */
@media (max-width: 640px) {
  .detailed .space-y-2 > div {
    @apply text-sm;
  }
}
</style> 