using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetStudio.Common;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;

namespace NetStudio.Omron;

public class OmronUtility
{
	private static Dictionary<string, string> memories = new Dictionary<string, string>
	{
		{ "SYS", "SYS" },
		{ "CIO", "CIO" },
		{ "A", "A" },
		{ "T", "T" },
		{ "C", "C" },
		{ "D", "D" },
		{ "E", "E" },
		{ "H", "H" },
		{ "W", "W" },
		{ "DR", "DR" },
		{ "IR", "IR" },
		{ "TK", "TK" },
		{ "TR", "TR" }
	};

	public static string GetMemory(string address)
	{
		string key = string.Join("", from char_0 in address
			where char.IsLetter(char_0)
			select (char_0));
		return memories[key];
	}

	public static int GetWordAddress(string address, bool isFins = false)
	{
		string memory = GetMemory(address);
		string text = address.Substring(memory.Length, address.Length - memory.Length);
		string s = ((!text.Contains(".")) ? text : text.Split('.')[0]);
		if (memory.Equals("C", StringComparison.OrdinalIgnoreCase))
		{
			return int.Parse(s) + (isFins ? 32768 : 2048);
		}
		return int.Parse(s);
	}

	public static int GetBitAddress(string address)
	{
		string memory = GetMemory(address);
		string text = address.Substring(memory.Length, address.Length - memory.Length);
		string s = "0";
		if (text.Contains("."))
		{
			s = text.Split('.')[1];
		}
		return int.Parse(s);
	}

	public static int GetIndexOfWordAddress(Tag startTag, Tag endTag)
	{
		return endTag.WordAddress - startTag.WordAddress;
	}

	public static int GetTotalBits(string address)
	{
		string memory = GetMemory(address);
		string text = address.Substring(memory.Length, address.Length - memory.Length);
        string s = "0";
        string text2;
        if (text.Contains("."))
        {
            string[] array = text.Split('.');
            text2 = array[0];
            s = array[1];
        }
        else
        {
            text2 = text;
        }
        return 16 * int.Parse(text2) + int.Parse(s);
	}

	public static void IncrementWordAddress(Tag tg)
	{
        string memory = GetMemory(tg.Address);
        string text = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
        string text2 = !text.Contains(".") ? text : text.Split('.')[0];
        int num2 = int.Parse(text2);
        int num;
        switch (tg.DataType)
        {
            case DataType.BOOL:
                {
                    string s = "0";
                    if (text.Contains("."))
                    {
                        s = text.Split('.')[1];
                    }
                    int num3 = int.Parse(s);
                    num3++;
                    if (num3 > 15)
                    {
                        num3 = 0;
                        num2++;
                    }
                    tg.Address = $"{memory}{num2}.{num3.ToString("D2")}";
                    SetTagName(tg);
                    return;
                }
            default:
                throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
            case DataType.LINT:
            case DataType.ULINT:
            case DataType.LWORD:
            case DataType.LREAL:
                num = 4;
                break;
            case DataType.INT:
            case DataType.UINT:
            case DataType.WORD:
            case DataType.TIME16:
                num = 1;
                break;
            case DataType.DINT:
            case DataType.UDINT:
            case DataType.DWORD:
            case DataType.REAL:
            case DataType.TIME32:
                num = 2;
                break;
            case DataType.STRING:
                if (tg.Resolution <= 0)
                {
                    tg.Resolution = 64;
                }
                num = tg.Resolution;
                break;
        }
        num2 += num;
		tg.Address = $"{memory}{num2}";
		SetTagName(tg);
	}

	public static ushort GetSizeOfDataType(Tag tg)
	{
        switch (tg.DataType)
		{
		default:
			return 1;
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
		case DataType.STRING:
			if (tg.Resolution <= 0)
			{
				tg.Resolution = 64;
			}
			if (tg.Resolution % 2 != 0)
			{
				tg.Resolution++;
			}
			return (ushort)(tg.Resolution / 2);
		}
	}

