using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(ULINT))]
public struct ULINT : IComparable, IFormattable
{
	public const ulong MaxValue = ulong.MaxValue;

	public const ulong MinValue = 0uL;

	public ulong Value { get; set; }

	public static implicit operator ulong(ULINT value)
	{
		return value.Value;
	}

	public static implicit operator ULINT(ulong value)
	{
		return new ULINT(value);
	}

	public static implicit operator ULINT(string value)
	{
		return new ULINT(value);
	}

	public ULINT(ulong value)
	{
		Value = value;
	}

	public ULINT(string value, bool isHexa = false)
	{
		try
		{
			if (isHexa)
			{
				Value = ulong.Parse(value, NumberStyles.HexNumber);
			}
			else
			{
				Value = ulong.Parse(value);
			}
		}
		catch (OverflowException)
		{
			throw new OverflowException($"Value was either too large or too small for a ULINT. The range of values for ULINT values is from {0L} to {-1L}.");
		}
	}

	public ULINT(byte[] values)
	{
		Value = BitConverter.ToUInt64(values);
	}

	public static ULINT Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		if (typeStyles == TypeStyles.HexNumber && typeStyles == TypeStyles.HexNumber)
		{
			return new ULINT(ulong.Parse(BYTE.SortHex(value, byteOrder), NumberStyles.HexNumber));
		}
		return new ULINT(ulong.Parse(value));
	}

	public static ULINT[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		ULINT[] array = new ULINT[value_hex.Length / 16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 16, 16), byteOrder);
		}
		return array;
	}

	public static string ToHex(ULINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.ToHex(BitConverter.GetBytes(value.Value), byteOrder);
	}

	public static string ToHex(ULINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += ToHex(values[i], byteOrder);
		}
		return text;
	}

	public static ULINT Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new ULINT(BitConverter.ToUInt64(BYTE.SortBytes(values, byteOrder)));
	}

	public static ULINT[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		ULINT[] array = new ULINT[values.Length / 8];
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

	public static byte[] ToBytes(ULINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(ULINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
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
		return Value.CompareTo((ULINT)target);
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
