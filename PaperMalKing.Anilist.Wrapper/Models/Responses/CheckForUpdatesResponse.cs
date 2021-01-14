using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models.Responses
{
    internal sealed class CheckForUpdatesResponse
    {
        public bool HasNextPage => this.User.Favourites.HasNextPage ||
                                   this.ListActivities.PageInfo.HasNextPage ||
                                   this.Reviews.PageInfo.HasNextPage;

        [JsonPropertyName("User")]
        public User User { get; init; } = null!;

        [JsonPropertyName("AnimeList")]
        public MediaListCollection AnimeList { get; init; } = null!;

        [JsonPropertyName("MangaList")]
        public MediaListCollection MangaList { get; init; } = null!;

        [JsonPropertyName("ActivitiesPage")]
        public Page<ListActivity> ListActivities { get; init; } = Page<ListActivity>.Empty;

        [JsonPropertyName("ReviewsPage")]
        public Page<Review> Reviews { get; init; } = Page<Review>.Empty;
    }
}