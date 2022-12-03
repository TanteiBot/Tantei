#region LICENSE

// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

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