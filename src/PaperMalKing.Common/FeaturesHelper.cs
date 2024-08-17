// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Common;

public static class FeaturesHelper<T>
	where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	private static EnumInfo<T>[]? _featuresInfo;

	private static EnumInfo<T>[] FeaturesInfo =>
		Volatile.Read(ref _featuresInfo) ?? Interlocked.CompareExchange(ref _featuresInfo, CreateFeaturesInfo(), comparand: null) ?? _featuresInfo;

	public static IReadOnlyList<EnumInfo<T>> Features => FeaturesInfo;

	public static T Parse(string value)
	{
		return FeaturesInfo.Find(x => x.EnumValue.Equals(value, StringComparison.OrdinalIgnoreCase) ||
									  x.Description.Equals(value, StringComparison.OrdinalIgnoreCase))!.Value;
	}

	[SuppressMessage("Performance", "EA0006:Replace uses of 'Enum.GetName' and 'Enum.ToString' for improved performance", Justification = "Generics don't have access to non-generic extensions")]
	private static EnumInfo<T>[] CreateFeaturesInfo()
	{
		var ti = typeof(T).GetTypeInfo();
		Debug.Assert(Enum.GetUnderlyingType(typeof(T)) == typeof(ulong), $"All features must have {nameof(UInt64)} as underlying type");
		return Enum.GetValues<T>().Where(v =>
			ti.DeclaredMembers.First(xm => xm.Name.Equals(v.ToString(), StringComparison.Ordinal))
			  .GetCustomAttribute<EnumDescriptionAttribute>() is not null).Select(value =>
		{
			Debug.Assert((value.ToUInt64(NumberFormatInfo.InvariantInfo) & (value.ToUInt64(CultureInfo.InvariantCulture) - 1UL)) == 0UL,
				$"All features of {nameof(T)} must be a power of 2");
			var name = value.ToString();
			var attribute = ti.DeclaredMembers.First(xm => xm.Name.Equals(name, StringComparison.Ordinal))
							  .GetCustomAttribute<EnumDescriptionAttribute>()!;

			return new EnumInfo<T>(name, attribute.Description, attribute.Summary, value);
		}).ToArray();
	}
}