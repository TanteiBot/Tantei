// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.Api.Contracts.Responses;

internal sealed record InvitableGuildResponse(string GuildId, string Name, string? IconUrl);
