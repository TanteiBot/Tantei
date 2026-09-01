// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using DSharpPlus.Exceptions;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static class SearchPostFailure
{
	public static bool IsForbidden(Exception exception) => exception is DiscordException { WebResponse.ResponseCode: (int)HttpStatusCode.Forbidden };
}
