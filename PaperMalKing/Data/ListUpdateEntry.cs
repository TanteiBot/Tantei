/*
 * Code looks awful i know it, probably will rewrite together with MalService
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DSharpPlus.Entities;
using PaperMalKing.MyAnimeList;
using PaperMalKing.MyAnimeList.Jikan.Data.Interfaces;

namespace PaperMalKing.Data
{
	/// <summary>
	/// Contains info about single user list update
	/// </summary>
	public sealed class ListUpdateEntry
	{
		private static ConcurrentDictionary<StatusType, DiscordColor> Colors =>
			new ConcurrentDictionary<StatusType, DiscordColor>(new[]
			{
				// Mal blue color for completed entries
				new KeyValuePair<StatusType, DiscordColor>(StatusType.Completed, new DiscordColor("#214290")),
				// Mal red for dropped entries
				new KeyValuePair<StatusType, DiscordColor>(StatusType.Dropped, new DiscordColor("#a22b2d")),
				// Mal green for entries in progress
				new KeyValuePair<StatusType, DiscordColor>(StatusType.InProgress, new DiscordColor("#29b236")),
				//Mal yellow for entries on hold
				new KeyValuePair<StatusType, DiscordColor>(StatusType.OnHold, new DiscordColor("#f9d556")),
				// Mal grey for entries being in plans to read or watch
				new KeyValuePair<StatusType, DiscordColor>(StatusType.PlanToCheck, new DiscordColor("#c4c4c4")),
				// Black color if somehow entity is undefined
				new KeyValuePair<StatusType, DiscordColor>(StatusType.Undefined, DiscordColor.Black)
			});

		/// <summary>
		/// User to which update is related
		/// </summary>
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

		public ListUpdateEntry(PmkUser user, IMalEntity malEntity, string status, DateTime? updatedDate)
		{
			this.Entry = new UpdateEntry(malEntity, updatedDate);
			this.User = user;
			this._status = status;
			if (malEntity is IListEntry li)
			{
				this._progressStatus = li.UsersStatus;
			}
			else
				this._progressStatus = StatusType.PlanToCheck;
		}

		/// <summary>
		/// Creates an embed that represents this update
		/// </summary>
		/// <returns>Embed that represents this update</returns>
		public DiscordEmbed CreateEmbed()
		{
			var embedBuilder = new DiscordEmbedBuilder()
							   .WithAuthor(this.User.MalUsername, this.User.Url, this.User.MalAvatarUrl)
							   .WithTitle(this.Entry.Title).WithUrl(this.Entry.TitleUrl)
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

			var color = Colors.GetValueOrDefault(this._progressStatus, DiscordColor.Black);

			embedBuilder.WithColor(color);

			return embedBuilder.Build();
		}

		/// <summary>
		/// Item that was updated
		/// </summary>
		public sealed class UpdateEntry
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
				var title = malEntity.Title.Trim();
				var type = malEntity.Type.Trim();
				if (title.EndsWith($"({type})", StringComparison.InvariantCultureIgnoreCase) // Title is looking fine the way he is
					||
					// I don't want to give titles more standard for bot look
					// because it will mess up titles like "The Last: Naruto the Movie"
					title.EndsWith(type, StringComparison.InvariantCultureIgnoreCase))
					this.Title = title;
				else
					this.Title =
						$"{title} ({type})"; // Append type to title. This will do for most of the titles on MAL

				this.TitleUrl = malEntity.Url;

				this.ImageUrl = malEntity.ImageUrl;

				if (malEntity is IListEntry listEntry)
					this.Score = listEntry.Score;
			}
		}
	}
}