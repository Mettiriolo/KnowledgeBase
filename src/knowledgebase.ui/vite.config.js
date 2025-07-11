import { defineConfig } from 'vite'
import plugin from '@vitejs/plugin-vue'
import { resolve } from 'path'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [
    plugin({
      template: {
        compilerOptions: {
          isCustomElement: (tag) => tag.startsWith('custom-')
        }
      }
    }),
    tailwindcss()
  ],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src')
    }
  },
  build: {
    target: 'es2020',
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true
      }
    },
    rollupOptions: {
      output: {
        manualChunks: (id) => {
          if (id.includes('node_modules')) {
            if (id.includes('vue') || id.includes('pinia') || id.includes('vue-router')) {
              return 'vendor-vue'
            }
            if (id.includes('@toast-ui') || id.includes('prismjs')) {
              return 'vendor-editor'
            }
            if (id.includes('lodash') || id.includes('date-fns') || id.includes('axios')) {
              return 'vendor-utils'
            }
            return 'vendor'
          }
          
          if (id.includes('src/components')) {
            return 'components'
          }
          
          if (id.includes('src/stores')) {
            return 'stores'
          }
          
          if (id.includes('src/services')) {
            return 'services'
          }
        },
        chunkFileNames: (chunkInfo) => {
          const facadeModuleId = chunkInfo.facadeModuleId
          if (facadeModuleId) {
            return `assets/[name]-[hash].js`
          }
          return `assets/[name]-[hash].js`
        }
      }
    },
    chunkSizeWarningLimit: 1000,
    sourcemap: false,
    cssCodeSplit: true
  },
  server: {
    host: '0.0.0.0',
    port: 3000,
    strictPort: true,
    cors: true
  },
  preview: {
    port: 8080,
    strictPort: true
  },
  optimizeDeps: {
    include: [
      'vue',
      'vue-router',
      'pinia',
      'axios',
      'lodash-es',
      'date-fns',
      'dompurify',
      'marked'
    ],
    exclude: ['@toast-ui/editor']
  },
  define: {
    __VUE_OPTIONS_API__: false,
    __VUE_PROD_DEVTOOLS__: false
  }
})
