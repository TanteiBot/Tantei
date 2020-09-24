using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	[Table("MyAnimeListUserFavoriteAnimes")]
	public class UserFavoriteAnime : IUserFavoriteListEntry
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long Id { get; private set; }

		public string Name { get; private set; }

		public string Url { get; private set; }

		public string Type { get; private set; }

		public int StartYear { get; private set; }

		public string? ImageUrl { get; private set; }

		public User User { get; set; } = null!;

		public long UserId { get; set; }

		public UserFavoriteAnime(long id, string name, string url, string type, int startYear, string imageUrl)
		{
			this.Id = id;
			this.Name = name;
			this.Url = url;
			this.Type = type;
			this.StartYear = startYear;
			this.ImageUrl = imageUrl;
		}
	}
}