using Microsoft.EntityFrameworkCore.Design;

namespace PaperMalKing.Database
{
	public sealed class DesignTypeDbContext : IDesignTimeDbContextFactory<DatabaseContext>
	{
		public DatabaseContext CreateDbContext(string[] args)
		{
			if (args.Length == 0)
				return new();
			return string.IsNullOrEmpty(args?[0]) ? new () : new (args[0]);
		}
	}
}