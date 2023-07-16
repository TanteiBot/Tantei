// Tantei.
// Copyright (C) 2021-2023 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;

namespace PaperMalKing.Database.Models.MyAnimeList;

public sealed class MalFavoriteCompany : BaseMalFavorite, IEquatable<MalFavoriteCompany>
{
	public bool Equals(MalFavoriteCompany? other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return this.UserId == other.UserId && this.Id == other.Id;
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is MalFavoriteCompany other && this.Equals(other));

	public override int GetHashCode() => HashCode.Combine(this.UserId, this.Id);

	public static bool operator ==(MalFavoriteCompany? left, MalFavoriteCompany? right) => Equals(left, right);

	public static bool operator !=(MalFavoriteCompany? left, MalFavoriteCompany? right) => !Equals(left, right);
}