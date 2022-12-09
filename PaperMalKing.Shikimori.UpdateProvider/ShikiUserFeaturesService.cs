﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Attributes;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.Shikimori.Wrapper.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	public sealed class ShikiUserFeaturesService : IUserFeaturesService<ShikiUserFeatures>
	{
		private readonly ShikiClient _client;
		private readonly ILogger<ShikiUserFeaturesService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly Dictionary<ShikiUserFeatures, (string, string)> Descriptions = new();

		IReadOnlyDictionary<ShikiUserFeatures, (string, string)> IUserFeaturesService<ShikiUserFeatures>.Descriptions => this.Descriptions;


		public ShikiUserFeaturesService(ShikiClient client, ILogger<ShikiUserFeaturesService> logger, IServiceProvider serviceProvider)
		{
			this._client = client;
			this._logger = logger;
			this._serviceProvider = serviceProvider;

			var t = typeof(ShikiUserFeatures);
			var ti = t.GetTypeInfo();
			var values = Enum.GetValues(t).Cast<ShikiUserFeatures>().Where(v => v != ShikiUserFeatures.None);

			foreach (var enumVal in values)
			{
				var name = enumVal.ToString();
				var fieldVal = ti.DeclaredMembers.First(xm => xm.Name == name);
				var attribute = fieldVal!.GetCustomAttribute<FeatureDescriptionAttribute>()!;

				this.Descriptions[enumVal] = (attribute.Description, attribute.Summary);
			}
		}

		public async Task EnableFeaturesAsync(IReadOnlyList<ShikiUserFeatures> features, ulong userId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var dbUser = db.ShikiUsers.Include(su => su.Favourites)
								 .FirstOrDefault(su => su.DiscordUserId == userId);
			if (dbUser == null)
				throw new UserFeaturesException("You must register first before enabling features");
			var total = features.Aggregate((acc, next) => acc | next);
			var lastHistoryEntry = new ulong?();
			dbUser.Features |= total;
			for (var i = 0; i < features.Count; i++)
			{
				var feature = features[i];
				switch (feature)
				{
					case ShikiUserFeatures.AnimeList:
					case ShikiUserFeatures.MangaList:
					{
						if (lastHistoryEntry.HasValue)
							break;
						var (data, _) = await this._client.GetUserHistoryAsync(dbUser.Id, 1, 1, HistoryRequestOptions.Any, CancellationToken.None)
												  .ConfigureAwait(false);
						lastHistoryEntry = data.MaxBy(h => h.Id)!.Id;
						break;
					}
					case ShikiUserFeatures.Favourites:
					{
						var favourites = await this._client.GetUserFavouritesAsync(dbUser.Id, CancellationToken.None).ConfigureAwait(false);
						dbUser.Favourites = favourites.AllFavourites.Select(fe => new ShikiFavourite
						{
							Id = fe.Id,
							Name = fe.Name,
							FavType = fe.GenericType!,
							User = dbUser
						}).ToList();
						break;
					}
				}
			}

			if (lastHistoryEntry.HasValue)
				dbUser.LastHistoryEntryId = lastHistoryEntry.Value;
			db.ShikiUsers.Update(dbUser);
			await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
		}

		public async Task DisableFeaturesAsync(IReadOnlyList<ShikiUserFeatures> features, ulong userId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var dbUser = db.ShikiUsers.Include(su => su.Favourites).FirstOrDefault(su => su.DiscordUserId == userId);
			if (dbUser == null)
				throw new UserFeaturesException("You must register first before disabling features");

			var total = features.Aggregate((acc, next) => acc | next);

			dbUser.Features &= ~total;
			if (features.Any(x => x == ShikiUserFeatures.Favourites))
				dbUser.Favourites.Clear();

			db.ShikiUsers.Update(dbUser);
			await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
		}

		public ValueTask<string> EnabledFeaturesAsync(ulong userId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var dbUser = db.ShikiUsers.AsNoTrackingWithIdentityResolution().FirstOrDefault(su => su.DiscordUserId == userId);
			if (dbUser == null)
				throw new UserFeaturesException("You must register first before checking for enabled features");

			return ValueTask.FromResult(dbUser.Features.Humanize());
		}
	}
}