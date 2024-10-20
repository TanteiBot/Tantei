// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;

namespace PaperMalKing.Common;

public sealed record FavoriteIdType(uint Id, byte Type) : IComparable<FavoriteIdType>, IComparable
{
	public int CompareTo(FavoriteIdType? other)
	{
		if (ReferenceEquals(this, other))
		{
			return 0;
		}

		if (other is null)
		{
			return 1;
		}

		var typeComparison = this.Type.CompareTo(other.Type);
		if (typeComparison != 0)
		{
			return typeComparison;
		}

		return this.Id.CompareTo(other.Id);
	}

	public int CompareTo(object? obj)
	{
		if (obj is null)
		{
			return 1;
		}

		if (ReferenceEquals(this, obj))
		{
			return 0;
		}

		return obj is FavoriteIdType other ? this.CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(FavoriteIdType)}", nameof(obj));
	}

	public static bool operator <(FavoriteIdType? left, FavoriteIdType? right) => Comparer<FavoriteIdType>.Default.Compare(left, right) < 0;

	public static bool operator >(FavoriteIdType? left, FavoriteIdType? right) => Comparer<FavoriteIdType>.Default.Compare(left, right) > 0;

	public static bool operator <=(FavoriteIdType? left, FavoriteIdType? right) => Comparer<FavoriteIdType>.Default.Compare(left, right) <= 0;

	public static bool operator >=(FavoriteIdType? left, FavoriteIdType? right) => Comparer<FavoriteIdType>.Default.Compare(left, right) >= 0;
}
