// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus;

namespace PaperMalKing.Startup.Web;

public static class DiscordApiConstants
{
	public const string BaseUrl = "https://discord.com/api/v10/";

	public const string AuthorizeUrl = "https://discord.com/oauth2/authorize";

	public const string InviteScopes = "bot%20applications.commands";

	private const Permissions RequiredInvitePermissions = Permissions.AccessChannels |
														  Permissions.SendMessages |
														  Permissions.EmbedLinks |
														  Permissions.AttachFiles |
														  Permissions.UseApplicationCommands;

	public static readonly string InvitePermissions = ((ulong)RequiredInvitePermissions).ToString(CultureInfo.InvariantCulture);
}