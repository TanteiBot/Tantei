using System;

namespace PaperMalKing.Exceptions
{
	public sealed class GuildManagementException : Exception
	{
		public ulong? GuildId { get; }

		public ulong? ChannelId { get; }

		/// <inheritdoc />
		public GuildManagementException(string message, ulong? guildId = default, ulong? channelId = default) : base(message)
		{
			this.GuildId = guildId;
			this.ChannelId = channelId;
		}
	}
}