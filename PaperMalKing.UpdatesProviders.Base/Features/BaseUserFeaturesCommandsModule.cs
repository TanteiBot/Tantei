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

using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Features
{
    public abstract class BaseUserFeaturesCommandsModule<T> : BaseCommandModule where T : struct, Enum, IComparable, IConvertible, IFormattable
    {
        protected readonly IUserFeaturesService<T> UserFeaturesService;
        protected readonly ILogger<BaseUserFeaturesCommandsModule<T>> Logger;

        protected BaseUserFeaturesCommandsModule(IUserFeaturesService<T> userFeaturesService, ILogger<BaseUserFeaturesCommandsModule<T>> logger)
        {
            this.UserFeaturesService = userFeaturesService;
            this.Logger = logger;
            var featureType = typeof(T);
            var fields = featureType.GetFields();
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (field.Name.Equals("value__")) continue;
                var featureDescriptionAttribute = field.GetCustomAttribute<FeatureDescriptionAttribute>();
                if (featureDescriptionAttribute == null)
                    throw new ArgumentNullException(featureType.Name);
            }
        }


        public virtual async Task EnableFeatureCommand(CommandContext context, T feature)
        {
            this.Logger.LogInformation("Trying to enable {Feature} feature for {Username}", feature, context.Member.DisplayName);
            try
            {
                await this.UserFeaturesService.EnableFeature(feature, context.User.Id);
            }
            catch (Exception ex)
            {
                var embed = ex is UserFeaturesException<T> ufe
                    ? EmbedTemplate.ErrorEmbed(context, ufe.Message, $"Failed enabling {ufe.Feature.ToString().ToLowerInvariant()}")
                    : EmbedTemplate.UnknownErrorEmbed(context);
                await context.RespondAsync(embed: embed.Build());
                this.Logger.LogError(ex, "Failed to enable {Feature} for {Username}", feature, context.Member.DisplayName);
                throw;
            }

            this.Logger.LogInformation("Successfully enabled {Feature} feature for {Username}", feature, context.Member.DisplayName);
            await context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context,
                $"Successfully enabled {feature.ToString().ToLowerInvariant()} for you"));
        }

        public virtual async Task DisableFeatureCommand(CommandContext context, T feature)
        {
            this.Logger.LogInformation("Trying to disable {Feature} feature for {Username}", feature, context.Member.DisplayName);
            try
            {
                await this.UserFeaturesService.DisableFeature(feature, context.User.Id);
            }
            catch (Exception ex)
            {
                var embed = ex is UserFeaturesException<T> ufe
                    ? EmbedTemplate.ErrorEmbed(context, ufe.Message, $"Failed disabling {ufe.Feature.ToString().ToLowerInvariant()}")
                    : EmbedTemplate.UnknownErrorEmbed(context);
                await context.RespondAsync(embed: embed.Build());
                this.Logger.LogError(ex, "Failed to disable {Feature} for {Username}", feature, context.Member.DisplayName);
                throw;
            }

            this.Logger.LogInformation("Successfully disabled {Feature} feature for {Username}", feature, context.Member.DisplayName);
            await context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context,
                $"Successfully disabled {feature.ToString().ToLowerInvariant()} for you"));
        }

        public virtual Task ListFeaturesCommand(CommandContext context)
        {
            var featureType = typeof(T);
            var fields = featureType.GetFields();
            var sb = new StringBuilder();
            sb.AppendLine();
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (field.Name.Equals("value__")) continue;
                var featureDescriptionAttribute = field.GetCustomAttribute<FeatureDescriptionAttribute>()!;
                sb.AppendLine(
                    $"{i.ToString()}. {featureDescriptionAttribute.Description} - {featureDescriptionAttribute.Summary.ToLowerInvariant()}");
            }

            return context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context, "All features").WithDescription(sb.ToString()));
        }
    }
}