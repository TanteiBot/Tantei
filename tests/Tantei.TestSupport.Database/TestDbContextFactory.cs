// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database;

namespace Tantei.TestSupport;

public sealed class TestDbContextFactory(DbContextOptions<DatabaseContext> options) : IDbContextFactory<DatabaseContext>
{
	public DatabaseContext CreateDbContext() => new(options);
}
