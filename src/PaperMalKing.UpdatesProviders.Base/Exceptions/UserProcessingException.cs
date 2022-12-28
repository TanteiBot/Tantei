// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;

namespace PaperMalKing.UpdatesProviders.Base.Exceptions;

public sealed class UserProcessingException : Exception
{
	public BaseUser? User { get; }

	public UserProcessingException(BaseUser user, string message) : base(message) => this.User = user;

	public UserProcessingException(string message) : base(message)
	{ }
}