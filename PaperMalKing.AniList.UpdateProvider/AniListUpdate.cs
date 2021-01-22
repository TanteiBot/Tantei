﻿#region LICENSE
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