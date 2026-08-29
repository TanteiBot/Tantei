// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Startup.Web;

namespace Tantei.Tests;

public sealed class InvitePermissionsTests
{
	[Test]
	public async Task InvitePermissionsCoverEverythingTheUpdatePosterNeeds()
	{
		await Assert.That(DiscordApiConstants.InvitePermissions).IsEqualTo("2147535872");
	}
}
