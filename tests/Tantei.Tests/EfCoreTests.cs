// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database;

namespace Tantei.Tests;

public class EfCoreTests
{
	[Test]
	public async Task DbSchemaHaveNotChanged()
	{
		await using var db = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite().Options);
		var schema = db.Database.GenerateCreateScript();
		await Verify(schema);
	}

	[Test]
	public async Task DbNoPendingMigrationsAreLeft()
	{
		await using var dbContext = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite(static x => x.MigrationsAssembly("PaperMalKing.Database.Migrations")).Options);
		await Assert.That(dbContext.Database.HasPendingModelChanges()).IsFalse();
	}
}