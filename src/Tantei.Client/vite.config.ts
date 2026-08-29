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
    ui({
      ui: {
        icon: {
          clientBundle: {
            icons: ["circle-flags:gb", "circle-flags:lang-uk"],
          },
        },
        colors: {
          primary: "fern-frond",
          secondary: "stromboli",
          neutral: "burnt-malt",
        },
      },
    }),
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
    license: {
      fileName: "licenses.json",
    },
  },
  server: {
    proxy: {
      "/api": {
        target: "https://localhost:7131",
        changeOrigin: true,
      },
    },
  },
});
