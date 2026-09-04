// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base.Search.Tests;

public sealed class PickerRendererTests
{
	private const string ProviderDisplayName = "MyAnimeList";
	private const int ExpectedRowCount = 2;
	private const int NextButtonIndex = 2;
	private static readonly Guid SearchId = SearchTestIdentity.Value;

	[Test]
	public async Task RendersTwentyFiveResultsAndCorrectPageBoundaries()
	{
		var snapshot = PickerSnapshot.Create([.. Enumerable.Range(1, 26).Select(Result)]);

		var first = PickerRenderer.Render(snapshot, SearchId, page: 0, ProviderDisplayName);
		var last = PickerRenderer.Render(snapshot, SearchId, page: 1, ProviderDisplayName);
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
	public async Task ProviderBuiltDescriptionAndEveryComponentValueStayWithinDiscordLimits()
	{
		var longTitle = string.Concat(Enumerable.Repeat("é", 100));
		var snapshot = PickerSnapshot.Create(
		[
			new(42U, longTitle, MatchRank.Primary, "TV · 2004 · ★ 8.88 · 1.4M members", static _ => new()),
		]);

		var view = PickerRenderer.Render(snapshot, SearchId, page: 0, ProviderDisplayName);
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

	[Test]
	public async Task AnOverlongProviderDescriptionIsTruncatedToTheDiscordLimit()
	{
		var description = string.Concat(Enumerable.Repeat("word · ", 40));
		var snapshot = PickerSnapshot.Create([new SearchResult(1U, "Title", MatchRank.Primary, description, static _ => new())]);

		var view = PickerRenderer.Render(snapshot, SearchId, page: 0, ProviderDisplayName);
		var option = ((DiscordSelectComponent)view.Rows[0][0]).Options.Single();

		await Assert.That(option.Description!.Length).IsLessThanOrEqualTo(PickerRenderer.OptionDescriptionLimit);
		await Assert.That(option.Description!.EndsWith('…')).IsTrue();
	}

	[Test]
	public async Task AnAstralBaseFollowedByCombiningMarksAtTheCutoffStaysValidUtf16()
	{
		const int paddingLength = 96;
		const int tailLength = 40;
		var title = new string('a', paddingLength) + "𝐀́́" + new string('b', tailLength);
		var snapshot = PickerSnapshot.Create([new SearchResult(1U, title, MatchRank.Primary, "TV · 1 members", static _ => new())]);

		var view = PickerRenderer.Render(snapshot, SearchId, page: 0, ProviderDisplayName);
		var label = ((DiscordSelectComponent)view.Rows[0][0]).Options.Single().Label;

		await Assert.That(label.Length).IsLessThanOrEqualTo(PickerRenderer.OptionLabelLimit);
		await Assert.That(label.IsNormalized()).IsTrue();
		await Assert.That(label.EndsWith('…')).IsTrue();
	}

	private static SearchResult Result(int id) => new(
		(uint)id,
		$"Result {id.ToString(CultureInfo.InvariantCulture)}",
		MatchRank.Contains,
		"TV",
		static _ => new());
}
