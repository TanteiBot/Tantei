// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Collections.Generic;

namespace PaperMalKing.Options;

public sealed class CommandsOptions
{
	public const string Commands = "Discord:Commands";

	public required IReadOnlyList<string> Prefixes { get; set; }

	public bool EnableMentionPrefix { get; set; }

	public bool CaseSensitive { get; set; }
}