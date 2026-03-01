// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;

namespace Tantei.Tests;

public sealed class UngroupedTests
{
	[Test]
	public async Task DTOCorrectlyParsesGitVersionDate()
	{
		await Assert.That(() => DateTimeOffset.ParseExact(GitVersionInformation.CommitDate, "o", CultureInfo.InvariantCulture)).ThrowsNothing();
	}
}