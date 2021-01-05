using System.Collections.Generic;
using DSharpPlus.Entities;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	internal sealed class ShikiUpdate : IUpdate
	{
		/// <inheritdoc />
		public IReadOnlyList<DiscordEmbedBuilder> UpdateEmbeds { get; }

		public ShikiUpdate(List<DiscordEmbedBuilder> updateEmbeds)
		{
			updateEmbeds.ForEach(deb => deb.WithFooter("Shikimori", Constants.ICON_URL));
			this.UpdateEmbeds = updateEmbeds;
		}
	}
}