// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using DSharpPlus.Entities;

namespace PaperMalKing.Common;

public sealed record EnumInfo<T>(string EnumValue, string Description, string Summary, T Value)
	where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	public DiscordApplicationCommandOptionChoice ToDiscordApplicationCommandOptionChoice()
	{
		return new($"{this.Description}: {this.Summary}", this.Description);
	}
}