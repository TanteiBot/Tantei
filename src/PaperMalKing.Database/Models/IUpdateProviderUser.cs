// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.Database.Models;

public interface IUpdateProviderUser<TFeature> : IUpdateProviderUser
	where TFeature : unmanaged, Enum
{
	TFeature Features { get; set; }
}

public interface IUpdateProviderUser
{
	ulong DiscordUserId { get; init; }

	DiscordUser DiscordUser { get; set; }

	List<CustomUpdateColor> Colors { get; set; }
}