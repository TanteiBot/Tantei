// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class TenraiEnrichmentCircuitTests
{
	private const int FailureThreshold = 5;
	private const int SustainedAttempts = 10;
	private const string MalformedEnvelope = "{}";
	private const string PartiallyUsableDetails = "{\"data\":{\"themes\":{},\"demographics\":[{\"name\":\"Josei\"}]}}";
	private const string ValidDetails = "{\"data\":{\"themes\":[{\"name\":\"Action\"}],\"demographics\":[]}}";
	private const string ValidCharacters =
		"{\"data\":[{\"voice_actors\":[{\"language\":\"Japanese\"," +
		"\"person\":{\"name\":\"Megumi Hayashibara\",\"url\":\"https://myanimelist.net/people/14\"}}]}]}";

	private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	public async Task MalformedDetailResponsesOpenTheSharedCircuit()
	{
		using var scope = new ClientScope(_ => JsonResponse(MalformedEnvelope));
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			_ = await scope.Client.GetAnimeDetailsAsync(1L, cancellationToken);
		}

		await Assert.That(scope.Requests).IsEqualTo(FailureThreshold);
		var afterOpen = await scope.Client.GetAnimeDetailsAsync(1L, cancellationToken);
		await Assert.That(afterOpen).IsEqualTo(Abstractions.Models.MediaInfo.Empty);
		await Assert.That(scope.Requests).IsEqualTo(FailureThreshold);
	}

	[Test]
	public async Task MalformedCharacterResponsesOpenTheSharedCircuit()
	{
		using var scope = new ClientScope(_ => JsonResponse(MalformedEnvelope));
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			_ = await scope.Client.GetAnimeSeiyuAsync(1L, cancellationToken);
		}

		await Assert.That(scope.Requests).IsEqualTo(FailureThreshold);
		var afterOpen = await scope.Client.GetAnimeSeiyuAsync(1L, cancellationToken);
		await Assert.That(afterOpen).IsEmpty();
		await Assert.That(scope.Requests).IsEqualTo(FailureThreshold);
	}

	[Test]
	public async Task UnparseableSuccessResponsesOpenTheCircuit()
	{
		using var scope = new ClientScope(_ => JsonResponse("not-json"));
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			_ = await scope.Client.GetAnimeDetailsAsync(1L, cancellationToken);
		}

		await Assert.That(scope.Gate.Check()).IsEqualTo(TenraiSuppression.CircuitOpen);
	}

	[Test]
	public async Task PartiallyUsableMalformedDetailsNeverOpenTheCircuit()
	{
		using var scope = new ClientScope(_ => JsonResponse(PartiallyUsableDetails));
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		for (var attempt = 0; attempt < SustainedAttempts; attempt++)
		{
			var result = await scope.Client.GetMangaDetailsAsync(2L, cancellationToken);
			await Assert.That(string.Join('|', result.Demographic)).IsEqualTo("Josei");
		}

		await Assert.That(scope.Gate.Check()).IsNull();
	}

	[Test]
	public async Task SuccessfulResponsesNeverOpenTheCircuit()
	{
		using var scope = new ClientScope(_ => JsonResponse(ValidDetails));
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		for (var attempt = 0; attempt < SustainedAttempts; attempt++)
		{
			_ = await scope.Client.GetAnimeDetailsAsync(1L, cancellationToken);
		}

		await Assert.That(scope.Gate.Check()).IsNull();
	}

	[Test]
	public async Task OpenCircuitFailsFastAcrossEveryOperation()
	{
		using var scope = new ClientScope(request =>
			request.RequestUri!.AbsolutePath.EndsWith("/characters", StringComparison.Ordinal)
				? JsonResponse(ValidCharacters)
				: JsonResponse(MalformedEnvelope));
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;
		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			_ = await scope.Client.GetAnimeDetailsAsync(1L, cancellationToken);
		}

		var requestsWhenOpen = scope.Requests;
		var anime = await scope.Client.GetAnimeDetailsAsync(1L, cancellationToken);
		var manga = await scope.Client.GetMangaDetailsAsync(2L, cancellationToken);
		var seiyu = await scope.Client.GetAnimeSeiyuAsync(3L, cancellationToken);

		await Assert.That(anime).IsEqualTo(Abstractions.Models.MediaInfo.Empty);
		await Assert.That(manga).IsEqualTo(Abstractions.Models.MediaInfo.Empty);
		await Assert.That(seiyu).IsEmpty();
		await Assert.That(scope.Requests).IsEqualTo(requestsWhenOpen);
	}

	private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
	{
		Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
	};

	private sealed class ClientScope : IDisposable
	{
		private readonly FakeHttpMessageHandler _handler;
		private readonly HttpClient _tenraiClient;
		private int _requests;

		public ClientScope(Func<HttpRequestMessage, HttpResponseMessage> respond)
		{
			this._handler = new((request, _) =>
			{
				Interlocked.Increment(ref this._requests);
				return Task.FromResult(respond(request));
			});
			this._tenraiClient = new(this._handler, disposeHandler: false)
			{
				BaseAddress = new("https://example.test/v1/"),
			};
			this.Gate = new(new ManualTimeProvider(Now), NullLogger<TenraiGate>.Instance);
			this.Client = new(NullLogger<TenraiEnrichment>.Instance, this._tenraiClient, this.Gate);
		}

		public TenraiGate Gate { get; }

		public TenraiEnrichment Client { get; }

		public int Requests => Volatile.Read(ref this._requests);

		public void Dispose()
		{
			this._tenraiClient.Dispose();
			this._handler.Dispose();
		}
	}

	private sealed class FakeHttpMessageHandler(
		Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			respond(request, cancellationToken);
	}
}
