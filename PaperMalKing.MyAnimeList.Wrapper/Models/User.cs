using System;
using PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

namespace PaperMalKing.MyAnimeList.Wrapper.Models
{
	internal sealed class User
	{
		private string? _avatarUrl;
		private string? _profileUrl;
		internal string Username { get; init; } = null!;

		internal string ProfileUrl => this._profileUrl ??= $"{Constants.PROFILE_URL}{this.Username}";

		internal string AvatarUrl =>
			this._avatarUrl ??=
				$"{Constants.USER_AVATAR}{this.Id.ToString()}.jpg?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()}";

		internal int Id { get; init; }

		internal bool HasPublicAnimeUpdates => this.LatestAnimeUpdate is not null;

		internal bool HasPublicMangaUpdates => this.LatestMangaUpdate is not null;

		internal LatestInProfileUpdate? LatestMangaUpdate { get; init; }

		internal LatestInProfileUpdate? LatestAnimeUpdate { get; init; }

		internal UserFavorites Favorites { get; init; } = null!;
	}
}