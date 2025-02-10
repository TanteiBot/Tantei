// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database;

namespace Tantei.Tests;

public sealed class UngroupedTests
{
	[Fact]
	public void DTOCorrectlyParsesGitVersionDate()
	{
		_ = DateTimeOffset.ParseExact(GitVersionInformation.CommitDate, "o", CultureInfo.InvariantCulture);

		Assert.True(true);
	}

	[Fact]
	public async Task DbSchemaHaveNotChanged()
	{
		await using var db = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite().Options);
		var schema = db.Database.GenerateCreateScript();
		await Verify(schema);
	}
}