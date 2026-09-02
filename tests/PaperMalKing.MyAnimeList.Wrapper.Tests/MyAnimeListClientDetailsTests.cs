// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class MyAnimeListClientDetailsTests
{
	private const long AnimeId = 5114L;
	private const long MangaId = 2L;

	[Test]
	[Arguments("anime", AnimeId)]
	[Arguments("manga", MangaId)]
	public async Task DetailsUseTheMatchingTenraiEndpointAndMapOwnedMediaInfo(string mediaType, long mediaId)
	{
		HttpRequestMessage? capturedRequest = null;
		using var handler = new FakeHttpMessageHandler((request, _) =>
		{
			capturedRequest = request;
			return Task.FromResult(JsonResponse(
				"{\"data\":{\"themes\":[{\"name\":\"Action\"},{\"name\":\"Action\"},{\"name\":\"Adventure\"}]," +
				"\"demographics\":[{\"name\":\"Shounen\"},{\"name\":\"Kids\"}]}}"));
		});
		using var scope = new ClientScope(handler);
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		MediaInfo result = mediaType.Equals("anime", StringComparison.Ordinal)
			? await scope.Client.GetAnimeDetailsAsync(mediaId, cancellationToken)
			: await scope.Client.GetMangaDetailsAsync(mediaId, cancellationToken);

		await Assert.That(capturedRequest).IsNotNull();
		var expectedPath = $"/v1/{mediaType}/{mediaId.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
		await Assert.That(capturedRequest!.RequestUri!.AbsolutePath).IsEqualTo(expectedPath);
		await Assert.That(string.Join('|', result.Themes)).IsEqualTo("Action|Action|Adventure");
		await Assert.That(string.Join('|', result.Demographic)).IsEqualTo("Shounen|Kids");
	}

	[Test]
	public async Task MalformedThemeEntriesDoNotDiscardValidSiblingsOrDemographics()
	{
		using var handler = RespondWith(
			"{\"data\":{\"themes\":[null,{\"name\":\"Mystery\"},{\"name\":12},\"bad\",{\"name\":\" \"},{\"name\":\"Mystery\"}]," +
			"\"demographics\":[false,{\"name\":null},{\"name\":\"Seinen\"},{\"name\":\"\"},null]}}");
		using var scope = new ClientScope(handler);

		var result = await scope.Client.GetAnimeDetailsAsync(1L, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(string.Join('|', result.Themes)).IsEqualTo("Mystery|Mystery");
		await Assert.That(string.Join('|', result.Demographic)).IsEqualTo("Seinen");
	}

	[Test]
	[Arguments("{\"data\":{\"themes\":{},\"demographics\":[{\"name\":\"Josei\"}]}}", new[] { "Josei" }, new string[0])]
	[Arguments("{\"data\":{\"themes\":[{\"name\":\"School\"}],\"demographics\":false}}", new string[0], new[] { "School" })]
	public async Task MalformedDetailFieldDoesNotDiscardTheOtherField(string json, string[] demographics, string[] themes)
	{
		using var handler = RespondWith(json);
		using var scope = new ClientScope(handler);

		var result = await scope.Client.GetMangaDetailsAsync(2L, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(string.Join('|', result.Demographic)).IsEqualTo(string.Join('|', demographics));
		await Assert.That(string.Join('|', result.Themes)).IsEqualTo(string.Join('|', themes));
	}

	[Test]
	[Arguments("{\"data\":{\"themes\":null}}")]
	[Arguments("{\"data\":{}}")]
	[Arguments("not-json")]
	public async Task ResponseWithoutUsableDetailsReturnsEmpty(string json)
	{
		using var handler = RespondWith(json);
		using var scope = new ClientScope(handler);

		var result = await scope.Client.GetAnimeDetailsAsync(1L, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(result.Themes).IsEmpty();
		await Assert.That(result.Demographic).IsEmpty();
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

		var result = await scope.Client.GetMangaDetailsAsync(2L, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(result.Themes).IsEmpty();
		await Assert.That(result.Demographic).IsEmpty();
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
			_ = await scope.Client.GetAnimeDetailsAsync(1L, cancellationSource.Token);
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
			this.Client = new(NullLogger<MyAnimeListClient>.Instance, null!, null!, this._tenraiClient, null!);
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
