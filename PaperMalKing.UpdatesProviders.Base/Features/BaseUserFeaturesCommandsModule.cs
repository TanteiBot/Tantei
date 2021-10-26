#region LICENSE

// PaperMalKing.
// Copyright (C) 2021 N0D4N
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

using System.Diagnostics.CodeAnalysis;
using DSharpPlus.CommandsNext;
using Humanizer;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Features
{
	[SuppressMessage("Microsoft.Design", "CA1051")]
	[SuppressMessage("Microsoft.Design", "CA1308")]
	public abstract class BaseUserFeaturesCommandsModule<T> : BaseCommandModule where T : unmanaged, Enum, IComparable, IConvertible, IFormattable
	{
		protected readonly IUserFeaturesService<T> UserFeaturesService;
		protected readonly ILogger<BaseUserFeaturesCommandsModule<T>> Logger;

		protected BaseUserFeaturesCommandsModule(IUserFeaturesService<T> userFeaturesService, ILogger<BaseUserFeaturesCommandsModule<T>> logger)
		{
			this.UserFeaturesService = userFeaturesService;
			this.Logger = logger;
		}

		public virtual async Task EnableFeatureCommand(CommandContext context, params T[] features)
		{
			if (!features.Any())
				return;
			this.Logger.LogInformation("Trying to enable {Features} feature for {Username}", features, context.Member.DisplayName);
			try
			{
				await this.UserFeaturesService.EnableFeaturesAsync(features, context.User.Id).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var embed = ex is UserFeaturesException ufe
					? EmbedTemplate.ErrorEmbed(context, ufe.Message, $"Failed enabling {features.Humanize().ToLowerInvariant()}")
					: EmbedTemplate.UnknownErrorEmbed(context);
				await context.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
				this.Logger.LogError(ex, "Failed to enable {Features} for {Username}", features, context.Member.DisplayName);
				throw;
			}

			this.Logger.LogInformation("Successfully enabled {Features} feature for {Username}", features, context.Member.DisplayName);
			await context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context,
																		 $"Successfully enabled {features.Humanize()} for you")).ConfigureAwait(false);
		}

		public virtual async Task DisableFeatureCommand(CommandContext context, params T[] features)
		{
			if (!features.Any())
				return;
			this.Logger.LogInformation("Trying to disable {Features} feature for {Username}", features, context.Member.DisplayName);
			try
			{
				await this.UserFeaturesService.DisableFeaturesAsync(features, context.User.Id).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var embed = ex is UserFeaturesException ufe
					? EmbedTemplate.ErrorEmbed(context, ufe.Message, $"Failed disabling {features.Humanize().ToLowerInvariant()}")
					: EmbedTemplate.UnknownErrorEmbed(context);
				await context.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
				this.Logger.LogError(ex, "Failed to disable {Features} for {Username}", features, context.Member.DisplayName);
				throw;
			}

			this.Logger.LogInformation("Successfully disabled {Features} feature for {Username}", features, context.Member.DisplayName);
			await context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context,
																		 $"Successfully disabled {features.Humanize()} for you")).ConfigureAwait(false);
		}

		public virtual Task ListFeaturesCommand(CommandContext context) =>
			context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context, "All features")
													 .WithDescription(string.Join(";\n",
																				  this.UserFeaturesService.Descriptions.Values
																					  .Select(tuple => $"[{tuple.Item1}] - {tuple.Item2}"))));

		public virtual async Task EnabledFeaturesCommand(CommandContext context)
		{
			var featuresDesc = await this.UserFeaturesService.EnabledFeaturesAsync(context.User.Id).ConfigureAwait(false);
			await context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context, "Your enabled features").WithDescription(featuresDesc)).ConfigureAwait(false);
		}
	}
}