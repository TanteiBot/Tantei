using System;
using DSharpPlus;
using DSharpPlus.EventArgs;
using PaperMalKing.Utilities;

namespace PaperMalKing.Services
{
	public sealed class LogService
	{
		private readonly object _logLock;

		private readonly ClockService _clock;

		public LogService(ClockService clock)
		{
			this._logLock = new object();
			this._clock = clock;
		}

		public void Log(LogLevel level, string application, string message, DateTime timestamp,
						Exception ex = null)
		{
			lock (this._logLock)
			{
				if (level == LogLevel.Debug)
					Console.ForegroundColor = ConsoleColor.DarkGreen;
				else if (level == LogLevel.Info)
					Console.ForegroundColor = ConsoleColor.White;
				else if (level == LogLevel.Warning)
					Console.ForegroundColor = ConsoleColor.DarkYellow;
				else if (level == LogLevel.Error)
					Console.ForegroundColor = ConsoleColor.DarkRed;
				else if (level == LogLevel.Critical)
				{
					Console.BackgroundColor = ConsoleColor.DarkRed;
					Console.ForegroundColor = ConsoleColor.Black;
				}

				string timestampFormat;
#if DEBUG
				timestampFormat = "dd.MM.yy HH\\:mm\\:ss.fff";
#else
				timestampFormat = "dd.MM.yy HH\\:mm\\:ss";
#endif

				Console.Write(
					$"[{timestamp.ToString(timestampFormat)}] [{application.ToFixedWidth(14)}] [{level.ToShortName()}]");
				Console.ResetColor();
				Console.WriteLine($" {message}{(ex != null ? $"\n{ex}" : "")}");
			}
		}


		public void Log(LogLevel level, string application, string message, Exception ex = null) =>
			this.Log(level, application, message, this._clock.Now, ex);

		public void LogEventHandler(object sender, DebugLogMessageEventArgs e) =>
			this.Log(e.Level, e.Application, e.Message, e.Timestamp, e.Exception);
	}
}