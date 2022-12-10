// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaperMalKing.UpdatesProviders.Base.Features;

// ReSharper disable once TypeParameterCanBeVariant
public interface IUserFeaturesService<T> where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	IReadOnlyDictionary<T, (string, string)> Descriptions { get; }
	Task EnableFeaturesAsync(IReadOnlyList<T> features, ulong userId);

	Task DisableFeaturesAsync(IReadOnlyList<T> features, ulong userId);

	ValueTask<string> EnabledFeaturesAsync(ulong userId);
}