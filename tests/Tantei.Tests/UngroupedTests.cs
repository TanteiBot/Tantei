// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Globalization;

namespace Tantei.Tests;

public sealed class UngroupedTests
{
	[Fact]
	public void DTOCorrectlyParsesGitVersionDate()
	{
		DateTimeOffset.ParseExact(GitVersionInformation.CommitDate, "o", CultureInfo.InvariantCulture);

		Assert.True(true);
	}
}