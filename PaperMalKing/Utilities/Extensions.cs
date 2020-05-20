using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DSharpPlus;

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

		public static string ToShortName(this LogLevel level)
		{
			return level switch
			{
				LogLevel.Debug => "DEB",
				LogLevel.Info => "INF",
				LogLevel.Warning => "WRN",
				LogLevel.Error => "ERR",
				LogLevel.Critical => "CRT",
				_ => "UNK"
			};
		}
		
		public static IList<T> Shuffle<T>(this IList<T> list)
		{
			using(var provider = new RNGCryptoServiceProvider())
			{
				int n = list.Count;
				while(n > 1)
				{
					byte[] box = new byte[sizeof(int)];
					provider.GetBytes(box);
					int bit = BitConverter.ToInt32(box, 0);
					int k = Math.Abs(bit) % n;
					n--;
					T value = list[k];
					list[k] = list[n];
					list[n] = value;
				}
			}

			return list;
		}
	}
}