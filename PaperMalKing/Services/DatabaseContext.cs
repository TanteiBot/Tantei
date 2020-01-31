using Microsoft.EntityFrameworkCore;
using PaperMalKing.Data;

namespace PaperMalKing.Services
{
	public class DatabaseContext : DbContext
	{
		public DbSet<PmkUser> Users { get; set; }

		private readonly string _connectionString;
		public DatabaseContext(BotConfig config)
		{
			this._connectionString = config.Database.ConnectionString;
		}

		public DatabaseContext(DbContextOptions<DatabaseContext> options, BotConfig config) : base(options)
		{
			this._connectionString = config.Database.ConnectionString;
		}

		/// <inheritdoc />
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
				optionsBuilder.UseSqlite(this._connectionString);
		}
	}
}
