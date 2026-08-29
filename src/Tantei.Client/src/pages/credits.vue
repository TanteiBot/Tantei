<script setup lang="ts">
import { computed, ref } from "vue";

import { useGetCredits } from "@/api/gen/hooks/credits";
import type { LicenseResponse } from "@/api/gen/types/LicenseResponse";
import LicenseList from "@/components/LicenseList.vue";

const ownLicenseUrl = "https://www.gnu.org/licenses/agpl-3.0.html";

const { data: credits, isLoading, isError } = useGetCredits({ query: { staleTime: Infinity } });

const search = ref("");

const isFiltering = computed(() => search.value.trim() !== "");

function matching(licenses: LicenseResponse[] | undefined): LicenseResponse[] {
  if (!licenses) {
    return [];
  }

  const term = search.value.trim().toLowerCase();

  return term === ""
    ? licenses
    : licenses.filter((license) => license.name.toLowerCase().includes(term));
}

const client = computed(() => matching(credits.value?.client));
const server = computed(() => matching(credits.value?.server));
</script>

<template>
  <div class="flex flex-col gap-6">
    <div class="flex flex-col gap-2">
      <h1 class="text-highlighted text-2xl font-bold">{{ $tStrict("credits.title") }}</h1>

      <p class="text-muted text-sm">
        {{ $tStrict("credits.description") }}
        <ULink :to="ownLicenseUrl" target="_blank" rel="noopener">
          {{ $tStrict("credits.ownLicense") }}
        </ULink>
      </p>
    </div>

    <USkeleton v-if="isLoading" class="h-64 w-full" />

    <UAlert
      v-else-if="isError"
      color="error"
      variant="subtle"
      :title="$tStrict('common.error.title')"
    />

    <template v-else>
      <UInput
        v-model="search"
        icon="i-lucide-search"
        :placeholder="$tStrict('credits.search')"
        :aria-label="$tStrict('credits.search')"
      />

      <p
        v-if="isFiltering && client.length === 0 && server.length === 0"
        class="text-muted text-sm"
      >
        {{ $tStrict("credits.noMatches") }}
      </p>

      <UCard v-if="client.length > 0 || !isFiltering">
        <template #header>
          <div class="flex items-center gap-2">
            <h2 class="text-highlighted grow text-lg font-semibold">
              {{ $tStrict("credits.browser.title") }}
            </h2>

            <UBadge color="neutral" variant="subtle">{{ client.length }}</UBadge>
          </div>

          <p class="text-muted mt-1 text-sm">{{ $tStrict("credits.browser.description") }}</p>
        </template>

        <LicenseList :licenses="client" />
      </UCard>

      <UCard v-if="server.length > 0 || !isFiltering">
        <template #header>
          <div class="flex items-center gap-2">
            <h2 class="text-highlighted grow text-lg font-semibold">
              {{ $tStrict("credits.server.title") }}
            </h2>

            <UBadge color="neutral" variant="subtle">{{ server.length }}</UBadge>
          </div>

          <p class="text-muted mt-1 text-sm">{{ $tStrict("credits.server.description") }}</p>
        </template>

        <LicenseList :licenses="server" />
      </UCard>
    </template>
  </div>
</template>
