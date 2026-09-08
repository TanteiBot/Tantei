// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.AniList.UpdateProvider.Search;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Interfaces;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.AniList;
using static PaperMalKing.AniList.UpdateProvider.Extensions;

namespace PaperMalKing.AniList.UpdateProvider;

internal static class FavouriteToDiscordEmbedBuilderConverter
{
	private const string VoiceActorOccupation = "Voice Actor";

	private static DiscordEmbedBuilder InitialFavouriteEmbedBuilder(ISiteUrlable value, User user, bool added, AniListUser dbUser)
	{
		var color = dbUser.Colors.Find(added
			? static c => c.UpdateType == (byte)AniListUpdateType.FavouriteAdded
			: static c => c.UpdateType == (byte)AniListUpdateType.FavouriteRemoved)?.ColorValue ?? (added ? ProviderConstants.AniListBlue : ProviderConstants.AniListRed);

		var eb = new DiscordEmbedBuilder().WithAniListAuthor(user).WithColor(color)
										  .WithDescription($"{(added ? "Added" : "Removed")} favourite").WithUrl(value.Url);
		if (value is IImageble imageble)
		{
			eb.WithThumbnail(imageble.Image?.ImageUrl);
		}

		return eb;
	}

	private static DiscordEmbedBuilder AddShortMediaLink(this DiscordEmbedBuilder eb, string fieldName, Media? media, TitleLanguage language)
	{
		if (media is null)
		{
			return eb;
		}

		eb.AddField(fieldName, Formatter.MaskedUrl(media.Title.GetTitle(language), new(media.Url)), inline: true);
		return eb;
	}

	private static DiscordEmbedBuilder AddDescription(this DiscordEmbedBuilder eb, string? description, AniListUserFeatures features)
	{
		if (!features.HasFlag(AniListUserFeatures.Description) || string.IsNullOrWhiteSpace(description))
		{
			return eb;
		}

		return eb.AddFieldIfPresent("Description", NormalizeDescription(description).Trim().Truncate(DescriptionLimit));
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

	private static DiscordEmbedBuilder Convert(Media media, User user, bool added, AniListUser dbUser)
	{
		var eb = InitialFavouriteEmbedBuilder(media, user, added, dbUser).WithMediaTitle(media, user.Options.TitleLanguage, dbUser.Features)
																		 .WithTotalSubEntries(media);
		eb.AddFieldIfPresent("Score", AniListScoreFormatter.Format(media.AverageScore, user.MediaListOptions?.ScoreFormat ?? ScoreFormat.POINT_100),
			inline: true);
		eb.EnrichWithMediaInfo(media, user, dbUser.Features);
		eb.Description += $" {media.Type.Humanize(LetterCasing.Sentence)}";
		return eb;
	}

	private static DiscordEmbedBuilder Convert(Character character, User user, bool added, AniListUser dbUser)
	{
		return InitialFavouriteEmbedBuilder(character, user, added, dbUser)
			   .WithTitle($"{character.Name.GetName(user.Options.TitleLanguage)} [Character]")
			   .AddDescription(character.Description, dbUser.Features)
			   .AddShortMediaLink("From", character.Media.Values.FirstOrDefault(), user.Options.TitleLanguage);
	}

	private static DiscordEmbedBuilder Convert(Staff staff, User user, bool added, AniListUser dbUser)
	{
		var isVoiceActor = staff.PrimaryOccupations.Contains(VoiceActorOccupation, StringComparer.OrdinalIgnoreCase);
		var bestKnownWork = (isVoiceActor ? staff.CharacterMedia : staff.StaffMedia).Nodes.FirstOrDefault();

		return InitialFavouriteEmbedBuilder(staff, user, added, dbUser)
			   .WithTitle($"{staff.Name.GetName(user.Options.TitleLanguage)} [{staff.PrimaryOccupations.FirstOrDefault() ?? "Staff"}]")
			   .AddDescription(staff.Description, dbUser.Features)
			   .AddShortMediaLink("From", bestKnownWork, user.Options.TitleLanguage);
	}

	private static DiscordEmbedBuilder Convert(Studio studio, User user, bool added, AniListUser dbUser)
	{
		return InitialFavouriteEmbedBuilder(studio, user, added, dbUser).WithTitle($"{studio.Name} [Studio]")
																	   .AddShortMediaLink("Known for", studio.Media.Nodes.FirstOrDefault(),
																		   user.Options.TitleLanguage);
	}
}
