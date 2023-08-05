// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Interfaces;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.AniList;
using static PaperMalKing.AniList.UpdateProvider.Extensions;

namespace PaperMalKing.AniList.UpdateProvider;

internal static class FavouriteToDiscordEmbedBuilderConverter
{
	[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase")]
	private static readonly FrozenDictionary<Type, Func<ISiteUrlable, User, bool, AniListUserFeatures, DiscordEmbedBuilder>> Converters = new Dictionary<Type, Func<ISiteUrlable, User, bool, AniListUserFeatures, DiscordEmbedBuilder>>()
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
				if (features.HasFlag( AniListUserFeatures.MediaDescription) && !string.IsNullOrEmpty(staff.Description))
				{
					var mediaDescription = staff.Description.StripHtml();
					mediaDescription = SourceRemovalRegex().Replace(mediaDescription, string.Empty);
					mediaDescription = EmptyLinesRemovalRegex().Replace(mediaDescription, string.Empty);
					mediaDescription = mediaDescription.Trim().Truncate(350);
					if (!string.IsNullOrEmpty(mediaDescription))
						eb.AddField("Description", mediaDescription, inline: false);
				}

				var mostPopularWork = staff.StaffMedia.Nodes.FirstOrDefault();
				if (mostPopularWork is not null)
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
		},
	}.ToFrozenDictionary(optimizeForReading: true);

	private static DiscordEmbedBuilder InitialFavouriteEmbedBuilder(ISiteUrlable value, User user, bool added)
	{
		var eb = new DiscordEmbedBuilder().WithAniListAuthor(user).WithColor(added ? ProviderConstants.AniListBlue : ProviderConstants.AniListRed)
										  .WithDescription($"{(added ? "Added" : "Removed")} favourite").WithUrl(value.Url);
		if (value is IImageble imageble) eb.WithThumbnail(imageble.Image?.ImageUrl);
		return eb;
	}

	private static DiscordEmbedBuilder AddShortMediaLink(this DiscordEmbedBuilder eb, string fieldName, Media media, TitleLanguage language)
	{
		eb.AddField(fieldName, Formatter.MaskedUrl(media.Title.GetTitle(language), new Uri(media.Url)), inline: true);
		return eb;
	}

	public static DiscordEmbedBuilder Convert(ISiteUrlable obj, User user, bool added, AniListUserFeatures features)
	{
		var func = Converters[obj.GetType()];
		return func(obj, user, added, features);
	}
}