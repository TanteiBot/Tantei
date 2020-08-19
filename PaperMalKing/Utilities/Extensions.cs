using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DSharpPlus;

namespace PaperMalKing.Utilities
{
	static class Extensions
	{
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
	}
}