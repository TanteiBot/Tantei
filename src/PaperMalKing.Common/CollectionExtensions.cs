// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;

namespace PaperMalKing.Common;

public static class CollectionExtensions
{
	public static (IReadOnlyList<T> AddedValues, IReadOnlyList<T> RemovedValues) GetDifference<T>(
		this IReadOnlyList<T> original, IReadOnlyList<T> resulting) where T : IEquatable<T>
	{
		var originalHs = new HashSet<T>(original);
		var resultingHs = new HashSet<T>(resulting);
		if (originalHs.SetEquals(resultingHs))
		{
			return (Array.Empty<T>(), Array.Empty<T>());
		}
		originalHs.ExceptWith(resulting);
		resultingHs.ExceptWith(original);
		var added = resultingHs.ToArray();
		var removed = originalHs.ToArray();
		return (added, removed);
	}

	[SuppressMessage("Design", "CA1002:Do not expose generic lists")]
	public static List<TEntity> SortBy<TEntity, TProperty>(this List<TEntity> source, Func<TEntity, TProperty> selector) where TProperty : IComparable<TProperty>
	{
		source.Sort((f, s) => selector(f).CompareTo(selector(s)));
		return source;
	}

	[SuppressMessage("Design", "CA1002:Do not expose generic lists")]
	public static List<TEntity> SortByDescending<TEntity, TProperty>(this List<TEntity> source, Func<TEntity, TProperty> selector) where TProperty : IComparable<TProperty>
	{
		source.Sort((f, s) => -selector(f).CompareTo(selector(s)));
		return source;
	}

	[SuppressMessage("Design", "CA1002:Do not expose generic lists")]
	public static List<TEntity> SortByThenBy<TEntity, TProperty, TOtherProperty>(this List<TEntity> source, Func<TEntity, TProperty> firstSelector, Func<TEntity, TOtherProperty> secondSelector) where TProperty : IComparable<TProperty> where TOtherProperty: IComparable<TOtherProperty>
	{
		source.Sort((f, s) =>
		{
			var r = firstSelector(f).CompareTo(firstSelector(s));
			return r == 0 ? secondSelector(f).CompareTo(secondSelector(s)) : r;
		});
		return source;
	}

	public static void ForEach<T>(this IList<T> list, Action<T> action)
	{
		for (var i = 0; i < list.Count; i++)
		{
			action(list[i]);
		}
	}

	public static bool Exists<T>(this T[] array, Predicate<T> predicate)
	{
		return Array.Exists(array, predicate);
	}

	public static T? Find<T>(this T[] array, Predicate<T> predicate)
	{
		return Array.Find(array, predicate);
	}

	public static bool TrueForAll<T>(this T[] array, Predicate<T> predicate)
	{
		return Array.TrueForAll(array, predicate);
	}
}