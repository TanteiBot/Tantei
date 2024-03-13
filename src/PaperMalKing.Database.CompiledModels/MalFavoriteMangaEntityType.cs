﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using PaperMalKing.Database.Models.MyAnimeList;

#pragma warning disable 219, 612, 618
#nullable disable

namespace PaperMalKing.Database.CompiledModels
{
    internal partial class MalFavoriteMangaEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "PaperMalKing.Database.Models.MyAnimeList.MalFavoriteManga",
                typeof(MalFavoriteManga),
                baseEntityType,
                discriminatorProperty: "FavoriteType",
                discriminatorValue: MalFavoriteType.Manga);

            var startYear = runtimeEntityType.AddProperty(
                "StartYear",
                typeof(ushort),
                propertyInfo: typeof(BaseMalListFavorite).GetProperty("StartYear", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(BaseMalListFavorite).GetField("<StartYear>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                sentinel: (ushort)0);
            startYear.TypeMapping = UShortTypeMapping.Default.Clone(
                comparer: new ValueComparer<ushort>(
                    (ushort v1, ushort v2) => v1 == v2,
                    (ushort v) => (int)v,
                    (ushort v) => v),
                keyComparer: new ValueComparer<ushort>(
                    (ushort v1, ushort v2) => v1 == v2,
                    (ushort v) => (int)v,
                    (ushort v) => v),
                providerValueComparer: new ValueComparer<ushort>(
                    (ushort v1, ushort v2) => v1 == v2,
                    (ushort v) => (int)v,
                    (ushort v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "INTEGER"));

            var type = runtimeEntityType.AddProperty(
                "Type",
                typeof(string),
                propertyInfo: typeof(BaseMalListFavorite).GetProperty("Type", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(BaseMalListFavorite).GetField("<Type>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            type.TypeMapping = SqliteStringTypeMapping.Default;

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("UserId") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("UserId") }),
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                required: true);

            var user = declaringEntityType.AddNavigation("User",
                runtimeForeignKey,
                onDependent: true,
                typeof(MalUser),
                propertyInfo: typeof(BaseMalFavorite).GetProperty("User", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(BaseMalFavorite).GetField("<User>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var favoriteMangas = principalEntityType.AddNavigation("FavoriteMangas",
                runtimeForeignKey,
                onDependent: false,
                typeof(IList<MalFavoriteManga>),
                propertyInfo: typeof(MalUser).GetProperty("FavoriteMangas", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(MalUser).GetField("<FavoriteMangas>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "MalFavorites");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
