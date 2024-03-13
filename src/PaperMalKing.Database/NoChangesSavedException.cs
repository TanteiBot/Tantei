// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using System;

namespace PaperMalKing.Database;

public sealed class NoChangesSavedException : Exception
{
	public NoChangesSavedException()
		: base("No changes were saved to database")
	{
	}
}