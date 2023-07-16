// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models;

public sealed class BotUser
{
	[Key]
	[Required]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public uint UserId { get; internal init; }

	public DiscordUser? DiscordUser { get; init; }
}