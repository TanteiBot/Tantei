<script setup lang="ts">
import { computed } from "vue";

import { SUPPORTED_LOCALES, currentLocale, type Locale } from "@/i18n";
import { tStrict } from "@/i18n/strict";

const localeIcons: Record<Locale, string> = {
  en: "circle-flags:gb",
  uk: "circle-flags:lang-uk",
};

const items = computed(() =>
  SUPPORTED_LOCALES.map((code) => ({
    label: tStrict(`language.${code}`),
    value: code,
    icon: localeIcons[code],
  })),
);
</script>

<template>
  <USelect
    v-model="currentLocale"
    :items="items"
    :aria-label="$tStrict('language.label')"
    :icon="localeIcons[currentLocale]"
    :ui="{ content: 'min-w-fit' }"
    color="neutral"
    variant="ghost"
    size="sm"
  />
</template>
