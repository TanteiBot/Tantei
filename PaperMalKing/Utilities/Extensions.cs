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
	}
}