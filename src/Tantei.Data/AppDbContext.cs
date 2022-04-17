// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Tantei.Core.Models.AniList.Users;
using Tantei.Core.Models.MyAnimeList.Users;
using Tantei.Core.Models.Shikimori.Users;
using Tantei.Core.Models.Users;
using Tantei.Data.Abstractions;

namespace Tantei.Data;

public sealed class AppDbContext : DbContext, IUserDbContext<AniListUser>, IUserDbContext<ShikimoriUser>, IUserDbContext<MalUser>
{
	public DbSet<BotUser> Users => this._users ??= this.Set<BotUser>();

	public DbSet<AniListUser> AniListUsers => this._aniListUsers ??= this.Set<AniListUser>();

	public DbSet<ShikimoriUser> ShikimoriUsers => this._shikimoriUsers ??= this.Set<ShikimoriUser>();

	public DbSet<MalUser> MalUsers => this._malUsers ??= this.Set<MalUser>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		static void RegisterConverter<T>(ValueConverter valueConverter, ModelBuilder modelBuilder)
		{
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(T) || p.PropertyType == typeof(T?));
				foreach (var property in properties)
				{
					modelBuilder.Entity(entityType.Name).Property(property.Name).HasConversion(valueConverter);
				}
			}
		}

		base.OnModelCreating(modelBuilder);
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
		RegisterConverter<DateTimeOffset>(new DateTimeOffsetToBinaryConverter(), modelBuilder);
		RegisterConverter<ulong>(new ValueConverter<ulong, long>(arg => (long)arg, arg => (ulong)arg), modelBuilder);
	}

	#region ExplicitImplementations

	DbSet<BotUser> IUserDbContext<AniListUser>.BotUsers => this.Users;

	DbSet<MalUser> IUserDbContext<MalUser>.Users => this.MalUsers;

	DbSet<BotUser> IUserDbContext<MalUser>.BotUsers => this.Users;

	DbSet<ShikimoriUser> IUserDbContext<ShikimoriUser>.Users => this.ShikimoriUsers;

	DbSet<BotUser> IUserDbContext<ShikimoriUser>.BotUsers => this.Users;

	DbSet<AniListUser> IUserDbContext<AniListUser>.Users => this.AniListUsers;

	#endregion

	#region PrivateFields

	private DbSet<BotUser>? _users;

	private DbSet<AniListUser>? _aniListUsers;

	private DbSet<ShikimoriUser>? _shikimoriUsers;

	private DbSet<MalUser>? _malUsers;

	#endregion
}