Tantei
=================

[![.NET build](https://github.com/TanteiBot/Tantei/actions/workflows/build.yml/badge.svg)](https://github.com/TanteiBot/Tantei/actions/workflows/build.yml) ![GitHub](https://img.shields.io/github/license/TanteiBot/Tantei?label=License&style=flat-square)

Tantei is Discord bot that tracks user's updates from various anime/manga list websites such as [MyAnimeList](https://myanimelist.net), [Shikimori](https://shikimori.one), [AniList](https://anilist.co) and posts them to Discord server/servers.

Installation and running
---------------------
Prerequisites: .NET 6.0 Runtime ([download](https://dot.net/download) / [installation guide](https://docs.microsoft.com/en-us/dotnet/core/install/))
1. Download [latest release](https://github.com/TanteiBot/Tantei/releases/latest/download/PaperMalKing.zip)
2. Extract it, and move into extracted directory
3. Fill out details in `template.appsettings.json` (normally it would be token of your Discord bot and path where you would like to store database file)
4. Rename `template.appsettings.json` to `appsettings.json`

Run with `dotnet PaperMalKing.dll`

Building and developing
---------------------
Prerequisites: git, .NET 6.0 SDK
1. ```
   git clone https://github.com/TanteiBot/Tantei.git
   ```
2. ```
   cd Tantei
   ```
3. ```
   dotnet build -c Release
   ```

Notice
---------------------
Project is unofficial and is not affiliated with MyAnimeList.net, Shikimori.one, AniList.co or any other website/application from which project can get users updates.

License
---------------------

Copyright 2021 N0D4N
<img align="right" src="https://www.gnu.org/graphics/agplv3-with-text-100x42.png">

Licensed under the AGPLv3: https://www.gnu.org/licenses/agpl-3.0.html 