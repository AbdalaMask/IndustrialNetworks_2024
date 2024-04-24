using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(LWORD))]
public struct LWORD : IComparable
{
	public string Value { get; set; }

	public static implicit operator LWORD(ulong value)
	{
		return new LWORD(value);
	}

	public static implicit operator LWORD(string value)
	{
		return new LWORD(value);
	}

	public LWORD(ulong data)
	{
		Value = data.ToString("X16");
	}

	public LWORD(string value_hex)
	{
		Validate(value_hex.Trim());
		Value = value_hex.Trim();
	}

	public LWORD(byte[] values)
	{
		Value = BitConverter.ToUInt64(values).ToString("X16");
	}

	public static LWORD Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		Validate(value);
		if (typeStyles == TypeStyles.HexNumber)
		{
			return new LWORD(BYTE.SortHexToWriteWordType(value, byteOrder));
		}
		return new LWORD(ulong.Parse(value));
	}

	public static LWORD[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		LWORD[] array = new LWORD[value_hex.Length / 16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 16, 16), byteOrder);
		}
		return array;
	}

	public static string ToHex(LWORD data, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		Validate(data.Value);
		return BYTE.SortHex(data.Value, byteOrder);
	}

	public static string ToHex(LWORD[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += ToHex(values[i], byteOrder);
		}
		return text;
	}

	public static LWORD Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new LWORD(BitConverter.ToUInt64(BYTE.SortBytes(values, byteOrder)));
	}

	public static LWORD[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		LWORD[] array = new LWORD[values.Length / 8];
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

	public static byte[] ToBytes(LWORD value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytesToWriteWordType(BYTE.GetBytesFromHex(value.Value), byteOrder);
	}

	public static byte[] ToBytes(LWORD[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	private static string ByteToHex(byte value)
	{
		return value.ToString("X2");
	}

	private static string BytesToHex(byte[] values)
	{
		return string.Join("", values.Select((byte byte_0) => byte_0.ToString("X2")));
	}

	private static void Validate(string value)
	{
		if (string.IsNullOrEmpty(value) || (!string.IsNullOrEmpty(value) && value.Length != 16))
		{
			throw new FormatException("LWORD: format wrong.");
		}
	}

	public int CompareTo(object? target)
	{
		if (target == null)
		{
			return 0;
		}
		return Value.CompareTo((LWORD)target);
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
