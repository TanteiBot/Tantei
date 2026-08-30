// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

internal sealed class FakeSearchMessageTarget : IPickerMessageTarget
{
	public const string PostOperation = "post";
	public const string DeleteOperation = "delete";
	public const string EditOperation = "edit";

	public List<string> Operations { get; } = [];

	public List<PickerView> Edits { get; } = [];

	public List<DiscordEmbedBuilder> Posts { get; } = [];

	public Exception? PostException { get; init; }

	public Exception? DeleteException { get; init; }

	public Task SendPublicAsync(DiscordEmbedBuilder embed)
	{
		this.Operations.Add(PostOperation);
		this.Posts.Add(embed);
		return this.PostException is null ? Task.CompletedTask : Task.FromException(this.PostException);
	}

	public Task DeleteOriginalAsync()
	{
		this.Operations.Add(DeleteOperation);
		return this.DeleteException is null ? Task.CompletedTask : Task.FromException(this.DeleteException);
	}

	public Task EditOriginalAsync(PickerView view)
	{
		this.Operations.Add(EditOperation);
		this.Edits.Add(view);
		return Task.CompletedTask;
	}
}
