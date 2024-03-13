// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using PaperMalKing.Common;

namespace PaperMalKing.UpdatesProviders.Base.Features;

public sealed class FeaturesChoiceProvider<T> : IChoiceProvider
	where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	private static Task<IEnumerable<DiscordApplicationCommandOptionChoice>>? _choices;

	private static Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Choices =>
		Volatile.Read(ref _choices) ?? Interlocked.CompareExchange(ref _choices, CreateChoicesAsync(), comparand: null) ?? _choices;

	[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "Task is always complete")]
	public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
	{
		return Choices;
	}

	private static Task<IEnumerable<DiscordApplicationCommandOptionChoice>> CreateChoicesAsync()
	{
		var choices = FeaturesHelper<T>.Features.Select(x => new DiscordApplicationCommandOptionChoice($"{x.Description}: {x.Summary}", x.Description))
									   .ToArray();
		return Task.FromResult(choices.AsEnumerable());
	}
}