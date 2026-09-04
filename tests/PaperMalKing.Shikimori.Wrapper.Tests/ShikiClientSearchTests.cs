// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using System.Text;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.Shikimori.Wrapper.Tests;

public sealed class ShikiClientSearchTests
{
	[Test]
	public async Task AnimeSearchIssuesTheRequestAndDeserializesTheResult()
	{
		const ulong expectedId = 5114UL;
		const long expectedPopularity = 300L;
		const int expectedYear = 2009;
		string? capturedBody = null;
		using var handler = new FakeHttpMessageHandler(async request =>
		{
			if (request.Content is { } content)
			{
				capturedBody = await content.ReadAsStringAsync(TestContext.Current!.Execution.CancellationToken);
			}

			return JsonResponse(
				"{\"data\":{\"media\":[{\"id\":5114,\"name\":\"Fullmetal Alchemist: Brotherhood\",\"russian\":\"Стальной алхимик\"," +
				"\"english\":\"Fullmetal Alchemist: Brotherhood\",\"japanese\":\"鋼の錬金術師\",\"synonyms\":[\"Hagane no Renkinjutsushi\"]," +
				"\"kind\":\"tv\",\"score\":9.1,\"status\":\"released\",\"rating\":\"r_plus\",\"airedOn\":{\"year\":2009}," +
				"\"statusesStats\":[{\"count\":100},{\"count\":200}],\"url\":\"/animes/5114\"}]}}");
		});
		using var scope = new ClientScope(handler);

		var results = await scope.Client.SearchAnimeAsync("Fullmetal", AnimeKind.Tv, includeNsfw: false, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(capturedBody).IsNotNull();
		await Assert.That(capturedBody!).Contains("Fullmetal");
		var media = results.Single();
		await Assert.That(media.Id).IsEqualTo(expectedId);
		await Assert.That(media.Name).IsEqualTo("Fullmetal Alchemist: Brotherhood");
		await Assert.That(media.RussianName).IsEqualTo("Стальной алхимик");
		await Assert.That(media.JapaneseName).IsEqualTo("鋼の錬金術師");
		await Assert.That(media.Synonyms).Contains("Hagane no Renkinjutsushi");
		await Assert.That(media.Kind).IsEqualTo("tv");
		await Assert.That(media.Year).IsEqualTo(expectedYear);
		await Assert.That(media.Popularity).IsEqualTo(expectedPopularity);
		await Assert.That(media.Url).IsEqualTo("https://shikimori.io/animes/5114");
		await Assert.That(media.IsAdult).IsFalse();
	}

	[Test]
	public async Task AnimeSearchRecognizesHentaiRatingAsAdult()
	{
		using var handler = new FakeHttpMessageHandler(_ => Task.FromResult(JsonResponse(
			"{\"data\":{\"media\":[{\"id\":1,\"name\":\"Adult\",\"synonyms\":[],\"kind\":\"ova\",\"rating\":\"rx\"," +
			"\"statusesStats\":[],\"url\":\"https://shikimori.io/animes/1\"}]}}")));
		using var scope = new ClientScope(handler);

		var results = await scope.Client.SearchAnimeAsync("Adult", kind: null, includeNsfw: true, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(results.Single().IsAdult).IsTrue();
	}

	[Test]
	public async Task MangaSearchDeserializesTheResult()
	{
		const ulong expectedId = 2UL;
		using var handler = new FakeHttpMessageHandler(_ => Task.FromResult(JsonResponse(
			"{\"data\":{\"media\":[{\"id\":2,\"name\":\"Berserk\",\"synonyms\":[],\"kind\":\"manga\",\"score\":9.3," +
			"\"status\":\"ongoing\",\"statusesStats\":[{\"count\":500}],\"url\":\"/mangas/2\"}]}}")));
		using var scope = new ClientScope(handler);

		var results = await scope.Client.SearchMangaAsync("Berserk", MangaKind.Manga, includeNsfw: false, TestContext.Current!.Execution.CancellationToken);

		var media = results.Single();
		await Assert.That(media.Id).IsEqualTo(expectedId);
		await Assert.That(media.Name).IsEqualTo("Berserk");
		await Assert.That(media.IsAdult).IsFalse();
		await Assert.That(media.Url).IsEqualTo("https://shikimori.io/mangas/2");
	}

	private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
	{
		Content = new StringContent(json, Encoding.UTF8, "application/json"),
	};

	private sealed class ClientScope : IDisposable
	{
		private readonly FakeHttpMessageHandler _restHandler;
		private readonly HttpClient _restClient;
		private readonly HttpClient _graphQlHttpClient;
		private readonly GraphQLHttpClient _graphQlClient;

		public ClientScope(HttpMessageHandler handler)
		{
			this._restHandler = new(_ => throw new InvalidOperationException("The REST client should not be used"));
			this._restClient = new(this._restHandler, disposeHandler: false);
			this._graphQlHttpClient = new(handler, disposeHandler: false);
			var options = new GraphQLHttpClientOptions { EndPoint = new(Abstractions.Constants.GraphQlBaseUrl) };
			this._graphQlClient = new(options, new SystemTextJsonSerializer(), this._graphQlHttpClient);
			this.Client = new(this._restClient, NullLogger<ShikiClient>.Instance, this._graphQlClient);
		}

		public ShikiClient Client { get; }

		public void Dispose()
		{
			this._graphQlClient.Dispose();
			this._graphQlHttpClient.Dispose();
			this._restClient.Dispose();
			this._restHandler.Dispose();
		}
	}

	private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
	}
}
