using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(TIME32))]
public struct TIME32 : IComparable, IFormattable
{
	private string displayFormat;

	public TimeType Type { get; set; }

	public TimeSpan Value { get; set; }

	public static implicit operator TIME32(TimeSpan value)
	{
		return new TIME32(value);
	}

	public static implicit operator TIME32(string value)
	{
		return new TIME32(value);
	}

	public TIME32(TimeSpan value, string format = "hh\\:mm\\:ss\\.ff")
	{
		Type = TimeType.NORMAL;
		displayFormat = "hh\\:mm\\:ss\\.ff";
		displayFormat = format;
		Value = value;
	}

	public TIME32(string value, int resolution = 1, bool isHexa = false, string format = "hh\\:mm\\:ss\\.ff")
	{
		Type = TimeType.NORMAL;
		displayFormat = "hh\\:mm\\:ss\\.ff";
		displayFormat = format;
		if (isHexa)
		{
			Value = TimeSpan.FromMilliseconds(resolution * uint.Parse(value, NumberStyles.HexNumber));
		}
		else
		{
			Value = TimeSpan.Parse(value);
		}
	}

	public TIME32(byte[] bytes, int resolution)
	{
		Type = TimeType.NORMAL;
		displayFormat = "hh\\:mm\\:ss\\.ff";
		uint num = BitConverter.ToUInt32(bytes);
		Value = TimeSpan.FromMilliseconds(resolution * num);
	}

	public static TIME32 Parse(byte[] values, int resolution)
	{
		int num = BitConverter.ToInt32(values);
		return new TIME32(TimeSpan.FromMilliseconds(resolution * num));
	}

	public static TIME32 Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		int num = BitConverter.ToInt32(BYTE.SortBytes(values, byteOrder));
		return new TIME32(TimeSpan.FromMilliseconds(resolution * num));
	}

	public static TIME32[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		TIME32[] array = new TIME32[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = Parse(new byte[4]
			{
				values[i],
				values[i + 1],
				values[i + 2],
				values[i + 3]
			}, byteOrder, resolution);
		}
		return array;
	}

	public static byte[] ToBytes(TIME32 value, ByteOrder byteOrder, int resolution = 1)
	{
		return BYTE.SortBytes(BitConverter.GetBytes((int)(value.Value.TotalMilliseconds / (double)resolution)), byteOrder);
	}

	public static byte[] ToBytes(TIME32[] values, ByteOrder byteOrder, int resolution = 1)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ToBytes(values[i], byteOrder, resolution));
		}
		return list.ToArray();
	}

	public static TIME32 Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		string s = ((byteOrder == ByteOrder.BigEndian || byteOrder != ByteOrder.LittleEndian) ? (value.Substring(2, 2) + value.Substring(0, 2) + value.Substring(6, 2) + value.Substring(4, 2)) : (value.Substring(0, 2) + value.Substring(2, 2) + value.Substring(4, 2) + value.Substring(6, 2)));
		TimeSpan value2 = ((typeStyles != TypeStyles.HexNumber) ? TimeSpan.Parse(s) : TimeSpan.FromMilliseconds(resolution * int.Parse(s, NumberStyles.HexNumber)));
		return new TIME32(value2);
	}

	public static TIME32[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		TIME32[] array = new TIME32[value_hex.Length / 8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 8, 8), byteOrder, resolution);
		}
		return array;
	}

	public static int[] ToDINT(TIME32[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		int[] array = new int[values.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (int)(values[i].Value.TotalMilliseconds / (double)resolution);
		}
		return array;
	}

	public static string ToHex(TIME32 value, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		return DINT.ToHex((int)(value.Value.TotalMilliseconds / (double)resolution), byteOrder);
	}

	public static string ToHex(TIME32[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += DINT.ToHex((int)(values[i].Value.TotalMilliseconds / (double)resolution), byteOrder);
		}
		return text;
	}

	public int CompareTo(object? target)
	{
		if (target == null)
		{
			return 0;
		}
		return Value.CompareTo((TIME32)target);
	}

	public override string ToString()
	{
		if (Value.Milliseconds == 0)
		{
			return Value.ToString(displayFormat).Replace(".000", "");
		}
		return Value.ToString(displayFormat);
	}

	public string ToString(string format)
	{
		return Value.ToString(format);
	}

	public string ToString(string? format, IFormatProvider? formatProvider)
	{
		if (string.IsNullOrEmpty(format))
		{
			return Value.ToString();
		}
		return Value.ToString(format);
	}

	public void SetDisplayFormat(string format)
	{
		if (string.IsNullOrEmpty(format) || string.IsNullOrWhiteSpace(format))
		{
			format = "hh\\:mm\\:ss\\.ff";
		}
		displayFormat = format;
	}
}
