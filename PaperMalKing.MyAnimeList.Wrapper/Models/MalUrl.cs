using PaperMalKing.MyAnimeList.Wrapper.Parsers;

namespace PaperMalKing.MyAnimeList.Wrapper.Models
{
	internal sealed class MalUrl
	{
		private int? _id;
		internal string Url { get; }

		internal int Id => this._id ??= CommonParser.ExtractIdFromMalUrl(this.Url);

		public MalUrl(string url)
		{
			this.Url = url;
		}
	}
}