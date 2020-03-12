using System.Collections.Generic;
using Newtonsoft.Json;

namespace PaperMalKing.MyAnimeList.Jikan.Data.Models
{
	/// <summary>
	/// User's anime list model class.
	/// </summary>
	public sealed class UserAnimeList : BaseJikanRequest
	{
		/// <summary>
		/// Collection of user's anime on their anime list.
		/// </summary>
		[JsonProperty(PropertyName = "anime")]
		public ICollection<AnimeListEntry> Anime { get; set; }
	}
}