// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.AniList;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal static class SearchEmbedBuilder
{
	private const int AuthorNameLimit = 256;
	private const int UrlLimit = 2048;

	public static DiscordEmbedBuilder Build(
		SearchMedia media,
		AniListUserFeatures features,
		TitleLanguage titleLanguage,
		string requesterDisplayName,
		string? avatarUrl)
	{
		ArgumentNullException.ThrowIfNull(media);
		ArgumentException.ThrowIfNullOrWhiteSpace(requesterDisplayName);

		var eb = new DiscordEmbedBuilder()
				 .WithUrl(media.Url)
				 .WithMediaTitle(media, titleLanguage, features)
				 .WithColor(ProviderConstants.AniListBlue)
				 .WithAniListFooter()
				 .WithImageUrl($"https://img.anili.st/media/{media.Id}");
		eb.Thumbnail = null;
		eb.Author = new()
		{
			Name = $"Requested by {requesterDisplayName}".Truncate(AuthorNameLimit),
			IconUrl = IsValidUrl(avatarUrl) ? avatarUrl : null,
		};

		eb.WithTotalSubEntries(media);
		eb.AddFieldIfPresent("Popularity", AniListSearchPresentation.FormatPopularity(media.Popularity), inline: true);
		eb.EnrichWithTextInfo(media, features);

		return eb;
	}

	private static bool IsValidUrl([NotNullWhen(true)] string? url) => !string.IsNullOrWhiteSpace(url) && url.Length <= UrlLimit;
}
