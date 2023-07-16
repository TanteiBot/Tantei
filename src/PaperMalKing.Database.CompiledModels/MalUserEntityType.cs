﻿// <auto-generated />
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.MyAnimeList;

#pragma warning disable 219, 612, 618
#nullable enable

namespace PaperMalKing.Database.CompiledModels
{
    internal partial class MalUserEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType? baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "PaperMalKing.Database.Models.MyAnimeList.MalUser",
                typeof(MalUser),
                baseEntityType);

            var userId = runtimeEntityType.AddProperty(
                "UserId",
                typeof(uint),
                propertyInfo: typeof(MalUser).GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<UserId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw,
                sentinel: 0u);

            var discordUserId = runtimeEntityType.AddProperty(
                "DiscordUserId",
                typeof(ulong),
                propertyInfo: typeof(MalUser).GetProperty("DiscordUserId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<DiscordUserId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                sentinel: 0ul);

            var favoritesIdHash = runtimeEntityType.AddProperty(
                "FavoritesIdHash",
                typeof(string),
                propertyInfo: typeof(MalUser).GetProperty("FavoritesIdHash", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<FavoritesIdHash>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueGenerated: ValueGenerated.OnAdd);
            favoritesIdHash.AddAnnotation("Relational:DefaultValue", "");

            var features = runtimeEntityType.AddProperty(
                "Features",
                typeof(MalUserFeatures),
                propertyInfo: typeof(MalUser).GetProperty("Features", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<Features>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueGenerated: ValueGenerated.OnAdd,
                sentinel: MalUserFeatures.None);
            features.AddAnnotation("Relational:DefaultValue", MalUserFeatures.AnimeList | MalUserFeatures.MangaList | MalUserFeatures.Favorites | MalUserFeatures.Mention | MalUserFeatures.Website | MalUserFeatures.MediaFormat | MalUserFeatures.MediaStatus);

            var lastAnimeUpdateHash = runtimeEntityType.AddProperty(
                "LastAnimeUpdateHash",
                typeof(string),
                propertyInfo: typeof(MalUser).GetProperty("LastAnimeUpdateHash", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<LastAnimeUpdateHash>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var lastMangaUpdateHash = runtimeEntityType.AddProperty(
                "LastMangaUpdateHash",
                typeof(string),
                propertyInfo: typeof(MalUser).GetProperty("LastMangaUpdateHash", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<LastMangaUpdateHash>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var lastUpdatedAnimeListTimestamp = runtimeEntityType.AddProperty(
                "LastUpdatedAnimeListTimestamp",
                typeof(DateTimeOffset),
                propertyInfo: typeof(MalUser).GetProperty("LastUpdatedAnimeListTimestamp", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<LastUpdatedAnimeListTimestamp>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueConverter: new DateTimeOffsetToBinaryConverter(),
                sentinel: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            var lastUpdatedMangaListTimestamp = runtimeEntityType.AddProperty(
                "LastUpdatedMangaListTimestamp",
                typeof(DateTimeOffset),
                propertyInfo: typeof(MalUser).GetProperty("LastUpdatedMangaListTimestamp", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<LastUpdatedMangaListTimestamp>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueConverter: new DateTimeOffsetToBinaryConverter(),
                sentinel: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            var username = runtimeEntityType.AddProperty(
                "Username",
                typeof(string),
                propertyInfo: typeof(MalUser).GetProperty("Username", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<Username>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var key = runtimeEntityType.AddKey(
                new[] { userId });
            runtimeEntityType.SetPrimaryKey(key);

            var index = runtimeEntityType.AddIndex(
                new[] { discordUserId },
                unique: true);

            var index0 = runtimeEntityType.AddIndex(
                new[] { features });

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("DiscordUserId")! },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("DiscordUserId")! })!,
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                unique: true,
                required: true);

            var discordUser = declaringEntityType.AddNavigation("DiscordUser",
                runtimeForeignKey,
                onDependent: true,
                typeof(DiscordUser),
                propertyInfo: typeof(MalUser).GetProperty("DiscordUser", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<DiscordUser>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "MalUsers");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
