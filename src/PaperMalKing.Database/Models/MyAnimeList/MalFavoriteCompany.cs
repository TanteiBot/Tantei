// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;

namespace PaperMalKing.Database.Models.MyAnimeList;

public sealed class MalFavoriteCompany : BaseMalFavorite, IEquatable<MalFavoriteCompany>
{
	public bool Equals(MalFavoriteCompany? other)
	{
		return other is not null && (ReferenceEquals(this, other) || (this.UserId == other.UserId && this.Id == other.Id));
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is MalFavoriteCompany other && this.Equals(other));

	public override int GetHashCode() => HashCode.Combine(this.UserId, this.Id);

	public static bool operator ==(MalFavoriteCompany? left, MalFavoriteCompany? right) => Equals(left, right);

	public static bool operator !=(MalFavoriteCompany? left, MalFavoriteCompany? right) => !Equals(left, right);
}