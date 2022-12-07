#region LICENSE

// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
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
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Enums;
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
					eb.Description += $" {media.Type.ToString().ToLowerInvariant()}";
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
						   .AddShortMediaLink("From", media, user.Options.TitleLanguage);
				}
			},
			{
				typeof(Staff), (obj, user, added, features) =>
				{
					var staff = (obj as Staff)!;
					var eb = InitialFavouriteEmbedBuilder(staff, user, added)
						.WithTitle($"{staff.Name.GetName(user.Options.TitleLanguage)} [{staff.PrimaryOccupations.FirstOrDefault() ?? "Staff"}]");
					if ((features & AniListUserFeatures.MediaDescription) != 0 && !string.IsNullOrEmpty(staff.Description))
					{
						var mediaDescription = staff.Description.StripHtml();
						mediaDescription = SourceRemovalRegex().Replace(mediaDescription, string.Empty);
						mediaDescription = EmptyLinesRemovalRegex().Replace(mediaDescription, string.Empty);
						mediaDescription = mediaDescription.Trim().Truncate(350);
						if (!string.IsNullOrEmpty(mediaDescription))
							eb.AddField("Description", mediaDescription, false);
					}

					var mostPopularWork = staff.StaffMedia.Nodes.FirstOrDefault();
					if (mostPopularWork != null)
					{
						eb.AddShortMediaLink("Most popular work", mostPopularWork, user.Options.TitleLanguage);
					}

					return eb;
				}
			},
			{
				typeof(Studio), (obj, user, added, _) =>
				{
					var studio = (obj as Studio)!;
					var media = studio.Media.Nodes[0];
					return InitialFavouriteEmbedBuilder(studio, user, added).WithTitle($"{studio.Name} [Studio]")
																			.AddShortMediaLink("Most popular title", media,
																				user.Options.TitleLanguage);
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

		private static DiscordEmbedBuilder AddShortMediaLink(this DiscordEmbedBuilder eb, string fieldName, Media media, TitleLanguage language)
		{
			eb.AddField(fieldName, Formatter.MaskedUrl(media.Title.GetTitle(language), new Uri(media.Url)), true);
			return eb;
		}

		public static DiscordEmbedBuilder Convert(ISiteUrlable obj, User user, bool added, AniListUserFeatures features)
		{
			var func = Converters[obj.GetType()];
			return func(obj, user, added, features);
		}
	}
}