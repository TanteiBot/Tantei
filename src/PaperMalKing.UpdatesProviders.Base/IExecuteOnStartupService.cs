// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.UpdatesProviders.Base;

public interface IExecuteOnStartupService
{
	Task ExecuteAsync(CancellationToken cancellationToken);
}