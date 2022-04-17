// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models;

public interface INameUrlable
{
	string Name { get; }
	Uri? Url { get; }
}