// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models;

public sealed class DiscordOAuthToken
{
	[Key]
	[Required]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public ulong DiscordUserId { get; init; }

	public required string AccessToken { get; set; }

	public required string RefreshToken { get; set; }

	public required DateTimeOffset ExpiresAt { get; set; }

	public required DateTimeOffset LastUsedAt { get; set; }
}
