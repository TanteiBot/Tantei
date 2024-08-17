// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace PaperMalKing.UpdatesProviders.Base.UpdateProvider;

public interface IUpdateProvider
{
	string Name { get; }

	event AsyncEventHandler<UpdateFoundEventArgs> UpdateFoundEvent;

	Task TriggerStoppingAsync();

	DateTimeOffset? DateTimeOfNextUpdate { get; }

	bool IsUpdateInProgress { get; }
}