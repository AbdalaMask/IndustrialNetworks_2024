using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(UINT))]
public struct UINT : IComparable, IFormattable
{
	public const ushort MaxValue = ushort.MaxValue;

	public const ushort MinValue = 0;

	public ushort Value { get; set; }

	public static implicit operator ushort(UINT value)
	{
		return value.Value;
	}

	public static implicit operator UINT(ushort value)
	{
		return new UINT(value);
	}

	public static implicit operator UINT(string value)
	{
		return new UINT(value);
	}

	public UINT(ushort value)
	{
		Value = value;
	}

	public UINT(string value, bool isHexa = false)
	{
		try
		{
			if (isHexa)
			{
				Value = ushort.Parse(value, NumberStyles.HexNumber);
			}
			else
			{
				Value = ushort.Parse(value);
			}
		}
		catch (OverflowException)
		{
			throw new OverflowException($"Value was either too large or too small for an UINT. The range of values for UINT values is from {0} to {65535}.");
		}
	}

	public UINT(byte[] values)
	{
		Value = BitConverter.ToUInt16(values);
	}

	public static UINT Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		if (typeStyles == TypeStyles.HexNumber)
		{
			return Parse(BitConverter.GetBytes(Convert.ToUInt16(value, 16)), byteOrder);
		}
		return new UINT(ushort.Parse(value));
	}

	public static UINT[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		UINT[] array = new UINT[value_hex.Length / 4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 4, 4), byteOrder);
		}
		return array;
	}

	public static string ToHex(UINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.ToHex(BitConverter.GetBytes(value.Value), byteOrder);
	}

	public static string ToHex(UINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += ToHex(values[i], byteOrder);
		}
		return text;
	}

	public static UINT Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new UINT(BitConverter.ToUInt16(BYTE.SortBytes(values, byteOrder)));
	}

	public static UINT[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		UINT[] array = new UINT[values.Length / 2];
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

	public static byte[] ToBytes(UINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(UINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
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
		return Value.CompareTo((UINT)target);
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
