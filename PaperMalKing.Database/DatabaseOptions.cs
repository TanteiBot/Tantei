namespace PaperMalKing.Database
{
	public sealed class DatabaseOptions
	{
		public const string Database = nameof(Database);

		public string ConnectionString { get; init; } = null!;
	}
}