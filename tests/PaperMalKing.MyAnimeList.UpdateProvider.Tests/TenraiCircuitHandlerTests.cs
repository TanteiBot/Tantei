// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.MyAnimeList.UpdateProvider.Installer;
using PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;
using Polly.RateLimiting;
using Polly.Timeout;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests;

public sealed class TenraiCircuitHandlerTests
{
	private const int FailureThreshold = 5;
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	[Arguments(HttpStatusCode.RequestTimeout)]
	[Arguments(HttpStatusCode.InternalServerError)]
	[Arguments(HttpStatusCode.BadGateway)]
	[Arguments(HttpStatusCode.ServiceUnavailable)]
	[Arguments(HttpStatusCode.GatewayTimeout)]
	public async Task TerminalTransientStatusesOpenTheCircuit(HttpStatusCode statusCode)
	{
		using var scope = new HandlerScope((_, _) => Task.FromResult(new HttpResponseMessage(statusCode)));

		await scope.SendManyAsync(FailureThreshold);

		await Assert.That(scope.Circuit.IsOpen).IsTrue();
	}

	[Test]
	[Arguments(HttpStatusCode.OK)]
	[Arguments(HttpStatusCode.NotFound)]
	[Arguments(HttpStatusCode.BadRequest)]
	[Arguments(HttpStatusCode.Unauthorized)]
	[Arguments(HttpStatusCode.Forbidden)]
	[Arguments(HttpStatusCode.MethodNotAllowed)]
	[Arguments(HttpStatusCode.TooManyRequests)]
	[Arguments(HttpStatusCode.NotImplemented)]
	public async Task ExcludedStatusesNeverOpenTheCircuit(HttpStatusCode statusCode)
	{
		using var scope = new HandlerScope((_, _) => Task.FromResult(new HttpResponseMessage(statusCode)));

		await scope.SendManyAsync(FailureThreshold);

		await Assert.That(scope.Circuit.IsOpen).IsFalse();
	}

	[Test]
	public async Task NetworkFailuresOpenTheCircuit()
	{
		using var scope = new HandlerScope((_, _) => Task.FromException<HttpResponseMessage>(new HttpRequestException("network")));

		await scope.SendManyAsync(FailureThreshold);

		await Assert.That(scope.Circuit.IsOpen).IsTrue();
	}

	[Test]
	public async Task InternalTimeoutsOpenTheCircuit()
	{
		using var scope = new HandlerScope((_, _) => Task.FromException<HttpResponseMessage>(new TimeoutRejectedException("timeout")));

		await scope.SendManyAsync(FailureThreshold);

		await Assert.That(scope.Circuit.IsOpen).IsTrue();
	}

	[Test]
	public async Task QueueRejectionAndCooldownSuppressionNeverOpenTheCircuit()
	{
		using var scope = new HandlerScope((_, _) =>
			Task.FromException<HttpResponseMessage>(new RateLimiterRejectedException("suppressed")));

		await scope.SendManyAsync(FailureThreshold);

		await Assert.That(scope.Circuit.IsOpen).IsFalse();
	}

	[Test]
	public async Task CallerCancellationNeverOpensTheCircuit()
	{
		using var cancellationSource = new CancellationTokenSource();
		cancellationSource.Cancel();
		using var scope = new HandlerScope((_, token) => Task.FromException<HttpResponseMessage>(new OperationCanceledException(token)));

		for (var attempt = 0; attempt < FailureThreshold; attempt++)
		{
			try
			{
				using var response = await scope.Client.GetAsync("anime/1", cancellationSource.Token);
			}
			catch (OperationCanceledException exception)
			{
				_ = exception;
			}
		}

		await Assert.That(scope.Circuit.IsOpen).IsFalse();
	}

	private sealed class HandlerScope : IDisposable
	{
		private readonly TenraiCircuitHandler _handler;
		private readonly FakeHttpMessageHandler _inner;

		public HandlerScope(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
		{
			this.Circuit = new(new ManualTimeProvider(Start), NullLogger<TenraiCircuit>.Instance);
			this._inner = new(respond);
			this._handler = new(this.Circuit) { InnerHandler = this._inner, };
			this.Client = new(this._handler, disposeHandler: false)
			{
				BaseAddress = new("https://example.test/v1/"),
			};
		}

		public TenraiCircuit Circuit { get; }

		public HttpClient Client { get; }

		public async Task SendManyAsync(int count)
		{
			for (var attempt = 0; attempt < count; attempt++)
			{
				try
				{
					using var response = await this.Client.GetAsync(
						"anime/1", TestContext.Current!.Execution.CancellationToken);
				}
				catch (Exception exception) when (exception is not OperationCanceledException)
				{
					_ = exception;
				}
			}
		}

		public void Dispose()
		{
			this.Client.Dispose();
			this._handler.Dispose();
			this._inner.Dispose();
		}
	}

	private sealed class FakeHttpMessageHandler(
		Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			respond(request, cancellationToken);
	}
}
