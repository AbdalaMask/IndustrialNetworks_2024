using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(UDINT))]
public struct UDINT : IComparable, IFormattable
{
	public const uint MaxValue = uint.MaxValue;

	public const uint MinValue = 0u;

	public uint Value { get; set; }

	public static implicit operator uint(UDINT value)
	{
		return value.Value;
	}

	public static implicit operator UDINT(uint value)
	{
		return new UDINT(value);
	}

	public static implicit operator UDINT(string value)
	{
		return new UDINT(value);
	}

	public UDINT(uint value)
	{
		Value = value;
	}

	public UDINT(string value, bool isHexa = false)
	{
		try
		{
			if (isHexa)
			{
				Value = uint.Parse(value, NumberStyles.HexNumber);
			}
			else
			{
				Value = uint.Parse(value);
			}
		}
		catch (OverflowException)
		{
			throw new OverflowException($"Value was either too large or too small for an UDINT. The range of values for UDINT values is from {0} to {-1}.");
		}
	}

	public UDINT(byte[] values)
	{
		Value = BitConverter.ToUInt32(values);
	}

	public static UDINT Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		if (typeStyles == TypeStyles.HexNumber)
		{
			return new UDINT(uint.Parse(BYTE.SortHex(value, byteOrder), NumberStyles.HexNumber));
		}
		return new UDINT(uint.Parse(value));
	}

	public static UDINT[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		UDINT[] array = new UDINT[value_hex.Length / 8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 8, 8), byteOrder);
		}
		return array;
	}

	public static string ToHex(UDINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.ToHex(BitConverter.GetBytes(value.Value), byteOrder);
	}

	public static string ToHex(UDINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += ToHex(values[i], byteOrder);
		}
		return text;
	}

	public static UDINT Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new UDINT(BitConverter.ToUInt32(BYTE.SortBytes(values, byteOrder)));
	}

	public static UDINT[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		UDINT[] array = new UDINT[values.Length / 4];
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

	public static byte[] ToBytes(UDINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(UDINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
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
		return Value.CompareTo((UDINT)target);
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
