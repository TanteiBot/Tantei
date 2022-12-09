// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using DSharpPlus.CommandsNext;

namespace PaperMalKing.UpdatesProviders.Base
{
	public interface ICommandsService
	{
		CommandsNextExtension CommandsExtension { get; }
	}
}