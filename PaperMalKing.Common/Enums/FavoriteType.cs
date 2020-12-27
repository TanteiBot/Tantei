namespace PaperMalKing.Common.Enums
{
	public enum FavoriteType : byte
	{
		Anime = 0,
		Manga = 1,
		Character = 2,
		Person = 3,
		BaseList = byte.MaxValue - 1,
		Base = byte.MaxValue
	}
}