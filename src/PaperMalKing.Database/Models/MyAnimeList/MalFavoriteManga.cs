// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;

namespace PaperMalKing.Database.Models.MyAnimeList;

public sealed class MalFavoriteManga : BaseMalListFavorite, IEquatable<MalFavoriteManga>
{
	public bool Equals(MalFavoriteManga? other)
	{
		return other is not null && (ReferenceEquals(this, other) || (this.UserId == other.UserId && this.Id == other.Id));
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is MalFavoriteManga other && this.Equals(other));

	public override int GetHashCode() => HashCode.Combine(this.UserId, this.Id);

	public static bool operator ==(MalFavoriteManga? left, MalFavoriteManga? right) => Equals(left, right);

	public static bool operator !=(MalFavoriteManga? left, MalFavoriteManga? right) => !Equals(left, right);
}