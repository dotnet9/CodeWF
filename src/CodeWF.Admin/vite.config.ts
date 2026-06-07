import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  base: "/admin/",
  plugins: [react()],
  build: {
    chunkSizeWarningLimit: 1100,
    rollupOptions: {
      output: {
        manualChunks: {
          antd: ["antd", "@ant-design/icons"]
        }
      }
    }
  },
  server: {
    allowedHosts: ["dotnet9.com", "codewf.com"],
    port: 5001,
    strictPort: false
  }
});
