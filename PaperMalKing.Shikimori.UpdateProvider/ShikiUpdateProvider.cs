using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	internal sealed class ShikiUpdateProvider : BaseUpdateProvider
	{
		private readonly IOptions<ShikiOptions> _options;

		private readonly ShikiClient _client;

		private readonly IServiceProvider _serviceProvider;

		/// <inheritdoc />
		public ShikiUpdateProvider(ILogger<ShikiUpdateProvider> logger, IOptions<ShikiOptions> options, ShikiClient client,
								   IServiceProvider serviceProvider) : base(logger,
			TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
		{
			this._options = options;
			this._client = client;
			this._serviceProvider = serviceProvider;
		}

		/// <inheritdoc />
		public override string Name => Constants.NAME;

		/// <inheritdoc />
		public override event UpdateFoundEventHandler? UpdateFoundEvent;

		/// <inheritdoc />
		protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
		{
			this.Logger.LogInformation("Starting to check for updates");
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

			await foreach (var dbUser in db.ShikiUsers.Include(u => u.DiscordUser).ThenInclude(du => du.Guilds).Include(u => u.Favourites)
										   .Where(u => u.DiscordUser.Guilds.Any()).AsAsyncEnumerable().WithCancellation(cancellationToken))
			{
				
			}
		}
	}
}