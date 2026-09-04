// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;

namespace PaperMalKing.UpdatesProviders.Base.Search.Tests;

public sealed class PickerCustomIdTests
{
	private static readonly Guid SearchId = SearchTestIdentity.Value;

	[Test]
	public async Task GuidIdentityRoundTripsThroughTheDiscordCustomIdBoundary()
	{
		var value = PickerCustomId.Create(SearchId, PickerAction.Next);

		var valid = PickerCustomId.TryParse(value, out var parsed);

		await Assert.That(value).IsEqualTo(PickerCustomId.Prefix + SearchId.ToString("N", CultureInfo.InvariantCulture) + ":next");
		await Assert.That(valid).IsTrue();
		await Assert.That(parsed.SearchId).IsEqualTo(SearchId);
		await Assert.That(parsed.Action).IsEqualTo(PickerAction.Next);
	}

	[Test]
	public async Task ParsingRejectsMalformedNonNAndInvalidGuidText()
	{
		var malformed = PickerCustomId.TryParse("search:not-a-picker", out _);
		var nonN = PickerCustomId.TryParse("search:01234567-89ab-cdef-0123-456789abcdef:next", out _);
		var invalidGuid = PickerCustomId.TryParse("search:0123456789abcdef0123456789abcdeg:next", out _);
		var wrongPrefix = PickerCustomId.TryParse("other:0123456789abcdef0123456789abcdef:next", out _);
		var invalidAction = PickerCustomId.TryParse("search:0123456789abcdef0123456789abcdef:unknown", out _);
		var extraPart = PickerCustomId.TryParse("search:0123456789abcdef0123456789abcdef:next:1", out _);

		await Assert.That(malformed).IsFalse();
		await Assert.That(nonN).IsFalse();
		await Assert.That(invalidGuid).IsFalse();
		await Assert.That(wrongPrefix).IsFalse();
		await Assert.That(invalidAction).IsFalse();
		await Assert.That(extraPart).IsFalse();
	}
}
