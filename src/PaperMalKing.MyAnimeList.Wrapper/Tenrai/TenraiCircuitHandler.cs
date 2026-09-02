// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Polly.Timeout;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class TenraiCircuitHandler : DelegatingHandler
{
	private readonly TenraiCircuit _circuit;

	public TenraiCircuitHandler(TenraiCircuit circuit)
	{
		ArgumentNullException.ThrowIfNull(circuit);
		this._circuit = circuit;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		try
		{
			var response = await base.SendAsync(request, cancellationToken);
			if (IsTransientFailure(response.StatusCode))
			{
				this._circuit.RecordTerminalFailure();
			}

			return response;
		}
		catch (Exception exception) when (exception is HttpRequestException or TimeoutRejectedException)
		{
			this._circuit.RecordTerminalFailure();
			throw;
		}
	}

	private static bool IsTransientFailure(HttpStatusCode statusCode) => statusCode is
		HttpStatusCode.RequestTimeout or
		HttpStatusCode.InternalServerError or
		HttpStatusCode.BadGateway or
		HttpStatusCode.ServiceUnavailable or
		HttpStatusCode.GatewayTimeout;
}
