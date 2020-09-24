using System;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Options;
using PaperMalKing.Options;
using PaperMalKing.Services.Background;

namespace PaperMalKing.Services
{
	public sealed class CommandsService
	{
		private readonly CommandsOptions _options;
		public readonly CommandsNextExtension CommandsExtension;

		public CommandsService(IOptions<CommandsOptions> options, IServiceProvider provider,
							   DiscordService discordService)
		{
			var config = new CommandsNextConfiguration
			{
				CaseSensitive = this._options.CaseSensitive,
				EnableMentionPrefix = this._options.EnableMentionPrefix,
				StringPrefixes = this._options.Prefixes,
				DmHelp = false,
				Services = provider
			};

			this.CommandsExtension = discordService.Client.UseCommandsNext(config);

			this._options = options.Value;
		}
	}
}