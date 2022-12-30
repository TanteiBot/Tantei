// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Threading.Tasks;

namespace PaperMalKing.UpdatesProviders.Base.Features;

public interface IUserFeaturesService<in T> where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	Task EnableFeaturesAsync(T feature, ulong userId);

	Task DisableFeaturesAsync(T feature, ulong userId);

	ValueTask<string> EnabledFeaturesAsync(ulong userId);
}