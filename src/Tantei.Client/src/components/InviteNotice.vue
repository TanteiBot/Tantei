<script setup lang="ts">
import { computed } from "vue";

import { inviteMode } from "@/api/gen/types/InviteMode";
import type { InviteModeKey } from "@/api/gen/types/InviteMode";
import type { TranslationKey } from "@/i18n/strict";

const { mode } = defineProps<{ mode: InviteModeKey }>();

const noticeKey = computed<TranslationKey | null>(() => {
  switch (mode) {
    case inviteMode.Private:
      return "invite.notice.private";
    case inviteMode.SemiPrivate:
      return "invite.notice.semiPrivate";
    default:
      return null;
  }
});
</script>

<template>
  <UAlert
    v-if="noticeKey"
    color="neutral"
    variant="subtle"
    icon="i-lucide-info"
    :description="$tStrict(noticeKey)"
  />
</template>
