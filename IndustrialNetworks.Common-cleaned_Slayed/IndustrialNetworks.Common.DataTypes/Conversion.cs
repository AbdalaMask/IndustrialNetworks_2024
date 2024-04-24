using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace NetStudio.Common.DataTypes;

public class Conversion
{
	public const byte BytesOfShort = 2;

	public const byte BytesOfUShort = 2;

	public const byte BytesOfInt = 4;

	public const byte BytesOfUInt = 4;

	public const byte BytesOfFloat = 4;

	public const byte BytesOfLong = 8;

	public const byte BytesOfULong = 8;

	public const byte BytesOfDouble = 8;

	public static int GetSizeOfDataType(DataType dataType)
	{
        switch (dataType)
		{
		default:
			throw new NotSupportedException();
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
			return 4;
		case DataType.BOOL:
		case DataType.INT:
		case DataType.UINT:
		case DataType.WORD:
		case DataType.TIME16:
			return 1;
		case DataType.DINT:
		case DataType.UDINT:
		case DataType.DWORD:
		case DataType.REAL:
		case DataType.TIME32:
			return 2;
		}
	}

	public static int OctalToInt(int octalNumber)
	{
		return Convert.ToInt32(octalNumber.ToString(), 8);
	}

	public static int IntToOctal(int number)
	{
		return int.Parse(Convert.ToString(number, 8));
	}

	public static int IncrementOctal(int octalNumber)
	{
		return int.Parse(Convert.ToString(Convert.ToInt32(octalNumber.ToString(), 8) + 1, 8));
	}

	public static int OctalToDecimal(int octalNumber)
	{
		int num = 0;
		int num2 = 1;
		int num3 = octalNumber;
		while (num3 > 0)
		{
			int num4 = num3 % 10;
			num3 /= 10;
			num += num4 * num2;
			num2 *= 8;
		}
		return num;
	}

	public static int DecimalToOctal(int decimalNumber)
	{
		return int.Parse(Convert.ToString(decimalNumber, 8));
	}

	public static T[] BytesToArray<T>(byte[] data, ByteOrder byteOrder = ByteOrder.BigEndian) where T : struct
	{
		Type typeFromHandle = typeof(T);
		T[] result = Array.Empty<T>();
		if (typeFromHandle == typeof(bool))
		{
			result = (T[])(object)BytesToBools(data);
		}
		else if (typeFromHandle == typeof(byte))
		{
			result = (T[])(object)data;
		}
		else if (typeFromHandle == typeof(char))
		{
			result = (T[])(object)BytesToChars(data);
		}
		else if (typeFromHandle == typeof(short))
		{
			result = (T[])(object)BytesToShorts(data, byteOrder);
		}
		else if (typeFromHandle == typeof(ushort))
		{
			result = (T[])(object)BytesToUShorts(data, byteOrder);
		}
		else if (typeFromHandle == typeof(int))
		{
			result = (T[])(object)BytesToInts(data, byteOrder);
		}
		else if (typeFromHandle == typeof(uint))
		{
			result = (T[])(object)BytesToUInts(data, byteOrder);
		}
		else if (typeFromHandle == typeof(float))
		{
			result = (T[])(object)BytesToFloats(data, byteOrder);
		}
		else if (typeFromHandle == typeof(long))
		{
			result = (T[])(object)BytesToLongs(data, byteOrder);
		}
		else if (typeFromHandle == typeof(ulong))
		{
			result = (T[])(object)BytesToLongs(data, byteOrder);
		}
		else if (typeFromHandle == typeof(double))
		{
			result = (T[])(object)BytesToDoubles(data, byteOrder);
		}
		return result;
	}

