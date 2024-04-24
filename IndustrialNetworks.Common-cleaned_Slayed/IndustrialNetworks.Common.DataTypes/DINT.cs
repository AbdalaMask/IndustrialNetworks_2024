using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(DINT))]
public struct DINT : IComparable, IFormattable
{
	public const int MaxValue = int.MaxValue;

	public const int MinValue = int.MinValue;

	public int Value { get; set; }

	public static implicit operator int(DINT value)
	{
		return value.Value;
	}

	public static implicit operator DINT(int value)
	{
		return new DINT(value);
	}

	public static implicit operator DINT(string value)
	{
		return new DINT(value);
	}

	public DINT(int value)
	{
		Value = value;
	}

	public DINT(string value, bool isHexa = false)
	{
		try
		{
			if (isHexa)
			{
				Value = int.Parse(value, NumberStyles.HexNumber);
			}
			else
			{
				Value = int.Parse(value);
			}
		}
		catch (OverflowException)
		{
			throw new OverflowException($"Value was either too large or too small for a DINT. The range of values for DINT values is from {int.MinValue} to {int.MaxValue}.");
		}
	}

	public DINT(byte[] values)
	{
		Value = BitConverter.ToInt32(values);
	}

	public static DINT Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		if (typeStyles == TypeStyles.HexNumber)
		{
			return new DINT(int.Parse(BYTE.SortHex(value, byteOrder), NumberStyles.HexNumber));
		}
		return new DINT(int.Parse(value));
	}

	public static DINT[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		DINT[] array = new DINT[value_hex.Length / 8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 8, 8), byteOrder);
		}
		return array;
	}

	public static string ToHex(DINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.ToHex(BitConverter.GetBytes(value.Value), byteOrder);
	}

	public static string ToHex(DINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += ToHex(values[i], byteOrder);
		}
		return text;
	}

	public static DINT Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new DINT(BitConverter.ToInt32(BYTE.SortBytes(values, byteOrder)));
	}

	public static DINT[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		DINT[] array = new DINT[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = Parse(new byte[4]
			{
				values[i],
				values[i + 1],
				values[i + 2],
				values[i + 3]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] ToBytes(DINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(DINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
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
		return Value.CompareTo((DINT)target);
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
