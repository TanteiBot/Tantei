// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class TenraiAttempt
{
	private const long NoRetryAfter = -1L;
	private const string RetryAfterTicksHeaderName = "X-Tenrai-Attempt-Retry-After-Ticks";
	private const string RetryCountHeaderName = "X-Tenrai-Attempt-Retries";
	private static readonly HttpRequestOptionsKey<TenraiAttempt> OptionsKey = new("Tenrai.Attempt");
	private long _retryAfterTicks = NoRetryAfter;
	private int _retryCount;

	public TenraiAttemptFacts Facts => new(Volatile.Read(ref this._retryCount), ToRetryAfter(Volatile.Read(ref this._retryAfterTicks)));

	public static TenraiAttempt Attach(HttpRequestMessage request)
	{
		ArgumentNullException.ThrowIfNull(request);
		var attempt = new TenraiAttempt();
		request.Options.Set(OptionsKey, attempt);
		return attempt;
	}

	public static TenraiAttempt? From(HttpRequestMessage? request) =>
		request is not null && request.Options.TryGetValue(OptionsKey, out var attempt) ? attempt : null;

	public static TenraiAttemptFacts Read(IReadOnlyDictionary<string, IEnumerable<string>> headers)
	{
		ArgumentNullException.ThrowIfNull(headers);
		return new(checked((int)ReadNumber(headers, RetryCountHeaderName, defaultValue: 0L)),
			ToRetryAfter(ReadNumber(headers, RetryAfterTicksHeaderName, NoRetryAfter)));
	}

	public void RecordRetry() => Interlocked.Increment(ref this._retryCount);

	public void RecordRetryAfter(TimeSpan? retryAfter)
	{
		if (retryAfter is { } value)
		{
			Volatile.Write(ref this._retryAfterTicks, value.Ticks);
		}
	}

	public void WriteTo(HttpResponseMessage response)
	{
		ArgumentNullException.ThrowIfNull(response);
		var facts = this.Facts;
		_ = response.Headers.TryAddWithoutValidation(RetryCountHeaderName, facts.RetryCount.ToString(CultureInfo.InvariantCulture));
		if (facts.RetryAfter is { } retryAfter)
		{
			_ = response.Headers.TryAddWithoutValidation(RetryAfterTicksHeaderName, retryAfter.Ticks.ToString(CultureInfo.InvariantCulture));
		}
	}

	private static TimeSpan? ToRetryAfter(long ticks) => ticks < 0L ? null : TimeSpan.FromTicks(ticks);

	private static long ReadNumber(IReadOnlyDictionary<string, IEnumerable<string>> headers, string name, long defaultValue) =>
		headers.TryGetValue(name, out var values) && values.FirstOrDefault() is { } value &&
		long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
			? parsed
			: defaultValue;
}
