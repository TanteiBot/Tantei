import { fileURLToPath, URL } from "node:url";

import { defineConfig } from "vite";
import VueRouter from "vue-router/vite";
import vue from "@vitejs/plugin-vue";
import ui from "@nuxt/ui/vite";
import vueDevTools from "vite-plugin-vue-devtools";

export default defineConfig({
  base: "/",
  plugins: [VueRouter(), vue(), ui(), vueDevTools()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  build: {
    outDir: fileURLToPath(new URL("../PaperMalKing/wwwroot", import.meta.url)),
    emptyOutDir: true,
  },
  server: {
    proxy: {
      "/api": {
        target: "http://localhost:5010",
        changeOrigin: true,
      },
    },
  },
});
