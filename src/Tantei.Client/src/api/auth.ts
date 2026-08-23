import { useQueryClient } from "@tanstack/vue-query";
import type { ComputedRef, Ref } from "vue";
import { computed } from "vue";
import { useRouter } from "vue-router";

import { getCurrentUserQueryKey, useGetCurrentUser } from "./gen/hooks/auth/useGetCurrentUser";
import { useSignOut as useSignOutMutation } from "./gen/hooks/auth/useSignOut";
import type { CurrentUserResponse } from "./gen/types/CurrentUserResponse";
import { signInUrl } from "./signInUrl";

export interface AuthState {
  user: ComputedRef<CurrentUserResponse | null>;
  isSignedIn: ComputedRef<boolean>;
  isLoading: Ref<boolean>;
}

export function useAuth(): AuthState {
  const { data, isPending } = useGetCurrentUser();

  return {
    user: computed(() => data.value?.user ?? null),
    isSignedIn: computed(() => data.value?.user != null),
    isLoading: isPending,
  };
}

export function signIn(returnUrl?: string): void {
  window.location.assign(signInUrl(returnUrl));
}

export function useSignOut(): { signOut: () => void; isSigningOut: Ref<boolean> } {
  const queryClient = useQueryClient();
  const router = useRouter();

  const { mutate, isPending } = useSignOutMutation({
    mutation: {
      onSuccess: async () => {
        await queryClient.invalidateQueries({ queryKey: getCurrentUserQueryKey() });
        await router.push({ name: "/" });
      },
    },
  });

  return {
    signOut: () => {
      mutate(undefined);
    },
    isSigningOut: isPending,
  };
}
