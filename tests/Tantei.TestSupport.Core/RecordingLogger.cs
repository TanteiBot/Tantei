// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;

namespace Tantei.TestSupport;

public sealed class RecordingLogger<T> : ILogger<T>
{
	public List<RecordedLogEntry> Entries { get; } = [];

	public List<object?> Scopes { get; } = [];

	public RecordedLogEntry Single() => this.Entries.Single();

	public IDisposable BeginScope<TState>(TState state)
		where TState : notnull
	{
		this.Scopes.Add(state);
		return NullScope.Instance;
	}

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		ArgumentNullException.ThrowIfNull(formatter);
		KeyValuePair<string, object?>[] fields = state is IReadOnlyList<KeyValuePair<string, object?>> tags ? [.. tags] : [];
		this.Entries.Add(new(logLevel, eventId, formatter(state, exception), exception, fields));
	}

	private sealed class NullScope : IDisposable
	{
		public static readonly NullScope Instance = new();

		public void Dispose()
		{
		}
	}
}
