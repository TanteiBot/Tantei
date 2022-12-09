// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Humanizer;

namespace PaperMalKing.UpdatesProviders.Base.Features;

public class FeatureArgumentConverter<T> : IArgumentConverter<T> where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	public Task<Optional<T>> ConvertAsync(string value, CommandContext ctx)
	{
		try
		{
			return Task.FromResult(new Optional<T>(value.DehumanizeTo<T>()));
		}
		#pragma warning disable CA1031
		catch
			#pragma warning restore CA1031
		{
			return Task.FromResult(Optional.FromNoValue<T>());
		}
	}
}