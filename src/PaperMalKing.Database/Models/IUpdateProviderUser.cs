// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;

namespace PaperMalKing.Database.Models;

public interface IUpdateProviderUser<TFeature> : IUpdateProviderUser where TFeature : unmanaged, Enum
{
	public TFeature Features { get; set; }
}

public interface IUpdateProviderUser
{
	public ulong DiscordUserId { get; init; }
	public DiscordUser DiscordUser { get; init; }
}