// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.UpdateProviders.Abstractions;

public interface IUpdateProvider : IAsyncDisposable
{
	string Name { get; }

	event UpdateFoundEvent? UpdateFound;

	void Start();
}