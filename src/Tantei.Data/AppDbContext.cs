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
using Tantei.Core.Models.Users;
using Tantei.Core.Models.Users.AniList;
using Tantei.Core.Models.Users.MyAnimeList;
using Tantei.Core.Models.Users.Shikimori;

namespace Tantei.Data;

public sealed class AppDbContext : DbContext
{
	#nullable disable
	public DbSet<BotUser> Users { get; init; }

	public DbSet<AniListUser> AniListUsers { get; init; }

	public DbSet<ShikimoriUser> ShikimoriUsers { get; init; }

	public DbSet<MalUser> MalUsers { get; init; }
	#nullable restore

	/// <inheritdoc />
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
}