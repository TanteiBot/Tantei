#region LICENSE

// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Database;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Services
{
	public sealed class OnStartupActionsExecutingService : IHostedService
	{
		private readonly IServiceProvider _serviceProvider;

		public OnStartupActionsExecutingService(IServiceProvider serviceProvider)
		{
			this._serviceProvider = serviceProvider;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using var scope = this._serviceProvider.CreateScope();

			scope.ServiceProvider.GetRequiredService<ICommandsService>();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			await db.Database.MigrateAsync(CancellationToken.None).ConfigureAwait(false);
			var s = this._serviceProvider.GetRequiredService<UpdatePublishingService>();
			var services = this._serviceProvider.GetServices<IExecuteOnStartupService>();
			foreach (var service in services)
			{
				if (cancellationToken.IsCancellationRequested)
					return;

				await service.ExecuteAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}