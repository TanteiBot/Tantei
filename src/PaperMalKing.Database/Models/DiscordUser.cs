// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.Database.Models;

public sealed class DiscordUser
{
	[SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed", Justification = "It's handled by EF Core")]
	private uint BotUserId { get; init; }

	[Key]
	[Required]
	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	public ulong DiscordUserId { get; init; }

	[ForeignKey(nameof(BotUserId))]
	public required BotUser BotUser { get; set; }

	public required IList<DiscordGuild> Guilds { get; init; }

#pragma warning disable S103
	public override string ToString() => $"{nameof(this.BotUserId)}: {this.BotUserId}, {nameof(this.DiscordUserId)}: {this.DiscordUserId}, {nameof(this.BotUser)}: {this.BotUser}, {nameof(this.Guilds)}: {this.Guilds}";
#pragma warning restore S103
}