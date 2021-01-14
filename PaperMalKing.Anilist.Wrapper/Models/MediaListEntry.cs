using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class MediaListEntry
    {
        private readonly Dictionary<ScoreFormat, byte> _scores = new();

        [JsonPropertyName("status")]
        public MediaListStatus Status { get; init; }

        [JsonPropertyName("repeat")]
        public ushort Repeat { get; init; }

        [JsonPropertyName("notes")]
        public string? Notes { get; init; }

        [JsonPropertyName("advancedScores")]
        public Dictionary<string, float>? AdvancedScores { get; init; }

        [JsonPropertyName("point100Score")]
        internal byte Point100Score
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
            init => this._scores.Add(ScoreFormat.POINT_100, value);
        }

        [JsonPropertyName("point10Score")]
        internal byte Point10Score
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
            init => this._scores.Add(ScoreFormat.POINT_10, value);
        }

        [JsonPropertyName("point5Score")]
        internal byte Point5Score
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
            init => this._scores.Add(ScoreFormat.POINT_5, value);
        }

        [JsonPropertyName("point3Score")]
        internal byte Point3Score
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
            init => this._scores.Add(ScoreFormat.POINT_3, value);
        }

        [JsonPropertyName("id")]
        public ulong Id { get; init; }

        public string GetScore(ScoreFormat scoreFormat)
        {
            return scoreFormat switch
            {
                ScoreFormat.POINT_100 => $"{this._scores[scoreFormat].ToString()}/100",
                ScoreFormat.POINT_10_DECIMAL => $"{(this._scores[ScoreFormat.POINT_100] * 1.0d / 10).ToString(CultureInfo.InvariantCulture)}/10",
                ScoreFormat.POINT_10 => $"{this._scores[scoreFormat].ToString()}/10",
                ScoreFormat.POINT_5 => $"{this._scores[scoreFormat].ToString()}/5",
                ScoreFormat.POINT_3 => this._scores[scoreFormat] switch
                {
                    1 => ":(",
                    2 => ":|",
                    3 => ":)",
                    _ => throw new ArgumentOutOfRangeException(nameof(scoreFormat), scoreFormat, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(scoreFormat), scoreFormat, null)
            };
        }
    }
}