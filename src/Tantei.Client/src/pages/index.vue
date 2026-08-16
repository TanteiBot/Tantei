<script setup lang="ts">
import { ref } from "vue";
import ApiPing from "../components/ApiPing.vue";

const count = ref(0);
</script>

<template>
  <div class="flex flex-col gap-6">
    <UCard>
      <template #header>
        <h2 class="text-highlighted text-lg font-semibold">Nuxt UI is wired up</h2>
      </template>

      <p class="text-muted mb-4">
        This card, badge, and button are all Nuxt UI components, auto-imported by the
        <code>@nuxt/ui/vite</code> plugin &mdash; no explicit imports needed.
      </p>

      <div class="flex items-center gap-3">
        <UButton icon="i-lucide-plus" @click="count++"> Clicked {{ count }} times </UButton>
        <UBadge color="primary" variant="subtle">v4</UBadge>
      </div>
    </UCard>

    <ApiPing />
  </div>
</template>
