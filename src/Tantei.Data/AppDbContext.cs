// Tantei.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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