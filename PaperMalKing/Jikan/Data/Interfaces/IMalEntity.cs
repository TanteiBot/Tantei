namespace PaperMalKing.Jikan.Data.Interfaces
{
	public interface IMalEntity
	{
		/// <summary>
		/// ID associated with MyAnimeList.
		/// </summary>
		long MalId { get; set; }

		/// <summary>
		/// Title of the entry.
		/// </summary>
		string Title { get; set; }

		/// <summary>
		/// Entry's image URL
		/// </summary>
		string ImageUrl { get; set; }

		/// <summary>
		/// Entry's URL
		/// </summary>
		string Url { get; set; }

		/// <summary>
		/// Entry type
		/// </summary>
		string Type { get; set; }
	}
}