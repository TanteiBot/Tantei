// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal static class MediaFormatChoices
{
	public static Task<IEnumerable<DiscordApplicationCommandOptionChoice>> CreateChoicesAsync(ReadOnlySpan<MediaFormat> formats)
	{
		var choices = new DiscordApplicationCommandOptionChoice[formats.Length];
		for (var i = 0; i < formats.Length; i++)
		{
			var format = formats[i];
			choices[i] = new DiscordApplicationCommandOptionChoice(format.Humanize(LetterCasing.Sentence), format.ToString());
		}

		return Task.FromResult(choices.AsEnumerable());
	}

	public static MediaFormat? Parse(string? value) => Enum.TryParse<MediaFormat>(value, out var format) ? format : null;
}
