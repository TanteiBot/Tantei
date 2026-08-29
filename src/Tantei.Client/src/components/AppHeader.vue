<script setup lang="ts">
import { useRoute } from "vue-router";

import { useAuth, useSignOut, signIn } from "@/api/auth";
import AuthMenu from "@/components/AuthMenu.vue";

const route = useRoute();
const { user, isLoading } = useAuth();
const { signOut } = useSignOut();
</script>

<template>
  <UHeader :toggle="false" class="bg-muted/60 dark:bg-elevated border-b border-default">
    <template #title>{{ $tStrict("app.title") }}</template>

    <template #right>
      <AuthMenu
        :user="user"
        :is-loading="isLoading"
        @sign-in="signIn(route.fullPath)"
        @sign-out="signOut"
      />
    </template>
  </UHeader>
</template>
