export function signInUrl(returnUrl?: string): string {
  const query = returnUrl === undefined ? "" : `?returnUrl=${encodeURIComponent(returnUrl)}`;
  return `/api/auth/sign-in${query}`;
}
