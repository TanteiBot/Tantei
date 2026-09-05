// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using EntityFramework.Exceptions.Sqlite;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaperMalKing.Database;

namespace Tantei.TestSupport;

public static class SqliteInMemoryDatabase
{
	public static async Task<(IDbContextFactory<DatabaseContext> Factory, SqliteConnection Connection, IDataProtectionProvider? DataProtection)> CreateAsync(
		bool addDataProtection = false,
		Action<DatabaseContext>? seed = null)
	{
		var connection = new SqliteConnection("Filename=:memory:");
		await connection.OpenAsync();

		var services = new ServiceCollection();
		services.AddDbContextFactory<DatabaseContext>(o => o.UseSqlite(connection).UseExceptionProcessor());
		if (addDataProtection)
		{
			services.AddDataProtection();
		}

		var provider = services.BuildServiceProvider();

		var factory = provider.GetRequiredService<IDbContextFactory<DatabaseContext>>();
		await using (var db = factory.CreateDbContext())
		{
			await db.Database.EnsureCreatedAsync();
			seed?.Invoke(db);
		}

		return (factory, connection, addDataProtection ? provider.GetRequiredService<IDataProtectionProvider>() : null);
	}
}
