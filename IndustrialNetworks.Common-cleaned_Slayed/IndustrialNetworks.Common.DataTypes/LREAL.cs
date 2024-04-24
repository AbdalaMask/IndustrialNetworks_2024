using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(LREAL))]
public struct LREAL : IComparable, IFormattable
{
	public const double MaxValue = double.MaxValue;

	public const double MinValue = double.MinValue;

	public double Value { get; set; }

	public static implicit operator double(LREAL value)
	{
		return value.Value;
	}

	public static implicit operator LREAL(double value)
	{
		return new LREAL(value);
	}

	public static implicit operator LREAL(string value)
	{
		return new LREAL(value);
	}

	public LREAL(double value)
	{
		Value = value;
	}

	public LREAL(string value, bool isHexa = false)
	{
		try
		{
			if (isHexa)
			{
				byte[] bytes = BitConverter.GetBytes(ulong.Parse(value, NumberStyles.HexNumber));
				Value = BitConverter.ToDouble(bytes, 0);
			}
			else
			{
				Value = double.Parse(value);
			}
		}
		catch (OverflowException)
		{
			throw new OverflowException($"Value was either too large or too small for an LREAL. The range of values for LREAL values is from {double.MinValue} to {double.MaxValue}.");
		}
	}

	public LREAL(byte[] values)
	{
		Value = BitConverter.ToDouble(values);
	}

	public static LREAL Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		if (typeStyles == TypeStyles.HexNumber)
		{
			byte[] array = new byte[value.Length / 2];
			for (int i = 0; i < value.Length; i += 2)
			{
				array[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
			}
			return Parse(array, byteOrder);
		}
		return new LREAL(double.Parse(value));
	}

	public static LREAL[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		LREAL[] array = new LREAL[value_hex.Length / 16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 16, 16), byteOrder);
		}
		return array;
	}

	public static string ToHex(LREAL value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.ToHex(BitConverter.GetBytes(value.Value), byteOrder);
	}

	public static string ToHex(LREAL[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += ToHex(values[i], byteOrder);
		}
		return text;
	}

	public static LREAL Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new LREAL(BitConverter.ToDouble(BYTE.SortBytes(values, byteOrder)));
	}

	public static LREAL[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		LREAL[] array = new LREAL[values.Length / 8];
		for (int i = 0; i < values.Length; i += 8)
		{
			byte[] values2 = new byte[8]
			{
				values[i],
				values[i + 1],
				values[i + 2],
				values[i + 3],
				values[i + 4],
				values[i + 5],
				values[i + 6],
				values[i + 7]
			};
			array[i / 8] = Parse(values2, byteOrder);
		}
		return array;
	}

	public static byte[] ToBytes(LREAL value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(LREAL[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
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
		return Value.CompareTo((LREAL)target);
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
