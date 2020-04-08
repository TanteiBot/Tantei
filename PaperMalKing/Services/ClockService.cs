/*
https://www.nimaara.com/high-resolution-clock-in-net/
*/

using System;
using System.Diagnostics;

namespace PaperMalKing.Services
{
	public sealed class ClockService
	{
		private readonly long _maxIdleTime;
		private const long TicksMultiplier = 1000 * TimeSpan.TicksPerMillisecond;

		private DateTime _startTime;

		private double _startTimestamp;

		private readonly object _lockObject;

		public ClockService()
		{
			this._maxIdleTime = TimeSpan.FromSeconds(10).Ticks;
			this._startTime = DateTime.UtcNow;
			this._startTimestamp = Stopwatch.GetTimestamp();
			this._lockObject = new object();
		}

		public DateTime UtcNow
		{
			get
			{
				lock (this._lockObject)
				{
					double endTimestamp = Stopwatch.GetTimestamp();

					var durationInTicks = (endTimestamp - this._startTimestamp) / Stopwatch.Frequency * TicksMultiplier;
					if (durationInTicks >= this._maxIdleTime)
					{
						this._startTimestamp = Stopwatch.GetTimestamp();
						this._startTime = DateTime.UtcNow;
						return this._startTime;
					}

					return this._startTime.AddTicks((long) durationInTicks);
				}
			}
		}

		public DateTime Now => this.UtcNow.ToLocalTime();
	}
}