// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed class FavouriteEntry : IEquatable<FavouriteEntry>, IComparable<FavouriteEntry>, IComparable, IMultiLanguageName
{
	[JsonIgnore]
	public string? GenericType { get; set; }

	[JsonIgnore]
	public string? SpecificType { get; set; } = null;

	[JsonPropertyName("id")]
	public uint Id { get; set; }

	[JsonPropertyName("name")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	[JsonRequired]
	public string Name { get; set; } = null!;

	[JsonPropertyName("russian")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public string? RussianName { get; internal set; }

	public string? ImageUrl => Utils.GetImageUrl(this.GenericType!, this.Id);

	public string? Url => Utils.GetUrl(this.GenericType!, this.Id);

	public bool Equals(FavouriteEntry? other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return string.Equals(this.GenericType, other.GenericType, StringComparison.Ordinal) && this.Id == other.Id;
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FavouriteEntry other && this.Equals(other);

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

		if (ReferenceEquals(null, other))
		{
			return 1;
		}

		var genericTypeComparison = string.Compare(this.GenericType, other.GenericType, StringComparison.Ordinal);
		if (genericTypeComparison != 0)
		{
			return genericTypeComparison;
		}

		return this.Id.CompareTo(other.Id);
	}

	public int CompareTo(object? obj)
	{
		if (ReferenceEquals(null, obj))
		{
			return 1;
		}

		if (ReferenceEquals(this, obj))
		{
			return 0;
		}

		return obj is FavouriteEntry other ? this.CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(FavouriteEntry)}", nameof(obj));
	}

	public static bool operator <(FavouriteEntry? left, FavouriteEntry? right) => Comparer<FavouriteEntry>.Default.Compare(left, right) < 0;

	public static bool operator >(FavouriteEntry? left, FavouriteEntry? right) => Comparer<FavouriteEntry>.Default.Compare(left, right) > 0;

	public static bool operator <=(FavouriteEntry? left, FavouriteEntry? right) => Comparer<FavouriteEntry>.Default.Compare(left, right) <= 0;

	public static bool operator >=(FavouriteEntry? left, FavouriteEntry? right) => Comparer<FavouriteEntry>.Default.Compare(left, right) >= 0;
}