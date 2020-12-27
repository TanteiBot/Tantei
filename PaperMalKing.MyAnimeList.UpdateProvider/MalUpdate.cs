using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	internal sealed class MalUpdate : IUpdate
	{
		public MalUpdate(IEnumerable<DiscordEmbedBuilder> embeds)
		{
			this.UpdateEmbeds = embeds.Select(builder => builder.WithMalUpdateProviderFooter()).ToArray();
		}

		public MalUpdate(IReadOnlyList<DiscordEmbedBuilder> embeds) => this.UpdateEmbeds = embeds;
		
		/// <inheritdoc />
		public IReadOnlyList<DiscordEmbedBuilder> UpdateEmbeds { get; }
	}
}