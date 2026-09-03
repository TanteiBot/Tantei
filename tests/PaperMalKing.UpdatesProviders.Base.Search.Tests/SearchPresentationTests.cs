// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.UpdatesProviders.Base.Search.Tests;

public sealed class SearchPresentationTests
{
	private const uint Zero = 0U;
	private const uint BelowThousand = 999U;
	private const uint OneThousand = 1_000U;
	private const uint Thousands = 1_200U;
	private const uint OneMillion = 1_000_000U;
	private const uint Millions = 1_400_000U;

	[Test]
	[Arguments(Zero, "0")]
	[Arguments(BelowThousand, "999")]
	[Arguments(OneThousand, "1K")]
	[Arguments(Thousands, "1.2K")]
	[Arguments(OneMillion, "1M")]
	[Arguments(Millions, "1.4M")]
	public async Task AbbreviateCountThousandsBecomeKAndMillionsBecomeM(uint count, string expected)
	{
		await Assert.That(SearchPresentation.AbbreviateCount(count)).IsEqualTo(expected);
	}

	[Test]
	public async Task ComposeOptionDescriptionJoinsPartsWithTheSeparator()
	{
		await Assert.That(SearchPresentation.ComposeOptionDescription(["TV", "2004", "★ 8.5", "1.4M members"]))
			.IsEqualTo("TV · 2004 · ★ 8.5 · 1.4M members");
	}

	[Test]
	public async Task ComposeOptionDescriptionSkipsNullAndWhitespaceParts()
	{
		await Assert.That(SearchPresentation.ComposeOptionDescription(["TV", null, "  ", "1.4M"])).IsEqualTo("TV · 1.4M");
	}

	[Test]
	public async Task ComposeOptionDescriptionOfNoPresentPartsIsEmpty()
	{
		await Assert.That(SearchPresentation.ComposeOptionDescription([null, "", "   "])).IsEqualTo(string.Empty);
	}
}
