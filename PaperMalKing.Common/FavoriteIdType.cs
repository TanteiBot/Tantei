// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;

namespace PaperMalKing.Common;

public sealed record FavoriteIdType(uint Id, byte Type) : IComparable<FavoriteIdType>
{
	public int CompareTo(FavoriteIdType? other)
	{
		if (ReferenceEquals(this, other))
		{
			return 0;
		}

		if (ReferenceEquals(null, other))
		{
			return 1;
		}

		var idComparison = this.Id.CompareTo(other.Id);
		if (idComparison != 0)
		{
			return idComparison;
		}

		return this.Type.CompareTo(other.Type);
	}

	public static bool operator <(FavoriteIdType left, FavoriteIdType right)
	{
		return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
	}

	public static bool operator <=(FavoriteIdType left, FavoriteIdType right)
	{
		return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
	}

	public static bool operator >(FavoriteIdType left, FavoriteIdType right)
	{
		return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
	}

	public static bool operator >=(FavoriteIdType left, FavoriteIdType right)
	{
		return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
	}
}
