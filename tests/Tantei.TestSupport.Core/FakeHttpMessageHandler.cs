// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace Tantei.TestSupport;

public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
	private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _respond;
	private int _callCount;

	public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
	{
		ArgumentNullException.ThrowIfNull(respond);
		this._respond = respond;
	}

	public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
	{
		ArgumentNullException.ThrowIfNull(respond);
		this._respond = (request, _) => Task.FromResult(respond(request));
	}

	public int CallCount => this._callCount;

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		Interlocked.Increment(ref this._callCount);
		return this._respond(request, cancellationToken);
	}
}
