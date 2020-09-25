using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database.Models;

namespace PaperMalKing.Database.Repositories.Discord
{
	public sealed class DiscordGuildsRepository : BaseRepository
	{
		private readonly DbSet<DiscordGuild> _guilds;

		public DiscordGuildsRepository(DatabaseContext context) : base(context)
		{
			this._guilds = context.DiscordGuilds;
		}

		public ConfiguredTaskAwaitable<DiscordGuild> GetGuildByIdAsync(long guildId)
		{
			return this._guilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId).ConfigureAwait(false)!;
		}

		public void ChangePostingChannelId(DiscordGuild guild, long channelId)
		{
			guild.PostingChannelId = channelId;
			this._guilds.Update(guild);
		}

		public async Task<long?> GetPostingChannelIdByGuildId(long guildId)
		{
			var guild = await this._guilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId).ConfigureAwait(false);
			return guild.PostingChannelId;
		}
	}
}