namespace PaperMalKing.Shikimori.Wrapper.Models
{
	internal sealed class Paginatable<T>
	{
		public T Data { get; }

		public int CurrentPage { get; }

		public bool HasNextPage { get; }

		public Paginatable(T data, int currentPage, bool hasNextPage)
		{
			this.Data = data;
			this.CurrentPage = currentPage;
			this.HasNextPage = hasNextPage;
		}
	}
}