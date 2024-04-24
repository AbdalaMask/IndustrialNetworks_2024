using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace NetStudio.Common.DataTypes;

[KnownType(typeof(BYTE))]
public struct BYTE
{
	public const byte MaxValue = byte.MaxValue;

	public const byte MinValue = 0;

	public byte Value { get; set; }

	public static implicit operator byte(BYTE value)
	{
		return value.Value;
	}

	public static implicit operator BYTE(byte value)
	{
		return new BYTE(value);
	}

	public static implicit operator BYTE(string value)
	{
		return new BYTE(value);
	}

	public BYTE(byte value)
	{
		Value = value;
	}

	public BYTE(string value)
	{
		Value = byte.Parse(value);
	}

	public static byte[] SortBytes(byte[] values, ByteOrder byteOrder)
	{
		int num = values.Length;
		switch (num)
		{
		default:
			throw new InvalidDataException($"Byte order is undefined with the number of bytes={num}. The number of valid bytes is: 2, 4, 8 bytes");
		case 8:
			return byteOrder switch
			{
				ByteOrder.LittleEndian => new byte[8]
				{
					values[0],
					values[1],
					values[2],
					values[3],
					values[4],
					values[5],
					values[6],
					values[7]
				}, 
				ByteOrder.BigEndianByteSwap => new byte[8]
				{
					values[6],
					values[7],
					values[4],
					values[5],
					values[2],
					values[3],
					values[0],
					values[1]
				}, 
				ByteOrder.LittleEndianByteSwap => new byte[8]
				{
					values[1],
					values[0],
					values[3],
					values[2],
					values[5],
					values[4],
					values[7],
					values[6]
				}, 
				_ => new byte[8]
				{
					values[7],
					values[6],
					values[5],
					values[4],
					values[3],
					values[2],
					values[1],
					values[0]
				}, 
			};
		case 4:
			return byteOrder switch
			{
				ByteOrder.LittleEndian => new byte[4]
				{
					values[0],
					values[1],
					values[2],
					values[3]
				}, 
				ByteOrder.BigEndianByteSwap => new byte[4]
				{
					values[2],
					values[3],
					values[0],
					values[1]
				}, 
				ByteOrder.LittleEndianByteSwap => new byte[4]
				{
					values[1],
					values[0],
					values[3],
					values[2]
				}, 
				_ => new byte[4]
				{
					values[3],
					values[2],
					values[1],
					values[0]
				}, 
			};
		case 2:
			switch (byteOrder)
			{
			case ByteOrder.LittleEndian:
			case ByteOrder.BigEndianByteSwap:
				return new byte[2]
				{
					values[0],
					values[1]
				};
			default:
				return new byte[2]
				{
					values[1],
					values[0]
				};
			}
		}
	}

