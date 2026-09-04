// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal sealed class AnimeMediaFormatChoiceProvider : IEnumChoiceProvider<MediaFormat>
{
	private static readonly MediaFormat[] Formats =
	[
		MediaFormat.TV,
		MediaFormat.TvShort,
		MediaFormat.Movie,
		MediaFormat.Special,
		MediaFormat.OVA,
		MediaFormat.ONA,
		MediaFormat.Music,
	];

	public static Task<IEnumerable<DiscordApplicationCommandOptionChoice>> CreateChoicesAsync() => MediaFormatChoices.CreateChoicesAsync(Formats);

	public static MediaFormat? Parse(string? value) => MediaFormatChoices.Parse(value);
}
