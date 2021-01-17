using System.Collections.Generic;
using DSharpPlus.Entities;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.AniList.UpdateProvider
{
    internal sealed class AniListUpdate : IUpdate
    {
        private static readonly DiscordEmbedBuilder.EmbedFooter AniListFooter = new()
        {
            Text = Constants.NAME,
            IconUrl = Constants.ICON_URL
        };
        
        public IReadOnlyList<DiscordEmbedBuilder> UpdateEmbeds { get; }

        public AniListUpdate(IReadOnlyList<DiscordEmbedBuilder> updateEmbeds)
        {
            this.UpdateEmbeds = updateEmbeds.ForEach(u => u.Footer = AniListFooter);
        }
    }
}