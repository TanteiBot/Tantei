namespace PaperMalKing.MyAnimeList
{
	public enum RssLoadResult
	{
		EmptyList = 0,

		Ok = 200,

		Forbidden = 403,

		NotFound = 404,

		BadGateway = 502,

		ServiceUnavailable = 503,

		GatewayTimeout = 504,

		Unknown = 999
	}
}