using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(TIME16))]
public struct TIME16 : IComparable
{
	private string displayFormat;

	public TimeSpan Value { get; set; }

	public static implicit operator TIME16(TimeSpan value)
	{
		return new TIME16(value);
	}

	public static implicit operator TIME16(string value)
	{
		return new TIME16(value);
	}

	public TIME16(TimeSpan value)
	{
		displayFormat = "hh\\:mm\\:ss\\.ff";
		Value = value;
	}

	public TIME16(string value, int resolution = 1, bool isHexa = false)
	{
		displayFormat = "hh\\:mm\\:ss\\.ff";
		if (isHexa)
		{
			Value = TimeSpan.FromMilliseconds(resolution * ushort.Parse(value, NumberStyles.HexNumber));
		}
		else
		{
			Value = TimeSpan.Parse(value);
		}
	}

	public TIME16(byte[] bytes, int resolution)
	{
		displayFormat = "hh\\:mm\\:ss\\.ff";
		ushort num = BitConverter.ToUInt16(bytes);
		Value = TimeSpan.FromMilliseconds(resolution * num);
	}

	public static TIME16 Parse(byte[] values, int resolution)
	{
		ushort num = BitConverter.ToUInt16(values);
		return new TIME16(TimeSpan.FromMilliseconds(resolution * num));
	}

	public static TIME16 Parse(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		ushort num = BitConverter.ToUInt16(BYTE.SortBytes(values, byteOrder));
		return new TIME16(TimeSpan.FromMilliseconds(resolution * num));
	}

	public static TIME16[] ParseArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		TIME16[] array = new TIME16[values.Length / 2];
		for (int i = 0; i < values.Length; i += 2)
		{
			array[i / 2] = Parse(new byte[2]
			{
				values[i],
				values[i + 1]
			}, byteOrder, resolution);
		}
		return array;
	}

	public static byte[] ToBytes(TIME16 value, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		return BYTE.SortBytes(BitConverter.GetBytes((ushort)(value.Value.TotalMilliseconds / (double)resolution)), byteOrder);
	}

	public static byte[] ToBytes(TIME16[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ToBytes(values[i], byteOrder, resolution));
		}
		return list.ToArray();
	}

	public static TIME16 Parse(string value, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1, TypeStyles typeStyles = TypeStyles.HexNumber)
	{
		string s = ((byteOrder == ByteOrder.BigEndian || byteOrder != ByteOrder.LittleEndian) ? (value.Substring(2, 2) + value.Substring(0, 2)) : value);
		return new TIME16(typeStyles switch
		{
			TypeStyles.HexNumber => TimeSpan.FromMilliseconds(resolution * ushort.Parse(s, NumberStyles.HexNumber)), 
			TypeStyles.Decimal => TimeSpan.FromMilliseconds(resolution * ushort.Parse(s)), 
			_ => TimeSpan.Parse(s), 
		});
	}

	public static TIME16[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		TIME16[] array = new TIME16[value_hex.Length / 4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Parse(value_hex.Substring(i * 4, 4), byteOrder, resolution);
		}
		return array;
	}

	public static ushort[] ToINT(TIME16[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		ushort[] array = new ushort[values.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (ushort)(values[i].Value.TotalMilliseconds / (double)resolution);
		}
		return array;
	}

	public static string ToHex(TIME16 value, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		return UINT.ToHex((ushort)(value.Value.TotalMilliseconds / (double)resolution), byteOrder);
	}

	public static string ToHex(TIME16[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			text += UINT.ToHex((ushort)(values[i].Value.TotalMilliseconds / (double)resolution), byteOrder);
		}
		return text;
	}

	public int CompareTo(object? target)
	{
		if (target == null)
		{
			return 0;
		}
		return Value.CompareTo((TIME16)target);
	}

	public override string ToString()
	{
		return Value.ToString("hh\\:mm\\:ss\\.ff");
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
