namespace PaperMalKing.Options
{
	public sealed class DiscordOptions
	{
		public const string Discord = "Discord";

		public string Token { get; set; } = null!;

		public byte ActivityType { get; set; }

		public string PresenceText { get; set; } = null!;
	}
}