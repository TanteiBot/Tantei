CREATE TABLE [Users] (
    [DiscordId] INTEGER NOT NULL CONSTRAINT [PK_Users] PRIMARY KEY,
    [MalUsername] TEXT NOT NULL,
    [LastUpdate] TEXT NOT NULL
);