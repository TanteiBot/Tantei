// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace PaperMalKing.Common
{
    public static class CollectionExtensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            Span<byte> box = stackalloc byte[sizeof(int)];
            while (n > 1)
            {
                RandomNumberGenerator.Fill(box);
                var bit = BitConverter.ToInt32(box);
                var k = Math.Abs(bit) % n;
                n--;
                (list[k], list[n]) = (list[n], list[k]);
			}

            return list;
        }

        public static (IReadOnlyList<T> AddedValues, IReadOnlyList<T> RemovedValues) GetDifference<T>(
            this IReadOnlyList<T> original, IReadOnlyList<T> resulting) where T : IEquatable<T>
        {
            var originalHs = new HashSet<T>(original);
            var resultingHs = new HashSet<T>(resulting);
            originalHs.ExceptWith(resulting);
            resultingHs.ExceptWith(original);
            var added = resultingHs.ToArray() ?? Array.Empty<T>();
            var removed = originalHs.ToArray() ?? Array.Empty<T>();
            return (added, removed);
        }

        public static T[] ForEach<T>(this T[] array, Action<T> action)
        {
            Array.ForEach(array, action);
            return array;
        }
	}
}