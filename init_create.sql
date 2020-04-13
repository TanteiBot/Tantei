CREATE TABLE [Guilds] (
    [GuildId] INTEGER NOT NULL CONSTRAINT [PK_Guilds] PRIMARY KEY,
    [ChannelId] INTEGER NULL
);

CREATE TABLE [Users] (
    [DiscordId] INTEGER NOT NULL CONSTRAINT [PK_Users] PRIMARY KEY,
    [MalUsername] TEXT NOT NULL,
    [LastUpdate] TEXT NOT NULL,
    [MalId] INTEGER
);  

CREATE TABLE [GuildUsers] (
    [DiscordId] INTEGER NOT NULL,
    [GuildId] INTEGER NOT NULL,
    CONSTRAINT [PK_GuildUsers] PRIMARY KEY ([DiscordId], [GuildId]),
    CONSTRAINT [FK_GuildUsers_Users_DiscordId] FOREIGN KEY ([DiscordId]) REFERENCES [Users] ([DiscordId]) ON DELETE CASCADE,
    CONSTRAINT [FK_GuildUsers_Guilds_GuildId] FOREIGN KEY ([GuildId]) REFERENCES [Guilds] ([GuildId]) ON DELETE CASCADE
);

CREATE INDEX [IX_GuildUsers_GuildId] ON [GuildUsers] ([GuildId]);