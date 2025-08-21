import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import fs from 'fs' // Import module 'fs' của Node.js
// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    sourcemap: false, // Tắt việc tạo source map cho bản build production
  },
   /*
  server: {
    https: {
      key: fs.readFileSync('/home/bnt/.config/code-server/ssl/key.pem'),
      cert: fs.readFileSync('/home/bnt/.config/code-server/ssl/cert.pem'),
    },
    port: 5173,
    host: true
  },
  */
})
