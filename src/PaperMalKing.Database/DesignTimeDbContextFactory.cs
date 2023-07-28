// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaperMalKing.Database;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
	public DatabaseContext CreateDbContext(string[] args)
	{
		Array.ForEach(args, Console.WriteLine);
		var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
		optionsBuilder.UseSqlite(o => o.MigrationsAssembly("PaperMalKing.Database.Migrations"));
		return new DatabaseContext(optionsBuilder.Options);
	}
}