	public static byte[] ArrayToBytes<T>(T[] data, ByteOrder byteOrder = ByteOrder.BigEndian) where T : struct
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(bool))
		{
			return BoolsToBytes((bool[])(object)data);
		}
		if (typeFromHandle == typeof(byte))
		{
			return (byte[])(object)data;
		}
		if (typeFromHandle == typeof(char))
		{
			return CharsToBytes((char[])(object)data);
		}
		if (typeFromHandle == typeof(short))
		{
			return ShortsToBytes((short[])(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(ushort))
		{
			return UShortsToBytes((ushort[])(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(int))
		{
			return IntsToBytes((int[])(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(uint))
		{
			return UIntsToBytes((uint[])(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(float))
		{
			return FloatsToBytes((float[])(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(long))
		{
			return LongsToBytes((long[])(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(ulong))
		{
			return ULongsToBytes((ulong[])(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(double))
		{
			return DoublesToBytes((double[])(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(string))
		{
			throw new NotSupportedException();
		}
		return Array.Empty<byte>();
	}

	public static T BytesToSingle<T>(byte[] data, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		Type typeFromHandle = typeof(T);
		T result = default(T);
		if (typeFromHandle == typeof(bool))
		{
			return (T)(object)BytesToBool(data);
		}
		if (typeFromHandle == typeof(byte))
		{
			return (T)(object)data[0];
		}
		if (typeFromHandle == typeof(char))
		{
			return (T)(object)BytesToChar(data);
		}
		if (typeFromHandle == typeof(short))
		{
			return (T)(object)BytesToShort(data, byteOrder);
		}
		if (typeFromHandle == typeof(ushort))
		{
			return (T)(object)BytesToUShort(data, byteOrder);
		}
		if (typeFromHandle == typeof(int))
		{
			return (T)(object)BytesToInt(data, byteOrder);
		}
		if (typeFromHandle == typeof(uint))
		{
			return (T)(object)BytesToUInt(data, byteOrder);
		}
		if (typeFromHandle == typeof(float))
		{
			return (T)(object)BytesToFloat(data, byteOrder);
		}
		if (typeFromHandle == typeof(long))
		{
			return (T)(object)BytesToLong(data, byteOrder);
		}
		if (typeFromHandle == typeof(ulong))
		{
			return (T)(object)BytesToULong(data, byteOrder);
		}
		if (typeFromHandle == typeof(double))
		{
			return (T)(object)BytesToDouble(data, byteOrder);
		}
		if (typeFromHandle == typeof(string))
		{
			return (T)(object)BytesToString(data, Encoding.ASCII);
		}
		return result;
	}

	public static byte[] SingleToBytes<T>(T data, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(bool))
		{
			return BoolToBytes((bool)(object)data);
		}
		if (typeFromHandle == typeof(byte))
		{
			return new byte[1] { (byte)(object)data };
		}
		if (typeFromHandle == typeof(char))
		{
			return new byte[1] { Convert.ToByte(data) };
		}
		if (typeFromHandle == typeof(short))
		{
			return ShortToBytes((short)(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(ushort))
		{
			return UShortToBytes((ushort)(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(int))
		{
			return IntToBytes((int)(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(uint))
		{
			return UIntToBytes((uint)(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(float))
		{
			return FloatToBytes((float)(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(long))
		{
			return LongToBytes((long)(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(ulong))
		{
			return ULongToBytes((ulong)(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(double))
		{
			return DoubleToBytes((double)(object)data, byteOrder);
		}
		if (typeFromHandle == typeof(string))
		{
			return StringToBytes((string)(object)data, Encoding.ASCII);
		}
		return Array.Empty<byte>();
	}

	public static T[] HexToArray<T>(string data, ByteOrder byteOrder = ByteOrder.BigEndian) where T : struct
	{
		Type typeFromHandle = typeof(T);
		T[] result = Array.Empty<T>();
		if (typeFromHandle == typeof(bool))
		{
			result = (T[])(object)HexToBools(data);
		}
		else if (typeFromHandle == typeof(short))
		{
			result = (T[])(object)HexToShorts(data, byteOrder);
		}
		else if (typeFromHandle == typeof(ushort))
		{
			result = (T[])(object)HexToUShorts(data, byteOrder);
		}
		else if (typeFromHandle == typeof(int))
		{
			result = (T[])(object)HexToInts(data, byteOrder);
		}
		else if (typeFromHandle == typeof(uint))
		{
			result = (T[])(object)HexToUInts(data, byteOrder);
		}
		else if (typeFromHandle == typeof(float))
		{
			result = (T[])(object)HexToFloats(data, byteOrder);
		}
		return result;
	}

	public static bool HexToBool(string dataHex)
	{
		return short.Parse(dataHex, NumberStyles.HexNumber) > 0;
	}

	public static bool[] HexToBools(string dataHex)
	{
		BitArray bitArray = new BitArray(HexToBytes(dataHex));
		bool[] array = new bool[bitArray.Length];
		bitArray.CopyTo(array, 0);
		return array;
	}

	public static string BoolsToHex(bool[] values)
	{
		return BytesToHex(BoolsToBytes(values));
	}

	public static string BoolToHex(bool value)
	{
		return Convert.ToByte(value).ToString("X2");
	}

	public static bool BytesToBool(byte[] values)
	{
		BitArray bitArray = new BitArray(values);
		bool[] array = new bool[bitArray.Length];
		bitArray.CopyTo(array, 0);
		return array[0];
	}

	public static bool[] BytesToBools(byte[] values)
	{
		BitArray bitArray = new BitArray(values);
		bool[] array = new bool[bitArray.Length];
		bitArray.CopyTo(array, 0);
		return array;
	}

	public static byte[] BoolToBytes(bool value)
	{
		byte b = Convert.ToByte(value);
		return new byte[1] { b };
	}

	public static byte[] BoolsToBytes(bool[] values)
	{
		int num = values.Length / 8 + ((values.Length % 8 != 0) ? 1 : 0);
		BitArray bitArray = new BitArray(values);
		byte[] array = new byte[num];
		bitArray.CopyTo(array, 0);
		return array;
	}

	public static string ByteToHex(byte value)
	{
		return value.ToString("X2");
	}

	public static string BytesToHex(byte[] values)
	{
		return string.Join("", values.Select((byte byte_0) => byte_0.ToString("X2")));
	}

	public static byte HexToByte(string IP)
	{
		return byte.Parse(IP, NumberStyles.HexNumber);
	}

	public static byte[] HexToBytes(string IP)
	{
		byte[] array = new byte[IP.Length / 2];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Convert.ToByte(IP.Substring(i * 2, 2), 16);
		}
		return array;
	}

	public static byte[] BytesToAsciiBytes(params byte[] values)
	{
		return Encoding.ASCII.GetBytes(values.SelectMany((byte byte_0) => byte_0.ToString("X2")).ToArray());
	}

	public static char BytesToChar(byte[] values)
	{
		return Encoding.ASCII.GetString(values)[0];
	}

	public static byte[] CharToBytes(char value)
	{
		return new byte[1] { Convert.ToByte(value) };
	}

	public static char[] BytesToChars(byte[] values)
	{
		return Encoding.ASCII.GetString(values).ToCharArray();
	}

	public static byte[] CharsToBytes(char[] values)
	{
		byte[] array = new byte[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			array[i] = (byte)values[i];
		}
		return array;
	}

	public static string StringToHex(string data)
	{
		return string.Join("", data.Select((char char_0) => $"{(byte)char_0:X2}"));
	}

	public static string HexToString(string dataHex)
	{
		byte[] source = HexToBytes(dataHex);
		return string.Join("", source.Select((byte byte_0) => (char)byte_0));
	}

	public static string BytesToString(byte[] values, Encoding encoding)
	{
        if (encoding == Encoding.ASCII)
		{
			return Encoding.ASCII.GetString(values);
		}
		if (encoding == Encoding.Unicode)
		{
			return Encoding.Unicode.GetString(values);
		}
		if (encoding == Encoding.BigEndianUnicode)
		{
			return Encoding.BigEndianUnicode.GetString(values);
		}
		if (encoding == Encoding.UTF8)
		{
			return Encoding.UTF8.GetString(values);
		}
		if (encoding == Encoding.UTF32)
		{
			return Encoding.UTF32.GetString(values);
		}
		if (encoding == Encoding.Latin1)
		{
			return Encoding.Latin1.GetString(values);
		}
		return Encoding.ASCII.GetString(values);
	}

	public static byte[] StringToBytes(string value, Encoding encoding)
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
		return Encoding.ASCII.GetBytes(value);
	}

	public static byte[] HexStringToBytes(string hexa)
	{
		if (hexa.Length % 2 == 0)
		{
			List<byte> list = new List<byte>();
			for (int i = 0; i < hexa.Length; i += 2)
			{
				list.Add(byte.Parse(hexa.Substring(i, 2) ?? "", NumberStyles.HexNumber));
			}
			return list.ToArray();
		}
		throw new FormatException();
	}

	public static short HexToShort(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BytesToShort(BitConverter.GetBytes(short.Parse(IP, NumberStyles.HexNumber)), byteOrder);
	}

	public static short[] HexToShorts(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		short[] array = new short[IP.Length / 4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToShort(IP.Substring(i * 4, 4), byteOrder);
		}
		return array;
	}

	public static string ShortToHex(short value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		byte[] array = Array.Empty<byte>();
		array = ((byteOrder == ByteOrder.BigEndian || byteOrder != ByteOrder.LittleEndian) ? new byte[2]
		{
			bytes[1],
			bytes[0]
		} : new byte[2]
		{
			bytes[0],
			bytes[1]
		});
		return string.Join("", array.Select((byte byte_0) => byte_0.ToString("X2")));
	}

	public static string ShortsToHex(short[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		foreach (short value in values)
		{
			text += ShortToHex(value, byteOrder);
		}
		return text;
	}

	public static short BytesToShort(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BitConverter.ToInt16(new byte[2]
			{
				values[0],
				values[1]
			});
		}
		return BitConverter.ToInt16(new byte[2]
		{
			values[1],
			values[0]
		});
	}

	public static short[] BytesToShorts(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		short[] array = new short[values.Length / 2];
		for (int i = 0; i < values.Length; i += 2)
		{
			array[i / 2] = BytesToShort(new byte[2]
			{
				values[i],
				values[i + 1]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] ShortToBytes(short value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] array = Array.Empty<byte>();
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return new byte[2]
			{
				bytes[0],
				bytes[1]
			};
		}
		return new byte[2]
		{
			bytes[1],
			bytes[0]
		};
	}

	public static byte[] ShortsToBytes(short[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ShortToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static string HexToWord(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return byteOrder switch
		{
			ByteOrder.BigEndian => IP.Substring(2, 2) + IP.Substring(0, 2), 
			_ => IP, 
		};
	}

	public static string[] HexToWords(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string[] array = new string[IP.Length / 4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToWord(IP.Substring(i * 4, 4), byteOrder);
		}
		return array;
	}

	public static string WordToHex(string value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
        return byteOrder switch
		{
			ByteOrder.BigEndian => value.Substring(2, 2) + value.Substring(0, 2), 
			_ => value.Substring(0, 2) + value.Substring(2, 2), 
		};
	}

	public static string WordsToHex(string[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		foreach (string text2 in values)
		{
			text += WordToHex(text2.Trim(), byteOrder);
		}
		return text;
	}

	public static ushort HexToUShort(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BytesToUShort(BitConverter.GetBytes(ushort.Parse(IP, NumberStyles.HexNumber)), byteOrder);
	}

	public static ushort[] HexToUShorts(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		ushort[] array = new ushort[IP.Length / 4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToUShort(IP.Substring(i * 4, 4), byteOrder);
		}
		return array;
	}

	public static string UShortToHex(ushort value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		byte[] array = Array.Empty<byte>();
		array = ((byteOrder == ByteOrder.BigEndian || byteOrder != ByteOrder.LittleEndian) ? new byte[2]
		{
			bytes[1],
			bytes[0]
		} : new byte[2]
		{
			bytes[0],
			bytes[1]
		});
		return string.Join("", array.Select((byte byte_0) => byte_0.ToString("X2")));
	}

	public static string UShortsToHex(ushort[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		foreach (ushort value in values)
		{
			text += UShortToHex(value, byteOrder);
		}
		return text;
	}

	public static ushort BytesToUShort(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BitConverter.ToUInt16(new byte[2]
			{
				values[0],
				values[1]
			});
		}
		return BitConverter.ToUInt16(new byte[2]
		{
			values[1],
			values[0]
		});
	}

	public static ushort[] BytesToUShorts(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		ushort[] array = new ushort[values.Length / 2];
		for (int i = 0; i < values.Length; i += 2)
		{
			array[i / 2] = BytesToUShort(new byte[2]
			{
				values[i],
				values[i + 1]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] UShortToBytes(ushort value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] array = Array.Empty<byte>();
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return new byte[2]
			{
				bytes[0],
				bytes[1]
			};
		}
		return new byte[2]
		{
			bytes[1],
			bytes[0]
		};
	}

	public static byte[] UShortsToBytes(ushort[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(UShortToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static int HexToInt(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BytesToInt(BitConverter.GetBytes(int.Parse(IP, NumberStyles.HexNumber)), byteOrder);
	}

	public static int[] HexToInts(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		int[] array = new int[IP.Length / 8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToInt(IP.Substring(i * 8, 8), byteOrder);
		}
		return array;
	}

	public static string IntToHex(int value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BytesToHex(new byte[4]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3]
			});
		}
		return BytesToHex(new byte[4]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2]
		});
	}

	public static string IntsToHex(int[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		foreach (int value in values)
		{
			text += IntToHex(value, byteOrder);
		}
		return text;
	}

	public static int BytesToInt(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BitConverter.ToInt32(new byte[4]
			{
				values[0],
				values[1],
				values[2],
				values[3]
			}, 0);
		}
		return BitConverter.ToInt32(new byte[4]
		{
			values[1],
			values[0],
			values[3],
			values[2]
		}, 0);
	}

	public static int[] BytesToInts(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		int[] array = new int[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = BytesToInt(new byte[4]
			{
				values[i],
				values[i + 1],
				values[i + 2],
				values[i + 3]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] IntToBytes(int value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] array = Array.Empty<byte>();
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return new byte[4]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3]
			};
		}
		return new byte[4]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2]
		};
	}

	public static byte[] IntsToBytes(int[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(IntToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static uint HexToUInt(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BytesToUInt(BitConverter.GetBytes(uint.Parse(IP, NumberStyles.HexNumber)), byteOrder);
	}

	public static uint[] HexToUInts(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		uint[] array = new uint[IP.Length / 8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToUInt(IP.Substring(i * 8, 8), byteOrder);
		}
		return array;
	}

	public static string UIntToHex(uint value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BytesToHex(new byte[4]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3]
			});
		}
		return BytesToHex(new byte[4]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2]
		});
	}

	public static string UIntsToHex(uint[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		for (int i = 0; i < values.Length; i++)
		{
			int value = (int)values[i];
			text += IntToHex(value, byteOrder);
		}
		return text;
	}

	public static uint BytesToUInt(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BitConverter.ToUInt32(new byte[4]
			{
				values[0],
				values[1],
				values[2],
				values[3]
			}, 0);
		}
		return BitConverter.ToUInt32(new byte[4]
		{
			values[1],
			values[0],
			values[3],
			values[2]
		}, 0);
	}

	public static uint[] BytesToUInts(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		uint[] array = new uint[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = BytesToUInt(new byte[4]
			{
				values[i],
				values[i + 1],
				values[i + 2],
				values[i + 3]
			}, byteOrder);
		}
		return array;
	}

	public static byte[] UIntToBytes(uint value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] array = Array.Empty<byte>();
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return new byte[4]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3]
			};
		}
		return new byte[4]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2]
		};
	}

	public static byte[] UIntsToBytes(uint[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(UIntToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static string HexToDWord(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return IP.Substring(0, 2) + IP.Substring(2, 2) + IP.Substring(4, 2) + IP.Substring(6, 2);
		}
		return IP.Substring(2, 2) + IP.Substring(0, 2) + IP.Substring(6, 2) + IP.Substring(4, 2);
	}

	public static string[] HexToDWords(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string[] array = new string[IP.Length / 8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToDWord(IP.Substring(i * 8, 8), byteOrder);
		}
		return array;
	}

	public static string DWordToHex(string value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
        if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return value.Substring(0, 2) + value.Substring(2, 2) + value.Substring(4, 2) + value.Substring(6, 2);
		}
		return value.Substring(2, 2) + value.Substring(0, 2) + value.Substring(6, 2) + value.Substring(4, 2);
	}

	public static string DWordsToHex(string[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		foreach (string text2 in values)
		{
			text += DWordToHex(text2.Trim(), byteOrder);
		}
		return text;
	}

	public static float HexToFloat(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BytesToFloat(BitConverter.GetBytes(Convert.ToInt32(IP, 16)), byteOrder);
	}

	public static float[] HexToFloats(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (IP == null)
		{
			throw new ArgumentNullException("The data is null");
		}
		if (IP.Length % 8 != 0)
		{
			throw new FormatException("Hex Character Count Not Even");
		}
		float[] array = new float[IP.Length / 8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToFloat(IP.Substring(i * 8, 8), byteOrder);
		}
		return array;
	}

	public static string FloatToHex(float value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BytesToHex(new byte[4]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3]
			});
		}
		return BytesToHex(new byte[4]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2]
		});
	}

	public static string FloatsToHex(float[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		foreach (float value in values)
		{
			text += FloatToHex(value, byteOrder);
		}
		return text;
	}

	public static float BytesToFloat(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BitConverter.ToSingle(new byte[4]
			{
				values[0],
				values[1],
				values[2],
				values[3]
			}, 0);
		}
		return BitConverter.ToSingle(new byte[4]
		{
			values[1],
			values[0],
			values[3],
			values[2]
		}, 0);
	}

	public static float[] BytesToFloats(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		float[] array = new float[values.Length / 4];
		for (int i = 0; i < values.Length; i += 4)
		{
			array[i / 4] = BytesToFloat(new byte[4]
			{
				values[i],
				values[i + 1],
				values[i + 2],
				values[i + 3]
			}, byteOrder);
		}
		return array;
	}

	public static float[] BytesToFloatArray(byte[] values)
	{
		List<float> list = new List<float>();
		for (int i = 0; i < values.Length; i += 4)
		{
			list.Add(BytesToFloat(new byte[4]
			{
				values[i],
				values[i + 1],
				values[i + 2],
				values[i + 3]
			}));
		}
		return list.ToArray();
	}

	public static byte[] FloatToBytes(float value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		byte[] array = Array.Empty<byte>();
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return bytes;
		}
		return new byte[4]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2]
		};
	}

	public static byte[] FloatsToBytes(float[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(FloatToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static long HexToLong(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BytesToLong(BitConverter.GetBytes(long.Parse(IP, NumberStyles.HexNumber)), byteOrder);
	}

	public static long[] HexToLongs(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		long[] array = new long[IP.Length / 16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToLong(IP.Substring(i * 16, 16), byteOrder);
		}
		return array;
	}

	public static string LongToHex(long value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BytesToHex(new byte[8]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3],
				bytes[4],
				bytes[5],
				bytes[6],
				bytes[7]
			});
		}
		return BytesToHex(new byte[8]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2],
			bytes[5],
			bytes[4],
			bytes[7],
			bytes[6]
		});
	}

	public static string LongsToHex(long[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		foreach (long value in values)
		{
			text += LongToHex(value, byteOrder);
		}
		return text;
	}

	public static long BytesToLong(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BitConverter.ToInt64(new byte[8]
			{
				values[0],
				values[1],
				values[2],
				values[3],
				values[4],
				values[5],
				values[6],
				values[7]
			}, 0);
		}
		return BitConverter.ToInt64(new byte[8]
		{
			values[1],
			values[0],
			values[3],
			values[2],
			values[5],
			values[4],
			values[7],
			values[6]
		}, 0);
	}

	public static long[] BytesToLongs(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		long[] array = new long[values.Length / 8];
		for (int i = 0; i < values.Length; i += 8)
		{
			array[i / 8] = BytesToLong(new byte[8]
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

	public static byte[] LongToBytes(long value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] array = Array.Empty<byte>();
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return new byte[8]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3],
				bytes[4],
				bytes[5],
				bytes[6],
				bytes[7]
			};
		}
		return new byte[8]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2],
			bytes[5],
			bytes[4],
			bytes[7],
			bytes[6]
		};
	}

	public static byte[] LongsToBytes(long[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(LongToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static ulong HexToULong(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BytesToULong(BitConverter.GetBytes(ulong.Parse(IP, NumberStyles.HexNumber)), byteOrder);
	}

	public static ulong[] HexToULongs(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		ulong[] array = new ulong[IP.Length / 16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToULong(IP.Substring(i * 16, 16), byteOrder);
		}
		return array;
	}

	public static string ULongToHex(ulong value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BytesToHex(new byte[8]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3],
				bytes[4],
				bytes[5],
				bytes[6],
				bytes[7]
			});
		}
		return BytesToHex(new byte[8]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2],
			bytes[5],
			bytes[4],
			bytes[7],
			bytes[6]
		});
	}

	public static string ULongsToHex(ulong[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		foreach (ulong value in values)
		{
			text += ULongToHex(value, byteOrder);
		}
		return text;
	}

	public static ulong BytesToULong(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BitConverter.ToUInt64(new byte[8]
			{
				values[0],
				values[1],
				values[2],
				values[3],
				values[4],
				values[5],
				values[6],
				values[7]
			}, 0);
		}
		return BitConverter.ToUInt64(new byte[8]
		{
			values[1],
			values[0],
			values[3],
			values[2],
			values[5],
			values[4],
			values[7],
			values[6]
		}, 0);
	}

	public static ulong[] BytesToULongs(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		ulong[] array = new ulong[values.Length / 8];
		for (int i = 0; i < values.Length; i += 8)
		{
			array[i / 8] = BytesToULong(new byte[8]
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

	public static byte[] ULongToBytes(ulong value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] array = Array.Empty<byte>();
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return new byte[8]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3],
				bytes[4],
				bytes[5],
				bytes[6],
				bytes[7]
			};
		}
		return new byte[8]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2],
			bytes[5],
			bytes[4],
			bytes[7],
			bytes[6]
		};
	}

	public static byte[] ULongsToBytes(ulong[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(ULongToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static double HexToDouble(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		return BytesToDouble(BitConverter.GetBytes(long.Parse(IP, NumberStyles.HexNumber)), byteOrder);
	}

	public static double[] HexToDoubles(string IP, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		double[] array = new double[IP.Length / 16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToDouble(IP.Substring(i * 16, 16), byteOrder);
		}
		return array;
	}

	public static string DoubleToHex(double value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BytesToHex(new byte[8]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3],
				bytes[4],
				bytes[5],
				bytes[6],
				bytes[7]
			});
		}
		return BytesToHex(new byte[8]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2],
			bytes[5],
			bytes[4],
			bytes[7],
			bytes[6]
		});
	}

	public static string DoublesToHex(double[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		string text = string.Empty;
		foreach (double value in values)
		{
			text += DoubleToHex(value, byteOrder);
		}
		return text;
	}

	public static double BytesToDouble(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return BitConverter.ToDouble(new byte[8]
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
		return BitConverter.ToDouble(new byte[8]
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
	}

	public static double[] BytesToDoubles(byte[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		double[] array = new double[values.Length / 8];
		for (int i = 0; i < values.Length; i += 8)
		{
			array[i / 8] = BytesToDouble(new byte[8]
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

	public static byte[] DoubleToBytes(double value, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		byte[] array = Array.Empty<byte>();
		if (byteOrder != 0 && byteOrder == ByteOrder.LittleEndian)
		{
			return new byte[8]
			{
				bytes[0],
				bytes[1],
				bytes[2],
				bytes[3],
				bytes[4],
				bytes[5],
				bytes[6],
				bytes[7]
			};
		}
		return new byte[8]
		{
			bytes[1],
			bytes[0],
			bytes[3],
			bytes[2],
			bytes[5],
			bytes[4],
			bytes[7],
			bytes[6]
		};
	}

	public static byte[] DoublesToBytes(double[] values, ByteOrder byteOrder = ByteOrder.BigEndian)
	{
		List<byte> list = new List<byte>();
		for (int i = 0; i < values.Length; i++)
		{
			list.AddRange(DoubleToBytes(values[i], byteOrder));
		}
		return list.ToArray();
	}

	public static TimeSpan HexToTime16(string IP, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		string s = IP;
		if (byteOrder == ByteOrder.BigEndian)
		{
			s = IP.Substring(2, 2) + IP.Substring(0, 2);
		}
		return TimeSpan.FromMilliseconds(resolution * short.Parse(s, NumberStyles.HexNumber));
	}

	public static TimeSpan[] HexToTime16s(string IP, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		TimeSpan[] array = new TimeSpan[IP.Length / 4];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToTime16(IP.Substring(i * 4, 4), byteOrder, resolution);
		}
		return array;
	}

	public static short[] Time16sToShorts(TimeSpan[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		short[] array = new short[values.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (short)(values[i].TotalMilliseconds / (double)resolution);
		}
		return array;
	}

	public static TimeSpan HexToTime32(string IP, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
        string text = byteOrder == ByteOrder.BigEndian || byteOrder != ByteOrder.LittleEndian ? IP.Substring(2, 2) + IP.Substring(0, 2) + IP.Substring(6, 2) + IP.Substring(4, 2) : IP.Substring(0, 2) + IP.Substring(2, 2) + IP.Substring(4, 2) + IP.Substring(6, 2);
        return TimeSpan.FromMilliseconds(resolution * int.Parse(text, NumberStyles.HexNumber));
	}

	public static TimeSpan[] HexToTime32s(string IP, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		TimeSpan[] array = new TimeSpan[IP.Length / 8];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = HexToTime32(IP.Substring(i * 8, 8), byteOrder, resolution);
		}
		return array;
	}

	public static int[] Time32sToInts(TimeSpan[] values, ByteOrder byteOrder = ByteOrder.BigEndian, int resolution = 1)
	{
		int[] array = new int[values.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (int)(values[i].TotalMilliseconds / (double)resolution);
		}
		return array;
	}

	public static byte[] ToBytes(DateTime dateTime)
	{
		byte[] array = new byte[9];
		int num = dateTime.Year / 100;
		int num2 = dateTime.Year % 100;
		array[0] = Convert.ToByte(num / 10 * 16 + num % 10);
		array[1] = Convert.ToByte(num2 / 10 * 16 + num2 % 10);
		array[2] = Convert.ToByte(dateTime.Month / 10 * 16 + dateTime.Month % 10);
		array[3] = Convert.ToByte(dateTime.Day / 10 * 16 + dateTime.Day % 10);
		array[4] = Convert.ToByte(dateTime.Hour / 10 * 16 + dateTime.Hour % 10);
		array[5] = Convert.ToByte(dateTime.Minute / 10 * 16 + dateTime.Minute % 10);
		array[6] = Convert.ToByte(dateTime.Second / 10 * 16 + dateTime.Second % 10);
		int num3 = dateTime.Millisecond * 10 / 100;
		int num4 = dateTime.Millisecond * 10 % 100;
		array[7] = Convert.ToByte(num3 / 10 * 16 + num3 % 10);
		array[8] = Convert.ToByte(num4 / 10 * 16 + num4 % 10);
		return array;
	}

	public static List<string> GetEncodings()
	{
		return new List<string> { "ASCII", "Unicode", "BigEndianUnicode", "UTF8", "Latin1", "UTF32" };
	}
}
