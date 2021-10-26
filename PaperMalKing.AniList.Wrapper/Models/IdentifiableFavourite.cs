// Tantei.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models
{
	public sealed class IdentifiableFavourite : IIdentifiable, IEquatable<IdentifiableFavourite>
	{
		[JsonPropertyName("id")]
		public ulong Id { get; init; }

		[JsonIgnore]
		public FavouriteType Type { get; set; }

		public bool Equals(IdentifiableFavourite? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return this.Id == other.Id && this.Type == other.Type;
		}

		public override bool Equals(object? obj)
		{
			return ReferenceEquals(this, obj) || obj is IdentifiableFavourite other && this.Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(this.Id, (int)this.Type);
		}
	}
}