/*
 * Code looks awful i know it, probably will rewrite together with MalService
 */
using System;
using DSharpPlus.Entities;
using PaperMalKing.Jikan.Data;
using PaperMalKing.Jikan.Data.Interfaces;
using PaperMalKing.Jikan.Data.Models;

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
		public readonly UserProfile UserProfile;

		public readonly PmkUser User;

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

		public ListUpdateEntry(UserProfile profile, PmkUser user, IMalEntity malEntity, string status, DateTime? updatedDate)
		{
			this.Entry = new UpdateEntry(malEntity,  updatedDate);
			this.UserProfile = profile;
			this.User = user;
			this._status = status;
			var statusTypeUnparsed = status.Split(" - ")[0].ToLower().Trim();
			if(statusTypeUnparsed.Contains("plan to"))
				this._progressStatus = StatusType.PlanToCheck;
			else if(statusTypeUnparsed.EndsWith("ing"))
				this._progressStatus = StatusType.InProgress;
			else
			{
				if (StatusType.TryParse(statusTypeUnparsed, true, out StatusType res))
					this._progressStatus = res;
				else
					this._progressStatus = StatusType.Undefined;
			}
		}


		public DiscordEmbed CreateEmbed()
		{
			var embedBuilder = new DiscordEmbedBuilder()
			.WithAuthor(this.UserProfile.Username, this.UserProfile.Url,
					this.UserProfile.ImageUrl ?? $"https://cdn.myanimelist.net/images/userimages/{this.UserProfile.UserId}.jpg")
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

			public UpdateEntry(IMalEntity malEntity, DateTime? entryUpdatedDate)
			{
				this.EntryUpdatedDate = entryUpdatedDate;
				this.Title = $"{malEntity.Title} ({malEntity.Type})";

				this.TitleUrl = malEntity.Url;

				this.ImageUrl = malEntity.ImageUrl;

				if(malEntity is IListEntry listEntry)
					this.Score = listEntry.Score;
			}
		}
	}
}