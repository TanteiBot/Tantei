<script setup lang="ts">
import type { RouteLocationRaw } from "vue-router";
import { RouterView } from "vue-router";

const links: { label: string; to: RouteLocationRaw; icon: string }[] = [
  { label: "Home", to: { name: "/" }, icon: "i-lucide-house" },
  { label: "About", to: { name: "/about" }, icon: "i-lucide-info" },
];
</script>

<template>
  <UApp>
    <UContainer class="py-8">
      <header class="mb-8 flex items-center justify-between">
        <h1 class="text-highlighted text-xl font-bold">Tantei</h1>
        <nav class="flex gap-2">
          <UButton
            v-for="link in links"
            :key="link.label"
            :to="link.to"
            :icon="link.icon"
            color="neutral"
            variant="ghost"
          >
            {{ link.label }}
          </UButton>
        </nav>
      </header>

      <RouterView />
    </UContainer>
  </UApp>
</template>
