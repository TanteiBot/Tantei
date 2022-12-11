// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaperMalKing.UpdatesProviders.Base;

public interface IUpdateProviderUserService
{
	abstract static string Name { get; }

	Task<BaseUser> AddUserAsync(string username, ulong userId, ulong guildId);

	Task<BaseUser> RemoveUserAsync(ulong userId);

	Task RemoveUserHereAsync(ulong userId, ulong guildId);

	IReadOnlyList<BaseUser> ListUsers(ulong guildId);
}