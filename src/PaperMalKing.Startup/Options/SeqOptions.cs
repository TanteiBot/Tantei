// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.Startup.Options;

public sealed class SeqOptions : OtlpOptions
{
	public const string Seq = "Seq";

	public required string ApiKey { get; init; }
}