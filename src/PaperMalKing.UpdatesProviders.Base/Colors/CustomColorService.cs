// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Colors;

[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
public sealed class CustomColorService<TUser, TUpdateType>
	where TUser : class, IUpdateProviderUser
	where TUpdateType : unmanaged, Enum
{
	public IDbContextFactory<DatabaseContext> DbContextFactory { get; }

	public CustomColorService(IDbContextFactory<DatabaseContext> dbContextFactory)
	{
		this.DbContextFactory = dbContextFactory;
	}

	public async Task SetColorAsync(ulong userId, TUpdateType updateType, DiscordColor color)
	{
		using var db = this.DbContextFactory.CreateDbContext();

		var user = db.Set<TUser>().TagWith("Getting user to set color").TagWithCallSite()
					 .FirstOrDefault(u => u.DiscordUserId == userId) ?? throw new UserProcessingException("You must create account first");
		var byteType = Unsafe.As<TUpdateType, byte>(ref updateType);

		user.Colors.RemoveAll(c => c.UpdateType == byteType);
		user.Colors.Add(new CustomUpdateColor
		{
			UpdateType = byteType,
			ColorValue = color.Value,
		});

		await db.SaveChangesAndThrowOnNoneAsync();
	}

	public async Task RemoveColorAsync(ulong userId, TUpdateType updateType)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var user = db.Set<TUser>().TagWith("Getting user to remove color").TagWithCallSite().FirstOrDefault(u => u.DiscordUserId == userId) ??
				   throw new UserProcessingException("You must create account first");

		var byteType = Unsafe.As<TUpdateType, byte>(ref updateType);

		user.Colors.RemoveAll(c => c.UpdateType == byteType);

		await db.SaveChangesAndThrowOnNoneAsync();
	}

	[SuppressMessage("Performance", "EA0006:Replace uses of \'Enum.GetName\' and \'Enum.ToString\' for improved performance", Justification = "We don't know type here")]
	public string? OverridenColors(ulong userId)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var colors = db.Set<TUser>().TagWith("Getting colors of a user").TagWithCallSite().AsNoTracking().Where(u => u.DiscordUserId == userId).Select(x => x.Colors).FirstOrDefault();

		if (colors is null or [])
		{
			return null;
		}

		return $"Your colors: {string.Join('\n',
			colors.Select(c =>
				$"{(TUpdateType)(object)c.UpdateType}: #{string.Create(CultureInfo.InvariantCulture, $"{c.ColorValue:X6}")}"))}";
	}
}