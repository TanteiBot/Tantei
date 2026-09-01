// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.UpdateProvider.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class MatchKeyTests
{
	[Test]
	[Arguments("Fate/Zero", "FATEZERO")]
	[Arguments("Fate Zero", "FATEZERO")]
	[Arguments("fatezero", "FATEZERO")]
	[Arguments("Pokémon", "POKEMON")]
	[Arguments("Crème Brûlée", "CREMEBRULEE")]
	[Arguments("Ｆａｔｅ１２３", "FATE123")]
	[Arguments("は\u3099", "ば")]
	[Arguments("\U00010428a1", "\U00010400A1")]
	public async Task CreateNormalizesTitlesWithoutLosingUnicodeLetters(string title, string expected)
	{
		var key = MatchKey.Create(title);

		await Assert.That(key.Value).IsEqualTo(expected);
	}

	[Test]
	public async Task CreateRemovesPunctuationSymbolsAndWhitespace()
	{
		var key = MatchKey.Create(" K-On! ☆ ");

		await Assert.That(key.Value).IsEqualTo("KON");
	}
}
