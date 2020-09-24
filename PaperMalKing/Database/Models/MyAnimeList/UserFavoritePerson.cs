using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	[Table("MyAnimeListUserFavoritePersons")]
	public class UserFavoritePerson : IUserFavorite
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long Id { get; private set; }

		public string Name { get; private set; }

		public string Url { get; private set; }

		public string? ImageUrl { get; private set; }

		public User User { get; set; } = null!;

		public long UserId { get; set; }

		public UserFavoritePerson(string name, string url, string? imageUrl, long id)
		{
			this.Name = name;
			this.Url = url;
			this.ImageUrl = imageUrl;
			this.Id = id;
		}
	}
}