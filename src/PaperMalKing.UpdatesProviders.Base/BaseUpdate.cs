// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

namespace PaperMalKing.UpdatesProviders.Base;

public sealed class BaseUpdate : IUpdate
{
	private readonly IAsyncEnumerable<UpdateContents> _asyncUpdates;

	public BaseUpdate(IAsyncEnumerable<UpdateContents> updates)
	{
		this._asyncUpdates = updates;
	}

	public async IAsyncEnumerable<UpdateContents> GetUpdateContentsAsync()
	{
		await foreach (var update in this._asyncUpdates)
		{
			yield return update;
		}
	}
}