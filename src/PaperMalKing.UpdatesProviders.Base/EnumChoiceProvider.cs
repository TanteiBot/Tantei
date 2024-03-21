// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace PaperMalKing.UpdatesProviders.Base;

public sealed class EnumChoiceProvider<TChoiceProvider, TEnum> : IChoiceProvider
	where TChoiceProvider : IEnumChoiceProvider<TEnum>
	where TEnum : unmanaged, Enum
{
	private static Task<IEnumerable<DiscordApplicationCommandOptionChoice>>? _choices;

	private static Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Choices =>
		Volatile.Read(ref _choices) ?? Interlocked.CompareExchange(ref _choices, TChoiceProvider.CreateChoicesAsync(), comparand: null) ?? _choices;

	[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "Task is always complete")]
	public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
	{
		return Choices;
	}
}