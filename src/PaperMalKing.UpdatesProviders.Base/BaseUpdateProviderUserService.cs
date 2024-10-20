// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base;

public abstract class BaseUpdateProviderUserService<TUser>
	where TUser : class, IUpdateProviderUser
{
	private readonly GeneralUserService _generalUserService;

	protected ILogger<BaseUpdateProviderUserService<TUser>> Logger { get; }

	protected IDbContextFactory<DatabaseContext> DbContextFactory { get; }

	protected BaseUpdateProviderUserService(ILogger<BaseUpdateProviderUserService<TUser>> logger, IDbContextFactory<DatabaseContext> dbContextFactory, GeneralUserService generalUserService)
	{
		this._generalUserService = generalUserService;
		this.Logger = logger;
		this.DbContextFactory = dbContextFactory;
	}

	public abstract string Name { get; }

	public abstract Task<BaseUser> AddUserAsync(ulong userId, ulong guildId, string? username = null);

	public void RemoveUser(ulong userId)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var rows = db.Set<TUser>().TagWith("Remove user").TagWithCallSite().Where(x => x.DiscordUserId == userId).ExecuteDelete();
		if (rows == 0)
		{
			throw new UserProcessingException($"You weren't tracked by {this.Name} update checker");
		}
	}

	public async Task RemoveUserHereAsync(ulong userId, ulong guildId)
	{
		await this._generalUserService.RemoveUserInGuildAsync(guildId, userId);
		this._generalUserService.RemoveUserIfInNoGuilds(userId);
	}

	public abstract IReadOnlyList<BaseUser> ListUsers(ulong guildId);

	[SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Tooling is buggy")]
	protected IReadOnlyList<BaseUser> ListUsersCore<TOrderType>(ulong guildId, Expression<Func<TUser, TOrderType>> orderExpression, Expression<Func<TUser, BaseUser>> selector)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		return db.Set<TUser>().TagWith("Query users in a guild").TagWithCallSite().Include(x => x.DiscordUser).ThenInclude(x => x.Guilds)
				 .Where(x => x.DiscordUser.Guilds.Any(guild => guild.DiscordGuildId == guildId)).OrderBy(orderExpression).Select(selector).ToArray();
	}
}