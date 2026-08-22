import "./assets/main.css";

import { createApp } from "vue";
import ui from "@nuxt/ui/vue-plugin";
import { VueQueryPlugin } from "@tanstack/vue-query";
import App from "./App.vue";
import router from "./router";
import { i18n } from "./i18n";
import { strictTranslate } from "./i18n/strict";
import { createQueryClient } from "./api/queryClient";

const app = createApp(App);

app.use(router);
app.use(i18n);
app.use(strictTranslate);
app.use(ui);
app.use(VueQueryPlugin, { queryClient: createQueryClient() });

app.mount("#app");
