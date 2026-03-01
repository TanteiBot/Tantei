// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.Common;

[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
public static class CollectionExtensions
{
	public static (IReadOnlyList<T> AddedValues, IReadOnlyList<T> RemovedValues) GetDifference<T>(this IReadOnlyList<T> original, IReadOnlyList<T> resulting)
		where T : IEquatable<T>
	{
		var originalHs = new HashSet<T>(original);
		var resultingHs = new HashSet<T>(resulting);
		if (originalHs.SetEquals(resultingHs))
		{
			return ([], []);
		}

		originalHs.ExceptWith(resulting);
		resultingHs.ExceptWith(original);
		var added = resultingHs.ToArray();
		var removed = originalHs.ToArray();
		return (added, removed);
	}

	extension<TEntity>(List<TEntity> source)
	{
		public List<TEntity> SortBy<TProperty>(Func<TEntity, TProperty> selector)
			where TProperty : IComparable<TProperty>
		{
			source.Sort((f, s) => selector(f).CompareTo(selector(s)));
			return source;
		}

		public List<TEntity> SortByDescending<TProperty>(Func<TEntity, TProperty> selector)
			where TProperty : IComparable<TProperty>
		{
			source.Sort((f, s) => -selector(f).CompareTo(selector(s)));
			return source;
		}

		public List<TEntity> SortByThenBy<TProperty, TOtherProperty>(Func<TEntity, TProperty> firstSelector, Func<TEntity, TOtherProperty> secondSelector)
			where TProperty : IComparable<TProperty>
			where TOtherProperty : IComparable<TOtherProperty>
		{
			source.Sort((f, s) =>
			{
				var r = firstSelector(f).CompareTo(firstSelector(s));
				return r == 0 ? secondSelector(f).CompareTo(secondSelector(s)) : r;
			});
			return source;
		}
	}

	extension<T>(T[] array)
	{
		public bool Exists(Predicate<T> predicate)
		{
			return Array.Exists(array, predicate);
		}

		public T? Find(Predicate<T> predicate)
		{
			return Array.Find(array, predicate);
		}

		public bool TrueForAll(Predicate<T> predicate)
		{
			return Array.TrueForAll(array, predicate);
		}
	}

	extension<T>(IEnumerable<T> values)
	{
		public string JoinToString() => values.JoinToString(", ");

		public string JoinToString(string separator) => string.Join(separator, values);

		public string JoinToString(char separator) => string.Join(separator, values);

	}

	public static void ForEach<T>(this IList<T> list, Action<T> action)
	{
		for (var i = 0; i < list.Count; i++)
		{
			action(list[i]);
		}
	}

	public static bool AddRange<T>(this HashSet<T> hs, IEnumerable<T> values)
	{
		return values.Aggregate(seed: true, (current, value) => hs.Add(value) && current);
	}
}