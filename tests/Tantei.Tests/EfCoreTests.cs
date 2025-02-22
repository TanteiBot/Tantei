// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database;

namespace Tantei.Tests;

public class EfCoreTests
{
	[Fact]
	public async Task DbSchemaHaveNotChanged()
	{
		await using var db = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite().Options);
		var schema = db.Database.GenerateCreateScript();
		await Verify(schema);
	}

	[Fact]
	public async Task DbNoPendingMigrationsAreLeft()
	{
		await using var dbContext = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
														.UseSqlite(x => x.MigrationsAssembly("PaperMalKing.Database.Migrations")).Options);

		Assert.False(dbContext.Database.HasPendingModelChanges());
	}
}