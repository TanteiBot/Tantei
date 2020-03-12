using System.Collections.Generic;
using Newtonsoft.Json;

namespace PaperMalKing.MyAnimeList.Jikan.Data.Models
{
	/// <summary>
	/// User's manga list model class.
	/// </summary>
	public sealed class UserMangaList : BaseJikanRequest
	{
		/// <summary>
		/// Collection of user's manga on their manga list.
		/// </summary>
		[JsonProperty(PropertyName = "manga")]
		public ICollection<MangaListEntry> Manga { get; set; }
	}
}