// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Api;

namespace Tantei.Tests;

public sealed class CreditsTests
{
	[Test]
	public async Task ServerLicensesAreEmbedded()
	{
		var licenses = CreditsProvider.ReadServerLicenses();

		await Assert.That(licenses.Length).IsGreaterThan(0);
	}

	[Test]
	public async Task ServerLicensesAllHaveNameAndVersion()
	{
		var licenses = CreditsProvider.ReadServerLicenses();
		var incomplete = licenses.Where(static license => string.IsNullOrWhiteSpace(license.Name) || string.IsNullOrWhiteSpace(license.Version))
								 .ToArray();

		await Assert.That(incomplete).IsEmpty();
	}
}
