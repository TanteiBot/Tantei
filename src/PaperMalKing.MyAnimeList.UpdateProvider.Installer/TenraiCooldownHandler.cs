// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.Wrapper.Tenrai;
using Polly.RateLimiting;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Installer;

internal sealed class TenraiCooldownHandler : DelegatingHandler
{
	private readonly TenraiCooldown _cooldown;
	private readonly TenraiEnrichmentTelemetry _telemetry;

	public TenraiCooldownHandler(TenraiCooldown cooldown, TenraiEnrichmentTelemetry telemetry)
	{
		ArgumentNullException.ThrowIfNull(cooldown);
		ArgumentNullException.ThrowIfNull(telemetry);
		this._cooldown = cooldown;
		this._telemetry = telemetry;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (!this._cooldown.IsActive)
		{
			return base.SendAsync(request, cancellationToken);
		}

		this._telemetry.Current?.RecordSuppression(TenraiSuppression.Cooldown);
		return Task.FromException<HttpResponseMessage>(new RateLimiterRejectedException("Tenrai Retry-After cooldown is active"));
	}
}
