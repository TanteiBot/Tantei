<script setup lang="ts">
import type { CurrentUserResponse } from "@/api/gen/types/CurrentUserResponse";

defineProps<{ user: CurrentUserResponse }>();
</script>

<template>
  <UCard>
    <div class="flex items-center gap-4">
      <UAvatar :src="user.avatarUrl ?? undefined" :alt="user.username" size="xl" />

      <div class="flex flex-col gap-2">
        <h2 class="text-highlighted text-lg font-semibold">{{ user.username }}</h2>

        <div class="flex flex-wrap items-center gap-2">
          <UBadge
            :color="user.isRegistered ? 'success' : 'neutral'"
            variant="subtle"
            :icon="user.isRegistered ? 'i-lucide-check' : 'i-lucide-minus'"
          >
            {{ $tStrict(user.isRegistered ? "me.registered.yes" : "me.registered.no") }}
          </UBadge>

          <UBadge v-if="user.isWebAdmin" color="primary" variant="subtle" icon="i-lucide-shield">
            {{ $tStrict("me.webAdmin") }}
          </UBadge>
        </div>
      </div>
    </div>

    <p v-if="!user.isRegistered" class="text-muted mt-4 text-sm">
      {{ $tStrict("me.registered.explanation") }}
    </p>
  </UCard>
</template>
