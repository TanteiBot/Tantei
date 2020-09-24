using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace PaperMalKing.Utilities
{
	static class Extensions
	{
		public static string ToFixedWidth(this string s, int newLength)
		{
			if (s.Length < newLength)
				return s.PadRight(newLength);

			if (s.Length > newLength)
				return s.Substring(0, newLength);

			return s;
		}

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
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}

			return list;
		}
	}
}