<script setup lang="ts">
import type { DropdownMenuItem } from "@nuxt/ui";
import { computed } from "vue";

import type { CurrentUserResponse } from "@/api/gen/types/CurrentUserResponse";
import { tStrict } from "@/i18n/strict";

const { user, isLoading = false } = defineProps<{
  user: CurrentUserResponse | null;
  isLoading?: boolean;
}>();

const emit = defineEmits<{
  signIn: [];
  signOut: [];
}>();

const items = computed<DropdownMenuItem[][]>(() => [
  [
    {
      label: tStrict("auth.myDetails"),
      icon: "i-lucide-user",
      to: { name: "/me" },
    },
    {
      label: tStrict("auth.signOut"),
      icon: "i-lucide-log-out",
      onSelect: () => emit("signOut"),
    },
  ],
]);
</script>

<template>
  <USkeleton v-if="isLoading" class="h-8 w-24" />

  <UDropdownMenu v-else-if="user" :items="items">
    <UButton
      color="neutral"
      variant="ghost"
      class="cursor-pointer"
      :aria-label="$tStrict('auth.menu')"
    >
      <UAvatar :src="user.avatarUrl ?? undefined" :alt="user.username" size="2xs" />
      <span class="hidden sm:inline">{{ user.username }}</span>
    </UButton>
  </UDropdownMenu>

  <UButton v-else icon="i-lucide-log-in" color="primary" @click="emit('signIn')">
    {{ $tStrict("auth.signIn") }}
  </UButton>
</template>
