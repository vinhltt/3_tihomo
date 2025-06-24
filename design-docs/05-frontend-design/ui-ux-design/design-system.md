# TiHoMo Design System

## 1. Design Philosophy

### 1.1 Core Principles
- **Simplicity**: Clean, minimal interface focused on usability
- **Consistency**: Unified visual language across all components
- **Accessibility**: WCAG 2.1 AA compliance for inclusive design
- **Performance**: Optimized for fast loading and smooth interactions
- **Mobile-First**: Responsive design prioritizing mobile experience

### 1.2 Design Values
- **Trust**: Building confidence through clear, reliable interactions
- **Clarity**: Making complex financial data easy to understand
- **Efficiency**: Enabling users to accomplish tasks quickly
- **Transparency**: Honest, straightforward communication

## 2. Color System

### 2.1 Primary Colors
```css
/* Primary Brand Colors */
--primary-50: #eff6ff;
--primary-100: #dbeafe;
--primary-200: #bfdbfe;
--primary-300: #93c5fd;
--primary-400: #60a5fa;
--primary-500: #3b82f6; /* Main brand color */
--primary-600: #2563eb;
--primary-700: #1d4ed8;
--primary-800: #1e40af;
--primary-900: #1e3a8a;
```

### 2.2 Secondary Colors
```css
/* Success Colors */
--success-50: #ecfdf5;
--success-500: #10b981;
--success-900: #064e3b;

/* Warning Colors */
--warning-50: #fffbeb;
--warning-500: #f59e0b;
--warning-900: #78350f;

/* Error Colors */
--error-50: #fef2f2;
--error-500: #ef4444;
--error-900: #7f1d1d;
```

### 2.3 Neutral Colors
```css
/* Gray Scale */
--gray-50: #f9fafb;
--gray-100: #f3f4f6;
--gray-200: #e5e7eb;
--gray-300: #d1d5db;
--gray-400: #9ca3af;
--gray-500: #6b7280;
--gray-600: #4b5563;
--gray-700: #374151;
--gray-800: #1f2937;
--gray-900: #111827;
```

## 3. Typography

### 3.1 Font Stack
```css
/* Primary Font Family */
font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', sans-serif;

/* Monospace Font (for numbers/data) */
font-family: 'JetBrains Mono', 'SF Mono', 'Monaco', 'Inconsolata', monospace;
```

### 3.2 Type Scale
```css
/* Headings */
.text-xs: 0.75rem (12px)
.text-sm: 0.875rem (14px)
.text-base: 1rem (16px)
.text-lg: 1.125rem (18px)
.text-xl: 1.25rem (20px)
.text-2xl: 1.5rem (24px)
.text-3xl: 1.875rem (30px)
.text-4xl: 2.25rem (36px)
```

### 3.3 Font Weights
```css
.font-light: 300
.font-normal: 400
.font-medium: 500
.font-semibold: 600
.font-bold: 700
```

## 4. Spacing System

### 4.1 Spacing Scale (Tailwind CSS)
```css
0: 0px
1: 0.25rem (4px)
2: 0.5rem (8px)
3: 0.75rem (12px)
4: 1rem (16px)
5: 1.25rem (20px)
6: 1.5rem (24px)
8: 2rem (32px)
10: 2.5rem (40px)
12: 3rem (48px)
16: 4rem (64px)
20: 5rem (80px)
24: 6rem (96px)
```

### 4.2 Layout Spacing
- **Component Padding**: 16px (p-4) minimum
- **Section Spacing**: 32px (space-y-8) between major sections
- **Element Spacing**: 8px (space-y-2) between related elements
- **Container Max Width**: 1200px (max-w-6xl)

## 5. Component Library

### 5.1 Button Components

#### Primary Button
```vue
<template>
  <button class="btn-primary">
    <slot />
  </button>
</template>

<style>
.btn-primary {
  @apply px-4 py-2 bg-primary-500 text-white rounded-lg 
         hover:bg-primary-600 focus:ring-2 focus:ring-primary-500 
         focus:ring-opacity-50 transition-colors duration-200;
}
</style>
```

#### Secondary Button
```vue
<template>
  <button class="btn-secondary">
    <slot />
  </button>
</template>

<style>
.btn-secondary {
  @apply px-4 py-2 border border-gray-300 text-gray-700 
         rounded-lg hover:bg-gray-50 focus:ring-2 
         focus:ring-primary-500 focus:ring-opacity-50 
         transition-colors duration-200;
}
</style>
```

### 5.2 Form Components

