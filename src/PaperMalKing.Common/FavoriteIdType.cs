// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;

namespace PaperMalKing.Common;

public sealed class FavoriteIdType : IComparable<FavoriteIdType>, IEquatable<FavoriteIdType>, IComparable
{
	public bool Equals(FavoriteIdType? other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return this.Id == other.Id && this.Type == other.Type;
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is FavoriteIdType other && this.Equals(other));

	public override int GetHashCode() => HashCode.Combine(this.Id, this.Type);

	public static bool operator ==(FavoriteIdType? left, FavoriteIdType? right) => Equals(left, right);

	public static bool operator !=(FavoriteIdType? left, FavoriteIdType? right) => !Equals(left, right);

	public FavoriteIdType(uint Id, byte Type)
	{
		this.Id = Id;
		this.Type = Type;
	}

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

	public int CompareTo(object? obj)
	{
		if (obj == null)
		{
			return 1;
		}

		if (obj is FavoriteIdType x)
		{
			return this.CompareTo(x);
		}

		throw new ArgumentException("", nameof(obj));
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

	public uint Id { get; init; }
	public byte Type { get; init; }
}
