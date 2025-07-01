import { defineConfig } from 'vite'
import plugin from '@vitejs/plugin-vue'
import { resolve } from 'path'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [plugin(), tailwindcss()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src')
    }
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          'vendor': ['vue', 'vue-router', 'pinia', 'axios'],
          'editor': ['@toast-ui/editor', 'prismjs'],
          'utils': ['lodash-es', 'date-fns']
        }
      }
    },
    chunkSizeWarningLimit: 1000
  }
})
