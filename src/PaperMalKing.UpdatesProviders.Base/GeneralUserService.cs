// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base;

public sealed class GeneralUserService(IDbContextFactory<DatabaseContext> _dbContextFactory, ILogger<GeneralUserService> _logger)
{
	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public async Task RemoveUserInGuildAsync(ulong guildId, ulong userId)
	{
		using var db = _dbContextFactory.CreateDbContext();
		var guild = db.DiscordGuilds.TagWith("Query user to remove him in a guild").TagWithCallSite().Include(g => g.Users)
					  .First(g => g.DiscordGuildId == guildId);
		var user = guild.Users.FirstOrDefault(u => u.DiscordUserId == userId) ?? throw new UserProcessingException("Such user wasn't found as registered in this guild");
		_logger.RemovingUser(user);
		guild.Users.Remove(user);
		await db.SaveChangesAndThrowOnNoneAsync();
	}
}