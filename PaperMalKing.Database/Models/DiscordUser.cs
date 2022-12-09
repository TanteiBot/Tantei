// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.Database.Models
{
	public sealed class DiscordUser
	{
		[SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed")]
		private long BotUserId { get; set; }

		[Key]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong DiscordUserId { get; init; }

		[ForeignKey(nameof(BotUserId))]
		public BotUser BotUser { get; set; } = null!;

		public ICollection<DiscordGuild> Guilds { get; init; } = null!;
	}
}