import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      // '@' is a shortcut to 'src/' — cleaner imports
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 5173,
    // Forwards /api calls to the .NET backend — avoids CORS in dev
    proxy: {
      '/api': {
        target: 'http://localhost:5170',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
