// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base;

public interface IEnumChoiceProvider<TEnum>
	where TEnum : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	static abstract Task<IEnumerable<DiscordApplicationCommandOptionChoice>> CreateChoicesAsync();
}