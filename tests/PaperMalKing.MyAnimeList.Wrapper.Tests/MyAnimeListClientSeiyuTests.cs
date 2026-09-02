// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class MyAnimeListClientSeiyuTests
{
	[Test]
	public async Task SeiyuUsesTenraiCharactersAndPreservesAcceptedActorsInReceivedOrder()
	{
		HttpRequestMessage? capturedRequest = null;
		using var handler = new FakeHttpMessageHandler((request, _) =>
		{
			capturedRequest = request;
			return Task.FromResult(JsonResponse(
				"{\"data\":[{\"voice_actors\":[" +
				"{\"language\":\"Japanese\",\"person\":{\"name\":\"Megumi Hayashibara\",\"url\":\"http://myanimelist.net/people/14\"}}," +
				"{\"language\":\"English\",\"person\":{\"name\":\"English Actor\",\"url\":\"https://myanimelist.net/people/15\"}}," +
				"{\"language\":\"japanese\",\"person\":{\"name\":\"Lowercase Actor\",\"url\":\"https://myanimelist.net/people/16\"}}]}," +
				"{\"voice_actors\":[" +
				"{\"language\":\"Japanese\",\"person\":{\"name\":\"Rie Kugimiya\",\"url\":\"https://www.myanimelist.net/people/8\"}}," +
				"{\"language\":\"Japanese\",\"person\":{\"name\":\"Megumi Hayashibara\",\"url\":\"http://myanimelist.net/people/14\"}}]}]}"));
		});
		using var scope = new ClientScope(handler);

		var result = await scope.Client.GetAnimeSeiyuAsync(5114L, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(capturedRequest).IsNotNull();
		await Assert.That(capturedRequest!.RequestUri!.AbsolutePath).IsEqualTo("/v1/anime/5114/characters");
		await Assert.That(string.Join('|', result.Select(static actor => $"{actor.Name}<{actor.Url}>"))).IsEqualTo(
			"Megumi Hayashibara<http://myanimelist.net/people/14>|" +
			"Rie Kugimiya<https://www.myanimelist.net/people/8>|" +
			"Megumi Hayashibara<http://myanimelist.net/people/14>");
	}

	[Test]
	public async Task InvalidActorsAndLinksDoNotDiscardValidSiblings()
	{
		using var handler = RespondWith(
			"{\"data\":[null,false,{\"voice_actors\":{}},{\"voice_actors\":[null,false," +
			"{\"language\":12,\"person\":{\"name\":\"Wrong Language Type\",\"url\":\"https://myanimelist.net/people/1\"}}," +
			"{\"language\":\"Japanese\",\"person\":null}," +
			"{\"language\":\"Japanese\",\"person\":false}," +
			"{\"language\":\"Japanese\",\"person\":{\"name\":\" \",\"url\":\"https://myanimelist.net/people/2\"}}," +
			"{\"language\":\"Japanese\",\"person\":{\"name\":\"Relative\",\"url\":\"/people/3\"}}," +
			"{\"language\":\"Japanese\",\"person\":{\"name\":\"FTP\",\"url\":\"ftp://myanimelist.net/people/4\"}}," +
			"{\"language\":\"Japanese\",\"person\":{\"name\":\"Other Host\",\"url\":\"https://example.com/people/5\"}}," +
			"{\"language\":\"Japanese\",\"person\":{\"name\":\"Spoofed Host\",\"url\":\"https://myanimelist.net.example.com/people/6\"}}," +
			"{\"language\":\"Japanese\",\"person\":{\"name\":12,\"url\":\"https://myanimelist.net/people/7\"}}," +
			"{\"language\":\"Japanese\",\"person\":{\"name\":\"Valid Sibling\",\"url\":\"https://seiyuu.myanimelist.net/people/8\"}}]}]}");
		using var scope = new ClientScope(handler);

		var result = await scope.Client.GetAnimeSeiyuAsync(1L, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(result).HasSingleItem();
		await Assert.That(result[0].Name).IsEqualTo("Valid Sibling");
		await Assert.That(result[0].Url).IsEqualTo("https://seiyuu.myanimelist.net/people/8");
	}

	[Test]
	[Arguments("{\"data\":null}")]
	[Arguments("{\"data\":{}}")]
	[Arguments("not-json")]
	public async Task UnusableResponseReturnsEmpty(string json)
	{
		using var handler = RespondWith(json);
		using var scope = new ClientScope(handler);

		var result = await scope.Client.GetAnimeSeiyuAsync(1L, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(result).IsEmpty();
	}

	[Test]
	[Arguments(HttpStatusCode.NotFound)]
	[Arguments(HttpStatusCode.BadRequest)]
	[Arguments(HttpStatusCode.ServiceUnavailable)]
	public async Task ProviderFailureReturnsEmpty(HttpStatusCode statusCode)
	{
		using var handler = new FakeHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage(statusCode)
		{
			Content = new StringContent("{\"status\":503,\"message\":\"unavailable\"}"),
		}));
		using var scope = new ClientScope(handler);

		var result = await scope.Client.GetAnimeSeiyuAsync(1L, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(result).IsEmpty();
	}

	[Test]
	public async Task SeiyuFailureDoesNotAffectDetailsEnrichment()
	{
		using var handler = new FakeHttpMessageHandler((request, _) => Task.FromResult(
			request.RequestUri!.AbsolutePath.EndsWith("/characters", StringComparison.Ordinal)
				? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
				{
					Content = new StringContent("{\"status\":503,\"message\":\"unavailable\"}"),
				}
				: JsonResponse("{\"data\":{\"themes\":[{\"name\":\"Action\"}],\"demographics\":[]}}")));
		using var scope = new ClientScope(handler);
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		var seiyu = await scope.Client.GetAnimeSeiyuAsync(1L, cancellationToken);
		var details = await scope.Client.GetAnimeDetailsAsync(1L, cancellationToken);

		await Assert.That(seiyu).IsEmpty();
		await Assert.That(details.Themes).HasSingleItem();
		await Assert.That(details.Themes[0]).IsEqualTo("Action");
	}

	[Test]
	public async Task CallerCancellationPropagatesUnchanged()
	{
		using var cancellationSource = new CancellationTokenSource();
		cancellationSource.Cancel();
		var expected = new OperationCanceledException(cancellationSource.Token);
		using var handler = new FakeHttpMessageHandler((_, _) => Task.FromException<HttpResponseMessage>(expected));
		using var scope = new ClientScope(handler);
		OperationCanceledException? actual = null;

		try
		{
			_ = await scope.Client.GetAnimeSeiyuAsync(1L, cancellationSource.Token);
		}
		catch (OperationCanceledException exception)
		{
			actual = exception;
		}

		await Assert.That(actual?.CancellationToken).IsEqualTo(cancellationSource.Token);
		await Assert.That(ReferenceEquals(actual, expected)).IsTrue();
	}

	private static FakeHttpMessageHandler RespondWith(string json) => new((_, _) => Task.FromResult(JsonResponse(json)));

	private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
	{
		Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
	};

	private sealed class ClientScope : IDisposable
	{
		private readonly HttpClient _tenraiClient;

		public ClientScope(HttpMessageHandler tenraiHandler)
		{
			this._tenraiClient = new(tenraiHandler, disposeHandler: false)
			{
				BaseAddress = new("https://example.test/v1/"),
			};
			this.Client = new(
				NullLogger<MyAnimeListClient>.Instance,
				null!,
				null!,
				this._tenraiClient,
				new TenraiCircuit(TimeProvider.System, NullLogger<TenraiCircuit>.Instance),
				new TenraiEnrichmentTelemetry());
		}

		public MyAnimeListClient Client { get; }

		public void Dispose() => this._tenraiClient.Dispose();
	}

	private sealed class FakeHttpMessageHandler(
		Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			respond(request, cancellationToken);
	}
}
