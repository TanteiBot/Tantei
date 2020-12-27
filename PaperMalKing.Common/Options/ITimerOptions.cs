namespace PaperMalKing.Common.Options
{
	public interface ITimerOptions<T>
	{
		int DelayBetweenChecksInMilliseconds { get; }
	}
}