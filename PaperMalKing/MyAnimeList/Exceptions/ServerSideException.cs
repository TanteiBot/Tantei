using System;

namespace PaperMalKing.MyAnimeList.Exceptions
{
	public sealed class ServerSideException : Exception
	{
		public readonly string Url;

		public ServerSideException(string url, string message) : base(message)
		{
			Url = url;
		}
	}
}
