export function loginUrl(returnUrl?: string): string {
  const query = returnUrl === undefined ? "" : `?returnUrl=${encodeURIComponent(returnUrl)}`;
  return `/api/auth/login${query}`;
}
