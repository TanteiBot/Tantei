// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Features;

public abstract class BaseUserFeaturesService<TUser, TFeature>
	where TUser : class, IUpdateProviderUser<TFeature>
	where TFeature : unmanaged, Enum
{
	protected ILogger<BaseUserFeaturesService<TUser, TFeature>> Logger { get; }

	protected IDbContextFactory<DatabaseContext> DbContextFactory { get; }

	protected BaseUserFeaturesService(IDbContextFactory<DatabaseContext> dbContextFactory, ILogger<BaseUserFeaturesService<TUser, TFeature>> logger)
	{
		this.Logger = logger;
		this.DbContextFactory = dbContextFactory;
	}

	public abstract Task EnableFeaturesAsync(TFeature feature, ulong userId);

	public async Task DisableFeaturesAsync(TFeature feature, ulong userId)
	{
		await using var db = this.DbContextFactory.CreateDbContext();
		var dbUser = db.Set<TUser>().TagWith("Query user to disable a feature").TagWithCallSite().FirstOrDefault(su => su.DiscordUserId == userId) ??
					 throw new UserFeaturesException("You must register first before disabling features");
		var features = dbUser.Features;
		var f = Unsafe.As<TFeature, ulong>(ref features);
		var featureValue = Unsafe.As<TFeature, ulong>(ref feature);
		if (!features.HasFlag(feature))
		{
			throw new UserFeaturesException("This feature wasn't enabled for you,so you cant enable it");
		}

		f &= ~featureValue;

		dbUser.Features = Unsafe.As<ulong, TFeature>(ref f);
		await this.DisableFeatureCleanupAsync(db, dbUser, feature);
		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None);
	}

	protected abstract ValueTask DisableFeatureCleanupAsync(DatabaseContext db, TUser user, TFeature featureToDisable);

	public string EnabledFeatures(ulong userId)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var features = db.Set<TUser>().TagWith("Query enabled features").TagWithCallSite().AsNoTracking().Where(u => u.DiscordUser.DiscordUserId == userId).Select(x => new TFeature?(x.Features))
						 .FirstOrDefault();
		if (!features.HasValue)
		{
			throw new UserFeaturesException("You must register first before checking for enabled features");
		}

		return features.Value.Humanize();
	}
}