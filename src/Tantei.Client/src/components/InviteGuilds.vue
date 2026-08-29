<script setup lang="ts">
import { computed } from "vue";

import type { InvitableGuildResponse } from "@/api/gen/types/InvitableGuildResponse";
import { inviteEligibility } from "@/api/gen/types/InviteEligibility";
import type { InviteEligibilityKey } from "@/api/gen/types/InviteEligibility";
import { inviteMode } from "@/api/gen/types/InviteMode";
import type { InviteModeKey } from "@/api/gen/types/InviteMode";
import { inviteUrl } from "@/api/inviteUrl";
import type { TranslationKey } from "@/i18n/strict";

const { mode, eligibility, guilds } = defineProps<{
  mode: InviteModeKey;
  eligibility: InviteEligibilityKey;
  guilds: InvitableGuildResponse[];
}>();

const messageKey = computed<TranslationKey | null>(() => {
  switch (eligibility) {
    case inviteEligibility.Unknown:
      return "invite.unavailable";
    case inviteEligibility.NotAllowed:
      return mode === inviteMode.SemiPrivate
        ? "invite.blocked.semiPrivate"
        : "invite.blocked.private";
    default:
      return null;
  }
});
</script>

<template>
  <UCard>
    <template #header>
      <h2 class="text-highlighted text-lg font-semibold">{{ $tStrict("invite.title") }}</h2>
    </template>

    <p v-if="messageKey" class="text-muted text-sm">{{ $tStrict(messageKey) }}</p>

    <ul v-else-if="guilds.length > 0" class="flex flex-col gap-3">
      <li v-for="guild in guilds" :key="guild.guildId" class="flex items-center gap-3">
        <UAvatar :src="guild.iconUrl ?? undefined" :alt="guild.name" size="sm" />
        <span class="text-highlighted grow">{{ guild.name }}</span>

        <UButton
          :to="inviteUrl(guild.guildId)"
          external
          target="_blank"
          rel="noopener"
          icon="i-lucide-plus"
          color="primary"
          size="sm"
        >
          {{ $tStrict("invite.button") }}
        </UButton>
      </li>
    </ul>

    <p v-else class="text-muted text-sm">{{ $tStrict("invite.empty") }}</p>
  </UCard>
</template>
