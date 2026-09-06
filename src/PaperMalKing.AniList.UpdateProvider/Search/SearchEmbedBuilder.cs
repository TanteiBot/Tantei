// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal static class SearchEmbedBuilder
{
	public static Task<DiscordEmbedBuilder> BuildAsync(
		IAniListClient client,
		SearchMedia media,
		AniListUserFeatures features,
		TitleLanguage titleLanguage,
		string requesterDisplayName,
		string? avatarUrl,
		ILogger logger,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(client);
		ArgumentNullException.ThrowIfNull(media);
		ArgumentNullException.ThrowIfNull(logger);

		return BuildCoreAsync(client, media, features, titleLanguage, requesterDisplayName, avatarUrl, logger, cancellationToken);
	}

	[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "A failed detail fetch degrades to the snapshot embed and must never reach the search spine")]
	private static async Task<DiscordEmbedBuilder> BuildCoreAsync(
		IAniListClient client,
		SearchMedia media,
		AniListUserFeatures features,
		TitleLanguage titleLanguage,
		string requesterDisplayName,
		string? avatarUrl,
		ILogger logger,
		CancellationToken cancellationToken)
	{
		var eb = Build(media, features, titleLanguage, requesterDisplayName, avatarUrl);

		if (!features.HasFlag(AniListUserFeatures.Seyu) || media.Type != ListType.Anime)
		{
			return eb;
		}

		try
		{
			var detail = await client.GetMediaWithSeyuAsync(media.Id, media.Type, cancellationToken).ConfigureAwait(false);
			if (detail is not null)
			{
				eb.EnrichWithSeyu(detail, titleLanguage);
			}
		}
		catch (Exception exception)
		{
			logger.SeyuDetailFetchFailed(media.Id, exception);
		}

		return eb;
	}

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
