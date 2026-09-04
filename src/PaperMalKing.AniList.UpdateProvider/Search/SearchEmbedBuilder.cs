// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal static class SearchEmbedBuilder
{
	public static DiscordEmbedBuilder Build(
		SearchMedia media,
		AniListUserFeatures features,
		TitleLanguage titleLanguage,
		string requesterDisplayName,
		string? avatarUrl)
	{
		ArgumentNullException.ThrowIfNull(media);

		var eb = new DiscordEmbedBuilder()
				 .WithUrl(media.Url)
				 .WithMediaTitle(media, titleLanguage, features)
				 .WithColor(ProviderConstants.AniListBlue)
				 .WithAniListFooter()
				 .WithImageUrl($"https://img.anili.st/media/{media.Id}");
		eb.Thumbnail = null;
		eb.WithRequestedByAuthor(requesterDisplayName, avatarUrl);

		eb.WithTotalSubEntries(media);
		eb.AddFieldIfPresent("Popularity", SearchPresentation.AbbreviateCount(media.Popularity), inline: true);
		eb.EnrichWithTextInfo(media, features);

		return eb;
	}
}
