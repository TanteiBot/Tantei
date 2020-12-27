using System.Globalization;

namespace PaperMalKing.Common
{
	public static class Helpers
	{
		public static string ToDiscordMention(uint id) => ToDiscordMention((ulong) id);

		public static string ToDiscordMention(int id) => ToDiscordMention((ulong) id);

		public static string ToDiscordMention(long id) => ToDiscordMention((ulong) id);

		public static string ToDiscordMention(ulong id) => $"<@!{id.ToString(CultureInfo.InvariantCulture)}>";
	}
}