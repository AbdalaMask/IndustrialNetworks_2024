using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(REAL))]
public struct REAL : IComparable, IFormattable
{
	public const float MaxValue = float.MaxValue;

	public const float MinValue = float.MinValue;

	public float Value { get; set; }

	public static implicit operator float(REAL value)
	{
		return value.Value;
	}

	public static implicit operator REAL(float value)
	{
		return new REAL(value);
	}

	public static implicit operator REAL(string value)
	{
		return new REAL(value);
	}

	public REAL(float value)
	{
		Value = value;
	}

	public REAL(string value, bool isHexa = false)
	{
		try
		{
			if (isHexa)
			{
				byte[] bytes = BitConverter.GetBytes(int.Parse(value, NumberStyles.HexNumber));
				Value = BitConverter.ToSingle(bytes, 0);
			}
			else
			{
				Value = float.Parse(value);
			}
		}
		catch (OverflowException)
		{
			throw new OverflowException($"Value was either too large or too small for an REAL. The range of values for REAL values is from {float.MinValue} to {float.MaxValue}.");
		}
	}

	public REAL(byte[] values)
	{
		Value = BitConverter.ToSingle(values);
	}

	public static REAL Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
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
		return new REAL(float.Parse(value));
	}

	public static REAL[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		REAL[] array = new REAL[value_hex.Length / 8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 8, 8), byteOrder);
		}
		return array;
	}

	public static string ToHex(REAL value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.ToHex(BitConverter.GetBytes(value.Value), byteOrder);
	}

	public static string ToHex(REAL[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += ToHex(values[i], byteOrder);
		}
		return text;
	}

	public static REAL Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] array = BYTE.SortBytes(values, byteOrder);
		return new REAL(BitConverter.ToSingle(array));
	}

	public static REAL[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		REAL[] array = new REAL[values.Length / 4];
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

	public static byte[] ToBytes(REAL value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(REAL[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
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
		return Value.CompareTo((REAL)target);
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
