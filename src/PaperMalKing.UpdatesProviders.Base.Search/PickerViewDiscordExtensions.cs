// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal static class PickerViewDiscordExtensions
{
	public static DiscordWebhookBuilder ToWebhookBuilder(this PickerView view)
	{
		var builder = new DiscordWebhookBuilder().WithContent(view.Content);
		foreach (var row in view.Rows)
		{
			builder.AddComponents(row);
		}

		return builder;
	}

	public static DiscordInteractionResponseBuilder ToInteractionResponseBuilder(this PickerView view)
	{
		var builder = new DiscordInteractionResponseBuilder().WithContent(view.Content);
		foreach (var row in view.Rows)
		{
			builder.AddComponents(row);
		}

		return builder;
	}
}
