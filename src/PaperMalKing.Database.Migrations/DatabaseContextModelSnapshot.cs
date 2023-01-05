﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaperMalKing.Database;

#nullable disable

namespace PaperMalKing.Database.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.1");

            modelBuilder.Entity("DiscordGuildDiscordUser", b =>
                {
                    b.Property<ulong>("GuildsDiscordGuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UsersDiscordUserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GuildsDiscordGuildId", "UsersDiscordUserId");

                    b.HasIndex("UsersDiscordUserId");

                    b.ToTable("DiscordGuildDiscordUser");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.AniList.AniListFavourite", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("FavouriteType")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id", "FavouriteType", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("AniListFavourites");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.AniList.AniListUser", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("DiscordUserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FavouritesIdHash")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("");

                    b.Property<ulong>("Features")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(127ul);

                    b.Property<long>("LastActivityTimestamp")
                        .HasColumnType("INTEGER");

                    b.Property<long>("LastReviewTimestamp")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("DiscordUserId")
                        .IsUnique();

                    b.HasIndex("Features");

                    b.ToTable("AniListUsers");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.BotUser", b =>
                {
                    b.Property<uint>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("UserId");

                    b.ToTable("BotUsers");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.DiscordGuild", b =>
                {
                    b.Property<ulong>("DiscordGuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("PostingChannelId")
                        .HasColumnType("INTEGER");

                    b.HasKey("DiscordGuildId");

                    b.HasIndex("DiscordGuildId");

                    b.ToTable("DiscordGuilds");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.DiscordUser", b =>
                {
                    b.Property<ulong>("DiscordUserId")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("BotUserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("DiscordUserId");

                    b.HasIndex("BotUserId")
                        .IsUnique();

                    b.HasIndex("DiscordUserId");

                    b.ToTable("DiscordUsers");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.BaseMalFavorite", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("FavoriteType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("NameUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id", "UserId", "FavoriteType");

                    b.HasIndex("FavoriteType");

                    b.HasIndex("Id");

                    b.HasIndex("UserId");

                    b.ToTable("MalFavorites", (string)null);

                    b.HasDiscriminator<byte>("FavoriteType");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalUser", b =>
                {
                    b.Property<uint>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("DiscordUserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FavoritesIdHash")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("");

                    b.Property<ulong>("Features")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(127ul);

                    b.Property<string>("LastAnimeUpdateHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastMangaUpdateHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("LastUpdatedAnimeListTimestamp")
                        .HasColumnType("INTEGER");

                    b.Property<long>("LastUpdatedMangaListTimestamp")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserId");

                    b.HasIndex("DiscordUserId")
                        .IsUnique();

                    b.HasIndex("Features");

                    b.ToTable("MalUsers");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.Shikimori.ShikiFavourite", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FavType")
                        .HasColumnType("TEXT");

                    b.Property<uint>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id", "FavType", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("ShikiFavourites");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.Shikimori.ShikiUser", b =>
                {
                    b.Property<uint>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("DiscordUserId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FavouritesIdHash")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("");

                    b.Property<ulong>("Features")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(127ul);

                    b.Property<ulong>("LastHistoryEntryId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("DiscordUserId")
                        .IsUnique();

                    b.HasIndex("Features");

                    b.ToTable("ShikiUsers");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalFavoriteAnime", b =>
                {
                    b.HasBaseType("PaperMalKing.Database.Models.MyAnimeList.BaseMalFavorite");

                    b.Property<ushort>("StartYear")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue((byte)1);
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalFavoriteCharacter", b =>
                {
                    b.HasBaseType("PaperMalKing.Database.Models.MyAnimeList.BaseMalFavorite");

                    b.Property<string>("FromTitleName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue((byte)3);
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalFavoriteCompany", b =>
                {
                    b.HasBaseType("PaperMalKing.Database.Models.MyAnimeList.BaseMalFavorite");

                    b.HasDiscriminator().HasValue((byte)5);
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalFavoriteManga", b =>
                {
                    b.HasBaseType("PaperMalKing.Database.Models.MyAnimeList.BaseMalFavorite");

                    b.Property<ushort>("StartYear")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasDiscriminator().HasValue((byte)2);
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalFavoritePerson", b =>
                {
                    b.HasBaseType("PaperMalKing.Database.Models.MyAnimeList.BaseMalFavorite");

                    b.HasDiscriminator().HasValue((byte)4);
                });

            modelBuilder.Entity("DiscordGuildDiscordUser", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.DiscordGuild", null)
                        .WithMany()
                        .HasForeignKey("GuildsDiscordGuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PaperMalKing.Database.Models.DiscordUser", null)
                        .WithMany()
                        .HasForeignKey("UsersDiscordUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.AniList.AniListFavourite", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.AniList.AniListUser", "User")
                        .WithMany("Favourites")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.AniList.AniListUser", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.DiscordUser", "DiscordUser")
                        .WithOne()
                        .HasForeignKey("PaperMalKing.Database.Models.AniList.AniListUser", "DiscordUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DiscordUser");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.DiscordUser", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.BotUser", "BotUser")
                        .WithOne("DiscordUser")
                        .HasForeignKey("PaperMalKing.Database.Models.DiscordUser", "BotUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BotUser");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalUser", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.DiscordUser", "DiscordUser")
                        .WithOne()
                        .HasForeignKey("PaperMalKing.Database.Models.MyAnimeList.MalUser", "DiscordUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DiscordUser");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.Shikimori.ShikiFavourite", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.Shikimori.ShikiUser", "User")
                        .WithMany("Favourites")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.Shikimori.ShikiUser", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.DiscordUser", "DiscordUser")
                        .WithOne()
                        .HasForeignKey("PaperMalKing.Database.Models.Shikimori.ShikiUser", "DiscordUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DiscordUser");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalFavoriteAnime", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.MyAnimeList.MalUser", "User")
                        .WithMany("FavoriteAnimes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalFavoriteCharacter", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.MyAnimeList.MalUser", "User")
                        .WithMany("FavoriteCharacters")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalFavoriteCompany", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.MyAnimeList.MalUser", "User")
                        .WithMany("FavoriteCompanies")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalFavoriteManga", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.MyAnimeList.MalUser", "User")
                        .WithMany("FavoriteMangas")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalFavoritePerson", b =>
                {
                    b.HasOne("PaperMalKing.Database.Models.MyAnimeList.MalUser", "User")
                        .WithMany("FavoritePeople")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.AniList.AniListUser", b =>
                {
                    b.Navigation("Favourites");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.BotUser", b =>
                {
                    b.Navigation("DiscordUser");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.MyAnimeList.MalUser", b =>
                {
                    b.Navigation("FavoriteAnimes");

                    b.Navigation("FavoriteCharacters");

                    b.Navigation("FavoriteCompanies");

                    b.Navigation("FavoriteMangas");

                    b.Navigation("FavoritePeople");
                });

            modelBuilder.Entity("PaperMalKing.Database.Models.Shikimori.ShikiUser", b =>
                {
                    b.Navigation("Favourites");
                });
#pragma warning restore 612, 618
        }
    }
}
