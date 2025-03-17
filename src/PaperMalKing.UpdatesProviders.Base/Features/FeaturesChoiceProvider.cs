// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;
using PaperMalKing.Common;

namespace PaperMalKing.UpdatesProviders.Base.Features;

public sealed class FeaturesChoiceProvider<T> : IEnumChoiceProvider<T>
	where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	private FeaturesChoiceProvider()
	{
	}

	[SuppressMessage("Roslynator", "RCS1158:Static member in generic type should use a type parameter", Justification = "Not really")]
	public static Task<IEnumerable<DiscordApplicationCommandOptionChoice>> CreateChoicesAsync()
	{
		var choices = FeaturesHelper<T>.Features.Select(static x => x.ToDiscordApplicationCommandOptionChoice()).ToArray();
		return Task.FromResult(choices.AsEnumerable());
	}
}