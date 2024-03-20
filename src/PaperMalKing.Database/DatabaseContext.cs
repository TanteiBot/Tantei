// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.Database.Models.Shikimori;

namespace PaperMalKing.Database;

public sealed class DatabaseContext : DbContext
{
	public DbSet<BotUser> BotUsers => this.Set<BotUser>();

	public DbSet<DiscordGuild> DiscordGuilds => this.Set<DiscordGuild>();

	public DbSet<DiscordUser> DiscordUsers => this.Set<DiscordUser>();

	public DbSet<MalUser> MalUsers => this.Set<MalUser>();

	public DbSet<BaseMalFavorite> BaseMalFavorites => this.Set<BaseMalFavorite>();

	public DbSet<MalFavoriteAnime> MalFavoriteAnimes => this.Set<MalFavoriteAnime>();

	public DbSet<MalFavoriteManga> MalFavoriteMangas => this.Set<MalFavoriteManga>();

	public DbSet<MalFavoriteCharacter> MalFavoriteCharacters => this.Set<MalFavoriteCharacter>();

	public DbSet<MalFavoritePerson> MalFavoritePersons => this.Set<MalFavoritePerson>();

	public DbSet<MalFavoriteCompany> MalFavoriteCompanies => this.Set<MalFavoriteCompany>();

	public DbSet<ShikiUser> ShikiUsers => this.Set<ShikiUser>();

	public DbSet<ShikiFavourite> ShikiFavourites => this.Set<ShikiFavourite>();

	public DbSet<AniListUser> AniListUsers => this.Set<AniListUser>();

	public DbSet<AniListFavourite> AniListFavourites => this.Set<AniListFavourite>();

	public DatabaseContext(DbContextOptions<DatabaseContext> options)
		: base(options)
	{
	}

	public async Task<int> SaveChangesAndThrowOnNoneAsync(CancellationToken cancellationToken = default)
	{
		var rows = await this.TrySaveChangesUntilDatabaseIsUnlockedAsync(cancellationToken);
		if (rows <= 0)
		{
			throw new NoChangesSavedException();
		}

		return rows;
	}

