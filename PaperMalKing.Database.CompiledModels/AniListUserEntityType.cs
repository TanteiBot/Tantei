﻿// <auto-generated />
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.AniList;

#pragma warning disable 219, 612, 618
#nullable enable

namespace PaperMalKing.Database.CompiledModels
{
    internal partial class AniListUserEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType? baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "PaperMalKing.Database.Models.AniList.AniListUser",
                typeof(AniListUser),
                baseEntityType);

            var id = runtimeEntityType.AddProperty(
                "Id",
                typeof(ulong),
                propertyInfo: typeof(AniListUser).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(AniListUser).GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);

            var discordUserId = runtimeEntityType.AddProperty(
                "DiscordUserId",
                typeof(ulong),
                propertyInfo: typeof(AniListUser).GetProperty("DiscordUserId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(AniListUser).GetField("<DiscordUserId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var features = runtimeEntityType.AddProperty(
                "Features",
                typeof(AniListUserFeatures),
                propertyInfo: typeof(AniListUser).GetProperty("Features", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(AniListUser).GetField("<Features>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                valueGenerated: ValueGenerated.OnAdd);
            features.AddAnnotation("Relational:DefaultValue", AniListUserFeatures.AnimeList | AniListUserFeatures.MangaList | AniListUserFeatures.Favourites | AniListUserFeatures.Mention | AniListUserFeatures.Website | AniListUserFeatures.MediaFormat | AniListUserFeatures.MediaStatus);

            var lastActivityTimestamp = runtimeEntityType.AddProperty(
                "LastActivityTimestamp",
                typeof(long),
                propertyInfo: typeof(AniListUser).GetProperty("LastActivityTimestamp", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(AniListUser).GetField("<LastActivityTimestamp>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var lastReviewTimestamp = runtimeEntityType.AddProperty(
                "LastReviewTimestamp",
                typeof(long),
                propertyInfo: typeof(AniListUser).GetProperty("LastReviewTimestamp", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(AniListUser).GetField("<LastReviewTimestamp>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var key = runtimeEntityType.AddKey(
                new[] { id });
            runtimeEntityType.SetPrimaryKey(key);

            var index = runtimeEntityType.AddIndex(
                new[] { discordUserId });

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("DiscordUserId")! },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("DiscordUserId")! })!,
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                required: true);

            var discordUser = declaringEntityType.AddNavigation("DiscordUser",
                runtimeForeignKey,
                onDependent: true,
                typeof(DiscordUser),
                propertyInfo: typeof(AniListUser).GetProperty("DiscordUser", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(AniListUser).GetField("<DiscordUser>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "AniListUsers");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
