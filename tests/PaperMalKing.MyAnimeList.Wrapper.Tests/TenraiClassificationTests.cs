// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;
using Polly.RateLimiting;
using Polly.Timeout;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class TenraiClassificationTests
{
	private const int NoRetryAfter = -1;
	private const int RetryAfterCeilingSeconds = 5;
	private const int BeyondRetryAfterCeilingSeconds = 6;
	private const int LastSuccessStatusCode = 299;

	[Test]
	[Arguments(HttpStatusCode.RequestTimeout)]
	[Arguments(HttpStatusCode.InternalServerError)]
	[Arguments(HttpStatusCode.BadGateway)]
	[Arguments(HttpStatusCode.ServiceUnavailable)]
	[Arguments(HttpStatusCode.GatewayTimeout)]
	public async Task TransientStatusesOpenTheCircuitAndAreRetried(HttpStatusCode statusCode)
	{
		var disposition = TenraiClassification.Classify(statusCode);

		await Assert.That(TenraiClassification.OpensCircuit(disposition)).IsTrue();
		await Assert.That(TenraiClassification.ShouldRetry(disposition, retryAfter: null)).IsTrue();
	}

	[Test]
	[Arguments(HttpStatusCode.OK)]
	[Arguments(HttpStatusCode.NotFound)]
	[Arguments(HttpStatusCode.BadRequest)]
	[Arguments(HttpStatusCode.Unauthorized)]
	[Arguments(HttpStatusCode.Forbidden)]
	[Arguments(HttpStatusCode.MethodNotAllowed)]
	[Arguments(HttpStatusCode.TooManyRequests)]
	[Arguments(HttpStatusCode.NotImplemented)]
	public async Task EveryOtherStatusNeitherOpensTheCircuitNorIsRetried(HttpStatusCode statusCode)
	{
		var disposition = TenraiClassification.Classify(statusCode);

		await Assert.That(TenraiClassification.OpensCircuit(disposition)).IsFalse();
		await Assert.That(TenraiClassification.ShouldRetry(disposition, retryAfter: null)).IsFalse();
	}

	[Test]
	[Arguments(HttpStatusCode.TooManyRequests, true)]
	[Arguments(HttpStatusCode.ServiceUnavailable, true)]
	[Arguments(HttpStatusCode.OK, false)]
	[Arguments(HttpStatusCode.NotFound, false)]
	[Arguments(HttpStatusCode.RequestTimeout, false)]
	[Arguments(HttpStatusCode.InternalServerError, false)]
	public async Task OnlyRateLimitedStatusesGateTheCooldown(HttpStatusCode statusCode, bool expected)
	{
		var gated = TenraiClassification.GatesCooldown(TenraiClassification.Classify(statusCode));

		await Assert.That(gated).IsEqualTo(expected);
	}

	[Test]
	[Arguments(HttpStatusCode.TooManyRequests, RetryAfterCeilingSeconds, true)]
	[Arguments(HttpStatusCode.TooManyRequests, BeyondRetryAfterCeilingSeconds, false)]
	[Arguments(HttpStatusCode.TooManyRequests, NoRetryAfter, false)]
	[Arguments(HttpStatusCode.ServiceUnavailable, RetryAfterCeilingSeconds, true)]
	[Arguments(HttpStatusCode.ServiceUnavailable, BeyondRetryAfterCeilingSeconds, false)]
	[Arguments(HttpStatusCode.ServiceUnavailable, NoRetryAfter, true)]
	public async Task RetryAfterDecidesRetriesOnlyForRateLimitedStatuses(
		HttpStatusCode statusCode,
		int retryAfterSeconds,
		bool expected)
	{
		var retryAfter = retryAfterSeconds is NoRetryAfter ? (TimeSpan?)null : TimeSpan.FromSeconds(retryAfterSeconds);

		var shouldRetry = TenraiClassification.ShouldRetry(TenraiClassification.Classify(statusCode), retryAfter);

		await Assert.That(shouldRetry).IsEqualTo(expected);
	}

	[Test]
	public async Task NotFoundIsClassifiedOnItsOwn()
	{
		await Assert.That(TenraiClassification.Classify(HttpStatusCode.NotFound)).IsEqualTo(TenraiDisposition.NotFound);
		await Assert.That(TenraiClassification.Classify((int)HttpStatusCode.NotFound)).IsEqualTo(TenraiDisposition.NotFound);
	}

	[Test]
	[Arguments((int)HttpStatusCode.OK, true)]
	[Arguments((int)HttpStatusCode.Created, true)]
	[Arguments((int)HttpStatusCode.NoContent, true)]
	[Arguments(LastSuccessStatusCode, true)]
	[Arguments((int)HttpStatusCode.MultipleChoices, false)]
	[Arguments((int)HttpStatusCode.BadRequest, false)]
	[Arguments((int)HttpStatusCode.NotFound, false)]
	[Arguments((int)HttpStatusCode.InternalServerError, false)]
	public async Task FailuresCarryingASuccessStatusAreSchemaFailures(int statusCode, bool expectedSchema)
	{
		var kind = TenraiClassification.FailureKind(TenraiClassification.Classify(statusCode));

		await Assert.That(kind).IsEqualTo(expectedSchema ? TenraiFailureKind.Schema : TenraiFailureKind.Transport);
	}

	[Test]
	public async Task OnlySchemaFailuresCountTowardTheCircuit()
	{
		await Assert.That(TenraiClassification.OpensCircuit(TenraiFailureKind.Schema)).IsTrue();
		await Assert.That(TenraiClassification.OpensCircuit(TenraiFailureKind.Transport)).IsFalse();
	}

	[Test]
	public async Task SuppressionIsNormalisedApartFromEveryOtherFault()
	{
		var fault = TenraiClassification.Fault(new TenraiSuppressedException(TenraiSuppression.Cooldown));

		await Assert.That(fault).IsEqualTo(TenraiFault.Suppressed);
	}

	[Test]
	public async Task QueueRejectionIsNormalisedApartFromEveryOtherFault()
	{
		var fault = TenraiClassification.Fault(new RateLimiterRejectedException("queued"));

		await Assert.That(fault).IsEqualTo(TenraiFault.Queue);
	}

	[Test]
	public async Task CancellationIsNormalisedApartFromEveryOtherFault()
	{
		await Assert.That(TenraiClassification.Fault(new OperationCanceledException())).IsEqualTo(TenraiFault.Cancelled);
		await Assert.That(TenraiClassification.Fault(new TaskCanceledException())).IsEqualTo(TenraiFault.Cancelled);
	}

	[Test]
	public async Task ApiFailuresAreNormalisedApartFromEveryOtherFault()
	{
		var fault = TenraiClassification.Fault(new TenraiApiException("failed", statusCode: 500, response: null,
			new Dictionary<string, IEnumerable<string>>(StringComparer.Ordinal), innerException: null));

		await Assert.That(fault).IsEqualTo(TenraiFault.Api);
	}

	[Test]
	public async Task EveryRemainingFaultIsTransport()
	{
		await Assert.That(TenraiClassification.Fault(new HttpRequestException("network"))).IsEqualTo(TenraiFault.Transport);
		await Assert.That(TenraiClassification.Fault(new TimeoutRejectedException("timeout"))).IsEqualTo(TenraiFault.Transport);
		await Assert.That(TenraiClassification.Fault(new TenraiTransportException(default, new HttpRequestException("network"))))
			.IsEqualTo(TenraiFault.Transport);
	}
}
