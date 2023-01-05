// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList;

public sealed class MalFavoritePerson : BaseMalFavorite, IEquatable<MalFavoritePerson>
{
	public bool Equals(MalFavoritePerson? other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return this.UserId == other.UserId && this.Id == other.Id;
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is MalFavoritePerson other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(this.UserId, this.Id);

	public static bool operator ==(MalFavoritePerson? left, MalFavoritePerson? right) => Equals(left, right);

	public static bool operator !=(MalFavoritePerson? left, MalFavoritePerson? right) => !Equals(left, right);
}