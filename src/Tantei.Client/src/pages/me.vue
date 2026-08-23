<script setup lang="ts">
import { useAuth, signIn } from "@/api/auth";
import { useGetManageableGuilds } from "@/api/gen/hooks/guilds";
import SignedInUserCard from "@/components/SignedInUserCard.vue";

const { user, isSignedIn, isLoading } = useAuth();

const { data: guilds } = useGetManageableGuilds({ query: { enabled: isSignedIn } });
</script>

<template>
  <div class="flex flex-col gap-6">
    <h1 class="text-highlighted text-2xl font-bold">{{ $tStrict("me.title") }}</h1>

    <USkeleton v-if="isLoading" class="h-32 w-full" />

    <UCard v-else-if="!user">
      <template #header>
        <h2 class="text-highlighted text-lg font-semibold">
          {{ $tStrict("me.signInPrompt.title") }}
        </h2>
      </template>

      <p class="text-muted mb-4">{{ $tStrict("me.signInPrompt.description") }}</p>

      <UButton icon="i-lucide-log-in" color="primary" @click="signIn($route.fullPath)">
        {{ $tStrict("auth.signIn") }}
      </UButton>
    </UCard>

    <template v-else>
      <SignedInUserCard :user="user" />

      <UCard>
        <template #header>
          <h2 class="text-highlighted text-lg font-semibold">
            {{ $tStrict("me.guilds.title") }}
          </h2>
        </template>

        <ul v-if="guilds && guilds.length > 0" class="flex flex-col gap-3">
          <li v-for="guild in guilds" :key="guild.guildId" class="flex items-center gap-3">
            <UAvatar :src="guild.iconUrl ?? undefined" :alt="guild.name" size="sm" />
            <span class="text-highlighted">{{ guild.name }}</span>
          </li>
        </ul>

        <p v-else class="text-muted text-sm">{{ $tStrict("me.guilds.empty") }}</p>
      </UCard>
    </template>
  </div>
</template>
