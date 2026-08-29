<script setup lang="ts">
import { computed } from "vue";

import { currentTheme, setTheme } from "@/theme";

const isDark = computed(() => currentTheme.value === "dark");

function toggle(): void {
  setTheme(isDark.value ? "light" : "dark");
}
</script>

<template>
  <UButton
    :icon="isDark ? 'i-lucide-moon' : 'i-lucide-sun'"
    :aria-label="$tStrict('theme.toggle')"
    color="neutral"
    variant="ghost"
    size="sm"
    @click="toggle"
  />
</template>
