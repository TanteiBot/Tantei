// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Threading.Tasks;

namespace PaperMalKing.UpdatesProviders.Base
{
	public delegate Task UpdateFoundEvent(UpdateFoundEventArgs eventArgs);
}