	public static ValidateResult TagValidate(Tag tg)
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid
		};
		switch (tg.DataType)
		{
		case DataType.BOOL:
			if (tg.Address.Contains("."))
			{
				string[] array = tg.Address.Split('.');
				string text = ("00" + array[1]).Right(2);
				tg.Address = array[0] + "." + text;
			}
			else
			{
				tg.Address += ".00";
			}
			break;
		default:
			throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
		case DataType.INT:
		case DataType.UINT:
		case DataType.WORD:
		case DataType.DINT:
		case DataType.UDINT:
		case DataType.DWORD:
		case DataType.REAL:
		case DataType.TIME16:
		case DataType.TIME32:
			if (tg.Address.Contains("."))
			{
				string[] array2 = tg.Address.Split('.');
				tg.Address = array2[0];
			}
			break;
		case DataType.STRING:
			if (tg.Resolution <= 0)
			{
				tg.Resolution = 64;
			}
			if (tg.Resolution % 2 != 0)
			{
				tg.Resolution++;
			}
			break;
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
			break;
		}
		if (string.IsNullOrEmpty(validateResult.Message))
		{
			validateResult.Status = ValidateStatus.Valid;
		}
		else
		{
			validateResult.Status = ValidateStatus.Invalid;
		}
		return validateResult;
	}

	private static void SetTagName(Tag tg)
	{
		tg.Name = tg.Address;
		switch (tg.DataType)
		{
		default:
			throw new NotSupportedException($"{tg.DataType}: " + "This data type is not supported.");
		case DataType.BOOL:
			break;
		case DataType.BYTE:
			tg.Name = tg.Name.Insert(1, "B");
			break;
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
			tg.Name = tg.Name.Insert(1, "L");
			break;
		case DataType.INT:
		case DataType.UINT:
		case DataType.WORD:
		case DataType.TIME16:
			tg.Name = tg.Name.Insert(1, "W");
			break;
		case DataType.DINT:
		case DataType.UDINT:
		case DataType.DWORD:
		case DataType.REAL:
		case DataType.TIME32:
			tg.Name = tg.Name.Insert(1, "D");
			break;
		case DataType.STRING:
			tg.Name = tg.Name.Insert(1, "STRG");
			break;
		}
	}

	public static byte[] Sort(byte[] values)
	{
		int num = values.Length;
		return num switch
		{
			8 => new byte[8]
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
			4 => new byte[4]
			{
				values[1],
				values[0],
				values[3],
				values[2]
			}, 
			2 => new byte[2]
			{
				values[1],
				values[0]
			}, 
			_ => throw new InvalidDataException($"Byte order is undefined with the number of bytes={num}. The number of valid bytes is: 2, 4, 8 bytes"), 
		};
	}

	public static byte[] Sort1(byte[] values)
	{
		int num = values.Length;
		return num switch
		{
			8 => new byte[8]
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
			4 => new byte[4]
			{
				values[1],
				values[0],
				values[3],
				values[2]
			}, 
			2 => new byte[2]
			{
				values[0],
				values[1]
			}, 
			_ => throw new InvalidDataException($"Byte order is undefined with the number of bytes={num}. The number of valid bytes is: 2, 4, 8 bytes"), 
		};
	}

	public static string Sort(string dataHex)
	{
		int length = dataHex.Length;
		return length switch
		{
			16 => dataHex.Substring(12, 2) + dataHex.Substring(14, 2) + dataHex.Substring(8, 2) + dataHex.Substring(10, 2) + dataHex.Substring(4, 2) + dataHex.Substring(6, 2) + dataHex.Substring(0, 2) + dataHex.Substring(2, 2), 
			8 => dataHex.Substring(4, 2) + dataHex.Substring(6, 2) + dataHex.Substring(0, 2) + dataHex.Substring(2, 2), 
			4 => dataHex.Substring(0, 2) + dataHex.Substring(2, 2), 
			_ => throw new InvalidDataException($"Byte order is undefined with the number of bytes={length}. The number of valid bytes is: 2, 4, 8 bytes"), 
		};
	}

	public static INT ToINT(byte[] bytes)
	{
		return BitConverter.ToInt16(Sort(bytes));
	}

	public static UINT ToUINT(byte[] bytes)
	{
		return BitConverter.ToUInt16(Sort(bytes));
	}

	public static WORD ToWORD(byte[] bytes)
	{
		return BitConverter.ToUInt16(Sort(bytes));
	}

	public static TIME16 ToTIME16(byte[] bytes, int resolution)
	{
		ushort num = BitConverter.ToUInt16(Sort(bytes));
		return TimeSpan.FromMilliseconds(resolution * num);
	}

	public static DINT ToDINT(byte[] bytes)
	{
		return BitConverter.ToInt32(Sort(bytes));
	}

	public static UDINT ToUDINT(byte[] bytes)
	{
		return BitConverter.ToUInt32(Sort(bytes));
	}

	public static DWORD ToDWORD(byte[] bytes)
	{
		return BitConverter.ToUInt32(Sort(bytes));
	}

	public static REAL ToREAL(byte[] bytes)
	{
		return BitConverter.ToSingle(Sort(bytes));
	}

	public static TIME32 ToTIME32(byte[] bytes, int resolution)
	{
		uint num = BitConverter.ToUInt32(Sort(bytes));
		return TimeSpan.FromMilliseconds(resolution * num);
	}

	public static LINT ToLINT(byte[] bytes)
	{
		return BitConverter.ToInt64(Sort(bytes));
	}

	public static ULINT ToULINT(byte[] bytes)
	{
		return BitConverter.ToUInt64(Sort(bytes));
	}

	public static LWORD ToLWORD(byte[] bytes)
	{
		return BitConverter.ToUInt64(Sort(bytes));
	}

	public static LREAL ToLREAL(byte[] bytes)
	{
		return BitConverter.ToDouble(Sort(bytes));
	}
}
