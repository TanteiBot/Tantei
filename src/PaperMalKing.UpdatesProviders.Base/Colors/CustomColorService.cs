// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Colors;

[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
public sealed class CustomColorService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TUser,
									   [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)] TUpdateType>(IDbContextFactory<DatabaseContext> dbContextFactory)
	where TUser : class, IUpdateProviderUser
	where TUpdateType : unmanaged, Enum
{
	public async Task SetColorAsync(ulong userId, TUpdateType updateType, DiscordColor color)
	{
		using var db = dbContextFactory.CreateDbContext();

		var user = db.Set<TUser>().TagWith("Getting user to set color").TagWithCallSite()
					 .FirstOrDefault(u => u.DiscordUserId == userId) ?? throw new UserProcessingException("You must create account first");
		var byteType = Unsafe.BitCast<TUpdateType, byte>(updateType);

		user.Colors.RemoveAll(c => c.UpdateType == byteType);
		user.Colors.Add(new()
		{
			UpdateType = byteType,
			ColorValue = color.Value,
		});

		await db.SaveChangesAndThrowOnNoneAsync();
	}

	public async Task RemoveColorAsync(ulong userId, TUpdateType updateType)
	{
		using var db = dbContextFactory.CreateDbContext();
		var user = db.Set<TUser>().TagWith("Getting user to remove color").TagWithCallSite().FirstOrDefault(u => u.DiscordUserId == userId) ??
				   throw new UserProcessingException("You must create account first");

		var byteType = Unsafe.BitCast<TUpdateType, byte>(updateType);

		user.Colors.RemoveAll(c => c.UpdateType == byteType);

		await db.SaveChangesAndThrowOnNoneAsync();
	}

	public string? OverridenColors(ulong userId)
	{
		using var db = dbContextFactory.CreateDbContext();
		var colors = db.Set<TUser>().TagWith("Getting colors of a user").TagWithCallSite().AsNoTracking().Where(u => u.DiscordUserId == userId).Select(x => x.Colors).FirstOrDefault();

		if (colors is null or [])
		{
			return null;
		}

		return $"Your colors: {colors.Select(c =>
			string.Create(CultureInfo.InvariantCulture, $"{(TUpdateType)(object)c.UpdateType}: #{c.ColorValue:X6}")).JoinToString('\n')}";
	}
}