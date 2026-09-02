// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Polly.RateLimiting;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Installer;

internal sealed class TenraiCooldownHandler : DelegatingHandler
{
	private readonly TenraiCooldown _cooldown;

	public TenraiCooldownHandler(TenraiCooldown cooldown)
	{
		ArgumentNullException.ThrowIfNull(cooldown);
		this._cooldown = cooldown;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
		this._cooldown.IsActive
			? Task.FromException<HttpResponseMessage>(new RateLimiterRejectedException("Tenrai Retry-After cooldown is active"))
			: base.SendAsync(request, cancellationToken);
}
