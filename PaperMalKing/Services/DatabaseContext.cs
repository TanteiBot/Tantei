using Microsoft.EntityFrameworkCore;
using PaperMalKing.Data;

namespace PaperMalKing.Services
{
	public class DatabaseContext : DbContext
	{
		public DbSet<PmkUser> Users { get; set; }

		public DbSet<PmkGuild> Guilds { get; set; }

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
				optionsBuilder.UseLazyLoadingProxies().UseSqlite(this._connectionString);
		}

		/// <inheritdoc />
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<GuildUsers>()
			.HasKey(x => new {x.DiscordId, x.GuildId});

			modelBuilder.Entity<GuildUsers>()
			.HasOne(gu => gu.User)
			.WithMany(u => u.Guilds)
			.HasForeignKey(gu => gu.DiscordId);

			modelBuilder.Entity<GuildUsers>()
			.HasOne(gu => gu.Guild)
			.WithMany(g => g.Users)
			.HasForeignKey(gu => gu.GuildId);
		}
	}
}