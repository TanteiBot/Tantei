import "./assets/main.css";
import ui from "@nuxt/ui/vue-plugin";
import { VueQueryPlugin } from "@tanstack/vue-query";
import { createApp } from "vue";

import { createQueryClient } from "./api/queryClient";
import App from "./App.vue";
import { i18n } from "./i18n";
import { strictTranslate } from "./i18n/strict";
import router from "./router";

const app = createApp(App);

app.use(router);
app.use(i18n);
app.use(strictTranslate);
app.use(ui);
app.use(VueQueryPlugin, { queryClient: createQueryClient() });

app.mount("#app");
