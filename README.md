Paper Mal King
=================


[![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/ndan/PaperMalKing/1/rewrite-v2)](https://dev.azure.com/ndan/PaperMalKing/_build?definitionId=1&_a=summary&repositoryFilter=1&branchFilter=2%2C2) [![Discord](https://discord.com/api/guilds/507267293647208487/widget.png?style=shield)](https://discord.gg/b43GycdVAV)  
Paper Mal King is Discord bot that tracks user's updates from various anime/manga list websites such as [MyAnimeList](https://myanimelist.net), [Shikimori](https://shikimori.one), [AniList](https://anilist.co) and posts them to Discord server/servers.

Installation
---------------------
Prerequisites: git, .NET SDK 5.0
- `git clone https://github.com/N0D4N/PaperMalKing.git`
- `cd PaperMalKing/`
- `dotnet publish -c Release -o publish/ PaperMalKing/PaperMalKing.csproj`
- `cd publish/`
- `cp template.appsettings.json appsettings.json`
- Fill necessary data in `appsettings.json`

Run with `dotnet PaperMalKing.dll`

Notice
---------------------
Project is unofficial and is not affiliated with MyAnimeList.net, Shikimori.one, AniList.co or any other website/application from which project can get users updates.

License
---------------------

Copyright 2021 N0D4N

Licensed under the AGPLv3: https://www.gnu.org/licenses/agpl-3.0.html

![License](https://www.gnu.org/graphics/agplv3-with-text-100x42.png)