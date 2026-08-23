import "../src/assets/main.css";
import ui from "@nuxt/ui/vue-plugin";
import type { Preview } from "@storybook/vue3-vite";
import { setup } from "@storybook/vue3-vite";
import { VueQueryPlugin } from "@tanstack/vue-query";

import { createQueryClient } from "../src/api/queryClient";
import { type Locale, i18n, SUPPORTED_LOCALES } from "../src/i18n";
import { strictTranslate } from "../src/i18n/strict";
import router from "../src/router";

const queryClient = createQueryClient();

setup((app) => {
  app.use(router);
  app.use(i18n);
  app.use(strictTranslate);
  app.use(ui);
  app.use(VueQueryPlugin, { queryClient });
});

const preview: Preview = {
  initialGlobals: {
    locale: "en",
  },
  globalTypes: {
    locale: {
      description: "Locale",
      toolbar: {
        icon: "globe",
        items: SUPPORTED_LOCALES.map((code) => ({ value: code, title: code.toUpperCase() })),
        dynamicTitle: true,
      },
    },
  },
  decorators: [
    (story, context) => {
      queryClient.clear();
      i18n.global.locale.value = context.globals.locale as Locale;
      return {
        components: { story },
        template: "<UApp><story /></UApp>",
      };
    },
  ],
};

export default preview;
