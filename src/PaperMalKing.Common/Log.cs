// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using Microsoft.Extensions.Logging;

namespace PaperMalKing.Common;

internal static class Log
{
	private static readonly Func<ILogger, string, string, string, IDisposable?> ExecutingCommandByUserCallback =
		LoggerMessage.DefineScope<string, string, string>("Executing command `{CommandName}` by user `{UserInfo}`. In Guild `{GuildInfo}` ");

	public static IDisposable? ExecutingCommandByUserScope(this ILogger<BotCommandsModule> logger, string commandName, string userInfo, string guildInfo)
		=> ExecutingCommandByUserCallback(logger, commandName, userInfo, guildInfo);
}