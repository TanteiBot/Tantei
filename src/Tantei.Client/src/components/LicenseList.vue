<script setup lang="ts">
import { ref } from "vue";

import type { LicenseResponse } from "@/api/gen/types/LicenseResponse";

const { licenses } = defineProps<{ licenses: LicenseResponse[] }>();

const expanded = ref(new Set<string>());

function keyOf(license: LicenseResponse): string {
  return `${license.name}@${license.version}`;
}

function isExpanded(license: LicenseResponse): boolean {
  return expanded.value.has(keyOf(license));
}

function toggle(license: LicenseResponse): void {
  const key = keyOf(license);
  const next = new Set(expanded.value);

  if (!next.delete(key)) {
    next.add(key);
  }

  expanded.value = next;
}
</script>

<template>
  <p v-if="licenses.length === 0" class="text-muted text-sm">{{ $tStrict("credits.empty") }}</p>

  <ul v-else class="divide-default divide-y">
    <li v-for="license in licenses" :key="keyOf(license)" class="py-1">
      <div class="flex flex-wrap items-center gap-2">
        <UButton
          v-if="license.text"
          color="neutral"
          variant="ghost"
          size="sm"
          class="grow justify-start"
          :icon="isExpanded(license) ? 'i-lucide-chevron-down' : 'i-lucide-chevron-right'"
          :aria-expanded="isExpanded(license)"
          @click="toggle(license)"
        >
          <span class="text-highlighted">{{ license.name }}</span>
          <span class="text-dimmed mx-2 text-xs">{{ license.version }}</span>
        </UButton>

        <div v-else class="flex grow items-center px-2.5 py-1.5 text-sm">
          <span class="text-highlighted">{{ license.name }}</span>
          <span class="text-dimmed mx-2 text-xs">{{ license.version }}</span>
        </div>

        <UButton
          v-if="license.url"
          :to="license.url"
          target="_blank"
          rel="noopener"
          external
          color="neutral"
          variant="subtle"
          size="xs"
          trailing-icon="i-lucide-external-link"
        >
          {{ license.identifier ?? $tStrict("credits.viewLicense") }}
        </UButton>

        <UBadge v-else-if="license.identifier" color="neutral" variant="subtle" size="sm">
          {{ license.identifier }}
        </UBadge>

        <span v-else class="text-dimmed px-2 text-xs">{{ $tStrict("credits.notDeclared") }}</span>
      </div>

      <pre
        v-if="license.text && isExpanded(license)"
        class="text-muted mt-1 mb-2 overflow-x-auto px-2.5 text-xs whitespace-pre-wrap"
        >{{ license.text }}</pre>
    </li>
  </ul>
</template>
