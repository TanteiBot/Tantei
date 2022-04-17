// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Tantei.Core.Models.Updates;
using Tantei.Core.Models.Users;

namespace Tantei.UpdateProviders.Abstractions;

public sealed class UpdateFoundEventArgs : EventArgs
{
	public BaseUserUpdate Update { get; }

	public BotUser User { get; }

	public UpdateFoundEventArgs(BaseUserUpdate update, BotUser user)
	{
		this.Update = update;
		this.User = user;
	}
}