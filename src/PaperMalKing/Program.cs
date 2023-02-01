// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using Microsoft.Extensions.Hosting;
using PaperMalKing.Startup;

var hostBuilder = Host.CreateDefaultBuilder(args);
hostBuilder.ConfigureBotServices();
hostBuilder.ConfigureBotHost();
var host = hostBuilder.Build();
await host.RunAsync().ConfigureAwait(false);