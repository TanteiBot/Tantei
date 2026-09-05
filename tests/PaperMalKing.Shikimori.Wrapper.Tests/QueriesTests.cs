// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.Shikimori.Wrapper.Tests;

public sealed class QueriesTests
{
	private const string PosterMarker = "poster {";

	private const string PersonRolesMarker = "personRoles";

	[Test]
	public async Task AnimeSearchWithoutKindOmitsTheKindArgument()
	{
		var query = Queries.GetAnimeSearchQuery(kind: null, includeNsfw: false);

		await Assert.That(query).Contains("media: animes (search: $search, limit: 50, censored: true)");
		await Assert.That(query).DoesNotContain("kind:");
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task AnimeSearchWithKindEmitsTheGraphQlKindToken()
	{
		var query = Queries.GetAnimeSearchQuery(AnimeKind.TvSpecial, includeNsfw: false);

		await Assert.That(query).Contains("kind: \"tv_special\"");
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task AnimeSearchInNsfwChannelDisablesCensorship()
	{
		var query = Queries.GetAnimeSearchQuery(kind: null, includeNsfw: true);

		await Assert.That(query).Contains("censored: false");
		await Assert.That(query).DoesNotContain("censored: true");
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task AnimeSearchSelectsIdentityAndEnrichmentFields()
	{
		var query = Queries.GetAnimeSearchQuery(kind: null, includeNsfw: false);

		foreach (var field in new[] { "id", "name", "russian", "english", "japanese", "synonyms", "kind", "score", "status", "airedOn { year }", "statusesStats { count }", "url", "rating", "studios" })
		{
			await Assert.That(query).Contains(field);
		}

		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task MangaSearchWithKindEmitsTheGraphQlKindTokenAndSelectsPublishers()
	{
		var query = Queries.GetMangaSearchQuery(MangaKind.LightNovel, includeNsfw: false);

		await Assert.That(query).Contains("media: mangas (search: $search, limit: 50, censored: true, kind: \"light_novel\")");
		await Assert.That(query).Contains("publishers");
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task MangaSearchInNsfwChannelDisablesCensorship()
	{
		var query = Queries.GetMangaSearchQuery(kind: null, includeNsfw: true);

		await Assert.That(query).Contains("censored: false");
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task AnimeQuerySkeletonIsAlwaysPresent()
	{
		var query = Queries.GetAnimeQuery(RequestOptions.None);

		await Assert.That(query).Contains("query ($ids: String)");
		await Assert.That(query).Contains("media: animes (ids: $ids)");
		await Assert.That(query).Contains(PosterMarker);
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	[Arguments(RequestOptions.Studio, "studios { name, id }")]
	[Arguments(RequestOptions.Director, "personRoles { roles_russian: rolesRu, roles: rolesEn, person { name, russian, id } }")]
	[Arguments(RequestOptions.Genres, "genres { name russian }")]
	[Arguments(RequestOptions.Description, "description")]
	public async Task AnimeQueryEmitsBlockOnlyWhenFlagPresent(RequestOptions flag, string marker)
	{
		var withFlag = Queries.GetAnimeQuery(flag);
		var withoutFlag = Queries.GetAnimeQuery(RequestOptions.None);

		await Assert.That(withFlag).Contains(marker);
		await Assert.That(withoutFlag).DoesNotContain(marker);
		GraphQlAssertions.AssertValidGraphQl(withFlag);
	}

	[Test]
	public async Task AnimeQueryPersonRolesOmitIsMangaka()
	{
		var query = Queries.GetAnimeQuery(RequestOptions.Director);

		await Assert.That(query).Contains(PersonRolesMarker);
		await Assert.That(query).DoesNotContain("isMangaka");
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task AnimeQueryWithNoFlagsOmitsEveryOptionalBlockButKeepsPoster()
	{
		var query = Queries.GetAnimeQuery(RequestOptions.None);

		foreach (var marker in new[] { "studios", "personRoles", "genres", "description" })
		{
			await Assert.That(query).DoesNotContain(marker);
		}

		await Assert.That(query).Contains(PosterMarker);
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task MangaQuerySkeletonIsAlwaysPresent()
	{
		var query = Queries.GetMangaQuery(RequestOptions.None);

		await Assert.That(query).Contains("query ($ids: String)");
		await Assert.That(query).Contains("media: mangas (ids: $ids)");
		await Assert.That(query).Contains(PosterMarker);
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	[Arguments(RequestOptions.Publisher, "publishers { name, id }")]
	[Arguments(RequestOptions.Mangaka, "person { name, russian, id, isMangaka }")]
	[Arguments(RequestOptions.Genres, "genres { name russian }")]
	[Arguments(RequestOptions.Description, "description")]
	public async Task MangaQueryEmitsBlockOnlyWhenFlagPresent(RequestOptions flag, string marker)
	{
		var withFlag = Queries.GetMangaQuery(flag);
		var withoutFlag = Queries.GetMangaQuery(RequestOptions.None);

		await Assert.That(withFlag).Contains(marker);
		await Assert.That(withoutFlag).DoesNotContain(marker);
		GraphQlAssertions.AssertValidGraphQl(withFlag);
	}

	[Test]
	public async Task MangaQueryPersonRolesIncludeIsMangaka()
	{
		var query = Queries.GetMangaQuery(RequestOptions.Mangaka);

		await Assert.That(query).Contains(PersonRolesMarker);
		await Assert.That(query).Contains("isMangaka");
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task MangaQueryWithNoFlagsOmitsEveryOptionalBlockButKeepsPoster()
	{
		var query = Queries.GetMangaQuery(RequestOptions.None);

		foreach (var marker in new[] { "publishers", "personRoles", "genres", "description" })
		{
			await Assert.That(query).DoesNotContain(marker);
		}

		await Assert.That(query).Contains(PosterMarker);
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task UserByIdQueryFormatsIdAndRoundTripsBraces()
	{
		var query = string.Format(CultureInfo.InvariantCulture, Queries.UserByIdQuery, 42u);

		await Assert.That(query).Contains("users (ids: [42], limit: 1)");
		await Assert.That(query).Contains("id");
		await Assert.That(query).Contains("nickname");
		await Assert.That(query).Contains("avatarUrl");
		await Assert.That(query).DoesNotContain("{{");
		await Assert.That(query).DoesNotContain("}}");
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task UserByNicknameQueryDeclaresNicknameVariableAndSelectsUserFields()
	{
		var query = Queries.UserByNicknameQuery.Text;

		await Assert.That(query).Contains("query ($nickname: String)");
		await Assert.That(query).Contains("users (search: $nickname, limit: 1)");
		await Assert.That(query).Contains("id");
		await Assert.That(query).Contains("nickname");
		await Assert.That(query).Contains("avatarUrl");
		GraphQlAssertions.AssertValidGraphQl(query);
	}
}
