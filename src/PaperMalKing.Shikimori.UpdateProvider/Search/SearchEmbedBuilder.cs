// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using System.Text;
using DSharpPlus.Entities;
using Humanizer;
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

		var titleSb = new StringBuilder();
		titleSb.Append(media.GetNameOrAltName(useRussian));
		if (features.HasFlag(ShikiUserFeatures.MediaFormat))
		{
			titleSb.Append(CultureInfo.InvariantCulture, $" ({(media.Kind ?? "Unknown").Humanize(LetterCasing.Sentence)})");
		}

		if (features.HasFlag(ShikiUserFeatures.MediaStatus) && !string.IsNullOrWhiteSpace(media.Status))
		{
			titleSb.Append(CultureInfo.InvariantCulture, $" [{media.Status.Humanize(LetterCasing.Sentence)}]");
		}

		eb.WithTitle(titleSb.ToString());

		if (media.Score is > 0f)
		{
			eb.AddField("Score", media.Score.Value.ToString("0.##", CultureInfo.InvariantCulture), inline: true);
		}

		eb.FillMediaInfo(media, features, type);

		return eb;
	}
}
