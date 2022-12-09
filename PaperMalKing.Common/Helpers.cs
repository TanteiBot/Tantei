// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Globalization;

namespace PaperMalKing.Common;

public static class Helpers
{
	public static string ToDiscordMention(ulong id) => $"<@!{id}>";
}