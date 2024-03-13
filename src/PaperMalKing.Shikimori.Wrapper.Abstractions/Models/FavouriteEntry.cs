// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CommunityToolkit.Diagnostics;
using PaperMalKing.Common.Json;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed class FavouriteEntry : IEquatable<FavouriteEntry>, IComparable<FavouriteEntry>, IComparable, IMultiLanguageName
{
	[JsonIgnore]
	public string? GenericType { get; set; }

	[JsonIgnore]
	public string? SpecificType { get; set; }

	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("name")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public required string Name { get; init; }

	[JsonPropertyName("russian")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public string? RussianName { get; init; }

	public string ImageUrl => Utils.GetImageUrl(this.GenericType!, this.Id);

	public string Url => Utils.GetUrl(this.GenericType!, this.Id);

	public bool Equals(FavouriteEntry? other)
	{
		return other is not null && (ReferenceEquals(this, other) || (string.Equals(this.GenericType, other.GenericType, StringComparison.Ordinal) && this.Id == other.Id));
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is FavouriteEntry other && this.Equals(other));

	public override int GetHashCode()
	{
		return HashCode.Combine(this.GenericType, this.Id);
	}

	public static bool operator ==(FavouriteEntry? left, FavouriteEntry? right) => Equals(left, right);

	public static bool operator !=(FavouriteEntry? left, FavouriteEntry? right) => !Equals(left, right);

	public int CompareTo(FavouriteEntry? other)
	{
		if (ReferenceEquals(this, other))
		{
			return 0;
		}

		if (other is null)
		{
			return 1;
		}

		var genericTypeComparison = string.CompareOrdinal(this.GenericType, other.GenericType);
		return genericTypeComparison != 0 ? genericTypeComparison : this.Id.CompareTo(other.Id);
	}

	public int CompareTo(object? obj)
	{
		return obj is FavouriteEntry other
			? this.CompareTo(other)
			: ThrowHelper.ThrowArgumentException<int>($"Object must be of type {nameof(FavouriteEntry)}", nameof(obj));
	}

	public static bool operator <(FavouriteEntry? left, FavouriteEntry? right) => Comparer<FavouriteEntry>.Default.Compare(left, right) < 0;

	public static bool operator >(FavouriteEntry? left, FavouriteEntry? right) => Comparer<FavouriteEntry>.Default.Compare(left, right) > 0;

	public static bool operator <=(FavouriteEntry? left, FavouriteEntry? right) => Comparer<FavouriteEntry>.Default.Compare(left, right) <= 0;

	public static bool operator >=(FavouriteEntry? left, FavouriteEntry? right) => Comparer<FavouriteEntry>.Default.Compare(left, right) >= 0;
}