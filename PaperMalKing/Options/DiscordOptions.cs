using System.Collections.Generic;
using DSharpPlus.Entities;

namespace PaperMalKing.Options
{
	public sealed class DiscordOptions
	{
		public const string Discord = "Discord";
		
		public string Token { get; set; }

		public string ActivityType { get; set; }

		public string PresenceText { get; set; }
	}
}