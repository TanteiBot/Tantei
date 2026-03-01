// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace PaperMalKing.UpdatesProviders.Base;

public sealed class EnumChoiceProvider<TChoiceProvider, TEnum> : IChoiceProvider
	where TChoiceProvider : IEnumChoiceProvider<TEnum>
	where TEnum : unmanaged, Enum
{
	[field: MaybeNull]
	[field: AllowNull]
	private static Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Choices =>
		Volatile.Read(ref field) ?? Interlocked.CompareExchange(ref field, TChoiceProvider.CreateChoicesAsync(), comparand: null) ?? field;

	[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "Task is always complete")]
	public Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
	{
		return Choices;
	}
}