using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(STRING))]
public struct STRING : IComparable
{
	public string Value { get; set; }

	public static implicit operator string(STRING value)
	{
		return value.Value;
	}

	public static implicit operator STRING(string value)
	{
		return new STRING(value);
	}

	public STRING(string value)
	{
		Value = value;
	}

	public STRING(char[] chars)
	{
		Value = new string(chars);
	}

	public static STRING ToHex(STRING value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = GetBytes(value, Encoding.ASCII);
		string text = string.Empty;
		switch (byteOrder)
		{
		default:
			text = string.Join("", bytes.Select((byte byte_0) => byte_0.ToString("X2")));
			break;
		case ByteOrder.BigEndian:
		{
			for (int i = 0; i < bytes.Length; i += 2)
			{
				text = text + bytes[i + 1].ToString("X2") + bytes[i].ToString("X2");
			}
			break;
		}
		}
		return new STRING(text);
	}

	public static STRING HexToString(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytesFromHex = BYTE.GetBytesFromHex(value_hex);
		string text = string.Empty;
		switch (byteOrder)
		{
		default:
			text = string.Join("", bytesFromHex.Select((byte byte_0) => (char)byte_0));
			break;
		case ByteOrder.BigEndian:
		{
			for (int i = 0; i < bytesFromHex.Length; i += 2)
			{
				text += $"{bytesFromHex[i + 1]}{bytesFromHex[i]}";
			}
			break;
		}
		}
		if (text.Contains('\0'))
		{
			text = text.Substring(0, text.IndexOf('\0'));
		}
		return new STRING(text);
	}

	public static STRING GetString(byte[] values)
	{
		string text = string.Empty;
		foreach (byte b in values)
		{
			ReadOnlySpan<char> readOnlySpan = text;
			char reference = (char)b;
			text = string.Concat(readOnlySpan, new ReadOnlySpan<char>(ref reference));
		}
		return new STRING(text);
	}

	public static STRING GetString(byte[] values, Encoding encoding)
	{
		if (encoding == Encoding.ASCII)
		{
			return new STRING(Encoding.ASCII.GetString(values));
		}
		if (encoding == Encoding.Unicode)
		{
			return new STRING(Encoding.Unicode.GetString(values));
		}
		if (encoding == Encoding.BigEndianUnicode)
		{
			return new STRING(Encoding.BigEndianUnicode.GetString(values));
		}
		if (encoding == Encoding.UTF8)
		{
			return new STRING(Encoding.UTF8.GetString(values));
		}
		if (encoding == Encoding.UTF32)
		{
			return new STRING(Encoding.UTF32.GetString(values));
		}
		if (encoding == Encoding.Latin1)
		{
			return new STRING(Encoding.Latin1.GetString(values));
		}
		return new STRING(Encoding.ASCII.GetString(values));
	}

	public static byte[] GetBytes(STRING value, Encoding encoding)
	{
		byte[] array = Array.Empty<byte>();
		if (encoding == Encoding.ASCII)
		{
			return Encoding.ASCII.GetBytes(value);
		}
		if (encoding == Encoding.Unicode)
		{
			return Encoding.Unicode.GetBytes(value);
		}
		if (encoding == Encoding.BigEndianUnicode)
		{
			return Encoding.BigEndianUnicode.GetBytes(value);
		}
		if (encoding == Encoding.UTF8)
		{
			return Encoding.UTF8.GetBytes(value);
		}
		if (encoding == Encoding.UTF32)
		{
			return Encoding.UTF32.GetBytes(value);
		}
		if (encoding == Encoding.Latin1)
		{
			return Encoding.Latin1.GetBytes(value);
		}
		return Encoding.Default.GetBytes(value);
	}

	public static BYTE[] HexToBytes(STRING value_hex)
	{
		if (value_hex.Value.Length % 2 != 0)
		{
			throw new FormatException();
		}
		List<BYTE> list = new List<BYTE>();
		for (int i = 0; i < value_hex.Value.Length; i += 2)
		{
			list.Add(byte.Parse(value_hex.Value.Substring(i, 2) ?? "", NumberStyles.HexNumber));
		}
		return list.ToArray();
	}

	public static byte[] ToBytes(string value_hex)
	{
		if (value_hex.Length % 2 != 0)
		{
			throw new FormatException();
		}
		List<byte> list = new List<byte>();
		for (int i = 0; i < value_hex.Length; i += 2)
		{
			list.Add(byte.Parse(value_hex.Substring(i, 2) ?? "", NumberStyles.HexNumber));
		}
		return list.ToArray();
	}

	public int CompareTo(object? target)
	{
		if (target == null)
		{
			return 0;
		}
		return Value.CompareTo((STRING)target);
	}

	public override string ToString()
	{
		return Value;
	}
}
