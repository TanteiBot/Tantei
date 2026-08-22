<script setup lang="ts">
import { computed } from "vue";
import { useI18n } from "vue-i18n";
import { usePing } from "@/api/gen/hooks/status";
import { ResponseError } from "@/api/gen/.kubb/client";
import { tStrict } from "@/i18n/strict";

const { locale } = useI18n();

const { data, error, isFetching, refetch } = usePing({ query: { enabled: false } });

const errorMessage = computed(() => {
  if (!error.value) {
    return null;
  }

  return error.value instanceof ResponseError
    ? tStrict("common.error.requestFailed", { status: error.value.status })
    : tStrict("common.error.unknown");
});
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
      <UButton icon="i-lucide-radio" :loading="isFetching" @click="refetch()">
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
      v-if="errorMessage"
      class="mt-3"
      color="error"
      variant="subtle"
      icon="i-lucide-triangle-alert"
      :title="$tStrict('common.error.title')"
      :description="errorMessage"
    />
  </UCard>
</template>
