<script setup lang="ts">
import { ref } from "vue";
import ApiPing from "@/components/ApiPing.vue";

const count = ref(0);
</script>

<template>
  <div class="flex flex-col gap-6">
    <UCard>
      <template #header>
        <h2 class="text-highlighted text-lg font-semibold">{{ $tStrict("home.card.title") }}</h2>
      </template>

      <i18n-t keypath="home.card.description" tag="p" class="text-muted mb-4" scope="global">
        <template #plugin><code>@nuxt/ui/vite</code></template>
      </i18n-t>

      <div class="flex items-center gap-3">
        <UButton icon="i-lucide-plus" @click="count++">
          {{ $tStrict("home.card.clicked", { count }) }}
        </UButton>
        <UBadge color="primary" variant="subtle">{{ $tStrict("home.card.version") }}</UBadge>
      </div>
    </UCard>

    <ApiPing />
  </div>
</template>
