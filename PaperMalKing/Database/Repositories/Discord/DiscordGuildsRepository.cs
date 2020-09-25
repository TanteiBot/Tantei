using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database.Models;
using PaperMalKing.Services;

namespace PaperMalKing.Database.Repositories.Discord
{
	public sealed class DiscordGuildsRepository : BaseRepository
	{
		public DiscordGuildsRepository(DatabaseContext context) : base(context)
		{ }

		public Task<DiscordGuild?> GetGuildByIdAsync(long guildId)
		{
			return this._context.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId)!;
		}

		public void ChangePostingChannelId(DiscordGuild guild, long channelId)
		{
			guild.PostingChannelId = channelId;
			this._context.Update(guild);
		}

		public async Task<long?> GetPostingChannelIdByGuildId(long guildId)
		{
			var guild = await this._context.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId);
			return guild.PostingChannelId;
		}
	}
}