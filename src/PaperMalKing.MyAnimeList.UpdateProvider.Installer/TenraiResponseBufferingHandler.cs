// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Installer;

internal sealed class TenraiResponseBufferingHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var response = await base.SendAsync(request, cancellationToken);
		var buffered = false;
		try
		{
			if (response.Content is not null)
			{
				await response.Content.LoadIntoBufferAsync(cancellationToken);
			}

			buffered = true;
			return response;
		}
		finally
		{
			if (!buffered)
			{
				response.Dispose();
			}
		}
	}
}
