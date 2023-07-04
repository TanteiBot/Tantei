// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace PaperMalKing.Startup;

internal sealed class SystemdTextFormatter : ITextFormatter
{
	private readonly MessageTemplateTextFormatter _messageTemplateTextFormatter;

	public SystemdTextFormatter(MessageTemplateTextFormatter messageTemplateTextFormatter)
	{
		this._messageTemplateTextFormatter = messageTemplateTextFormatter;
	}

	public void Format(LogEvent logEvent, TextWriter output)
	{
		var syslogLevel = logEvent.Level switch
		{
			LogEventLevel.Debug or LogEventLevel.Verbose => "<7>",
			LogEventLevel.Information                    => "<6>",
			LogEventLevel.Warning                        => "<4>",
			LogEventLevel.Error                          => "<3>",
			LogEventLevel.Fatal                          => "<2>",
			_                                            => throw new ArgumentOutOfRangeException(nameof(logEvent))
		};
		output.Write(syslogLevel);
		this._messageTemplateTextFormatter.Format(logEvent, output);
	}
}