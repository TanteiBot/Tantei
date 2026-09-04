// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base.Search.Tests;

public sealed class SearchEmbedExtensionsTests
{
	private const int AuthorNameLimit = 256;
	private const int UrlLimit = 2048;
	private const string AvatarUrl = "https://cdn.discordapp.com/avatars/1/avatar.png";

	[Test]
	public async Task WithRequestedByAuthorSetsTheNameAndAvatar()
	{
		var builder = new DiscordEmbedBuilder().WithRequestedByAuthor("nodan", AvatarUrl);

		await Assert.That(builder.Author.Name).IsEqualTo("Requested by nodan");
		await Assert.That(builder.Author.IconUrl).IsEqualTo(AvatarUrl);
	}

	[Test]
	public async Task WithRequestedByAuthorTruncatesAnOverlongName()
	{
		var builder = new DiscordEmbedBuilder().WithRequestedByAuthor(new string('n', 300), AvatarUrl);

		await Assert.That(builder.Author.Name.Length).IsLessThanOrEqualTo(AuthorNameLimit);
		await Assert.That(builder.Author.Name.EndsWith('…')).IsTrue();
	}

	[Test]
	public async Task WithRequestedByAuthorDropsAWhitespaceAvatar()
	{
		var builder = new DiscordEmbedBuilder().WithRequestedByAuthor("nodan", "   ");

		await Assert.That(builder.Author.IconUrl).IsNull();
	}

	[Test]
	public async Task WithRequestedByAuthorDropsAnOverlongAvatarUrl()
	{
		var builder = new DiscordEmbedBuilder().WithRequestedByAuthor("nodan", new string('a', UrlLimit + 1));

		await Assert.That(builder.Author.IconUrl).IsNull();
	}

	[Test]
	public async Task WithRequestedByAuthorRejectsAWhitespaceRequesterName()
	{
		await Assert.That(static () => new DiscordEmbedBuilder().WithRequestedByAuthor(" ", AvatarUrl)).Throws<ArgumentException>();
	}
}
