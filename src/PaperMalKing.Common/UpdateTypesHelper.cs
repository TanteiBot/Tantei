// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Common;

public static class UpdateTypesHelper<T>
	where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	private static EnumInfo<T>[]? _updateTypeInfo;

	private static EnumInfo<T>[] UpdateTypesInfo =>
		Volatile.Read(ref _updateTypeInfo) ?? Interlocked.CompareExchange(ref _updateTypeInfo, CreateFeaturesInfo(), comparand: null) ?? _updateTypeInfo;

	public static IReadOnlyList<EnumInfo<T>> UpdateTypes => UpdateTypesInfo;

	public static T Parse(string value)
	{
		return UpdateTypesInfo.Find(x => x.EnumValue.Equals(value, StringComparison.OrdinalIgnoreCase) ||
									x.Description.Equals(value, StringComparison.OrdinalIgnoreCase))!.Value;
	}

	[SuppressMessage("Performance", "EA0006:Replace uses of 'Enum.GetName' and 'Enum.ToString' for improved performance", Justification = "Generics don't have access to non-generic extensions")]
	private static EnumInfo<T>[] CreateFeaturesInfo()
	{
		var ti = typeof(T).GetTypeInfo();
		Debug.Assert(Enum.GetUnderlyingType(typeof(T)) == typeof(byte), $"All update types must have {nameof(Byte)} as underlying type");
		return Enum.GetValues<T>().Where(v => ti.DeclaredMembers.First(xm => string.Equals(xm.Name, v.ToString(), StringComparison.Ordinal))
												.GetCustomAttribute<EnumDescriptionAttribute>() is not null).Select(value =>
		{
			var name = value.ToString();
			var attribute = ti.DeclaredMembers.First(xm => string.Equals(xm.Name, name, StringComparison.Ordinal))
							  .GetCustomAttribute<EnumDescriptionAttribute>()!;

			return new EnumInfo<T>(name, attribute.Description, attribute.Summary, value);
		}).ToArray();
	}
}