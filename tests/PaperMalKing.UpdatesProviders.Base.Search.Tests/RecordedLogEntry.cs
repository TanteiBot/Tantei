// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;

namespace PaperMalKing.UpdatesProviders.Base.Search.Tests;

internal sealed record RecordedLogEntry(
	LogLevel Level,
	EventId EventId,
	string Message,
	Exception? Exception,
	IReadOnlyList<KeyValuePair<string, object?>> State);