	[SuppressMessage("Reliability", "EA0002:Use \'System.TimeProvider\' to make the code easier to test", Justification = "We already use it")]
	public async Task<int> TrySaveChangesUntilDatabaseIsUnlockedAsync(CancellationToken cancellationToken = default)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			try
			{
				return this.SaveChanges();
			}
			catch (SqliteException ex) when (ex.SqliteErrorCode == 5)
			{
				// Database is locked
				await Task.Delay(TimeSpan.FromMilliseconds(500), TimeProvider.System, cancellationToken);
			}
		}

		throw new TaskCanceledException("Saving changes were cancelled");
	}

	[SuppressMessage("Roslynator", "RCS1201:Use method chaining.", Justification = "It's not preferred when creating model")]
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<MalUser>(mu =>
		{
			// Constant value because default in app can be changed anytime
			mu.HasOne(x => x.DiscordUser).WithOne().HasForeignKey<MalUser>(x => x.DiscordUserId);
			mu.Property(u => u.Features).HasSentinel(MalUserFeatures.None).HasDefaultValue(MalUserFeatures.AnimeList | MalUserFeatures.MangaList | MalUserFeatures.Favorites |
																					  MalUserFeatures.Mention | MalUserFeatures.Website | MalUserFeatures.MediaFormat |
																					  MalUserFeatures.MediaStatus); // DO NOT CHANGE. Must always be a constant, otherwise SQLite will recreate table
			mu.Property(u => u.LastUpdatedMangaListTimestamp).HasConversion<DateTimeOffsetToBinaryConverter>();
			mu.Property(u => u.LastUpdatedAnimeListTimestamp).HasConversion<DateTimeOffsetToBinaryConverter>();
			mu.Property(p => p.FavoritesIdHash).HasDefaultValue("");
			mu.HasIndex(x => x.Features);
			mu.HasIndex(x => x.DiscordUserId);
			mu.OwnsMany(x => x.Colors, colors => colors.ToJson());
		});

		modelBuilder.Entity<DiscordGuild>(dg => dg.HasIndex(x => x.DiscordGuildId));

		modelBuilder.Entity<DiscordUser>(dg => dg.HasIndex(x => x.DiscordUserId));

		modelBuilder.Entity<BaseMalFavorite>(bmf =>
		{
			bmf.UseTphMappingStrategy();
			bmf.HasKey(k => new
			{
				k.Id,
				k.UserId,
				k.FavoriteType,
			});
			bmf.HasDiscriminator(x => x.FavoriteType)
			   .HasValue<MalFavoriteAnime>(MalFavoriteType.Anime)
			   .HasValue<MalFavoriteManga>(MalFavoriteType.Manga)
			   .HasValue<MalFavoriteCharacter>(MalFavoriteType.Character)
			   .HasValue<MalFavoritePerson>(MalFavoriteType.Person)
			   .HasValue<MalFavoriteCompany>(MalFavoriteType.Company);
			bmf.HasIndex(x => x.UserId);
			bmf.HasIndex(x => x.FavoriteType);
			bmf.HasIndex(x => x.Id);
			bmf.ToTable("MalFavorites");
		});

		modelBuilder.Entity<MalFavoriteAnime>().HasOne(x => x.User).WithMany(x => x.FavoriteAnimes).HasForeignKey(x => x.UserId);
		modelBuilder.Entity<MalFavoriteManga>().HasOne(x => x.User).WithMany(x => x.FavoriteMangas).HasForeignKey(x => x.UserId);
		modelBuilder.Entity<MalFavoriteCharacter>().HasOne(x => x.User).WithMany(x => x.FavoriteCharacters).HasForeignKey(x => x.UserId);
		modelBuilder.Entity<MalFavoritePerson>().HasOne(x => x.User).WithMany(x => x.FavoritePeople).HasForeignKey(x => x.UserId);
		modelBuilder.Entity<MalFavoriteCompany>().HasOne(x => x.User).WithMany(x => x.FavoriteCompanies).HasForeignKey(x => x.UserId);

		modelBuilder.Entity<ShikiUser>(su =>
		{
			su.HasOne(x => x.DiscordUser).WithOne().HasForeignKey<ShikiUser>(x => x.DiscordUserId);
			su.HasKey(k => k.Id);
			su.Property(u => u.Features).HasSentinel(ShikiUserFeatures.None).HasDefaultValue(ShikiUserFeatures.AnimeList | ShikiUserFeatures.MangaList | ShikiUserFeatures.Favourites |
														 ShikiUserFeatures.Mention | ShikiUserFeatures.Website | ShikiUserFeatures.MediaFormat |
														 ShikiUserFeatures.MediaStatus); // DO NOT CHANGE. Must always be a constant, otherwise SQLite will recreate table
			su.Property(x => x.FavouritesIdHash).HasDefaultValue("");
			su.HasIndex(x => x.Features);
			su.HasIndex(x => x.DiscordUserId);
			su.OwnsMany(x => x.Achievements, achs => achs.ToJson());
		});
		modelBuilder.Entity<ShikiFavourite>(sf =>
		{
			sf.HasKey(k => new
			{
				k.Id,
				k.FavType,
				k.UserId,
			});
			sf.HasIndex(p => p.UserId);
		});

		modelBuilder.Entity<AniListUser>(au =>
		{
			au.HasOne(x => x.DiscordUser).WithOne().HasForeignKey<AniListUser>(x => x.DiscordUserId);
			au.HasKey(k => k.Id);
			au.Property(u => u.Features).HasSentinel(AniListUserFeatures.None).HasDefaultValue(AniListUserFeatures.AnimeList | AniListUserFeatures.MangaList |
																							   AniListUserFeatures.Favourites | AniListUserFeatures.Mention | AniListUserFeatures.Website |
																							   AniListUserFeatures.MediaFormat | AniListUserFeatures.MediaStatus); // DO NOT CHANGE. Must always be a constant, otherwise SQLite will recreate table
			au.Property(x => x.FavouritesIdHash).HasDefaultValue("");
			au.HasIndex(x => x.Features);
			au.HasIndex(x => x.DiscordUserId);
			au.OwnsMany(x => x.Colors, colors => colors.ToJson());
		});

		modelBuilder.Entity<AniListFavourite>(af =>
		{
			af.HasKey(k => new
			{
				k.Id,
				k.FavouriteType,
				k.UserId,
			});
			af.HasIndex(x => x.UserId);
		});
	}
}