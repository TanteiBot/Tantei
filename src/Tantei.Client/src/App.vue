<script setup lang="ts">
import type { RouteLocationRaw } from "vue-router";
import { computed } from "vue";
import { RouterView } from "vue-router";
import { useI18n } from "vue-i18n";
import { en, uk } from "@nuxt/ui/locale";
import type { TranslationKey } from "@/i18n/strict";
import LangSwitcher from "@/components/LangSwitcher.vue";

const { locale } = useI18n();

const links: { labelKey: TranslationKey; to: RouteLocationRaw; icon: string }[] = [
  { labelKey: "nav.home", to: { name: "/" }, icon: "i-lucide-house" },
  { labelKey: "nav.about", to: { name: "/about" }, icon: "i-lucide-info" },
];

const uiLocale = computed(() => (locale.value === "uk" ? uk : en));
</script>

<template>
  <UApp :locale="uiLocale">
    <UContainer class="py-8">
      <header class="mb-8 flex items-center justify-between">
        <h1 class="text-highlighted text-xl font-bold">{{ $tStrict("app.title") }}</h1>
        <nav class="flex items-center gap-2">
          <UButton
            v-for="link in links"
            :key="link.labelKey"
            :to="link.to"
            :icon="link.icon"
            color="neutral"
            variant="ghost"
          >
            {{ $tStrict(link.labelKey) }}
          </UButton>
          <LangSwitcher />
        </nav>
      </header>

      <RouterView />
    </UContainer>
  </UApp>
</template>
