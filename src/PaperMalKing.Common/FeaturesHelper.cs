// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Common;

public static class FeaturesHelper<T> where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	private static IReadOnlyDictionary<T, (string EnumValue, string Description, string Summary)>? _featuresInfo;

	public static IReadOnlyDictionary<T, (string EnumValue, string Description, string Summary)> FeaturesInfo =>
		Volatile.Read(ref _featuresInfo) ?? Interlocked.CompareExchange(ref _featuresInfo, CreateFeaturesInfo(), null) ?? _featuresInfo;

	public static T Parse(string value)
	{
		return FeaturesInfo.First(x => x.Value.EnumValue.Equals(value, StringComparison.OrdinalIgnoreCase) ||
									   x.Value.Description.Equals(value, StringComparison.OrdinalIgnoreCase)).Key;
	}

	private static ReadOnlyDictionary<T, (string EnumValue, string Description, string Summary)> CreateFeaturesInfo()
	{
		var ti = typeof(T).GetTypeInfo();
		Debug.Assert(Enum.GetUnderlyingType(typeof(T)) == typeof(ulong), $"All features must have {nameof(UInt64)} as underlying type");

		return new ReadOnlyDictionary<T, (string EnumValue, string Description, string Summary)>(Enum.GetValues<T>()
			.Where(v => v.ToUInt64(provider: null) != 0UL).Select(value =>
			{
				var name = value.ToString();
				var attribute = ti.DeclaredMembers.First(xm => xm.Name == name).GetCustomAttribute<FeatureDescriptionAttribute>();
				if (attribute == null)
				{
					throw new InvalidOperationException($"All features must be marked with {nameof(FeatureDescriptionAttribute)}");
				}

				return (value, name, attribute.Description, attribute.Summary);
			}).ToDictionary(x => x.value, x => (EnumValue: x.name, Description: x.Description, Summary: x.Summary)));
	}
}