// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Polly.RateLimiting;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal static class TenraiClassification
{
	private const int MinRedirectStatusCode = 300;
	private const int MinSuccessStatusCode = 200;
	private static readonly TimeSpan MaximumRetryAfterDelay = TimeSpan.FromSeconds(5);

	public static TenraiDisposition Classify(HttpStatusCode statusCode) => statusCode switch
	{
		HttpStatusCode.NotFound => TenraiDisposition.NotFound,
		HttpStatusCode.RequestTimeout or
			HttpStatusCode.InternalServerError or
			HttpStatusCode.BadGateway or
			HttpStatusCode.GatewayTimeout => TenraiDisposition.Transient,
		HttpStatusCode.ServiceUnavailable => TenraiDisposition.TransientRateLimited,
		HttpStatusCode.TooManyRequests => TenraiDisposition.RateLimited,
		_ => IsSuccess((int)statusCode) ? TenraiDisposition.Success : TenraiDisposition.Terminal,
	};

	public static TenraiDisposition Classify(int statusCode) => Classify((HttpStatusCode)statusCode);

	public static TenraiFault Fault(Exception exception) => exception switch
	{
		TenraiSuppressedException => TenraiFault.Suppressed,
		RateLimiterRejectedException => TenraiFault.Queue,
		OperationCanceledException => TenraiFault.Cancelled,
		TenraiApiException => TenraiFault.Api,
		_ => TenraiFault.Transport,
	};

	public static bool OpensCircuit(TenraiDisposition disposition) => IsTransient(disposition);

	public static bool OpensCircuit(TenraiFailureKind kind) => kind is TenraiFailureKind.Schema;

	public static bool GatesCooldown(TenraiDisposition disposition) =>
		disposition is TenraiDisposition.RateLimited or TenraiDisposition.TransientRateLimited;

	public static bool ShouldRetry(TenraiDisposition disposition, TimeSpan? retryAfter) =>
		GatesCooldown(disposition) && retryAfter is { } delay ? delay <= MaximumRetryAfterDelay : IsTransient(disposition);

	public static TenraiFailureKind FailureKind(TenraiDisposition disposition) =>
		disposition is TenraiDisposition.Success ? TenraiFailureKind.Schema : TenraiFailureKind.Transport;

	private static bool IsTransient(TenraiDisposition disposition) =>
		disposition is TenraiDisposition.Transient or TenraiDisposition.TransientRateLimited;

	private static bool IsSuccess(int statusCode) => statusCode is >= MinSuccessStatusCode and < MinRedirectStatusCode;
}
