// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PaperMalKing.UpdatesProviders.Base.UpdateProvider;

public interface IUpdateProviderConfigurator<T> where T : IUpdateProvider
{
	abstract static void Configure(IConfiguration configuration, IServiceCollection serviceCollection);
}