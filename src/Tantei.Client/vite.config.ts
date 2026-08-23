import { fileURLToPath, URL } from "node:url";

import vueI18n from "@intlify/unplugin-vue-i18n/vite";
import ui from "@nuxt/ui/vite";
import vue from "@vitejs/plugin-vue";
import { defineConfig } from "vite";
import vueDevTools from "vite-plugin-vue-devtools";
import VueRouter from "vue-router/vite";

export default defineConfig({
  base: "/",
  plugins: [
    VueRouter(),
    vue(),
    ui(),
    vueI18n({
      include: [fileURLToPath(new URL("./src/i18n/locales/**", import.meta.url))],
    }),
    vueDevTools(),
  ],
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
