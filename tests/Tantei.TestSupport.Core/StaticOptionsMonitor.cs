// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Options;

namespace Tantei.TestSupport;

public sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
{
	public T CurrentValue => value;

	public T Get(string? name) => value;

	public IDisposable? OnChange(Action<T, string?> listener) => null;
}
