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
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.AniList;
using static PaperMalKing.AniList.UpdateProvider.Extensions;

namespace PaperMalKing.AniList.UpdateProvider
{
    internal static class FavouriteToDiscordEmbedBuilderConverter
    {
        private static readonly Dictionary<Type, Func<ISiteUrlable, User, bool, AniListUserFeatures, DiscordEmbedBuilder>> Converters = new()
        {
            {
                typeof(Media), (obj, user, added, features) =>
                {
                    var media = (obj as Media)!;
                    var eb = InitialFavouriteEmbedBuilder(media, user, added).WithMediaTitle(media, user.Options.TitleLanguage, features)
                                                                             .WithTotalSubEntries(media).EnrichWithMediaInfo(media, user, features);
                    eb.Description += $" {media?.Type.ToString().ToLowerInvariant()}";
                    return eb;
                }
            },
            {
                typeof(Character), (obj, user, added, _) =>
                {
                    var character = (obj as Character)!;
                    var media = character.Media.Values[0];
                    return InitialFavouriteEmbedBuilder(character, user, added)
                        .WithTitle($"{character.Name.GetName(user.Options.TitleLanguage)} [Character]")
                        .AddField("From", Formatter.MaskedUrl(media.Title.GetTitle(user.Options.TitleLanguage), new Uri(media.Url)), true);
                }
            },
            {
                typeof(Staff), (obj, user, added, features) =>
                {
                    var staff = (obj as Staff)!;
                    var eb = InitialFavouriteEmbedBuilder(staff, user, added).WithTitle($"{staff.Name.GetName(user.Options.TitleLanguage)} [Staff]");
                        if ((features & AniListUserFeatures.MediaDescription) != 0 && !string.IsNullOrEmpty(staff.Description))
                        {
                            var mediaDescription = staff.Description.StripHtml();
                            mediaDescription = SourceRemovalRegex.Replace(mediaDescription, string.Empty);
                            mediaDescription = EmptyLinesRemovalRegex.Replace(mediaDescription, string.Empty);
                            mediaDescription = mediaDescription.Trim().Truncate(350);
                            if (!string.IsNullOrEmpty(mediaDescription))
                                eb.AddField("Description", mediaDescription, false);
                        }
                    
                    return eb;
                }
            },
            {
                typeof(Studio), (obj, user, added, features) =>
                {
                    var studio = (obj as Studio)!;
                    var media = studio.Media.Nodes[0];
                    return InitialFavouriteEmbedBuilder(studio, user, added).WithTitle($"{studio.Name} [Studio]")
                        .AddField("Most popular title", Formatter.MaskedUrl(media.Title.GetTitle(user.Options.TitleLanguage), new Uri(media.Url)),
                            true);
                }
            }
        };

        private static DiscordEmbedBuilder InitialFavouriteEmbedBuilder(ISiteUrlable value, User user, bool added)
        {
            var eb = new DiscordEmbedBuilder().WithAniListAuthor(user).WithColor(added ? Constants.AniListBlue : Constants.AniListRed)
                .WithDescription($"{(added ? "Added" : "Removed")} favourite").WithUrl(value.Url);
            if (value is IImageble imageble) eb.WithThumbnail(imageble.Image.ImageUrl);
            return eb;
        }

        public static DiscordEmbedBuilder Convert(ISiteUrlable obj, User user, bool added, AniListUserFeatures features)
        {
            var func = Converters[obj.GetType()];
            return func(obj, user, added, features);
        }
    }
}