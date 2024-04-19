// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Startup.Services;

internal sealed class MigrateOnStartupService : IExecuteOnStartupService
{
	private readonly IDbContextFactory<DatabaseContext> _factory;

	public MigrateOnStartupService(IDbContextFactory<DatabaseContext> factory)
	{
		this._factory = factory;
	}

	public async Task ExecuteAsync(CancellationToken cancellationToken = default)
	{
		await using var db = this._factory.CreateDbContext();

		db.Database.Migrate();

		await Task.CompletedTask;
	}
}