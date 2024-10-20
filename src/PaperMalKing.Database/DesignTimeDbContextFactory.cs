// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PaperMalKing.Common;

namespace PaperMalKing.Database;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
	public DatabaseContext CreateDbContext(string[] args)
	{
		args.ForEach(Console.WriteLine);
		var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
		optionsBuilder.UseSqlite(static o => o.MigrationsAssembly("PaperMalKing.Database.Migrations"));
		return new(optionsBuilder.Options);
	}
}