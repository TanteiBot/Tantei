﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using AngleSharp.Dom;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;

internal static class CommentsParser
{
	public static string Parse(IDocument document)
	{
		var text = document.QuerySelector("#content > div.borderClass > div > a")!.GetAttribute("href")!;
		return text[(text.LastIndexOf('/') + 1)..];
	}
}