// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tantei.Core.Models.MyAnimeList.Users;

namespace Tantei.Data.ModelConfigurations.MyAnimeList;

internal sealed class MalUserConfiguration : IEntityTypeConfiguration<MalUser>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<MalUser> builder)
	{
		builder.Property(x => x.Id).ValueGeneratedNever();
	}
}