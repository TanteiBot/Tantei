// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.UpdateProvider.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class PickerCustomIdTests
{
	[Test]
	public async Task ParsesOnlyWellFormedMalSearchComponentIds()
	{
		var valid = PickerCustomId.TryParse("mal:search:0123456789abcdef0123456789abcdef:next", out var parsed);
		var wrongPrefix = PickerCustomId.TryParse("other:search:0123456789abcdef0123456789abcdef:next", out _);
		var invalidSearchId = PickerCustomId.TryParse("mal:search:not-opaque:next", out _);
		var invalidAction = PickerCustomId.TryParse("mal:search:0123456789abcdef0123456789abcdef:unknown", out _);
		var extraPart = PickerCustomId.TryParse("mal:search:0123456789abcdef0123456789abcdef:next:1", out _);

		await Assert.That(valid).IsTrue();
		await Assert.That(parsed.SearchId).IsEqualTo("0123456789abcdef0123456789abcdef");
		await Assert.That(parsed.Action).IsEqualTo(PickerAction.Next);
		await Assert.That(wrongPrefix).IsFalse();
		await Assert.That(invalidSearchId).IsFalse();
		await Assert.That(invalidAction).IsFalse();
		await Assert.That(extraPart).IsFalse();
	}
}
