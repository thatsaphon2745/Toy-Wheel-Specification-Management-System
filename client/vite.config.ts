import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { viteStaticCopy } from "vite-plugin-static-copy";
import path from "path";

export default defineConfig({
  plugins: [
    react(),
    // ✅ force copy ไฟล์ .gif ทั้งหมดจาก src/assets ไป dist/assets
    viteStaticCopy({
      targets: [
        {
          src: "src/assets/*.gif",   // ที่อยู่ไฟล์ใน src
          dest: "assets"             // ไปไว้ที่ dist/assets/
        }
      ]
    })
  ],
  build: {
    outDir: "dist",         // โฟลเดอร์ build
    assetsDir: "assets",    // เก็บ asset แยก
    rollupOptions: {
      output: {
        assetFileNames: "assets/[name].[hash][extname]",
      },
    },
  },
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "src"),
    },
  },
});
