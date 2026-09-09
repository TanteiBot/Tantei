// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus.Entities;
using PaperMalKing.Common.Enums;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.Shikimori.UpdateProvider.Search;

internal static class SearchEmbedBuilder
{
	public static DiscordEmbedBuilder Build<TMedia>(
		TMedia media,
		ListEntryType type,
		ShikiUserFeatures features,
		bool useRussian,
		string requesterDisplayName,
		string? avatarUrl)
		where TMedia : BaseMedia, ISearchMedia
	{
		ArgumentNullException.ThrowIfNull(media);

		var eb = new DiscordEmbedBuilder()
				 .WithUrl(media.Url)
				 .WithColor(Constants.ShikiBlue)
				 .WithShikiUpdateProviderFooter();
		eb.WithRequestedByAuthor(requesterDisplayName, avatarUrl);

		eb.WithShikiMediaTitle(media.GetNameOrAltName(useRussian), media.Kind, media.Status, features);

		if (media.Score is > 0f)
		{
			eb.AddField("Community score", media.Score.Value.ToString("0.##", CultureInfo.InvariantCulture), inline: true);
		}

		eb.FillMediaInfo(media, features, type);

		return eb;
	}
}
