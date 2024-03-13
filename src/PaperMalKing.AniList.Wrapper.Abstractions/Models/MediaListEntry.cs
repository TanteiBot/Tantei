// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class MediaListEntry
{
	[JsonPropertyName("status")]
	public MediaListStatus Status { get; init; }

	[JsonPropertyName("repeat")]
	public ushort Repeat { get; init; }

	[JsonPropertyName("notes")]
	public string? Notes { get; init; }

	[JsonPropertyName("advancedScores")]
	public Dictionary<string, float>? AdvancedScores { get; init; }

	[JsonPropertyName("point100Score")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public byte Point100Score { get; init; }

	[JsonPropertyName("point10Score")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public byte Point10Score { get; init; }

	[JsonPropertyName("point5Score")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public byte Point5Score { get; init; }

	[JsonPropertyName("point3Score")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public byte Point3Score { get; init; }

	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("customLists")]
	public IReadOnlyList<CustomList>? CustomLists { get; init; } = [];

	public string GetScore(ScoreFormat scoreFormat)
	{
		if (this.Point3Score is 0 && this.Point5Score is 0 && this.Point10Score is 0 && this.Point100Score is 0)
		{
			return "";
		}

		return scoreFormat switch
		{
			ScoreFormat.POINT_100 => $"{this.Point100Score}/100",
			ScoreFormat.POINT_10_DECIMAL => $"{(this.Point100Score * 1.0d / 10).ToString(CultureInfo.InvariantCulture)}/10",
			ScoreFormat.POINT_10 => $"{this.Point10Score}/10",
			ScoreFormat.POINT_5 => $"{this.Point5Score}/5",
			ScoreFormat.POINT_3 => this.Point3Score switch
			{
				1 => ":(",
				2 => ":|",
				3 => ":)",
				_ => throw new UnreachableException("Invalid data in AniList Point 3 score format"),
			},
			_ => throw new UnreachableException("Invalid score format"),
		};
	}
}