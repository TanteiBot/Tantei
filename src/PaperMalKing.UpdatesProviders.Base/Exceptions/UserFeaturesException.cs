// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;

namespace PaperMalKing.UpdatesProviders.Base.Exceptions;

public sealed class UserFeaturesException : Exception
{
	public UserFeaturesException(string message)
		: base(message)
	{
	}
}