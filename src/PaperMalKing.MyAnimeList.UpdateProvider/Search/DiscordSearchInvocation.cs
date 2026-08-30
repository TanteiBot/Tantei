// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.SlashCommands;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed record DiscordSearchInvocation : ISearchInvocation
{
	public required bool CanPostEmbed { get; init; }

	public required bool IncludeNsfw { get; init; }

	public required ulong DiscordUserId { get; init; }

	public required string RequesterDisplayName { get; init; }

	public required string? RequesterAvatarUrl { get; init; }

	public required ulong GuildId { get; init; }

	public required ulong ChannelId { get; init; }

	public required IPickerMessageTarget Target { get; init; }

	public static DiscordSearchInvocation Create(InteractionContext context, ISearchResultPostService postService)
	{
		ArgumentNullException.ThrowIfNull(context);
		var channel = context.Channel;
		var isThread = channel.IsThread;
		var permissionSource = isThread ? channel.Parent ?? channel : channel;
		return new()
		{
			CanPostEmbed = SearchChannelGate.CanPostEmbed(permissionSource.PermissionsFor(context.Guild.CurrentMember), isThread),
			IncludeNsfw = isThread ? channel.Parent?.IsNSFW ?? false : channel.IsNSFW,
			DiscordUserId = context.User.Id,
			RequesterDisplayName = context.Member.DisplayName,
			RequesterAvatarUrl = context.Member.GuildAvatarUrl ?? context.User.AvatarUrl,
			GuildId = context.Guild.Id,
			ChannelId = channel.Id,
			Target = new DiscordPickerMessageTarget(context.Interaction, channel, postService),
		};
	}
}
