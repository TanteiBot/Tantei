// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using System;
using Microsoft.EntityFrameworkCore;

namespace PaperMalKing.Database;

public sealed class NoChangesSavedException : Exception
{
	public DbContext? Context { get; }

	public NoChangesSavedException(DbContext? context)
	{
		this.Context = context;
	}
}