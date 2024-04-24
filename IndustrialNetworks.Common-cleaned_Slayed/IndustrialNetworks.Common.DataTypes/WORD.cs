using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(WORD))]
public struct WORD : IComparable
{
	public string Value { get; set; }

	public static implicit operator WORD(ushort value)
	{
		return new WORD(value);
	}

	public static implicit operator WORD(string value)
	{
		return new WORD(value);
	}

	public WORD(ushort data)
	{
		Value = data.ToString("X4");
	}

	public WORD(string value_hex)
	{
		Validate(value_hex.Trim());
		Value = value_hex.Trim();
	}

	public WORD(byte[] values)
	{
		Value = BitConverter.ToUInt16(values).ToString("X4");
	}

	public static WORD Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		if (typeStyles == TypeStyles.HexNumber)
		{
			return new WORD(BYTE.SortHex(value, byteOrder));
		}
		return new WORD(value);
	}

	public static WORD[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		WORD[] array = new WORD[value_hex.Length / 4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 4, 4), byteOrder);
		}
		return array;
	}

	public static string ToHex(WORD data, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		Validate(data.Value);
		return BYTE.SortHexToWriteWordType(data.Value, byteOrder);
	}

	public static string ToHex(WORD[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += ToHex(values[i], byteOrder);
		}
		return text;
	}

	public static WORD Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new WORD(BitConverter.ToUInt16(BYTE.SortBytes(values, byteOrder)));
	}

	public static WORD[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		WORD[] array = new WORD[values.Length / 2];
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

	public static byte[] ToBytes(WORD value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytesToWriteWordType(BYTE.GetBytesFromHex(value.Value), byteOrder);
	}

	public static byte[] ToBytes(WORD[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	private static void Validate(string value)
	{
		if (string.IsNullOrEmpty(value) || (!string.IsNullOrEmpty(value) && value.Length != 4))
		{
			throw new FormatException("WORD: format wrong.");
		}
	}

	public int CompareTo(object? target)
	{
		if (target == null)
		{
			return 0;
		}
		return Value.CompareTo((WORD)target);
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
