<script setup lang="ts">
import { computed } from "vue";

import { SUPPORTED_LOCALES, currentLocale } from "@/i18n";
import { tStrict } from "@/i18n/strict";

const items = computed(() =>
  SUPPORTED_LOCALES.map((code) => ({
    label: tStrict(`language.${code}`),
    value: code,
  })),
);
</script>

<template>
  <USelect
    v-model="currentLocale"
    :items="items"
    :aria-label="$tStrict('language.label')"
    icon="i-lucide-languages"
    color="neutral"
    variant="ghost"
  />
</template>
