using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.UpdateProvider
{
    internal static class FavouriteToDiscordEmbedBuilderConverter
    {
        private static readonly Dictionary<Type, Func<ISiteUrlable, User, bool, DiscordEmbedBuilder>> Converters = new()
        {
            {
                typeof(Media), (obj, user, added) =>
                {
                    var media = (obj as Media)!;
                    var eb = InitialFavouriteEmbedBuilder(media, user, added).WithMediaTitle(media, user.Options.TitleLanguage)
                        .WithTotalSubEntries(media);

                    return eb;
                }
            },
            {
                typeof(Character), (obj, user, added) =>
                {
                    var character = (obj as Character)!;
                    var media = character.Media.Values[0];
                    return InitialFavouriteEmbedBuilder(character, user, added).WithTitle(character.Name.GetName(user.Options.TitleLanguage))
                        .AddField("From", Formatter.MaskedUrl(media.Title.GetTitle(user.Options.TitleLanguage), new Uri(media.Url)), true);
                }
            },
            {
                typeof(Staff), (obj, user, added) =>
                {
                    var staff = (obj as Staff)!;
                    return InitialFavouriteEmbedBuilder(staff, user, added).WithTitle(staff.Name.GetName(user.Options.TitleLanguage));
                }
            },
            {
                typeof(Studio), (obj, user, added) =>
                {
                    var studio = (obj as Studio)!;
                    var media = studio.Media.Nodes[0];
                    return InitialFavouriteEmbedBuilder(studio, user, added).WithTitle(studio.Name)
                        .AddField("Authors of", Formatter.MaskedUrl(media.Title.GetTitle(user.Options.TitleLanguage), new Uri(media.Url)), true);
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

        public static DiscordEmbedBuilder Convert(ISiteUrlable obj, User user, bool added)
        {
            var func = Converters[obj.GetType()];
            return func(obj, user, added);
        }
    }
}