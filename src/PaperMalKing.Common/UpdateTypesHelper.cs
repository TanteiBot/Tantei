// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Common;

public static class UpdateTypesHelper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] T>
	where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
{
	[field: MaybeNull]
	[field: AllowNull]
	private static EnumInfo<T>[] UpdateTypesInfo =>
		Volatile.Read(ref field) ?? Interlocked.CompareExchange(ref field, CreateFeaturesInfo(), comparand: null) ?? field;

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
		return [.. Enum.GetValues<T>()
			.Where(v => Attribute.IsDefined(ti.DeclaredFields.First(xm => xm.Name.Equals(v.ToString(), StringComparison.Ordinal)), typeof(EnumDescriptionAttribute))).Select(value =>
		{
			var name = value.ToString();
			var attribute = ti.DeclaredFields.First(xm => xm.Name.Equals(name, StringComparison.Ordinal))
							  .GetCustomAttribute<EnumDescriptionAttribute>()!;

			return new EnumInfo<T>(name, attribute.Description, attribute.Summary, value);
		}),];
	}
}