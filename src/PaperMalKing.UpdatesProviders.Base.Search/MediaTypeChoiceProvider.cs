// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using Humanizer;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal sealed class MediaTypeChoiceProvider<T> : IEnumChoiceProvider<T>
	where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	private const string UnknownMediaType = "Unknown";

	public static Task<IEnumerable<DiscordApplicationCommandOptionChoice>> CreateChoicesAsync()
	{
		var choices = Enum.GetValues<T>()
			.Select(static mediaType => (Name: mediaType.ToString(), Label: mediaType.Humanize(LetterCasing.Sentence)))
			.Where(static mediaType => !string.Equals(mediaType.Name, UnknownMediaType, StringComparison.Ordinal))
			.Select(static mediaType => new DiscordApplicationCommandOptionChoice(mediaType.Label, mediaType.Name))
			.ToArray();
		return Task.FromResult(choices.AsEnumerable());
	}

	public static T? Parse(string? value) => Enum.TryParse<T>(value, out var mediaType) ? mediaType : null;
}
