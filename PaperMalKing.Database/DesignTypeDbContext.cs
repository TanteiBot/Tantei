// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using Microsoft.EntityFrameworkCore.Design;

namespace PaperMalKing.Database
{
	public sealed class DesignTypeDbContext : IDesignTimeDbContextFactory<DatabaseContext>
	{
		public DatabaseContext CreateDbContext(string[] args)
		{
			if (args.Length == 0)
				return new();
			return string.IsNullOrEmpty(args?[0]) ? new () : new (args[0]);
		}
	}
}