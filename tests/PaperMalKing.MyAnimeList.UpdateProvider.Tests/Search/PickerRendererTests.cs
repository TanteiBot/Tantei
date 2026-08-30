// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus.Entities;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class PickerRendererTests
{
	private const string SearchId = "0123456789abcdef0123456789abcdef";
	private const int ExpectedRowCount = 2;
	private const int NextButtonIndex = 2;

	[Test]
	public async Task RendersTwentyFiveResultsAndCorrectPageBoundaries()
	{
		var snapshot = PickerSnapshot.ForAnime([.. Enumerable.Range(1, 26).Select(static id => new RankedSearchResult<AnimeSearchResult>(Result(id), MatchRank.Contains))]);

		var first = PickerRenderer.Render(snapshot, SearchId, page: 0);
		var last = PickerRenderer.Render(snapshot, SearchId, page: 1);
		var firstSelect = (DiscordSelectComponent)first.Rows[0][0];
		var firstButtons = first.Rows[1].Cast<DiscordButtonComponent>().ToArray();
		var lastSelect = (DiscordSelectComponent)last.Rows[0][0];
		var lastButtons = last.Rows[1].Cast<DiscordButtonComponent>().ToArray();

		await Assert.That(first.Rows.Count).IsEqualTo(ExpectedRowCount);
		await Assert.That(firstSelect.Options.Count).IsEqualTo(PickerRenderer.PageSize);
		await Assert.That(lastSelect.Options).HasSingleItem();
		await Assert.That(firstButtons[0].Disabled).IsTrue();
		await Assert.That(firstButtons[1].Label).IsEqualTo($"Page 1/{ExpectedRowCount.ToString(CultureInfo.InvariantCulture)}");
		await Assert.That(firstButtons[NextButtonIndex].Disabled).IsFalse();
		await Assert.That(lastButtons[0].Disabled).IsFalse();
		await Assert.That(lastButtons[1].Label).IsEqualTo($"Page 2/{ExpectedRowCount.ToString(CultureInfo.InvariantCulture)}");
		await Assert.That(lastButtons[NextButtonIndex].Disabled).IsTrue();
	}

	[Test]
	public async Task OptionContentAndEveryComponentValueStayWithinDiscordLimits()
	{
		var longTitle = string.Concat(Enumerable.Repeat("e\u0301", 100));
		var snapshot = PickerSnapshot.ForAnime(
		[
			new(
			new()
			{
				Id = 42U,
				PrimaryTitle = longTitle,
				MediaType = AnimeMediaType.TV,
				Status = AnimeAiringStatus.CurrentlyAiring,
				Episodes = 12U,
				StartSeason = new() { Season = AnimeSeason.Spring, Year = 2004U, },
				Mean = 8.88,
				ListUserCount = 1_400_000U,
				Genres = [],
			},
			MatchRank.Primary),
		]);

		var view = PickerRenderer.Render(snapshot, SearchId, page: 0);
		var select = (DiscordSelectComponent)view.Rows[0][0];
		var option = select.Options.Single();

		await Assert.That(option.Label.Length).IsLessThanOrEqualTo(PickerRenderer.OptionLabelLimit);
		await Assert.That(option.Label.EndsWith('…')).IsTrue();
		await Assert.That(option.Value).IsEqualTo("0");
		await Assert.That(option.Description).IsEqualTo("TV · 2004 · ★ 8.88 · 1.4M members");
		await Assert.That(option.Description!.Length).IsLessThanOrEqualTo(PickerRenderer.OptionDescriptionLimit);
		await Assert.That(select.CustomId.Length).IsLessThanOrEqualTo(PickerRenderer.CustomIdLimit);
		await Assert.That(select.Placeholder.Length).IsLessThanOrEqualTo(PickerRenderer.PlaceholderLimit);
		await Assert.That(view.Rows.Count).IsLessThanOrEqualTo(PickerRenderer.ActionRowsLimit);
		await Assert.That(view.Rows.All(static row => row.Count <= PickerRenderer.ComponentsPerRowLimit)).IsTrue();
	}

	private static AnimeSearchResult Result(int id) => new()
	{
		Id = (uint)id,
		PrimaryTitle = $"Result {id.ToString(CultureInfo.InvariantCulture)}",
		MediaType = AnimeMediaType.TV,
		Status = AnimeAiringStatus.Unknown,
		Episodes = 0U,
		ListUserCount = (uint)id,
		Genres = [],
	};
}
