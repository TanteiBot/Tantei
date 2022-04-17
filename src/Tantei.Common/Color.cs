// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Common;

public readonly partial struct Color : IEquatable<Color>, IComparable<Color>
{
	private const int RgbRedShift = 16;
	private const int RgbGreenShift = 8;
	private const int RgbBlueShift = 0;

	public int Value { get; }

	public Color(int value)
	{
		this.Value = value;
	}

	public Color(byte r, byte g, byte b)
	{
		this.Value = (r << RgbRedShift) | (g << RgbGreenShift) | b;
	}

	public byte R => (byte)(this.Value >> RgbRedShift);
	public byte G => (byte)(this.Value >> RgbGreenShift);
	public byte B => (byte)(this.Value >> RgbBlueShift);

	public bool Equals(Color other) => this.Value.Equals(other.Value);

	public int CompareTo(Color other) => this.Value.CompareTo(other.Value);

	public override bool Equals(object? obj) => obj is Color other && this.Equals(other);

	public override int GetHashCode() => this.Value.GetHashCode();

	public static bool operator ==(Color left, Color right) => left.Equals(right);

	public static bool operator !=(Color left, Color right) => !left.Equals(right);

	public override string ToString() => $"#{this.Value:X6}, R: {this.R}, G: {this.G}, B:{this.B}";

	public static bool operator <(Color left, Color right) => left.CompareTo(right) < 0;

	public static bool operator <=(Color left, Color right) => left.CompareTo(right) <= 0;

	public static bool operator >(Color left, Color right) => left.CompareTo(right) > 0;

	public static bool operator >=(Color left, Color right) => left.CompareTo(right) >= 0;
}