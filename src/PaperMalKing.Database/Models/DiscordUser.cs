// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.Database.Models;

public sealed class DiscordUser
{
	[SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed")]
	private uint BotUserId { get; init; }

	[Key]
	[Required]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public ulong DiscordUserId { get; init; }

	[ForeignKey(nameof(BotUserId))]
	public required BotUser BotUser { get; set; }

	public required IList<DiscordGuild> Guilds { get; init; }
}