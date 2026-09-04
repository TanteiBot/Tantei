// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal static class PickerRenderer
{
	public const int PageSize = 25;
	public const int CustomIdLimit = 100;
	public const int OptionDescriptionLimit = 100;
	public const int OptionLabelLimit = 100;
	public const int OptionValueLimit = 100;
	public const int PlaceholderLimit = 150;
	public const int ComponentsPerRowLimit = 5;
	public const int ActionRowsLimit = 5;
	private const string Placeholder = "Pick a title to post";

	public static PickerView Render(PickerSnapshot snapshot, Guid searchId, int page, string providerDisplayName)
	{
		ArgumentNullException.ThrowIfNull(snapshot);
		ArgumentException.ThrowIfNullOrWhiteSpace(providerDisplayName);
		var boundedPage = Math.Clamp(page, 0, snapshot.PageCount - 1);
		var offset = boundedPage * PageSize;
		var options = snapshot.Results
			.Skip(offset)
			.Take(PageSize)
			.Select((result, index) => CreateOption(result, offset + index))
			.ToArray();
		var select = new DiscordSelectComponent(
			CreateCustomId(searchId, PickerAction.Pick),
			SearchText.Truncate(Placeholder, PlaceholderLimit),
			options);
		IReadOnlyList<DiscordComponent> selectRow = [select];
		IReadOnlyList<DiscordComponent> navigationRow =
		[
			new DiscordButtonComponent(
				ButtonStyle.Secondary,
				CreateCustomId(searchId, PickerAction.Previous),
				"◀ Prev",
				disabled: boundedPage == 0),
			new DiscordButtonComponent(
				ButtonStyle.Secondary,
				CreateCustomId(searchId, PickerAction.Page),
				$"Page {(boundedPage + 1).ToString(CultureInfo.InvariantCulture)}/{snapshot.PageCount.ToString(CultureInfo.InvariantCulture)}",
				disabled: true),
			new DiscordButtonComponent(
				ButtonStyle.Secondary,
				CreateCustomId(searchId, PickerAction.Next),
				"Next ▶",
				disabled: boundedPage == snapshot.PageCount - 1),
			new DiscordButtonComponent(
				ButtonStyle.Danger,
				CreateCustomId(searchId, PickerAction.Cancel),
				"Cancel"),
		];
		PickerView view = new($"Choose a {providerDisplayName} result to post.", [selectRow, navigationRow]);
		Validate(view);
		return view;
	}

	private static DiscordSelectComponentOption CreateOption(SearchResult result, int index) => new(
		SearchText.Truncate(result.PrimaryTitle, OptionLabelLimit),
		SearchText.Truncate(index.ToString(CultureInfo.InvariantCulture), OptionValueLimit),
		SearchText.Truncate(result.OptionDescription, OptionDescriptionLimit));

	private static string CreateCustomId(Guid searchId, PickerAction action)
	{
		var customId = PickerCustomId.Create(searchId, action);
		if (customId.Length > CustomIdLimit)
		{
			throw new ArgumentException("The Picker custom id exceeds Discord's limit.", nameof(searchId));
		}

		return customId;
	}

	private static void Validate(PickerView view)
	{
		if (view.Rows.Count > ActionRowsLimit || view.Rows.Any(static row => row.Count > ComponentsPerRowLimit))
		{
			throw new InvalidOperationException("The Picker exceeds Discord's component layout limits.");
		}
	}
}
