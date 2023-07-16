// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;

public interface IListType
{
	abstract static ListEntryType ListEntryType { get; }
	abstract static string LatestUpdatesUrl<TRequestOptions>(string username, TRequestOptions options) where TRequestOptions : unmanaged, Enum;
}