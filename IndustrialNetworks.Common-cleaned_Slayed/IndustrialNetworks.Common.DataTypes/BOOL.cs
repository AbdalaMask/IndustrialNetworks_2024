using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(BOOL))]
public struct BOOL
{
	public bool Value { get; set; }

	public static implicit operator bool(BOOL value)
	{
		return value.Value;
	}

	public static implicit operator BOOL(bool value)
	{
		return new BOOL(value);
	}

	public static implicit operator BOOL(string value)
	{
		return new BOOL(value);
	}

	public BOOL(bool value)
	{
		Value = value;
	}

	public BOOL(string value)
	{
		Value = bool.Parse(value);
	}

	public static BOOL Parse(string value_hex)
	{
		return new BOOL(short.Parse(value_hex, NumberStyles.HexNumber) > 0);
	}

	public static BOOL[] ToArray(string value_hex)
	{
		return GetBools(BYTE.FromHex(value_hex));
	}

	public static BOOL[] ParseArray(string value_hex, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return GetBools(BYTE.FromHex(value_hex), byteOrder);
	}

	public static string ToHex(BOOL[] values)
	{
		return BYTE.ToHex(ToBYTEs(values));
	}

	public static string ToHex(BOOL value)
	{
		return Convert.ToByte(value).ToString("X2");
	}

	public static BOOL[] GetBool(BYTE value)
	{
		return new BOOL[8]
		{
			new BOOL(((byte)value & 1) != 0),
			new BOOL(((byte)value & 2) != 0),
			new BOOL(((byte)value & 4) != 0),
			new BOOL(((byte)value & 8) != 0),
			new BOOL(((byte)value & 0x10) != 0),
			new BOOL(((byte)value & 0x20) != 0),
			new BOOL(((byte)value & 0x40) != 0),
			new BOOL(((byte)value & 0x80) != 0)
		};
	}

	public static BOOL[] GetBools(BYTE[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return values.SelectMany((BYTE byte_0) => GetBool(byte_0)).ToArray();
		}
		List<BOOL> list = new List<BOOL>();
		for (int i = 0; i < values.Length; i += 2)
		{
			list.AddRange(GetBool(values[i + 1]));
			list.AddRange(GetBool(values[i]));
		}
		return list.ToArray();
	}

	public static BOOL[] GetBools(byte[] values)
	{
		return values.SelectMany((byte byte_0) => GetBool(byte_0)).ToArray();
	}

	public static BOOL[] GetBools(byte[] values, ByteOrder byteOrder)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return values.SelectMany((byte byte_0) => GetBool(byte_0)).ToArray();
		}
		List<BOOL> list = new List<BOOL>();
		for (int i = 0; i < values.Length; i += 2)
		{
			list.AddRange(GetBool(values[i + 1]));
			list.AddRange(GetBool(values[i]));
		}
		return list.ToArray();
	}

	public static byte[] ToBytes(bool value)
	{
		byte b = Convert.ToByte(value);
		return new byte[1] { b };
	}

	public static byte[] ToBytes(BOOL value)
	{
		byte b = Convert.ToByte(value);
		return new byte[1] { b };
	}

	public static byte[] ToBYTEs(BOOL[] values)
	{
		byte[] array = new byte[values.Length / 8 + ((values.Length % 8 != 0) ? 1 : 0)];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < values.Length; i++)
		{
			if ((bool)values[i])
			{
				array[num] |= (byte)(1 << num2);
			}
			num2++;
			if (num2 == 8)
			{
				num2 = 0;
				num++;
			}
		}
		return array;
	}

	public static byte[] ToBytes(BOOL[] values)
	{
		byte[] array = new byte[values.Length / 8 + ((values.Length % 8 != 0) ? 1 : 0)];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < values.Length; i++)
		{
			if ((bool)values[i])
			{
				array[num] |= (byte)(1 << num2);
			}
			num2++;
			if (num2 == 8)
			{
				num2 = 0;
				num++;
			}
		}
		return array;
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
