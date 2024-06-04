import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path';
// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    },
  },
  server: {
    proxy: {
      '/api': {
        target: 'https://dotnet9.com', // 目标服务器地址"http://localhost:5100",//
        changeOrigin: true, // 是否改变源地址
      },
    },
  },
})
