// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Diagnostics;
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
	private static DiscordEmbedBuilder InitialFavouriteEmbedBuilder(ISiteUrlable value, User user, bool added, AniListUser dbUser)
	{
		var color = dbUser.Colors.Find(added
			? c => c.UpdateType == (byte)AniListUpdateType.FavouriteAdded
			: c => c.UpdateType == (byte)AniListUpdateType.FavouriteRemoved)?.ColorValue ?? (added ? ProviderConstants.AniListBlue : ProviderConstants.AniListRed);

		var eb = new DiscordEmbedBuilder().WithAniListAuthor(user).WithColor(color)
										  .WithDescription($"{(added ? "Added" : "Removed")} favourite").WithUrl(value.Url);
		if (value is IImageble imageble)
		{
			eb.WithThumbnail(imageble.Image?.ImageUrl);
		}

		return eb;
	}

	private static DiscordEmbedBuilder AddShortMediaLink(this DiscordEmbedBuilder eb, string fieldName, Media media, TitleLanguage language)
	{
		eb.AddField(fieldName, Formatter.MaskedUrl(media.Title.GetTitle(language), new Uri(media.Url)), inline: true);
		return eb;
	}

	public static DiscordEmbedBuilder Convert(ISiteUrlable obj, User user, bool added, AniListUser dbUser)
	{
		return obj switch
		{
			Media media => Convert(media, user, added, dbUser),
			Character character => Convert(character, user, added, dbUser),
			Staff staff => Convert(staff, user, added, dbUser),
			Studio studio => Convert(studio, user, added, dbUser),
			_ => throw new UnreachableException(),
		};
	}

	[SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "We need to output media type in lower-case")]
	private static DiscordEmbedBuilder Convert(Media media, User user, bool added, AniListUser dbUser)
	{
		var eb = InitialFavouriteEmbedBuilder(media, user, added, dbUser).WithMediaTitle(media, user.Options.TitleLanguage, dbUser.Features)
																		 .WithTotalSubEntries(media).EnrichWithMediaInfo(media, user, dbUser.Features);
		eb.Description += $" {media.Type.ToInvariantString().ToLowerInvariant()}";
		return eb;
	}

	private static DiscordEmbedBuilder Convert(Character character, User user, bool added, AniListUser dbUser)
	{
		var media = character.Media.Values[0];
		return InitialFavouriteEmbedBuilder(character, user, added, dbUser)
			   .WithTitle($"{character.Name.GetName(user.Options.TitleLanguage)} [Character]")
			   .AddShortMediaLink("From", media, user.Options.TitleLanguage);
	}

	private static DiscordEmbedBuilder Convert(Staff staff, User user, bool added, AniListUser dbUser)
	{
		var eb = InitialFavouriteEmbedBuilder(staff, user, added, dbUser)
			.WithTitle($"{staff.Name.GetName(user.Options.TitleLanguage)} [{staff.PrimaryOccupations.FirstOrDefault() ?? "Staff"}]");
		if (dbUser.Features.HasFlag(AniListUserFeatures.MediaDescription) && !string.IsNullOrEmpty(staff.Description))
		{
			var mediaDescription = staff.Description.StripHtml();
			mediaDescription = SourceRemovalRegex().Replace(mediaDescription, string.Empty);
			mediaDescription = EmptyLinesRemovalRegex().Replace(mediaDescription, string.Empty);
			mediaDescription = mediaDescription.Trim().Truncate(350);
			if (!string.IsNullOrEmpty(mediaDescription))
			{
				eb.AddField("Description", mediaDescription, inline: false);
			}
		}

		var mostPopularWork = staff.StaffMedia.Nodes.FirstOrDefault();
		if (mostPopularWork is not null)
		{
			eb.AddShortMediaLink("Most popular work", mostPopularWork, user.Options.TitleLanguage);
		}

		return eb;
	}

	private static DiscordEmbedBuilder Convert(Studio studio, User user, bool added, AniListUser dbUser)
	{
		var media = studio.Media.Nodes[0];
		return InitialFavouriteEmbedBuilder(studio, user, added, dbUser).WithTitle($"{studio.Name} [Studio]")
																.AddShortMediaLink("Most popular title", media, user.Options.TitleLanguage);
	}
}