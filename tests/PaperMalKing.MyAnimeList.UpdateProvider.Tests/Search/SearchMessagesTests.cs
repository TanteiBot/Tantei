// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class SearchMessagesTests
{
	private const int QueryLimit = 100;
	private const int CombiningSequenceLength = 2;

	[Test]
	public async Task AQueryWithinTheLimitIsQuotedWhole()
	{
		var query = new string('a', QueryLimit);

		await Assert.That(SearchMessages.NoResults(query)).IsEqualTo($"No results for {query}");
	}

	[Test]
	public async Task AnOverlongQueryIsTruncatedWithoutSplittingASurrogatePair()
	{
		var query = new string('a', QueryLimit - CombiningSequenceLength) + string.Concat(Enumerable.Repeat("\U0001F600", QueryLimit));

		var message = SearchMessages.NoResults(query);

		await Assert.That(message).EndsWith("…");
		await Assert.That(message).DoesNotContain("�");
		await Assert.That(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(message))).IsEqualTo(message);
	}

	[Test]
	public async Task AnOverlongQueryIsTruncatedWithoutSplittingAGraphemeCluster()
	{
		var query = string.Concat(Enumerable.Repeat("é", QueryLimit));

		var message = SearchMessages.TypeFilterEmpty(query);
		var quoted = message["No result for ".Length..^" matches the selected type.".Length];

		await Assert.That(quoted).EndsWith("…");
		await Assert.That((quoted.Length - 1) % CombiningSequenceLength).IsZero();
	}
}
