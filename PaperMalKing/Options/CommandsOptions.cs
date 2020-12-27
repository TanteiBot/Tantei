namespace PaperMalKing.Options
{
	public sealed class CommandsOptions
	{
		public const string Commands = "Discord:Commands";

		public string[] Prefixes { get; set; } = null!;

		public bool EnableMentionPrefix { get; set; }

		public bool CaseSensitive { get; set; }
	}
}