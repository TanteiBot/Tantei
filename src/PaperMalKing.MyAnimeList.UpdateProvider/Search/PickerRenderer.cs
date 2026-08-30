// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

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
	private const uint Thousand = 1_000U;
	private const uint Million = 1_000_000U;

	public static PickerView Render(PickerSnapshot snapshot, string searchId, int page)
	{
		ArgumentNullException.ThrowIfNull(snapshot);
		var boundedPage = Math.Clamp(page, 0, snapshot.PageCount - 1);
		var offset = boundedPage * PageSize;
		var options = snapshot.Results
			.Skip(offset)
			.Take(PageSize)
			.Select((result, index) => CreateOption(result, offset + index))
			.ToArray();
		var select = new DiscordSelectComponent(
			CreateCustomId(searchId, PickerAction.Pick),
			Truncate(Placeholder, PlaceholderLimit),
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
		PickerView view = new("Choose a MyAnimeList result to post.", [selectRow, navigationRow]);
		Validate(view);
		return view;
	}

	private static DiscordSelectComponentOption CreateOption(PickerSearchResult result, int index)
	{
		var descriptionParts = new List<string>(4);
		if (!string.IsNullOrWhiteSpace(result.MediaType))
		{
			descriptionParts.Add(result.MediaType);
		}

		if (result.Year.HasValue)
		{
			descriptionParts.Add(result.Year.Value.ToString(CultureInfo.InvariantCulture));
		}

		if (result.Mean.HasValue)
		{
			descriptionParts.Add($"★ {result.Mean.Value.ToString("0.##", CultureInfo.InvariantCulture)}");
		}

		descriptionParts.Add($"{FormatMemberCount(result.ListUserCount)} members");
		return new(
			Truncate(result.PrimaryTitle, OptionLabelLimit),
			Truncate(index.ToString(CultureInfo.InvariantCulture), OptionValueLimit),
			Truncate(string.Join(" · ", descriptionParts), OptionDescriptionLimit));
	}

	private static string FormatMemberCount(uint memberCount) => memberCount switch
	{
		>= Million => $"{(memberCount / (double)Million).ToString("0.#", CultureInfo.InvariantCulture)}M",
		>= Thousand => $"{(memberCount / (double)Thousand).ToString("0.#", CultureInfo.InvariantCulture)}K",
		_ => memberCount.ToString(CultureInfo.InvariantCulture),
	};

	private static string CreateCustomId(string searchId, PickerAction action)
	{
		var customId = PickerCustomId.Create(searchId, action);
		if (customId.Length > CustomIdLimit)
		{
			throw new ArgumentException("The Picker custom id exceeds Discord's limit.", nameof(searchId));
		}

		return customId;
	}

	private static string Truncate(string value, int maximumLength)
	{
		if (value.Length <= maximumLength)
		{
			return value;
		}

		var cutoff = maximumLength - 1;
		if (char.IsHighSurrogate(value[cutoff - 1]))
		{
			cutoff--;
		}

		while (cutoff > 0 && CharUnicodeInfo.GetUnicodeCategory(value, cutoff) == UnicodeCategory.NonSpacingMark)
		{
			cutoff--;
		}

		return $"{value[..cutoff]}…";
	}

	private static void Validate(PickerView view)
	{
		if (view.Rows.Count > ActionRowsLimit || view.Rows.Any(static row => row.Count > ComponentsPerRowLimit))
		{
			throw new InvalidOperationException("The Picker exceeds Discord's component layout limits.");
		}
	}
}
