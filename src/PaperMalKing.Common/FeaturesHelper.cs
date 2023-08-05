// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Common;

public static class FeaturesHelper<T> where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	private static IReadOnlyDictionary<T, (string EnumValue, string Description, string Summary)>? _featuresInfo;

	public static IReadOnlyDictionary<T, (string EnumValue, string Description, string Summary)> FeaturesInfo =>
		Volatile.Read(ref _featuresInfo) ?? Interlocked.CompareExchange(ref _featuresInfo, CreateFeaturesInfo(), comparand: null) ?? _featuresInfo;

	public static T Parse(string value)
	{
		return FeaturesInfo.First(x => x.Value.EnumValue.Equals(value, StringComparison.OrdinalIgnoreCase) ||
									   x.Value.Description.Equals(value, StringComparison.OrdinalIgnoreCase)).Key;
	}

	private static ReadOnlyDictionary<T, (string EnumValue, string Description, string Summary)> CreateFeaturesInfo()
	{
		var ti = typeof(T).GetTypeInfo();
		Debug.Assert(Enum.GetUnderlyingType(typeof(T)) == typeof(ulong), $"All features must have {nameof(UInt64)} as underlying type");

		return new (Enum.GetValues<T>()
			.Where(v => ti.DeclaredMembers.First(xm => string.Equals(xm.Name, v.ToString(), StringComparison.Ordinal)).GetCustomAttribute<FeatureDescriptionAttribute>() is not null).Select(value =>
			{
				Debug.Assert( (value.ToUInt64(NumberFormatInfo.InvariantInfo) & (value.ToUInt64(CultureInfo.InvariantCulture) - 1UL)) == 0UL, $"All features of {nameof(T)} must be a power of 2");
				var name = value.ToString();
				var attribute = ti.DeclaredMembers.First(xm => string.Equals(xm.Name, name, StringComparison.Ordinal)).GetCustomAttribute<FeatureDescriptionAttribute>()!;

				return (value, name, attribute.Description, attribute.Summary);
			}).ToDictionary(x => x.value, x => (EnumValue: x.name, x.Description, x.Summary)));
	}
}