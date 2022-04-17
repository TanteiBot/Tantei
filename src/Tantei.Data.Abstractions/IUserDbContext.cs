// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Microsoft.EntityFrameworkCore;
using Tantei.Core.Models.Users;

namespace Tantei.Data.Abstractions;

public interface IUserDbContext<T> where T : class, IUpdateProviderUser
{
	DbSet<BotUser> BotUsers { get; }

	DbSet<T> Users { get; }

	int SaveChanges();
}