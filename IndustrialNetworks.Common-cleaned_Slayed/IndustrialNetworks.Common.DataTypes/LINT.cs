using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(LINT))]
public struct LINT : IComparable, IFormattable
{
	public const long MaxValue = long.MaxValue;

	public const long MinValue = long.MinValue;

	public long Value { get; set; }

	public static implicit operator long(LINT value)
	{
		return value.Value;
	}

	public static implicit operator LINT(long value)
	{
		return new LINT(value);
	}

	public static implicit operator LINT(string value)
	{
		return new LINT(value);
	}

	public LINT(long value)
	{
		Value = value;
	}

	public LINT(string value, bool isHexa = false)
	{
		try
		{
			if (isHexa)
			{
				Value = long.Parse(value, NumberStyles.HexNumber);
			}
			else
			{
				Value = long.Parse(value);
			}
		}
		catch (OverflowException)
		{
			throw new OverflowException($"Value was either too large or too small for a LINT. The range of values for LINT values is from {long.MinValue} to {long.MaxValue}.");
		}
	}

	public LINT(byte[] values)
	{
		Value = BitConverter.ToInt64(values);
	}

	public static LINT Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		if (typeStyles == TypeStyles.HexNumber)
		{
			return new LINT(long.Parse(BYTE.SortHex(value, byteOrder), NumberStyles.HexNumber));
		}
		return new LINT(long.Parse(value));
	}

	public static LINT[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		LINT[] array = new LINT[value_hex.Length / 16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 16, 16), byteOrder);
		}
		return array;
	}

	public static string ToHex(LINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.ToHex(BitConverter.GetBytes(value.Value), byteOrder);
	}

	public static string ToHex(LINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (long num = 0L; num < values.Length; num++)
		{
			text += ToHex(values[num], byteOrder);
		}
		return text;
	}

	public static LINT Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new LINT(BitConverter.ToInt64(BYTE.SortBytes(values, byteOrder)));
	}

	public static LINT[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		LINT[] array = new LINT[values.Length / 8];
		for (int i = 0; i < values.Length; i += 8)
		{
			array[i / 8] = Parse(new byte[8]
			{
				values[i],
				values[i + 1],
				values[i + 2],
				values[i + 3],
				values[i + 4],
				values[i + 5],
				values[i + 6],
				values[i + 7]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] ToBytes(LINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(LINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public int CompareTo(object? target)
	{
		if (target == null)
		{
			return 0;
		}
		return Value.CompareTo((LINT)target);
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	public string ToString(string? format, IFormatProvider? formatProvider)
	{
		if (string.IsNullOrEmpty(format))
		{
			return Value.ToString();
		}
		return Value.ToString(format);
	}
}
