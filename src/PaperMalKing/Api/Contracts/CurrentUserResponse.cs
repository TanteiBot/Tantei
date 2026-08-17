// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.Api.Contracts;

public sealed record CurrentUserResponse(string DiscordUserId, string Username, string? AvatarUrl, bool IsRegistered, bool IsWebAdmin);
