// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;
using Humanizer;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal static class SearchEmbedExtensions
{
	private const int AuthorNameLimit = 256;
	private const int UrlLimit = 2048;

	public static DiscordEmbedBuilder WithRequestedByAuthor(this DiscordEmbedBuilder builder, string requesterDisplayName, string? avatarUrl)
	{
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentException.ThrowIfNullOrWhiteSpace(requesterDisplayName);
		builder.Author = new()
		{
			Name = $"Requested by {requesterDisplayName}".Truncate(AuthorNameLimit),
			IconUrl = IsValidAvatarUrl(avatarUrl) ? avatarUrl : null,
		};
		return builder;
	}

	private static bool IsValidAvatarUrl([NotNullWhen(true)] string? url) => !string.IsNullOrWhiteSpace(url) && url.Length <= UrlLimit;
}
