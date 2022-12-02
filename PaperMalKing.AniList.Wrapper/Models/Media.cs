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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models
{
	[SuppressMessage("Naming", "CA1724")]
    public sealed class Media : IImageble, ISiteUrlable, IIdentifiable
    {
        [JsonPropertyName("id")]
        public ulong Id { get; init; }

        [JsonPropertyName("title")]
        public MediaTitle Title { get; init; } = null!;

        [JsonPropertyName("type")]
        public ListType Type { get; init; }

        [JsonPropertyName("siteUrl")]
        public string Url { get; init; } = null!;

        [JsonPropertyName("format")]
        public MediaFormat? Format { get; init; }

        [JsonPropertyName("countryOfOrigin")]
        public string CountryOfOrigin { get; init; } = null!;

        [JsonPropertyName("status")]
        public MediaStatus Status { get; init; }

        [JsonPropertyName("episodes")]
        public ushort? Episodes { get; init; }

        [JsonPropertyName("chapters")]
        public ushort? Chapters { get; init; }

        [JsonPropertyName("volumes")]
        public ushort? Volumes { get; init; }

        [JsonPropertyName("image")]
        public Image Image { get; init; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; init; } = null!;

        [JsonPropertyName("genres")]
        public IReadOnlyList<string> Genres { get; init; } = Array.Empty<string>();

        [JsonPropertyName("tags")]
        public IReadOnlyList<MediaTag> Tags { get; init; } = Array.Empty<MediaTag>();

        [JsonPropertyName("studios")]
        public Connection<Studio> Studios { get; init; } = Connection<Studio>.Empty;

        [JsonPropertyName("staff")]
        public Connection<StaffEdge> Staff { get; init; } = Connection<StaffEdge>.Empty;
    }
}