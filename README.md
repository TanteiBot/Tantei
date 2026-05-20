Tantei
=================


[![.NET build](https://github.com/TanteiBot/Tantei/actions/workflows/build.yml/badge.svg)](https://github.com/TanteiBot/Tantei/actions/workflows/build.yml) ![GitHub](https://img.shields.io/github/license/TanteiBot/Tantei?label=License&style=flat-square)  
Tantei is Discord bot that tracks user's updates from various anime/manga list websites such as [MyAnimeList](https://myanimelist.net), [AniList](https://anilist.co), [Shikimori](https://shikimori.io) and posts them to Discord server/s.

Installation
---------------------

#### Running with Docker (recommended)

Prerequisites: [Docker](https://docs.docker.com/get-started/get-docker/) (Podman should work as well, but was not tested)

- `mkdir tantei && cd tantei`
- In the new folder create file `docker-compose.yml` and copy-paste following content there
    ```yaml
    services:
      tantei:
        image: ghcr.io/tanteibot/tantei:latest
        restart: unless-stopped
        container_name: tantei
        volumes:
          - <path-to-config-directory>:/config:ro
          - <path-to-data-directory>:/data
    ```
- Edit the file, replace `<path-to-config-directory>` with the path where the config file for Tantei will be stored.

  Replace `<path-to-data-directory>` with the path where the data for Tantei (database file) will be stored.
- In the directory, which you used to replace `<path-to-config-directory>`, create a file `appsettings.Production.json`, copy structure of its contents [from the repo](https://github.com/TanteiBot/Tantei/blob/release/v2/src/PaperMalKing/template.appsettings.Production.json)
- Edit the newly created file, provide your Discord bot's token, ClientId and secret, which can be found on [Discord's website](https://discord.com/developers/applications)
- By default bot tries to check updates from ALL available providers, to disable one, you need to set `DelayBetweenChecksInMilliseconds` in its section to -1. For example, here is how part of config may look like with disabled AniList provider
    ```json
    {
      "AniList": {
        "DelayBetweenChecksInMilliseconds": -1
      } 
    }
    ```
- To fetch list updates from MyAnimeList, you need to register your bot on [MAL's website](https://myanimelist.net/apiconfig), and set Client ID and Client Secret in the config.
- To fetch list updates for Shikimori, you need to create application on [Shikimori's website](https://shikimori.io/oauth) adn set its name in config
- Switch back to directory `tantei` directory you created in first step and start the app with `docker compose up`

#### Running on bare metal
Prerequisites: [ASP.NET Core Runtime 10](https://get.dot.net/10), wget, unzip

- Create a directory for the bot, `cd` there and obtain latest release from GitHub `wget https://github.com/TanteiBot/Tantei/releases/latest/download/Tantei.zip`
- `unzip Tantei.zip && cd output`
- `cp template.appsettings.Production.json appsettings.Production.json`
- Fill necessary data in `appsettings.Production.json`, see relevant section in setup guide for docker
- `dotnet PaperMalKing.dll`

In this scenario you will need to worry about setting up the bot to work in background, having it autorestart manually.

#### Building from source and running on bare metal

Prerequisites: git, [.NET SDK 10.0](https://get.dot.net/10)
- `git clone --branch v2 https://github.com/TanteiBot/Tantei.git`
- `cd Tantei/`
- `dotnet publish -c Release -o publish/ src/PaperMalKing/PaperMalKing.csproj`
- `cd publish/`
- `cp template.appsettings.Production.json appsettings.Production.json`
- Fill necessary data in `appsettings.Production.json`, see relevant section in setup guide for docker
- Run with `dotnet PaperMalKing.dll`

In this scenario you will need to worry about setting up the bot to work in background, having it autorestart manually.~~~~

Notice
---------------------
Project is unofficial and is not affiliated with MyAnimeList.net, Shikimori.io, AniList.co or any other website/application from which project can get users updates.

License
---------------------

Copyright 2021-2026 N0D4N

Licensed under the AGPLv3: https://www.gnu.org/licenses/agpl-3.0.html

[![License](https://www.gnu.org/graphics/agplv3-with-text-100x42.png)](https://www.gnu.org/licenses/agpl-3.0.html)
