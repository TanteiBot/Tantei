// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tantei.Core.Models.Shikimori.Users;

namespace Tantei.Data.ModelConfigurations.Shikimori;

internal sealed class ShikiUserConfiguration : IEntityTypeConfiguration<ShikimoriUser>
{
	/// <inheritdoc />
	public void Configure(EntityTypeBuilder<ShikimoriUser> builder)
	{
		builder.Property(x => x.Id).ValueGeneratedNever();
	}
}