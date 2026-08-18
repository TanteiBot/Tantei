// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;

namespace PaperMalKing.Startup.Web.Guilds;

public sealed record DiscordPartialGuild(ulong Id, string Name, string? IconUrl, Permissions Permissions);