// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.Database.Models.Shikimori;

namespace PaperMalKing.Database;

public class DatabaseContext : DbContext
{
	public DbSet<BotUser> BotUsers => this.Set<BotUser>();

	public DbSet<DiscordGuild> DiscordGuilds => this.Set<DiscordGuild>();

	public DbSet<DiscordUser> DiscordUsers => this.Set<DiscordUser>();

	public DbSet<MalUser> MalUsers => this.Set<MalUser>();

	public DbSet<MalFavoriteAnime> MalFavoriteAnimes => this.Set<MalFavoriteAnime>();

	public DbSet<MalFavoriteManga> MalFavoriteMangas => this.Set<MalFavoriteManga>();

	public DbSet<MalFavoriteCharacter> MalFavoriteCharacters => this.Set<MalFavoriteCharacter>();

	public DbSet<MalFavoritePerson> MalFavoritePersons => this.Set<MalFavoritePerson>();

	public DbSet<MalFavoriteCompany> MalFavoriteCompanies => this.Set<MalFavoriteCompany>();

	public DbSet<ShikiUser> ShikiUsers => this.Set<ShikiUser>();

	public DbSet<ShikiFavourite> ShikiFavourites => this.Set<ShikiFavourite>();

	public DbSet<AniListUser> AniListUsers => this.Set<AniListUser>();

	public DbSet<AniListFavourite> AniListFavourites => this.Set<AniListFavourite>();

	public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
	{ }

	#pragma warning disable MA0051
	protected override void OnModelCreating(ModelBuilder modelBuilder)
		#pragma warning restore MA0051
	{
		base.OnModelCreating(modelBuilder);

		// Constant value because default in app can be changed anytime
		modelBuilder.Entity<MalUser>(mu =>
		{
			mu.Property(u => u.Features).HasDefaultValue((MalUserFeatures)127ul);
			mu.Property(u => u.LastUpdatedMangaListTimestamp).HasConversion<DateTimeOffsetToBinaryConverter>();
			mu.Property(u => u.LastUpdatedAnimeListTimestamp).HasConversion<DateTimeOffsetToBinaryConverter>();
		});

		modelBuilder.Entity<MalFavoriteAnime>().HasKey(k => new
		{
			k.Id,
			k.UserId
		});
		modelBuilder.Entity<MalFavoriteManga>().HasKey(k => new
		{
			k.Id,
			k.UserId
		});
		modelBuilder.Entity<MalFavoriteCharacter>().HasKey(k => new
		{
			k.Id,
			k.UserId
		});
		modelBuilder.Entity<MalFavoritePerson>().HasKey(k => new
		{
			k.Id,
			k.UserId
		});
		modelBuilder.Entity<MalFavoriteCompany>().HasKey(k => new
		{
			k.Id,
			k.UserId
		});

		modelBuilder.Entity<ShikiUser>().HasKey(k => k.Id);

		modelBuilder.Entity<ShikiUser>().Property(u => u.Features)
					.HasDefaultValue((ShikiUserFeatures)127UL); // Constant value because default in app can be changed anytime

		modelBuilder.Entity<ShikiFavourite>().HasKey(k => new
		{
			k.Id,
			k.FavType,
			k.UserId
		});

		modelBuilder.Entity<AniListUser>().HasKey(k => k.Id);

		modelBuilder.Entity<AniListUser>().Property(u => u.Features).HasDefaultValue((AniListUserFeatures)127ul);
		modelBuilder.Entity<AniListFavourite>().HasKey(k => new
		{
			k.Id,
			k.FavouriteType,
			k.UserId
		});
	}
}