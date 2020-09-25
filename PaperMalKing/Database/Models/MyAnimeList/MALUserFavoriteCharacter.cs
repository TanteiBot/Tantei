using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	[Table("MyAnimeListUserFavoriteCharacters")]
	public class MALUserFavoriteCharacter : IMALUserFavorite
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long Id { get; private set; }

		public string Name { get; private set; }

		public string Url { get; private set; }

		public string ImageUrl { get; private set; }

		public string FromEntryName { get; private set; }

		public string FromEntryUrl { get; private set; }

		public MALUser MALUser { get; set; } = null!;

		public long UserId { get; set; }

		public MALUserFavoriteCharacter(string name, string url, string imageUrl, long id, string fromEntryName,
									 string fromEntryUrl)
		{
			this.Name = name;
			this.Url = url;
			this.ImageUrl = imageUrl;
			this.Id = id;
			this.FromEntryName = fromEntryName;
			this.FromEntryUrl = fromEntryUrl;
		}
	}
}