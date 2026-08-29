export function inviteUrl(guildId: string): string {
  return `/api/guilds/${encodeURIComponent(guildId)}/invite`;
}
