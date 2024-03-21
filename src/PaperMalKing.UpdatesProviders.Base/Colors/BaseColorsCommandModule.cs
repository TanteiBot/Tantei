// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base.Colors;

[SuppressMessage("Style", """VSTHRD200:Use "Async" suffix for async methods""", Justification = "This rule does not apply to commands")]
public abstract class BaseColorsCommandModule<TUser, TUpdateType> : BotCommandsModule
	where TUser : class, IUpdateProviderUser
	where TUpdateType : unmanaged, Enum
{
	protected ILogger<BaseColorsCommandModule<TUser, TUpdateType>> Logger { get; }

	protected CustomColorService<TUser, TUpdateType> ColorService { get; }

	protected BaseColorsCommandModule(ILogger<BaseColorsCommandModule<TUser, TUpdateType>> logger, CustomColorService<TUser, TUpdateType> colorService)
	{
		this.Logger = logger;
		this.ColorService = colorService;
	}

	public async virtual Task SetColor(InteractionContext context, string unparsedUpdateType, string colorValue)
	{
		TUpdateType updateType;
		try
		{
			var color = new DiscordColor(colorValue);
			updateType = UpdateTypesHelper<TUpdateType>.Parse(unparsedUpdateType);
			await this.ColorService.SetColorAsync(context.User.Id, updateType, color);
		}
		catch (Exception ex)
		{
			var embed = ex is ArgumentException or UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.Message) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			this.Logger.LogError(ex, "Failed to set color of {UnparsedUpdateType} to {ColorValue}", unparsedUpdateType, colorValue);
			throw;
		}

		await context.EditResponseAsync(EmbedTemplate.SuccessEmbed($"Successfully set color of {updateType}"));
	}

	public async virtual Task RemoveColor(InteractionContext context, string unparsedUpdateType)
	{
		TUpdateType updateType;
		try
		{
			updateType = UpdateTypesHelper<TUpdateType>.Parse(unparsedUpdateType);
			await this.ColorService.RemoveColorAsync(context.User.Id, updateType);
		}
		catch (Exception ex)
		{
			var embed = ex is ArgumentException or UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.Message) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			this.Logger.LogError(ex, "Failed to remove color of {UnparsedUpdateType}", unparsedUpdateType);
			throw;
		}

		await context.EditResponseAsync(EmbedTemplate.SuccessEmbed($"Successfully removed color of {updateType}"));
	}

	public virtual Task<DiscordMessage> ListOverridenColor(InteractionContext context)
	{
		var colors = this.ColorService.OverridenColors(context.User.Id);
		return context.EditResponseAsync(EmbedTemplate
										 .SuccessEmbed(string.IsNullOrWhiteSpace(colors) ? "You have no colors overriden" : "Your overriden colors")
										 .WithDescription(colors));
	}
}