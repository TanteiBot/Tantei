// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal readonly record struct TenraiSignal(TenraiSignalKind Kind, HttpResponseMessage? Response)
{
	public static TenraiSignal Failed { get; } = new(TenraiSignalKind.Failed, Response: null);

	public static TenraiSignal Attempted(HttpResponseMessage response)
	{
		ArgumentNullException.ThrowIfNull(response);
		return new(TenraiSignalKind.Attempted, response);
	}

	public static TenraiSignal Completed(HttpResponseMessage response)
	{
		ArgumentNullException.ThrowIfNull(response);
		return new(TenraiSignalKind.Completed, response);
	}
}
