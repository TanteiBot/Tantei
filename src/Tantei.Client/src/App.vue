<script setup lang="ts">
import { en, uk } from "@nuxt/ui/locale";
import { computed, defineAsyncComponent } from "vue";
import { useI18n } from "vue-i18n";
import { RouterView } from "vue-router";

import AppFooter from "@/components/AppFooter.vue";
import AppHeader from "@/components/AppHeader.vue";

const { locale } = useI18n();

const uiLocale = computed(() => (locale.value === "uk" ? uk : en));

const devtools = import.meta.env.DEV
  ? defineAsyncComponent(() =>
      import("@tanstack/vue-query-devtools").then((m) => m.VueQueryDevtools),
    )
  : null;
</script>

<template>
  <UApp :locale="uiLocale">
    <div class="flex min-h-screen flex-col">
      <AppHeader />

      <UContainer class="py-8">
        <RouterView />
      </UContainer>

      <AppFooter />
    </div>

    <component :is="devtools" v-if="devtools" />
  </UApp>
</template>
