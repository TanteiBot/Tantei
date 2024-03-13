// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;

namespace PaperMalKing.Common;

public sealed record FeaturesInfo<T>(string EnumValue, string Description, string Summary, T Value)
	where T : unmanaged, Enum, IComparable, IConvertible, IFormattable;