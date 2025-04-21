// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Startup.Options;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Startup.Services.ExecuteOnStartup;

internal sealed class WarnOnSeqOnStartupService(IServiceProvider serviceProvider, ILogger<WarnOnSeqOnStartupService> _logger) : IExecuteOnStartupService
{
	[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "It's supposed to log once only")]
	[SuppressMessage("Performance", "EA0000:Use source generated logging methods for improved performance", Justification = "It's supposed to log once only")]
	public Task ExecuteAsync(CancellationToken cancellationToken = default)
	{
		var options = serviceProvider.GetRequiredService<IOptions<SeqOptions>>();

		if (options.Value.IsEnabled)
		{
			_logger.LogWarning("Seq is deprecated. Use OTLP configuration instead");
		}

		return Task.CompletedTask;
	}
}