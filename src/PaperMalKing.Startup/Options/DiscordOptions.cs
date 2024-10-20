// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PaperMalKing.Startup.Options;

public sealed class DiscordOptions : IValidatableObject
{
	public const string Discord = "Discord";

	[Required]
	[StringLength(int.MaxValue, MinimumLength = 1)]
	public string Token { get; init; } = null!;

	[Required]
	[StringLength(int.MaxValue, MinimumLength = 1)]
	public string ClientId { get; init; } = null!;

	[Required]
	[StringLength(int.MaxValue, MinimumLength = 1)]
	public string ClientSecret { get; init; } = null!;

	[Required]
	[Length(1, int.MaxValue)]
	public IReadOnlyList<DiscordActivityOptions> Activities { get; init; } = [];

	public sealed class DiscordActivityOptions
	{
		[Required]
		[StringLength(int.MaxValue, MinimumLength = 1)]
		public string ActivityType { get; init; } = null!;

		[Required]
		[StringLength(int.MaxValue, MinimumLength = 1)]
		public string PresenceText { get; init; } = null!;

		[Required]
		[Range(1, int.MaxValue)]
		public int TimeToBeDisplayedInMilliseconds { get; init; }

		[Required]
		[StringLength(int.MaxValue, MinimumLength = 1)]
		public string Status { get; init; } = null!;
	}

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		var res = new List<ValidationResult>(8);

		Validator.TryValidateProperty(this.Token, new(this, serviceProvider: null, items: null) { MemberName = nameof(this.Token), }, res);
		Validator.TryValidateProperty(this.ClientId, new(this, serviceProvider: null, items: null) { MemberName = nameof(this.ClientId), }, res);
		Validator.TryValidateProperty(this.ClientSecret, new(this, serviceProvider: null, items: null) { MemberName = nameof(this.ClientSecret), }, res);
		Validator.TryValidateProperty(this.Activities, new(this, serviceProvider: null, items: null) { MemberName = nameof(this.Activities), }, res);

		if (this.Activities is not { Count: > 1 })
		{
			return res;
		}

		foreach (var activity in this.Activities)
		{
			Validator.TryValidateProperty(activity.ActivityType, new(activity, serviceProvider: null, items: null) { MemberName = nameof(activity.ActivityType), }, res);
			Validator.TryValidateProperty(activity.PresenceText, new(activity, serviceProvider: null, items: null) { MemberName = nameof(activity.PresenceText), }, res);
			Validator.TryValidateProperty(activity.TimeToBeDisplayedInMilliseconds, new(activity, serviceProvider: null, items: null) { MemberName = nameof(activity.TimeToBeDisplayedInMilliseconds), }, res);
			Validator.TryValidateProperty(activity.Status, new(activity, serviceProvider: null, items: null) { MemberName = nameof(activity.Status), }, res);
		}

		return res;
	}
}