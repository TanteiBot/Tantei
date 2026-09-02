// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Polly.Timeout;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class TenraiGateHandler : DelegatingHandler
{
	private readonly TenraiGate _gate;

	public TenraiGateHandler(TenraiGate gate)
	{
		ArgumentNullException.ThrowIfNull(gate);
		this._gate = gate;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (this._gate.Check() is { } suppression)
		{
			throw new TenraiSuppressedException(suppression);
		}

		try
		{
			var response = await base.SendAsync(request, cancellationToken);
			_ = this._gate.Record(TenraiSignal.Completed(response));
			return response;
		}
		catch (Exception exception) when (exception is HttpRequestException or TimeoutRejectedException)
		{
			_ = this._gate.Record(TenraiSignal.Failed);
			throw;
		}
	}
}
