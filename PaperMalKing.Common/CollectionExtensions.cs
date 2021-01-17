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
            using var provider = new RNGCryptoServiceProvider();
            var n = list.Count;
            Span<byte> box = stackalloc byte[sizeof(int)];
            while (n > 1)
            {
                provider.GetBytes(box);
                ReadOnlySpan<byte> readonlyBox = box;
                var bit = BitConverter.ToInt32(readonlyBox);
                var k = Math.Abs(bit) % n;
                n--;
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
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
        
        public static List<T> Add<T>(this List<T> list, T value)
        {
            list.Add(value);
            return list;
        }

        public static List<T> AddRange<T>(this List<T> list, IEnumerable<T> values)
        {
            list.AddRange(values);
            return list;
        }

        public static T[] ForEach<T>(this T[] array, Action<T> action)
        {
            Array.ForEach(array, action);
            return array;
        }

        public static List<T> ForEach<T>(this List<T> source, Action<T> action)
        {
            source.ForEach(action);
            return source;
        }

        public static IReadOnlyList<T> ForEach<T>(this IReadOnlyList<T> list, Action<T> action)
        {
            for (var i = 0; i < list.Count; i++) action(list[i]);
            return list;
        }
    }
}