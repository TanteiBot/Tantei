namespace PaperMalKing.Jikan.Data
{
	public enum StatusType
	{
		All = 0,

		/// <summary>
		/// Reading / Watching
		/// </summary>
		InProgress = 1,

		Completed = 2,

		OnHold = 3,

		Dropped = 4,

		/// <summary>
		/// Plan to read / Plan to watch
		/// </summary>
		PlanToCheck = 6,

		Undefined = 7
	}

	public static class StatusExtension
	{
		public static string GetMangaEndpoint(this StatusType status)
		{
			if (status == StatusType.All)
				return "all";
			if (status == StatusType.InProgress)
				return "reading";
			if (status == StatusType.Completed)
				return "completed";
			if (status == StatusType.OnHold)
				return "onhold";
			if (status == StatusType.Dropped)
				return "dropped";
			if (status == StatusType.PlanToCheck)
				return "plantoread";
			return "";
		}

		public static string GetAnimeEndpoint(this StatusType status)
		{
			if (status == StatusType.All)
				return "all";
			if (status == StatusType.InProgress)
				return "watching";
			if (status == StatusType.Completed)
				return "completed";
			if (status == StatusType.OnHold)
				return "onhold";
			if (status == StatusType.Dropped)
				return "dropped";
			if (status == StatusType.PlanToCheck)
				return "plantowatch";
			return "";
		}
	}
}
