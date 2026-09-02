// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class TenraiAttemptHandler : DelegatingHandler
{
	[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Every failure is rethrown carrying the attempt")]
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var attempt = TenraiAttempt.Attach(request);
		try
		{
			var response = await base.SendAsync(request, cancellationToken);
			attempt.WriteTo(response);
			return response;
		}
		catch (Exception exception)
		{
			switch (TenraiClassification.Fault(exception))
			{
				case TenraiFault.Suppressed:
					throw;
				case TenraiFault.Queue:
					throw new TenraiSuppressedException(TenraiSuppression.Queue);
				case TenraiFault.Cancelled when cancellationToken.IsCancellationRequested:
					throw;
				default:
					throw new TenraiTransportException(attempt.Facts, exception);
			}
		}
	}
}
