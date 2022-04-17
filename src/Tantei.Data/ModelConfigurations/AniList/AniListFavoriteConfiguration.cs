// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tantei.Core.Models.AniList.Users;

namespace Tantei.Data.ModelConfigurations.AniList;

internal sealed class AniListFavoriteConfiguration : IEntityTypeConfiguration<AniListFavorite>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<AniListFavorite> builder)
	{
		builder.HasKey(x => new
		{
			x.Id,
			x.Type,
			x.UserId
		});
	}
}