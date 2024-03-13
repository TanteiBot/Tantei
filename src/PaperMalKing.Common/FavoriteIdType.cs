// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using CommunityToolkit.Diagnostics;

namespace PaperMalKing.Common;

public sealed class FavoriteIdType : IComparable<FavoriteIdType>, IEquatable<FavoriteIdType>, IComparable
{
	public bool Equals(FavoriteIdType? other)
	{
		return other is not null && (ReferenceEquals(this, other) || (this.Id == other.Id && this.Type == other.Type));
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is FavoriteIdType other && this.Equals(other));

	public override int GetHashCode() => HashCode.Combine(this.Id, this.Type);

	public static bool operator ==(FavoriteIdType? left, FavoriteIdType? right) => Equals(left, right);

	public static bool operator !=(FavoriteIdType? left, FavoriteIdType? right) => !Equals(left, right);

	public FavoriteIdType(uint id, byte type)
	{
		this.Id = id;
		this.Type = type;
	}

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

		var idComparison = this.Id.CompareTo(other.Id);
		return idComparison != 0 ? idComparison : this.Type.CompareTo(other.Type);
	}

	public int CompareTo(object? obj)
	{
		if (obj is FavoriteIdType x)
		{
			return this.CompareTo(x);
		}

		ThrowHelper.ThrowArgumentException(nameof(obj), "");
		return default;
	}

	public static bool operator <(FavoriteIdType left, FavoriteIdType right)
	{
		return left is null ? right is not null : left.CompareTo(right) < 0;
	}

	public static bool operator <=(FavoriteIdType left, FavoriteIdType right)
	{
		return left is null || left.CompareTo(right) <= 0;
	}

	public static bool operator >(FavoriteIdType left, FavoriteIdType right)
	{
		return left?.CompareTo(right) > 0;
	}

	public static bool operator >=(FavoriteIdType left, FavoriteIdType right)
	{
		return left is null ? right is null : left.CompareTo(right) >= 0;
	}

	public uint Id { get; init; }

	public byte Type { get; init; }
}
