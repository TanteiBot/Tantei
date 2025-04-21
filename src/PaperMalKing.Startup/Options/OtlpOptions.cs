// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

namespace PaperMalKing.Startup.Options;

public class OtlpOptions
{
	public const string Otlp = "Otlp";

	public required bool IsEnabled { get; init; }

	public required string IngestionUrl { get; init; }

	public Dictionary<string, string>? AdditionalHeaders { get; init; }
}