<script setup lang="ts">
import { ref } from "vue";
import { api, type PingResponse } from "@/api/client";

const data = ref<PingResponse | null>(null);
const error = ref<string | null>(null);
const loading = ref(false);

async function ping(): Promise<void> {
  loading.value = true;
  error.value = null;

  try {
    const { data: pong, response } = await api.GET("/api/ping");

    if (pong === undefined) {
      throw new Error(`Request failed with status ${response.status}`);
    }

    data.value = pong;
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Unknown error";
    data.value = null;
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <UCard>
    <template #header>
      <h2 class="text-highlighted text-lg font-semibold">API wiring check</h2>
    </template>

    <p class="text-muted mb-4">
      Calls <code>GET /api/ping</code> on the backend through Vite's dev proxy.
    </p>

    <div class="flex items-center gap-3">
      <UButton icon="i-lucide-radio" :loading="loading" @click="ping"> Ping API </UButton>
      <UBadge v-if="data" color="success" variant="subtle" icon="i-lucide-check">
        {{ data.message }}
      </UBadge>
    </div>

    <p v-if="data" class="text-muted mt-3 text-sm">
      Server time: {{ new Date(data.timeUtc).toLocaleString() }}
    </p>

    <UAlert
      v-if="error"
      class="mt-3"
      color="error"
      variant="subtle"
      icon="i-lucide-triangle-alert"
      title="Request failed"
      :description="error"
    />
  </UCard>
</template>
