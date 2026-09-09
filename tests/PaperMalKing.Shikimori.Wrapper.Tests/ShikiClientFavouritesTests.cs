// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using System.Text;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions;

namespace PaperMalKing.Shikimori.Wrapper.Tests;

public sealed class ShikiClientFavouritesTests
{
	private const int ExpectedPostsForOneOverflowingKind = 2;

	private const string EmptyPayload = "{\"data\":{\"animes\":[],\"mangas\":[],\"characters\":[],\"people\":[]}}";

	[Test]
	public async Task AWholeTickOfFavouritesRidesOneGraphQlPost()
	{
		var postCount = 0;
		var bodies = new List<string>();
		using var handler = new FakeHttpMessageHandler(async (request, _) =>
		{
			postCount++;
			if (request.Content is { } content)
			{
				bodies.Add(await content.ReadAsStringAsync(TestContext.Current!.Execution.CancellationToken));
			}

			return JsonResponse(EmptyPayload);
		});
		using var scope = new ClientScope(handler);

		await scope.Client.GetFavouritesInfoAsync(new()
		{
			AnimeIds = Ids(Queries.FavouritesBatchLimit, offset: 0),
			MangaIds = Ids(Queries.FavouritesBatchLimit, offset: 1000),
			CharacterIds = Ids(Queries.FavouritesBatchLimit, offset: 2000),
			PersonIds = Ids(Queries.FavouritesBatchLimit, offset: 3000),
		}, RequestOptions.None, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(postCount).IsEqualTo(1);
		await Assert.That(bodies).Count().IsEqualTo(1);
		foreach (var alias in new[] { "animes (ids:", "mangas (ids:", "characters (ids:", "people (ids:" })
		{
			await Assert.That(bodies[0]).Contains(alias);
		}
	}

	[Test]
	public async Task MoreFavouritesOfOneKindThanTheBatchLimitAreSplitAcrossPosts()
	{
		var postCount = 0;
		using var handler = new FakeHttpMessageHandler((_, _) =>
		{
			postCount++;
			return Task.FromResult(JsonResponse(EmptyPayload));
		});
		using var scope = new ClientScope(handler);

		await scope.Client.GetFavouritesInfoAsync(new() { AnimeIds = Ids(Queries.FavouritesBatchLimit + 1, offset: 0), }, RequestOptions.None,
			TestContext.Current!.Execution.CancellationToken);

		await Assert.That(postCount).IsEqualTo(ExpectedPostsForOneOverflowingKind);
	}

	[Test]
	public async Task NoFavouritesMakeNoRequestAtAll()
	{
		using var handler = new FakeHttpMessageHandler((_, _) => throw new InvalidOperationException("No request should be made"));
		using var scope = new ClientScope(handler);

		var info = await scope.Client.GetFavouritesInfoAsync(new(), RequestOptions.None, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(info.Animes).IsEmpty();
	}

	[Test]
	public async Task PersonDetailsTolerateNullCollectionsInThePayload()
	{
		const string payload =
			"""
			{"works":null,"roles":[{"animes":[{"id":1,"name":"Voiced Show","score":"7.0","url":"/animes/1"}],"mangas":null}]}
			""";
		using var restHandler = new FakeHttpMessageHandler(_ => JsonResponse(payload));
		using var graphQlHandler = new FakeHttpMessageHandler(_ => throw new InvalidOperationException("No GraphQL request expected"));
		using var scope = new ClientScope(graphQlHandler, restHandler);

		var details = await scope.Client.GetPersonDetailsAsync(1, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(details!.Works).IsNull();
		await Assert.That(details.Roles![0].Mangas).IsNull();
		await Assert.That(details.Roles[0].Media?.Name).IsEqualTo("Voiced Show");
	}

	[Test]
	public async Task CharacterDetailsTolerateNullCollectionsInThePayload()
	{
		const string payload = """{"animes":[{"id":1,"name":"Shown In","score":"7.0","url":"/animes/1"}],"mangas":null}""";
		using var restHandler = new FakeHttpMessageHandler(_ => JsonResponse(payload));
		using var graphQlHandler = new FakeHttpMessageHandler(_ => throw new InvalidOperationException("No GraphQL request expected"));
		using var scope = new ClientScope(graphQlHandler, restHandler);

		var details = await scope.Client.GetCharacterDetailsAsync(1, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(details!.Mangas).IsNull();
		await Assert.That(details.Animes).Count().IsEqualTo(1);
	}

	private static uint[] Ids(int count, uint offset) => [.. Enumerable.Range(1, count).Select(i => offset + (uint)i)];

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

		public ClientScope(HttpMessageHandler handler, HttpMessageHandler? restHandler = null)
		{
			this._restHandler = new(_ => throw new InvalidOperationException("The REST client should not be used"));
			this._restClient = new(restHandler ?? this._restHandler, disposeHandler: false)
			{
				BaseAddress = new(Abstractions.Constants.BaseUrl),
			};
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
}
