﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.AniList;

#pragma warning disable 219, 612, 618
#nullable disable

namespace PaperMalKing.Database.CompiledModels
{
    [EntityFrameworkInternal]
    public partial class CustomUpdateColorEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "PaperMalKing.Database.Models.AniList.AniListUser.Colors#CustomUpdateColor",
                typeof(CustomUpdateColor),
                baseEntityType,
                sharedClrType: true,
                propertyCount: 4,
                foreignKeyCount: 1,
                keyCount: 1);

            var aniListUserId = runtimeEntityType.AddProperty(
                "AniListUserId",
                typeof(uint),
                afterSaveBehavior: PropertySaveBehavior.Throw,
                sentinel: 0u);
            aniListUserId.SetAccessors(
                uint (InternalEntityEntry entry) => (entry.FlaggedAsStoreGenerated(0) ? entry.ReadStoreGeneratedValue<uint>(0) : (entry.FlaggedAsTemporary(0) && entry.ReadShadowValue<uint>(0) == 0U ? entry.ReadTemporaryValue<uint>(0) : entry.ReadShadowValue<uint>(0))),
                uint (InternalEntityEntry entry) => entry.ReadShadowValue<uint>(0),
                uint (InternalEntityEntry entry) => entry.ReadOriginalValue<uint>(aniListUserId, 0),
                uint (InternalEntityEntry entry) => entry.ReadRelationshipSnapshotValue<uint>(aniListUserId, 0),
                object (ValueBuffer valueBuffer) => valueBuffer[0]);
            aniListUserId.SetPropertyIndexes(
                index: 0,
                originalValueIndex: 0,
                shadowIndex: 0,
                relationshipIndex: 0,
                storeGenerationIndex: 0);
            aniListUserId.TypeMapping = UIntTypeMapping.Default.Clone(
                comparer: new ValueComparer<uint>(
                    bool (uint v1, uint v2) => v1 == v2,
                    int (uint v) => ((int)(v)),
                    uint (uint v) => v),
                keyComparer: new ValueComparer<uint>(
                    bool (uint v1, uint v2) => v1 == v2,
                    int (uint v) => ((int)(v)),
                    uint (uint v) => v),
                providerValueComparer: new ValueComparer<uint>(
                    bool (uint v1, uint v2) => v1 == v2,
                    int (uint v) => ((int)(v)),
                    uint (uint v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "INTEGER"));
            aniListUserId.SetCurrentValueComparer(new EntryCurrentValueComparer<uint>(aniListUserId));

            var __synthesizedOrdinal = runtimeEntityType.AddProperty(
                "__synthesizedOrdinal",
                typeof(int),
                valueGenerated: ValueGenerated.OnAddOrUpdate,
                beforeSaveBehavior: PropertySaveBehavior.Ignore,
                afterSaveBehavior: PropertySaveBehavior.Throw,
                sentinel: 0);
            __synthesizedOrdinal.SetAccessors(
                int (InternalEntityEntry entry) => (entry.FlaggedAsStoreGenerated(1) ? entry.ReadStoreGeneratedValue<int>(1) : (entry.FlaggedAsTemporary(1) && entry.ReadShadowValue<int>(1) == 0 ? entry.ReadTemporaryValue<int>(1) : entry.ReadShadowValue<int>(1))),
                int (InternalEntityEntry entry) => entry.ReadShadowValue<int>(1),
                int (InternalEntityEntry entry) => entry.ReadOriginalValue<int>(__synthesizedOrdinal, 1),
                int (InternalEntityEntry entry) => entry.ReadRelationshipSnapshotValue<int>(__synthesizedOrdinal, 1),
                object (ValueBuffer valueBuffer) => valueBuffer[1]);
            __synthesizedOrdinal.SetPropertyIndexes(
                index: 1,
                originalValueIndex: 1,
                shadowIndex: 1,
                relationshipIndex: 1,
                storeGenerationIndex: 1);
            __synthesizedOrdinal.TypeMapping = IntTypeMapping.Default.Clone(
                comparer: new ValueComparer<int>(
                    bool (int v1, int v2) => v1 == v2,
                    int (int v) => v,
                    int (int v) => v),
                keyComparer: new ValueComparer<int>(
                    bool (int v1, int v2) => v1 == v2,
                    int (int v) => v,
                    int (int v) => v),
                providerValueComparer: new ValueComparer<int>(
                    bool (int v1, int v2) => v1 == v2,
                    int (int v) => v,
                    int (int v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "INTEGER"));
            __synthesizedOrdinal.SetCurrentValueComparer(new EntryCurrentValueComparer<int>(__synthesizedOrdinal));

            var colorValue = runtimeEntityType.AddProperty(
                "ColorValue",
                typeof(int),
                propertyInfo: typeof(CustomUpdateColor).GetProperty("ColorValue", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(CustomUpdateColor).GetField("<ColorValue>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                sentinel: 0);
            colorValue.SetGetter(
                int (CustomUpdateColor entity) => CustomUpdateColorUnsafeAccessors.ColorValue(entity),
                bool (CustomUpdateColor entity) => CustomUpdateColorUnsafeAccessors.ColorValue(entity) == 0,
                int (CustomUpdateColor instance) => CustomUpdateColorUnsafeAccessors.ColorValue(instance),
                bool (CustomUpdateColor instance) => CustomUpdateColorUnsafeAccessors.ColorValue(instance) == 0);
            colorValue.SetSetter(
                (CustomUpdateColor entity, int value) => CustomUpdateColorUnsafeAccessors.ColorValue(entity) = value);
            colorValue.SetMaterializationSetter(
                (CustomUpdateColor entity, int value) => CustomUpdateColorUnsafeAccessors.ColorValue(entity) = value);
            colorValue.SetAccessors(
                int (InternalEntityEntry entry) => CustomUpdateColorUnsafeAccessors.ColorValue(((CustomUpdateColor)(entry.Entity))),
                int (InternalEntityEntry entry) => CustomUpdateColorUnsafeAccessors.ColorValue(((CustomUpdateColor)(entry.Entity))),
                int (InternalEntityEntry entry) => entry.ReadOriginalValue<int>(colorValue, 2),
                int (InternalEntityEntry entry) => entry.GetCurrentValue<int>(colorValue),
                object (ValueBuffer valueBuffer) => valueBuffer[2]);
            colorValue.SetPropertyIndexes(
                index: 2,
                originalValueIndex: 2,
                shadowIndex: -1,
                relationshipIndex: -1,
                storeGenerationIndex: -1);
            colorValue.TypeMapping = IntTypeMapping.Default.Clone(
                comparer: new ValueComparer<int>(
                    bool (int v1, int v2) => v1 == v2,
                    int (int v) => v,
                    int (int v) => v),
                keyComparer: new ValueComparer<int>(
                    bool (int v1, int v2) => v1 == v2,
                    int (int v) => v,
                    int (int v) => v),
                providerValueComparer: new ValueComparer<int>(
                    bool (int v1, int v2) => v1 == v2,
                    int (int v) => v,
                    int (int v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "INTEGER"));

            var updateType = runtimeEntityType.AddProperty(
                "UpdateType",
                typeof(byte),
                propertyInfo: typeof(CustomUpdateColor).GetProperty("UpdateType", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(CustomUpdateColor).GetField("<UpdateType>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                sentinel: (byte)0);
            updateType.SetGetter(
                byte (CustomUpdateColor entity) => CustomUpdateColorUnsafeAccessors.UpdateType(entity),
                bool (CustomUpdateColor entity) => CustomUpdateColorUnsafeAccessors.UpdateType(entity) == 0,
                byte (CustomUpdateColor instance) => CustomUpdateColorUnsafeAccessors.UpdateType(instance),
                bool (CustomUpdateColor instance) => CustomUpdateColorUnsafeAccessors.UpdateType(instance) == 0);
            updateType.SetSetter(
                (CustomUpdateColor entity, byte value) => CustomUpdateColorUnsafeAccessors.UpdateType(entity) = value);
            updateType.SetMaterializationSetter(
                (CustomUpdateColor entity, byte value) => CustomUpdateColorUnsafeAccessors.UpdateType(entity) = value);
            updateType.SetAccessors(
                byte (InternalEntityEntry entry) => CustomUpdateColorUnsafeAccessors.UpdateType(((CustomUpdateColor)(entry.Entity))),
                byte (InternalEntityEntry entry) => CustomUpdateColorUnsafeAccessors.UpdateType(((CustomUpdateColor)(entry.Entity))),
                byte (InternalEntityEntry entry) => entry.ReadOriginalValue<byte>(updateType, 3),
                byte (InternalEntityEntry entry) => entry.GetCurrentValue<byte>(updateType),
                object (ValueBuffer valueBuffer) => valueBuffer[3]);
            updateType.SetPropertyIndexes(
                index: 3,
                originalValueIndex: 3,
                shadowIndex: -1,
                relationshipIndex: -1,
                storeGenerationIndex: -1);
            updateType.TypeMapping = ByteTypeMapping.Default.Clone(
                comparer: new ValueComparer<byte>(
                    bool (byte v1, byte v2) => v1 == v2,
                    int (byte v) => ((int)(v)),
                    byte (byte v) => v),
                keyComparer: new ValueComparer<byte>(
                    bool (byte v1, byte v2) => v1 == v2,
                    int (byte v) => ((int)(v)),
                    byte (byte v) => v),
                providerValueComparer: new ValueComparer<byte>(
                    bool (byte v1, byte v2) => v1 == v2,
                    int (byte v) => ((int)(v)),
                    byte (byte v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "INTEGER"));

            var key = runtimeEntityType.AddKey(
                new[] { aniListUserId, __synthesizedOrdinal });
            runtimeEntityType.SetPrimaryKey(key);

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("AniListUserId") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("Id") }),
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                required: true,
                ownership: true);

            var colors = principalEntityType.AddNavigation("Colors",
                runtimeForeignKey,
                onDependent: false,
                typeof(List<CustomUpdateColor>),
                propertyInfo: typeof(AniListUser).GetProperty("Colors", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(AniListUser).GetField("<Colors>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                eagerLoaded: true);

            colors.SetGetter(
                List<CustomUpdateColor> (AniListUser entity) => AniListUserUnsafeAccessors.Colors(entity),
                bool (AniListUser entity) => AniListUserUnsafeAccessors.Colors(entity) == null,
                List<CustomUpdateColor> (AniListUser instance) => AniListUserUnsafeAccessors.Colors(instance),
                bool (AniListUser instance) => AniListUserUnsafeAccessors.Colors(instance) == null);
            colors.SetSetter(
                (AniListUser entity, List<CustomUpdateColor> value) => AniListUserUnsafeAccessors.Colors(entity) = value);
            colors.SetMaterializationSetter(
                (AniListUser entity, List<CustomUpdateColor> value) => AniListUserUnsafeAccessors.Colors(entity) = value);
            colors.SetAccessors(
                List<CustomUpdateColor> (InternalEntityEntry entry) => AniListUserUnsafeAccessors.Colors(((AniListUser)(entry.Entity))),
                List<CustomUpdateColor> (InternalEntityEntry entry) => AniListUserUnsafeAccessors.Colors(((AniListUser)(entry.Entity))),
                null,
                List<CustomUpdateColor> (InternalEntityEntry entry) => entry.GetCurrentValue<List<CustomUpdateColor>>(colors),
                null);
            colors.SetPropertyIndexes(
                index: 0,
                originalValueIndex: -1,
                shadowIndex: -1,
                relationshipIndex: 2,
                storeGenerationIndex: -1);
            colors.SetCollectionAccessor<AniListUser, List<CustomUpdateColor>, CustomUpdateColor>(
                List<CustomUpdateColor> (AniListUser entity) => AniListUserUnsafeAccessors.Colors(entity),
                (AniListUser entity, List<CustomUpdateColor> collection) => AniListUserUnsafeAccessors.Colors(entity) = ((List<CustomUpdateColor>)(collection)),
                (AniListUser entity, List<CustomUpdateColor> collection) => AniListUserUnsafeAccessors.Colors(entity) = ((List<CustomUpdateColor>)(collection)),
                List<CustomUpdateColor> (AniListUser entity, Action<AniListUser, List<CustomUpdateColor>> setter) => ClrCollectionAccessorFactory.CreateAndSet<AniListUser, List<CustomUpdateColor>, List<CustomUpdateColor>>(entity, setter),
                List<CustomUpdateColor> () => new List<CustomUpdateColor>());
            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            var aniListUserId = runtimeEntityType.FindProperty("AniListUserId");
            var __synthesizedOrdinal = runtimeEntityType.FindProperty("__synthesizedOrdinal");
            var colorValue = runtimeEntityType.FindProperty("ColorValue");
            var updateType = runtimeEntityType.FindProperty("UpdateType");
            var key = runtimeEntityType.FindKey(new[] { aniListUserId, __synthesizedOrdinal });
            key.SetPrincipalKeyValueFactory(KeyValueFactoryFactory.CreateCompositeFactory(key));
            key.SetIdentityMapFactory(IdentityMapFactoryFactory.CreateFactory<IReadOnlyList<object>>(key));
            runtimeEntityType.SetOriginalValuesFactory(
                ISnapshot (InternalEntityEntry source) =>
                {
                    var entity = ((CustomUpdateColor)(source.Entity));
                    return ((ISnapshot)(new Snapshot<uint, int, int, byte>(((ValueComparer<uint>)(((IProperty)aniListUserId).GetValueComparer())).Snapshot(source.GetCurrentValue<uint>(aniListUserId)), ((ValueComparer<int>)(((IProperty)__synthesizedOrdinal).GetValueComparer())).Snapshot(source.GetCurrentValue<int>(__synthesizedOrdinal)), ((ValueComparer<int>)(((IProperty)colorValue).GetValueComparer())).Snapshot(source.GetCurrentValue<int>(colorValue)), ((ValueComparer<byte>)(((IProperty)updateType).GetValueComparer())).Snapshot(source.GetCurrentValue<byte>(updateType)))));
                });
            runtimeEntityType.SetStoreGeneratedValuesFactory(
                ISnapshot () => ((ISnapshot)(new Snapshot<uint, int>(((ValueComparer<uint>)(((IProperty)aniListUserId).GetValueComparer())).Snapshot(default(uint)), ((ValueComparer<int>)(((IProperty)__synthesizedOrdinal).GetValueComparer())).Snapshot(default(int))))));
            runtimeEntityType.SetTemporaryValuesFactory(
                ISnapshot (InternalEntityEntry source) => ((ISnapshot)(new Snapshot<uint, int>(default(uint), default(int)))));
            runtimeEntityType.SetShadowValuesFactory(
                ISnapshot (IDictionary<string, object> source) => ((ISnapshot)(new Snapshot<uint, int>((source.ContainsKey("AniListUserId") ? ((uint)(source["AniListUserId"])) : 0U), (source.ContainsKey("__synthesizedOrdinal") ? ((int)(source["__synthesizedOrdinal"])) : 0)))));
            runtimeEntityType.SetEmptyShadowValuesFactory(
                ISnapshot () => ((ISnapshot)(new Snapshot<uint, int>(default(uint), default(int)))));
            runtimeEntityType.SetRelationshipSnapshotFactory(
                ISnapshot (InternalEntityEntry source) =>
                {
                    var entity = ((CustomUpdateColor)(source.Entity));
                    return ((ISnapshot)(new Snapshot<uint, int>(((ValueComparer<uint>)(((IProperty)aniListUserId).GetKeyValueComparer())).Snapshot(source.GetCurrentValue<uint>(aniListUserId)), ((ValueComparer<int>)(((IProperty)__synthesizedOrdinal).GetKeyValueComparer())).Snapshot(source.GetCurrentValue<int>(__synthesizedOrdinal)))));
                });
            runtimeEntityType.Counts = new PropertyCounts(
                propertyCount: 4,
                navigationCount: 0,
                complexPropertyCount: 0,
                originalValueCount: 4,
                shadowCount: 2,
                relationshipCount: 2,
                storeGeneratedCount: 2);
            runtimeEntityType.AddAnnotation("Relational:ContainerColumnName", "Colors");
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
