<script setup lang="ts">
import { en, uk } from "@nuxt/ui/locale";
import { computed, defineAsyncComponent } from "vue";
import { useI18n } from "vue-i18n";
import type { RouteLocationRaw } from "vue-router";
import { RouterView, useRoute } from "vue-router";

import { useAuth, useSignOut, signIn } from "@/api/auth";
import AuthMenu from "@/components/AuthMenu.vue";
import LangSwitcher from "@/components/LangSwitcher.vue";
import type { TranslationKey } from "@/i18n/strict";

const { locale } = useI18n();
const route = useRoute();
const { user, isLoading } = useAuth();
const { signOut } = useSignOut();

const links: { labelKey: TranslationKey; to: RouteLocationRaw; icon: string }[] = [
  { labelKey: "nav.home", to: { name: "/" }, icon: "i-lucide-house" },
  { labelKey: "nav.about", to: { name: "/about" }, icon: "i-lucide-info" },
];

const uiLocale = computed(() => (locale.value === "uk" ? uk : en));

const devtools = import.meta.env.DEV
  ? defineAsyncComponent(() =>
      import("@tanstack/vue-query-devtools").then((m) => m.VueQueryDevtools),
    )
  : null;
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
          <AuthMenu
            :user="user"
            :is-loading="isLoading"
            @sign-in="signIn(route.fullPath)"
            @sign-out="signOut"
          />
        </nav>
      </header>

      <RouterView />
    </UContainer>

    <component :is="devtools" v-if="devtools" />
  </UApp>
</template>
