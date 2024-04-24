using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(INT))]
public struct INT : IComparable, IFormattable
{
	public const short MaxValue = short.MaxValue;

	public const short MinValue = short.MinValue;

	public short Value { get; set; }

	public static implicit operator short(INT value)
	{
		return value.Value;
	}

	public static implicit operator INT(short value)
	{
		return new INT(value);
	}

	public static implicit operator INT(string value)
	{
		return new INT(value);
	}

	public INT(short value)
	{
		Value = value;
	}

	public INT(string value, bool isHexa = false)
	{
		try
		{
			if (isHexa)
			{
				Value = short.Parse(value, NumberStyles.HexNumber);
			}
			else
			{
				Value = short.Parse(value);
			}
		}
		catch (OverflowException)
		{
			throw new OverflowException($"Value was either too large or too small for an INT. The range of values for INT values is from {-32768} to {32767}.");
		}
	}

	public INT(byte[] values)
	{
		Value = BitConverter.ToInt16(values);
	}

	public static INT Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		if (typeStyles == TypeStyles.HexNumber)
		{
			return Parse(BitConverter.GetBytes(Convert.ToInt16(value, 16)), byteOrder);
		}
		return new INT(short.Parse(value));
	}

	public static INT[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		INT[] array = new INT[value_hex.Length / 4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 4, 4), byteOrder);
		}
		return array;
	}

	public static string ToHex(INT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.ToHex(BitConverter.GetBytes(value.Value), byteOrder);
	}

	public static string ToHex(INT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += ToHex(values[i], byteOrder);
		}
		return text;
	}

	public static INT Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new INT(BitConverter.ToInt16(BYTE.SortBytes(values, byteOrder)));
	}

	public static INT[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		INT[] array = new INT[values.Length / 2];
		for (int i = 0; i < values.Length; i += 2)
		{
			array[i / 2] = Parse(new byte[2]
			{
				values[i],
				values[i + 1]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] ToBytes(INT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(INT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
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
		return Value.CompareTo((INT)target);
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