	public static byte[] SortBytesToWriteWordType(byte[] values, ByteOrder byteOrder)
	{
		int num = values.Length;
		switch (num)
		{
		default:
			throw new InvalidDataException($"Byte order is undefined with the number of bytes={num}. The number of valid bytes is: 2, 4, 8 bytes");
		case 8:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return new byte[8]
				{
					values[7],
					values[6],
					values[5],
					values[4],
					values[3],
					values[2],
					values[1],
					values[0]
				};
			}
			return new byte[8]
			{
				values[6],
				values[7],
				values[4],
				values[5],
				values[2],
				values[3],
				values[0],
				values[1]
			};
		case 4:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return new byte[4]
				{
					values[3],
					values[2],
					values[1],
					values[0]
				};
			}
			return new byte[4]
			{
				values[2],
				values[3],
				values[0],
				values[1]
			};
		case 2:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return new byte[2]
				{
					values[1],
					values[0]
				};
			}
			return new byte[2]
			{
				values[0],
				values[1]
			};
		}
	}

	public static string SortHex(string dataHex, ByteOrder byteOrder)
	{
		int length = dataHex.Length;
		switch (length)
		{
		default:
			throw new InvalidDataException($"Byte order is undefined with the number of bytes={length}. The number of valid bytes is: 2, 4, 8 bytes");
		case 16:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return dataHex.Substring(14, 2) + dataHex.Substring(12, 2) + dataHex.Substring(10, 2) + dataHex.Substring(8, 2) + dataHex.Substring(6, 2) + dataHex.Substring(4, 2) + dataHex.Substring(2, 2) + dataHex.Substring(0, 2);
			}
			return dataHex.Substring(12, 2) + dataHex.Substring(14, 2) + dataHex.Substring(8, 2) + dataHex.Substring(10, 2) + dataHex.Substring(4, 2) + dataHex.Substring(6, 2) + dataHex.Substring(0, 2) + dataHex.Substring(2, 2);
		case 8:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return dataHex.Substring(0, 2) + dataHex.Substring(2, 2) + dataHex.Substring(4, 2) + dataHex.Substring(6, 2);
			}
			return dataHex.Substring(4, 2) + dataHex.Substring(6, 2) + dataHex.Substring(0, 2) + dataHex.Substring(2, 2);
		case 4:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return dataHex;
			}
			return dataHex.Substring(2, 2) + dataHex.Substring(0, 2);
		}
	}

	public static string SortHexToWriteWordType(string dataHex, ByteOrder byteOrder)
	{
		int length = dataHex.Length;
		switch (length)
		{
		default:
			throw new InvalidDataException($"Byte order is undefined with the number of bytes={length}. The number of valid bytes is: 2, 4, 8 bytes");
		case 16:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return dataHex.Substring(14, 2) + dataHex.Substring(12, 2) + dataHex.Substring(10, 2) + dataHex.Substring(8, 2) + dataHex.Substring(6, 2) + dataHex.Substring(4, 2) + dataHex.Substring(2, 2) + dataHex.Substring(0, 2);
			}
			return dataHex.Substring(12, 2) + dataHex.Substring(14, 2) + dataHex.Substring(8, 2) + dataHex.Substring(10, 2) + dataHex.Substring(4, 2) + dataHex.Substring(6, 2) + dataHex.Substring(0, 2) + dataHex.Substring(2, 2);
		case 8:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return dataHex.Substring(6, 2) + dataHex.Substring(4, 2) + dataHex.Substring(2, 2) + dataHex.Substring(0, 2);
			}
			return dataHex.Substring(4, 2) + dataHex.Substring(6, 2) + dataHex.Substring(0, 2) + dataHex.Substring(2, 2);
		case 4:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return dataHex;
			}
			return dataHex.Substring(2, 2) + dataHex.Substring(0, 2);
		}
	}

	public static byte GetByteFromHex(string IP)
	{
		return byte.Parse(IP.PadLeft(2, '0'), NumberStyles.HexNumber);
	}

	public static BYTE[] FromHex(string IP)
	{
		BYTE[] array = new BYTE[IP.Length / 2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new BYTE(Convert.ToByte(IP.Substring(i * 2, 2), 16));
		}
		return array;
	}

	public static byte[] GetBytesFromHex(string IP)
	{
		byte[] array = new byte[IP.Length / 2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToByte(IP.Substring(i * 2, 2), 16);
		}
		return array;
	}

	public static string ToHex(byte value)
	{
		return value.ToString("X2");
	}

	public static string ToHex(BYTE[] values)
	{
		return string.Join("", values.Select((BYTE byte_0) => byte_0.Value.ToString("X2")));
	}

	public static string ToHex(byte[] values)
	{
		return string.Join("", values.Select((byte byte_0) => byte_0.ToString("X2")));
	}

	public static string ToHex(byte[] values, ByteOrder byteOrder)
	{
		int num = values.Length;
		switch (num)
		{
		default:
			throw new InvalidDataException($"Byte order is undefined with the number of bytes={num}. The number of valid bytes is: 2, 4, 8 bytes");
		case 8:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return ToHex(new byte[8]
				{
					values[0],
					values[1],
					values[2],
					values[3],
					values[4],
					values[5],
					values[6],
					values[7]
				});
			}
			return ToHex(new byte[8]
			{
				values[1],
				values[0],
				values[3],
				values[2],
				values[5],
				values[4],
				values[7],
				values[6]
			});
		case 4:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return ToHex(new byte[4]
				{
					values[0],
					values[1],
					values[2],
					values[3]
				});
			}
			return ToHex(new byte[4]
			{
				values[1],
				values[0],
				values[3],
				values[2]
			});
		case 2:
			if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
			{
				return ToHex(new byte[2]
				{
					values[0],
					values[1]
				});
			}
			return ToHex(new byte[2]
			{
				values[1],
				values[0]
			});
		}
	}

	public static INT ToInt(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new INT(BitConverter.ToInt16(SortBytes(values, byteOrder)));
	}

	public static INT[] ToIntArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		INT[] array = new INT[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = ToInt(new byte[2]
			{
				values[i],
				values[i + 1]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] ToBytes(BYTE[] values)
	{
		byte[] array = Array.Empty<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			array[i] = values[i];
		}
		return array;
	}

	public static byte[] ToBytes(INT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return SortBytes(BitConverter.GetBytes(value.Value), byteOrder);
	}

	public static byte[] ToBytes(INT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static UINT ToUInt(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new UINT(BitConverter.ToUInt16(SortBytes(values, byteOrder)));
	}

	public static UINT[] ToUIntArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		UINT[] array = new UINT[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = ToUInt(new byte[2]
			{
				values[i],
				values[i + 1]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] ToBytes(UINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return SortBytes(BitConverter.GetBytes(value), byteOrder);
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

	public static WORD ToWord(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new WORD(BitConverter.ToUInt16(SortBytes(values, byteOrder)));
	}

	public static WORD[] ToWordArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		WORD[] array = new WORD[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = ToWord(new byte[2]
			{
				values[i],
				values[i + 1]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] ToBytes(WORD value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return SortBytes(GetBytesFromHex(value.Value), byteOrder);
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

	public static DINT ToDInt(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new DINT(BitConverter.ToInt32(SortBytes(values, byteOrder)));
	}

	public static DINT[] ToDIntArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		DINT[] array = new DINT[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = ToDInt(new byte[4]
			{
				values[i],
				values[i + 1],
				values[i + 2],
				values[i + 3]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] ToBytes(DINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(DINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static UDINT ToUDInt(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new UDINT(BitConverter.ToUInt32(SortBytes(values, byteOrder)));
	}

	public static UDINT[] ToUDIntArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		UDINT[] array = new UDINT[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = ToUDInt(new byte[4]
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
		return SortBytes(BitConverter.GetBytes(value), byteOrder);
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

	public static DWORD ToDWord(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new DWORD(BitConverter.ToUInt32(SortBytesToWriteWordType(values, byteOrder)));
	}

	public static DWORD[] ToDWordArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		DWORD[] array = new DWORD[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = ToDWord(new byte[4]
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
		return SortBytes(GetBytesFromHex(value.Value), byteOrder);
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

	public static REAL ToReal(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new REAL(BitConverter.ToSingle(SortBytes(values, byteOrder)));
	}

	public static REAL[] ToRealArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		REAL[] array = new REAL[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = ToReal(new byte[4]
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
		return SortBytes(BitConverter.GetBytes(value), byteOrder);
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

	public static LREAL ToLReal(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new LREAL(BitConverter.ToDouble(SortBytes(values, byteOrder)));
	}

	public static LREAL[] ToLRealArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		LREAL[] array = new LREAL[values.Length / 4];
		for (int i = 0; i < values.Length; i += 8)
		{
			array[i / 8] = ToLReal(new byte[8]
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

	public static byte[] ToBytes(LREAL value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(LREAL[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static LINT ToLInt(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new LINT(BitConverter.ToInt64(SortBytes(values, byteOrder)));
	}

	public static LINT[] ToLIntArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		LINT[] array = new LINT[values.Length / 8];
		for (int i = 0; i < values.Length; i += 8)
		{
			array[i / 8] = ToLInt(new byte[8]
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

	public static byte[] ToBytes(LINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(LINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static ULINT ToULInt(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new ULINT(BitConverter.ToUInt64(SortBytes(values, byteOrder)));
	}

	public static ULINT[] ToULIntArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		ULINT[] array = new ULINT[values.Length / 8];
		for (int i = 0; i < values.Length; i += 8)
		{
			array[i / 8] = ToULInt(new byte[8]
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

	public static byte[] ToBytes(ULINT value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return SortBytes(BitConverter.GetBytes(value), byteOrder);
	}

	public static byte[] ToBytes(ULINT[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static LWORD ToLWord(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return new LWORD(BitConverter.ToUInt64(SortBytesToWriteWordType(values, byteOrder)));
	}

	public static LWORD[] ToLWordArray(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		LWORD[] array = new LWORD[values.Length / 8];
		for (int i = 0; i < values.Length; i += 8)
		{
			array[i / 8] = ToLWord(new byte[8]
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
		return SortBytesToWriteWordType(GetBytesFromHex(value.Value), byteOrder);
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

	public override string ToString()
	{
		return Value.ToString();
	}
}
