namespace PaperMalKing.Options
{
	public sealed class CommandsOptions
	{
		public const string Commands = "Commands";
		
		public string[] Prefixes { get; set; }

		public bool EnableMentionPrefix { get; set; }

		public bool CaseSensitive { get; set; }
	}
}