#### Input Field
```vue
<template>
  <div class="form-group">
    <label :for="id" class="form-label">{{ label }}</label>
    <input 
      :id="id"
      :type="type"
      :placeholder="placeholder"
      class="form-input"
      v-bind="$attrs"
    />
  </div>
</template>

<style>
.form-label {
  @apply block text-sm font-medium text-gray-700 mb-1;
}

.form-input {
  @apply w-full px-3 py-2 border border-gray-300 rounded-lg 
         focus:ring-2 focus:ring-primary-500 focus:border-primary-500 
         transition-colors duration-200;
}
</style>
```

### 5.3 Card Component
```vue
<template>
  <div class="card">
    <div v-if="title" class="card-header">
      <h3 class="card-title">{{ title }}</h3>
    </div>
    <div class="card-body">
      <slot />
    </div>
  </div>
</template>

<style>
.card {
  @apply bg-white rounded-lg shadow-sm border border-gray-200;
}

.card-header {
  @apply px-6 py-4 border-b border-gray-200;
}

.card-title {
  @apply text-lg font-semibold text-gray-900;
}

.card-body {
  @apply px-6 py-4;
}
</style>
```

## 6. Layout System

### 6.1 Grid System
```css
/* Container */
.container {
  @apply max-w-7xl mx-auto px-4 sm:px-6 lg:px-8;
}

/* Grid Layouts */
.grid-cols-1 { grid-template-columns: repeat(1, minmax(0, 1fr)); }
.grid-cols-2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
.grid-cols-3 { grid-template-columns: repeat(3, minmax(0, 1fr)); }
.grid-cols-4 { grid-template-columns: repeat(4, minmax(0, 1fr)); }
```

### 6.2 Responsive Breakpoints
```css
/* Tailwind CSS Breakpoints */
sm: 640px   /* Small devices */
md: 768px   /* Medium devices */
lg: 1024px  /* Large devices */
xl: 1280px  /* Extra large devices */
2xl: 1536px /* 2X large devices */
```

## 7. Dark Mode Support

### 7.1 Dark Mode Colors
```css
/* Dark mode overrides */
.dark {
  --bg-primary: #1f2937;
  --bg-secondary: #374151;
  --text-primary: #f9fafb;
  --text-secondary: #d1d5db;
  --border-color: #4b5563;
}
```

### 7.2 Dark Mode Implementation
```vue
<template>
  <div :class="{ 'dark': isDarkMode }">
    <!-- Content -->
  </div>
</template>

<script setup>
const isDarkMode = useDarkMode()
</script>
```

## 8. Icons & Illustrations

### 8.1 Icon System
- **Icon Library**: Heroicons
- **Icon Sizes**: 16px, 20px, 24px, 32px
- **Icon Weight**: Outline (primary), Solid (accent)

### 8.2 Icon Usage
```vue
<template>
  <icon-plus class="w-5 h-5" />
  <icon-user class="w-6 h-6 text-gray-500" />
</template>
```

## 9. Motion & Animation

### 9.1 Animation Principles
- **Subtle**: Animations should enhance, not distract
- **Fast**: Maximum 300ms duration for UI transitions
- **Purposeful**: Every animation should serve a function

### 9.2 Transition Classes
```css
.transition-fast { transition-duration: 150ms; }
.transition-normal { transition-duration: 200ms; }
.transition-slow { transition-duration: 300ms; }

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
```

## 10. Accessibility Guidelines

### 10.1 Color Contrast
- **Normal Text**: 4.5:1 minimum contrast ratio
- **Large Text**: 3:1 minimum contrast ratio
- **Non-text Elements**: 3:1 minimum contrast ratio

### 10.2 Focus Management
```css
.focus-visible {
  @apply outline-none ring-2 ring-primary-500 ring-opacity-50;
}
```

### 10.3 Screen Reader Support
- Use semantic HTML elements
- Provide alt text for images
- Include ARIA labels for complex interactions
- Ensure keyboard navigation support

## 11. Data Visualization

### 11.1 Chart Colors
```css
/* Chart Color Palette */
--chart-primary: #3b82f6;
--chart-secondary: #10b981;
--chart-tertiary: #f59e0b;
--chart-quaternary: #ef4444;
--chart-quinary: #8b5cf6;
```

### 11.2 Chart Typography
- **Chart Titles**: text-lg font-semibold
- **Axis Labels**: text-sm font-medium
- **Data Labels**: text-xs font-normal
- **Legends**: text-sm font-normal

## 12. Implementation Guidelines

### 12.1 CSS Architecture
- Use Tailwind CSS utility classes
- Create component-specific styles in `.vue` files
- Use CSS custom properties for theme values
- Follow BEM methodology for custom classes

### 12.2 Component Standards
- All components must be responsive
- Support both light and dark themes
- Include proper TypeScript types
- Provide comprehensive prop validation

---

This design system ensures consistency and quality across the TiHoMo application while maintaining flexibility for future enhancements.
