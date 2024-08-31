// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Startup.Services;

internal sealed class MigrateOnStartupService(IDbContextFactory<DatabaseContext> _factory) : IExecuteOnStartupService
{
	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public async Task ExecuteAsync(CancellationToken cancellationToken = default)
	{
		using var db = _factory.CreateDbContext();

		db.Database.Migrate();

		await Task.CompletedTask;
	}
}