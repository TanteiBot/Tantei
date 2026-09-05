// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus;
using PaperMalKing.Startup.Web;

namespace Tantei.Tests;

public sealed class InvitePermissionsTests
{
	[Test]
	public async Task InvitePermissionsCoverEverythingTheUpdatePosterNeeds()
	{
		var permissions = (Permissions)ulong.Parse(DiscordApiConstants.InvitePermissions, CultureInfo.InvariantCulture);
		await Assert.That(permissions.HasFlag(Permissions.AccessChannels)).IsTrue();
		await Assert.That(permissions.HasFlag(Permissions.SendMessages)).IsTrue();
		await Assert.That(permissions.HasFlag(Permissions.EmbedLinks)).IsTrue();
		await Assert.That(permissions.HasFlag(Permissions.AttachFiles)).IsTrue();
		await Assert.That(permissions.HasFlag(Permissions.UseApplicationCommands)).IsTrue();
	}
}
