// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.AniList.UpdateProvider.Search;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.UpdateProvider.Tests.Search;

public sealed class AniListScoreFormatterTests
{
	private const ushort SampleAverage = 85;
	private const ushort Point10WholeAverage = 80;
	private const ushort Point10FractionAverage = 83;
	private const ushort SadCeiling = 33;
	private const ushort NeutralFloor = 34;
	private const ushort NeutralCeiling = 66;
	private const ushort HappyFloor = 67;
	private const ushort MaxAverage = 100;
	private const string Sad = ":(";
	private const string Neutral = ":|";
	private const string Happy = ":)";

	[Test]
	[Arguments(ScoreFormat.POINT_100, "85/100")]
	[Arguments(ScoreFormat.POINT_10_DECIMAL, "8.5/10")]
	[Arguments(ScoreFormat.POINT_10, "8.5/10")]
	[Arguments(ScoreFormat.POINT_5, "4/5")]
	[Arguments(ScoreFormat.POINT_3, Happy)]
	public async Task RendersTheSampleAverageInEveryScoreFormat(ScoreFormat scoreFormat, string expected)
	{
		await Assert.That(AniListScoreFormatter.Format(SampleAverage, scoreFormat)).IsEqualTo(expected);
	}

	[Test]
	public async Task Point10IsRenderedAsADecimalForCommunityAverages()
	{
		await Assert.That(AniListScoreFormatter.Format(Point10WholeAverage, ScoreFormat.POINT_10)).IsEqualTo("8/10");
		await Assert.That(AniListScoreFormatter.Format(Point10FractionAverage, ScoreFormat.POINT_10)).IsEqualTo("8.3/10");
	}

	[Test]
	[Arguments(SadCeiling, Sad)]
	[Arguments(NeutralFloor, Neutral)]
	[Arguments(NeutralCeiling, Neutral)]
	[Arguments(HappyFloor, Happy)]
	[Arguments(MaxAverage, Happy)]
	public async Task Point3UsesSmileyThresholds(ushort averageScore, string expected)
	{
		await Assert.That(AniListScoreFormatter.Format(averageScore, ScoreFormat.POINT_3)).IsEqualTo(expected);
	}

	[Test]
	public async Task TheLowestAveragesRenderAsTheSadSmiley()
	{
		await Assert.That(AniListScoreFormatter.Format(0, ScoreFormat.POINT_3)).IsEqualTo(Sad);
	}

	[Test]
	[Arguments(ScoreFormat.POINT_100)]
	[Arguments(ScoreFormat.POINT_10_DECIMAL)]
	[Arguments(ScoreFormat.POINT_10)]
	[Arguments(ScoreFormat.POINT_5)]
	[Arguments(ScoreFormat.POINT_3)]
	public async Task NullAverageScoreOmitsTheScoreToken(ScoreFormat scoreFormat)
	{
		await Assert.That(AniListScoreFormatter.Format(averageScore: null, scoreFormat)).IsNull();
	}

	[Test]
	public async Task UnlinkedRequesterDefaultRendersAsPoint100()
	{
		await Assert.That(AniListScoreFormatter.Format(SampleAverage, ScoreFormat.POINT_100)).IsEqualTo("85/100");
	}
}
