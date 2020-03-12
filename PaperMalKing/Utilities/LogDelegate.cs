using System;
using DSharpPlus;

namespace PaperMalKing.Utilities
{
	public delegate void LogDelegate(LogLevel logLevel, string applicationName, string message,
		DateTime timestamp, Exception ex = null);
}