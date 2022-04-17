// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models;

public sealed record Character(string Name, Uri Url, Uri? ImageUrl, Character.MiniMedia From) : INameUrlable, IHasImage
{
	public sealed record MiniMedia(string Name, Uri Url) : INameUrlable;
}