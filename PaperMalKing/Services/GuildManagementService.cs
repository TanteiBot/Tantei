#region LICENSE

// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Exceptions;

namespace PaperMalKing.Services
{
	public sealed class GuildManagementService
	{
		private readonly ILogger<GuildManagementService> _logger;

		private readonly IServiceProvider _serviceProvider;

		private readonly UpdatePublishingService _updatePublishingService;
		private readonly DiscordClient _discordClient;

		public GuildManagementService(ILogger<GuildManagementService> logger, IServiceProvider serviceProvider,
									  UpdatePublishingService updatePublishingService, DiscordClient discordClient)
		{
			this._logger = logger;
			this._logger.LogTrace("Building {@GuildManagementService}", typeof(GuildManagementService));
			this._serviceProvider = serviceProvider;
			this._updatePublishingService = updatePublishingService;
			this._discordClient = discordClient;
			this._logger.LogTrace("Built {@GuildManagementService}", typeof(GuildManagementService));
		}

		public async Task<DiscordGuild> SetChannelAsync(ulong guildId, ulong channelId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId).ConfigureAwait(false);
			if (guild != null)
				throw new GuildManagementException("Server already have channel to post updates into", guildId, channelId);

			guild = new()
			{
				DiscordGuildId = guildId,
				PostingChannelId = channelId,
				Users = Array.Empty<DiscordUser>()
			};
			await db.DiscordGuilds.AddAsync(guild).ConfigureAwait(false);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
			var chn = await this._discordClient.GetChannelAsync(channelId).ConfigureAwait(false);
			this._updatePublishingService.AddChannel(chn);
			return guild;
		}

		public async Task RemoveGuildAsync(ulong guildId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId).ConfigureAwait(false);
			if (guild == null)
				throw new GuildManagementException("You can't remove this server from posting updates", guildId);

			db.DiscordGuilds.Remove(guild);
			this._updatePublishingService.RemoveChannel(guild.PostingChannelId);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		}

		public async Task UpdateChannelAsync(ulong guildId, ulong channelId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId).ConfigureAwait(false);
			if (guild == null)
				throw new GuildManagementException("You can't update channel for posting updates without setting it first", guildId, channelId);
			if (guild.PostingChannelId == channelId)
				throw new GuildManagementException("You can't update channel to the same channel", guildId, channelId);

			var opci = guild.PostingChannelId;
			guild.PostingChannelId = channelId;
			db.DiscordGuilds.Update(guild);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
			this._updatePublishingService.UpdateChannel(opci, await this._discordClient.GetChannelAsync(opci).ConfigureAwait(false));
		}

		public async Task RemoveUserAsync(ulong guildId, ulong userId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var guild = await db.DiscordGuilds.Include(g => g.Users).FirstOrDefaultAsync(g => g.DiscordGuildId == guildId).ConfigureAwait(false);
			var user = guild.Users.FirstOrDefault(u => u.DiscordUserId == userId);
			if (user == null)
				throw new GuildManagementException("Such user wasn't found as ");

			guild.Users.Remove(user);
			db.DiscordGuilds.Update(guild);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		}
	}
}