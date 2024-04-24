using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(DWORD))]
public struct DWORD : IComparable
{
	public string Value { get; set; }

	public static implicit operator DWORD(uint value)
	{
		return new DWORD(value);
	}

	public static implicit operator DWORD(string value)
	{
		return new DWORD(value);
	}

	public DWORD(uint data)
	{
		Value = data.ToString("X8");
	}

	public DWORD(string value_hex)
	{
		Validate(value_hex.Trim());
		Value = value_hex.Trim();
	}

	public DWORD(byte[] values)
	{
		Value = BitConverter.ToUInt32(values).ToString("X8");
	}

	public static DWORD Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		Validate(value);
		if (typeStyles == TypeStyles.HexNumber)
		{
			return new DWORD(BYTE.SortHex(value, byteOrder));
		}
		return new DWORD(uint.Parse(value));
	}

	public static DWORD[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		DWORD[] array = new DWORD[value_hex.Length / 8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 8, 8), byteOrder);
		}
		return array;
	}

	public static string ToHex(DWORD data, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		Validate(data.Value);
		return BYTE.SortHexToWriteWordType(data.Value, byteOrder);
	}

	public static string ToHex(DWORD[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += ToHex(values[i], byteOrder);
		}
		return text;
	}

	public static DWORD Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new DWORD(BitConverter.ToUInt32(BYTE.SortBytes(values, byteOrder)));
	}

	public static DWORD[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		DWORD[] array = new DWORD[values.Length / 4];
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

	public static byte[] ToBytes(DWORD value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BYTE.SortBytesToWriteWordType(BYTE.GetBytesFromHex(value.Value), byteOrder);
	}

	public static byte[] ToBytes(DWORD[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
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
		if (string.IsNullOrEmpty(value) || (!string.IsNullOrEmpty(value) && value.Length != 8))
		{
			throw new FormatException("DWORD: format wrong.");
		}
	}

	public int CompareTo(object? target)
	{
		if (target == null)
		{
			return 0;
		}
		return Value.CompareTo((DWORD)target);
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
