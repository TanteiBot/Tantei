using System;

namespace PaperMalKing.MyAnimeList.Wrapper.Exceptions
{
	internal sealed class RssLoadException : Exception
	{
		internal RssLoadResult LoadResult { get; init; }

		internal RssLoadException(RssLoadResult loadResult, string? message = "") : base(message)
		{
			this.LoadResult = loadResult;
		}
	}
}