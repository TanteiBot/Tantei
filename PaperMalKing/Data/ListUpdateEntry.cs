/*
 * Code looks awful i know it, probably will rewrite together with MalService
 */
using System;
using DSharpPlus.Entities;
using JikanDotNet;

namespace PaperMalKing.Data
{
	/// <summary>
	/// Contains info about single user list update
	/// </summary>
	public sealed class ListUpdateEntry
	{
		/// <summary>
		/// MyAnimeList user whose list has been updated
		/// </summary>
		public readonly UserProfile User;

		/// <summary>
		/// Item that was updated(such as manga or anime)
		/// </summary>
		public readonly UpdateEntry Entry;

		/// <summary>
		/// Status string that contains info about current user progress on updated item
		/// </summary>
		private readonly string _status;

		/// <summary>
		/// Current user progress on updated item, used for color of DiscordEmbed
		/// </summary>
		private readonly StatusType _progressStatus;

		public ListUpdateEntry(UserProfile profile, AnimeListEntry animeEntry, string status, StatusType progressStatus, DateTime? updatedDate) : this(profile,status,progressStatus)
		{
			this.Entry = new UpdateEntry(animeEntry,  updatedDate);
		}

		public ListUpdateEntry(UserProfile profile, MangaListEntry mangaEntry, string status, StatusType progressStatus, DateTime? updatedDate) : this(profile,status,progressStatus)
		{
			this.Entry = new UpdateEntry(mangaEntry, updatedDate);
		}

		public ListUpdateEntry(UserProfile profile, Manga manga, string status, StatusType progressStatus, DateTime? updatedDate) : this(profile,status,progressStatus)
		{
			this.Entry = new UpdateEntry(manga,  updatedDate);
		}

		public ListUpdateEntry(UserProfile profile, Anime anime, string status, StatusType progressStatus, DateTime? updatedDate) : this(profile, status,progressStatus)
		{
			this.Entry = new UpdateEntry(anime, updatedDate);
		}

		public ListUpdateEntry(UserProfile profile, string status, StatusType progressStatus)
		{
			this.User = profile;
			this._status = status;
			this._progressStatus = progressStatus;
		}


		public DiscordEmbed CreateEmbed()
		{
			var embedBuilder = new DiscordEmbedBuilder()
			.WithAuthor(this.User.Username, this.User.URL,
					this.User.ImageURL ?? $"https://cdn.myanimelist.net/images/userimages/{this.User.UserId}.jpg")
			.WithTitle(this.Entry.Title)
			.WithUrl(this.Entry.TitleUrl)
			.WithThumbnailUrl(this.Entry.ImageUrl);

			if (this.Entry.Score.HasValue && this.Entry.Score.Value != 0)
			{
				embedBuilder.AddField("Score", this.Entry.Score.Value.ToString(), true);
			}
			embedBuilder.AddField("Status", this._status, true);

			if (this.Entry.EntryUpdatedDate != null)
			{
				embedBuilder.WithTimestamp(this.Entry.EntryUpdatedDate);
			}

			DiscordColor color;
			if(this._progressStatus == StatusType.InProgress)
				color = new DiscordColor("#29b236"); // Mal green
			else if(this._progressStatus == StatusType.Completed)
				color = new DiscordColor("#214290"); // Mal blue
			else if(this._progressStatus == StatusType.OnHold)
				color = new DiscordColor("#f9d556"); //Mal yellow
			else if(this._progressStatus == StatusType.Dropped)
				color = new DiscordColor("#a22b2d"); // Mal red
			else if(this._progressStatus == StatusType.PlanToCheck)
				color = new DiscordColor("#c4c4c4"); // Mal grey
			else
				color = DiscordColor.Black;

			embedBuilder.WithColor(color);

			return embedBuilder.Build();
		}

		public enum StatusType
		{
			/// <summary>
			/// Reading / Watching
			/// </summary>
			InProgress = 1,

			Completed = 2,

			OnHold = 3,

			Dropped = 4,

			/// <summary>
			/// Plan to read / Plan to watch
			/// </summary>
			PlanToCheck = 6,

			Undefined = 7
		}

		/// <summary>
		/// Item that was updated
		/// </summary>
		public class UpdateEntry
		{
			/// <summary>
			/// Name of the item
			/// </summary>
			public readonly string Title;

			/// <summary>
			/// Url to the item
			/// </summary>
			public readonly string TitleUrl;

			/// <summary>
			/// Url to image of the title
			/// </summary>
			public readonly string ImageUrl;

			/// <summary>
			/// Score that user applied to title
			/// </summary>
			public readonly int? Score;

			/// <summary>
			/// Date and time when user updated this item
			/// </summary>
			public readonly DateTime? EntryUpdatedDate;

			public UpdateEntry(MangaListEntry mangaEntry, DateTime? entryUpdatedDate)
			{
				this.EntryUpdatedDate = entryUpdatedDate;
				this.Title = $"{mangaEntry.Title} ({mangaEntry.Type})";

				this.TitleUrl = mangaEntry.URL;

				this.ImageUrl = mangaEntry.ImageURL;

				this.Score = mangaEntry.Score;
			}

			public UpdateEntry(AnimeListEntry animeEntry, DateTime? entryUpdatedDate)
			{
				this.EntryUpdatedDate = entryUpdatedDate;
				this.Title = $"{animeEntry.Title} ({animeEntry.Type})";
				this.TitleUrl = animeEntry.URL;
				this.ImageUrl = animeEntry.ImageURL;
				this.Score = animeEntry.Score;
			}

			public UpdateEntry(Manga manga, DateTime? entryUpdatedDate)
			{
				this.EntryUpdatedDate = entryUpdatedDate;
				this.Title = $"{manga.Title} ({manga.Type})";
				this.TitleUrl = manga.LinkCanonical;
				this.ImageUrl = manga.ImageURL;
				this.Score = null;
			}

			public UpdateEntry(Anime anime, DateTime? entryUpdatedDate)
			{
				this.EntryUpdatedDate = entryUpdatedDate;
				this.Title = $"{anime.Title} ({anime.Type})";
				this.TitleUrl = anime.LinkCanonical;
				this.ImageUrl = anime.ImageURL;
				this.Score = null;
			}

		}
	}
}