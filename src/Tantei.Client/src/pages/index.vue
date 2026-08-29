<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";

import { useAuth, signIn } from "@/api/auth";
import { useGetSiteConfig } from "@/api/gen/hooks/config";
import InviteNotice from "@/components/InviteNotice.vue";
import type { TranslationKey } from "@/i18n/strict";

const route = useRoute();
const { isSignedIn, isLoading } = useAuth();
const { data: siteConfig } = useGetSiteConfig();

const signInErrorKey = computed<TranslationKey | null>(() => {
  switch (route.query["error"]) {
    case "cancelled":
      return "home.error.cancelled";
    case "expired":
      return "home.error.expired";
    case "failed":
      return "home.error.failed";
    default:
      return null;
  }
});

const returnUrl = computed(() => {
  const value = route.query["returnUrl"];
  return typeof value === "string" ? value : undefined;
});
</script>

<template>
  <div class="flex flex-col gap-6">
    <UAlert
      v-if="signInErrorKey"
      color="error"
      variant="subtle"
      icon="i-lucide-triangle-alert"
      :title="$tStrict('common.error.title')"
      :description="$tStrict(signInErrorKey)"
    />

    <InviteNotice v-if="!isLoading && !isSignedIn && siteConfig" :mode="siteConfig.inviteMode" />

    <div class="flex flex-col items-start gap-4 py-8">
      <h1 class="text-highlighted text-3xl font-bold">{{ $tStrict("home.hero.title") }}</h1>
      <p class="text-muted max-w-2xl text-lg">{{ $tStrict("home.hero.description") }}</p>

      <UButton
        v-if="isSignedIn"
        :to="{ name: '/me' }"
        icon="i-lucide-user"
        color="primary"
        size="lg"
      >
        {{ $tStrict("home.hero.ctaDetails") }}
      </UButton>

      <UButton v-else icon="i-lucide-log-in" color="primary" size="lg" @click="signIn(returnUrl)">
        {{ $tStrict("home.hero.ctaSignIn") }}
      </UButton>
    </div>
  </div>
</template>
