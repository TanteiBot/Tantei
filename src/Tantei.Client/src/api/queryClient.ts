import { QueryClient } from "@tanstack/vue-query";
import { ResponseError } from "./gen/.kubb/client";

function shouldRetry(failureCount: number, error: Error): boolean {
  if (error instanceof ResponseError && error.status < 500) {
    return false;
  }

  return failureCount < 2;
}

export function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: shouldRetry,
        staleTime: 30_000,
        refetchOnWindowFocus: false,
      },
      mutations: {
        retry: false,
      },
    },
  });
}
