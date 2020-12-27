using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

namespace PaperMalKing.MyAnimeList.Wrapper.Models
{
	internal class LatestInProfileUpdate
	{
		private (string inRssHash, string inProfileHash)? _hash = null;
		internal int Id { get; init; }

		internal GenericProgress Progress { get; init; }

		internal int ProgressValue { get; init; }

		internal int Score { get; init; }

		internal (string inRssHash, string inProfileHash) Hash => this._hash ??= Extensions.GetHash(this.Id, this.ProgressValue, this.Progress, this.Score);
	}
}