<script setup lang="ts">
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { api, type PingResponse } from "@/api/client";
import { tStrict } from "@/i18n/strict";

const { locale } = useI18n();

const data = ref<PingResponse | null>(null);
const error = ref<string | null>(null);
const loading = ref(false);

async function ping(): Promise<void> {
  loading.value = true;
  error.value = null;

  try {
    const { data: pong, response } = await api.GET("/api/ping");

    if (pong === undefined) {
      throw new Error(tStrict("common.error.requestFailed", { status: response.status }));
    }

    data.value = pong;
  } catch (err) {
    error.value = err instanceof Error ? err.message : tStrict("common.error.unknown");
    data.value = null;
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <UCard>
    <template #header>
      <h2 class="text-highlighted text-lg font-semibold">{{ $tStrict("apiPing.title") }}</h2>
    </template>

    <i18n-t keypath="apiPing.description" tag="p" class="text-muted mb-4" scope="global">
      <template #endpoint><code>GET /api/ping</code></template>
    </i18n-t>

    <div class="flex items-center gap-3">
      <UButton icon="i-lucide-radio" :loading="loading" @click="ping">
        {{ $tStrict("apiPing.button") }}
      </UButton>
      <UBadge v-if="data" color="success" variant="subtle" icon="i-lucide-check">
        {{ data.message }}
      </UBadge>
    </div>

    <p v-if="data" class="text-muted mt-3 text-sm">
      {{ $tStrict("apiPing.serverTime", { time: new Date(data.timeUtc).toLocaleString(locale) }) }}
    </p>

    <UAlert
      v-if="error"
      class="mt-3"
      color="error"
      variant="subtle"
      icon="i-lucide-triangle-alert"
      :title="$tStrict('common.error.title')"
      :description="error"
    />
  </UCard>
</template>
