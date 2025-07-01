<template>
  <div class="loading-spinner" :style="spinnerStyle"></div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  size: {
    type: String,
    default: 'medium',
    validator: (value) => ['small', 'medium', 'large'].includes(value)
  },
  color: {
    type: String,
    default: 'primary'
  }
})

const sizeClasses = {
  small: 'h-4 w-4 border',
  medium: 'h-8 w-8 border-b-2',
  large: 'h-12 w-12 border-b-2'
}

const spinnerStyle = computed(() => ({
  borderColor: props.color === 'primary' ? '#3b82f6' : props.color,
  width: sizeClasses[props.size].match(/w-(\d+)/)[0],
  height: sizeClasses[props.size].match(/h-(\d+)/)[0],
  borderWidth: props.size === 'small' ? '1px' : '2px'
}))
</script>