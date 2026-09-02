// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class TenraiCooldownLoggingTests
{
	private const int CooldownEngagedEventId = 1;
	private const int TwoEngagements = 2;
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	public async Task EngagingTheCooldownEmitsOneWarningWithTheDelay()
	{
		var logger = new RecordingLogger<TenraiCooldown>();
		var cooldown = new TenraiCooldown(new ManualTimeProvider(Start), logger);
		using var response = RateLimited(TimeSpan.FromSeconds(3));

		_ = cooldown.ApplyRetryAfter(response);

		var entry = logger.Single();
		await Assert.That(entry.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(entry.EventId.Id).IsEqualTo(CooldownEngagedEventId);
		await Assert.That(Field(entry, "RetryAfter")).IsEqualTo(TimeSpan.FromSeconds(3).ToString());
	}

	[Test]
	public async Task ExtendingAnActiveCooldownDoesNotEmitAnotherWarning()
	{
		var logger = new RecordingLogger<TenraiCooldown>();
		var time = new ManualTimeProvider(Start);
		var cooldown = new TenraiCooldown(time, logger);
		using var first = RateLimited(TimeSpan.FromSeconds(5));
		using var second = RateLimited(TimeSpan.FromSeconds(5));

		_ = cooldown.ApplyRetryAfter(first);
		time.Advance(TimeSpan.FromSeconds(1));
		_ = cooldown.ApplyRetryAfter(second);

		await Assert.That(logger.Entries.Count(static entry => entry.EventId.Id == CooldownEngagedEventId)).IsEqualTo(1);
	}

	[Test]
	public async Task ReEngagingAfterExpiryEmitsAnotherWarning()
	{
		var logger = new RecordingLogger<TenraiCooldown>();
		var time = new ManualTimeProvider(Start);
		var cooldown = new TenraiCooldown(time, logger);
		using var first = RateLimited(TimeSpan.FromSeconds(2));
		using var second = RateLimited(TimeSpan.FromSeconds(2));

		_ = cooldown.ApplyRetryAfter(first);
		time.Advance(TimeSpan.FromSeconds(3));
		_ = cooldown.ApplyRetryAfter(second);

		await Assert.That(logger.Entries.Count(static entry => entry.EventId.Id == CooldownEngagedEventId)).IsEqualTo(TwoEngagements);
	}

	[Test]
	public async Task MissingRetryAfterEmitsNoWarning()
	{
		var logger = new RecordingLogger<TenraiCooldown>();
		var cooldown = new TenraiCooldown(new ManualTimeProvider(Start), logger);
		using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

		_ = cooldown.ApplyRetryAfter(response);

		await Assert.That(logger.Entries).IsEmpty();
	}

	private static HttpResponseMessage RateLimited(TimeSpan retryAfter)
	{
		var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
		response.Headers.RetryAfter = new RetryConditionHeaderValue(retryAfter);
		return response;
	}

	private static string? Field(RecordedLogEntry entry, string name) =>
		entry.State.SingleOrDefault(field => string.Equals(field.Key, name, StringComparison.Ordinal)).Value?.ToString();
}
