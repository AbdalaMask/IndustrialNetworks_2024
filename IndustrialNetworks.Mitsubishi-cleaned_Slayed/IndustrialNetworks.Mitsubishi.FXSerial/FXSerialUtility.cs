using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;

namespace NetStudio.Mitsubishi.FXSerial;

public class FXSerialUtility
{
	public static readonly Dictionary<string, string> Memories = new Dictionary<string, string>
	{
		{ "S", "0000" },
		{ "X", "0400" },
		{ "Y", "0500" },
		{ "T_COIL", "0600" },
		{ "T", "0800" },
		{ "M", "0800" },
		{ "SM", "0F00" },
		{ "D", "1000" },
		{ "SD", "0E00" },
		{ "C16", "0A00" },
		{ "C32", "0C00" },
		{ "C_COIL", "0E00" }
	};

	public static string GetMemory(string address)
	{
		return string.Join("", from char_0 in address.Substring(0, 2)
			where char.IsLetter(char_0)
			select (char_0));
	}

	public static int GetByteAddress(Tag tg)
	{
		int offset = 0;
		string memory = GetMemory(tg.Address);
		string specialMemory = GetSpecialMemory(tg, ref offset);
		string value = Memories[specialMemory];
		if (IsBitMemory(memory))
		{
			if (IsDataTimerAndCounter(memory) && tg.DataType != 0)
			{
				offset = 2 * (int.Parse(tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length)) - offset);
				return (ushort)UINT.Parse(value, ByteOrder.LittleEndian) + offset;
			}
			offset = ((!IsOctal(memory)) ? (int.Parse(tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length)) - offset) : (Convert.ToUInt16(tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length), 8) - offset));
			return ((ushort)UINT.Parse(value, ByteOrder.LittleEndian) + offset) / 8;
		}
		if (tg.DataType == DataType.BYTE)
		{
			offset = int.Parse(tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length)) - offset;
			return (ushort)UINT.Parse(value, ByteOrder.LittleEndian) + offset;
		}
		offset = 2 * (int.Parse(tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length)) - offset);
		return (ushort)UINT.Parse(value, ByteOrder.LittleEndian) + offset;
	}

	public static int GetBitAddress(Tag tg)
	{
		string text = tg.Address.Substring(0, 1);
		int offset = 0;
		string specialMemory = GetSpecialMemory(tg, ref offset);
		offset = ((!IsOctal(text)) ? (int.Parse(tg.Address.Substring(text.Length, tg.Address.Length - text.Length)) - offset) : (Convert.ToInt32(tg.Address.Substring(text.Length, tg.Address.Length - text.Length), 8) - offset));
		string value = Memories[specialMemory];
		return offset + (ushort)UINT.Parse(value, ByteOrder.LittleEndian);
	}

	public static string GetSpecialMemory(Tag tg)
	{
		string text = tg.Address.Substring(0, 1).ToUpper();
		switch (text)
		{
		case "M":
		{
			int num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			if (num >= 8000)
			{
				text = "SM";
			}
			break;
		}
		case "C":
		{
			int num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			text = ((tg.DataType != 0) ? ((num < 200) ? "C16" : "C32") : "C_COIL");
			break;
		}
		case "T":
			text = ((tg.DataType != 0) ? "T" : "T_COIL");
			break;
		case "D":
		{
			int num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			if (num >= 8000)
			{
				text = "SD";
			}
			break;
		}
		}
		return text;
	}

	public static string GetSpecialMemory(Tag tg, ref int offset)
	{
		string text = tg.Address.Substring(0, 1).ToUpper();
        int num;
        switch (text)
		{
		case "M":
			num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			if (num >= 8000)
			{
				text = "SM";
				offset = 8000;
			}
			break;
		case "C":
			num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			if (tg.DataType == DataType.BOOL)
			{
				text = "C_COIL";
			}
			else if (num >= 200)
			{
				text = "C32";
				offset = 200;
			}
			else
			{
				text = "C16";
			}
			break;
		case "T":
			text = ((tg.DataType != 0) ? "T" : "T_COIL");
			break;
		case "D":
			num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			if (num >= 8000)
			{
				text = "SD";
				offset = 8000;
			}
			break;
		}
		return text;
	}

	public static int GetIndexOfByteAddress(Tag startTag, Tag endTag)
	{
		return endTag.ByteAddress - startTag.ByteAddress;
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

	public static string IncrementByteAddress(Tag tg)
	{
		string memory = GetMemory(tg.Address);
		string text = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
        string text2 = !text.Contains(".") ? text : text.Split('.')[0];
        int num = (IsOctal(memory) ? Convert.ToInt32(text2, 8) : int.Parse(text2));
		int sizeOfDataType = GetSizeOfDataType(tg);
		num = ((!IsBitMemory(memory)) ? (num + sizeOfDataType) : ((tg.DataType != 0) ? (num + sizeOfDataType * 8) : (num + sizeOfDataType)));
		string text3 = $"{memory}{num}";
		if (IsOctal(memory))
		{
			text3 = memory + Convert.ToString(num, 8);
		}
		tg.Address = text3;
		SetTagName(tg);
		return text3;
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

	public static ushort GetSizeOfDataType(Tag tg)
	{
        switch (tg.DataType)
		{
		default:
			throw new NotSupportedException("This data type is not supported.");
		case DataType.BOOL:
		case DataType.BYTE:
			return 1;
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
			return 8;
		case DataType.INT:
		case DataType.UINT:
		case DataType.WORD:
		case DataType.TIME16:
			return 2;
		case DataType.DINT:
		case DataType.UDINT:
		case DataType.DWORD:
		case DataType.REAL:
		case DataType.TIME32:
			return 4;
		}
	}

	public static ValidateResult TagValidate(Tag tg)
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid
		};
		tg.Address = (tg.Address ?? "").ToUpper();
		switch (tg.DataType)
		{
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
				string[] array = tg.Address.Split('.');
				tg.Address = array[0];
			}
			break;
		case DataType.BOOL:
		case DataType.STRING:
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

	public static bool IsOctal(string memory)
	{
		if (!(memory == "X"))
		{
			return memory == "Y";
		}
		return true;
	}

	public static bool IsBitMemory(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "CS";
		case "X":
		case "Y":
		case "M":
		case "SM":
		case "L":
		case "F":
		case "S":
		case "B":
		case "SB":
		case "V":
		case "T":
		case "C":
		case "TS":
		case "TC":
			return true;
		}
	}

	public static bool IsBitMemory(Tag tg)
	{
		string memory = GetMemory(tg.Address);
		switch (memory)
		{
		default:
			if (!((memory == "V") | (memory == "T")))
			{
				switch (memory)
				{
				default:
					return memory == "CS";
				case "C":
				case "TS":
				case "TC":
					break;
				}
			}
			break;
		case "X":
		case "Y":
		case "M":
		case "SM":
		case "L":
		case "F":
		case "S":
		case "B":
		case "SB":
			break;
		}
		return true;
	}

	public static bool IsDataTimerAndCounter(string memory)
	{
		if (!(memory == "T"))
		{
			return memory == "C";
		}
		return true;
	}

	public void EndCode(string code)
	{
		if (code == null)
		{
			return;
		}
		int length = code.Length;
		if (length != 2)
		{
			return;
		}
		switch (code[1])
		{
		case '0':
			if (!(code == "20"))
			{
				_ = code == "00";
				break;
			}
			throw new Exception("Could not create I/O table.");
		case '1':
			if (!(code == "01"))
			{
				if (!(code == "21"))
				{
					break;
				}
				throw new Exception("Not executable due to CPU Unit CPU error.");
			}
			throw new Exception("Not executable in RUN mode.");
		case '2':
			if (!(code == "02"))
			{
				break;
			}
			throw new Exception("Not executable in MONITOR mode.");
		case '3':
			switch (code)
			{
			case "A3":
				throw new Exception("Aborted due to FCS error in transmission data.");
			case "23":
				throw new Exception("User memory protected.");
			case "13":
				throw new Exception("FCS error.");
			case "03":
				throw new Exception("UM write-protected.");
			}
			break;
		case '4':
			switch (code)
			{
			case "A4":
				throw new Exception("Aborted due to format error in transmission data.");
			case "14":
				throw new Exception("Format error.");
			case "04":
				throw new Exception("Address over.");
			}
			break;
		case '5':
			if (!(code == "15"))
			{
				if (!(code == "A5"))
				{
					break;
				}
				throw new Exception("Aborted due to entry number data\r\nerror in transmission data.");
			}
			throw new Exception("Entry number data error.");
		case '6':
			if (!(code == "16"))
			{
				break;
			}
			throw new Exception("Command not supported.");
		case '8':
			if (!(code == "18"))
			{
				if (!(code == "A8"))
				{
					break;
				}
				throw new Exception("Aborted due to frame length error in\r\ntransmission data.");
			}
			throw new Exception("Frame length error.");
		case '9':
			if (!(code == "19"))
			{
				break;
			}
			throw new Exception("Not executable.");
		case 'B':
			if (!(code == "0B"))
			{
				break;
			}
			throw new Exception("Not executable in PROGRAM mode.");
		case '7':
		case ':':
		case ';':
		case '<':
		case '=':
		case '>':
		case '?':
		case '@':
		case 'A':
			break;
		}
	}

	public string CpuUnitErrors(BOOL[] bcd_errors)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if ((bool)bcd_errors[5])
		{
			stringBuilder.AppendLine("Battery error (A40204).");
		}
		if ((bool)bcd_errors[6])
		{
			stringBuilder.AppendLine("Special I/O Unit error (OR of A40206 and A40207).");
		}
		if ((bool)bcd_errors[7])
		{
			stringBuilder.AppendLine("FAL generated (A40215).");
		}
		if ((bool)bcd_errors[8])
		{
			stringBuilder.AppendLine("Memory error (A40115).");
		}
		if ((bool)bcd_errors[10])
		{
			stringBuilder.AppendLine("I/O bus error (A40114).");
		}
		if ((bool)bcd_errors[14])
		{
			stringBuilder.AppendLine("No end instruction error (FALS) (A40109 Program error).");
		}
		if ((bool)bcd_errors[15])
		{
			stringBuilder.AppendLine("System error (FALS) (A40106)");
		}
		if ((bool)bcd_errors[27])
		{
			stringBuilder.AppendLine("I/O verify error (A40209).");
		}
		if ((bool)bcd_errors[28])
		{
			stringBuilder.AppendLine("Cycle time overrun (A40108).");
		}
		if ((bool)bcd_errors[29])
		{
			stringBuilder.AppendLine("Number duplication (A40113)");
		}
		if ((bool)bcd_errors[30])
		{
			stringBuilder.AppendLine("I/O setting error (A40110).");
		}
		if ((bool)bcd_errors[31])
		{
			stringBuilder.AppendLine("SYSMAC BUS error (A40205).");
		}
		return stringBuilder.ToString();
	}

	public static string ToByteOrder(string dataHex)
	{
		int length = dataHex.Length;
		return length switch
		{
			16 => dataHex.Substring(14, 2) + dataHex.Substring(12, 2) + dataHex.Substring(10, 2) + dataHex.Substring(8, 2) + dataHex.Substring(6, 2) + dataHex.Substring(4, 2) + dataHex.Substring(2, 2) + dataHex.Substring(0, 2), 
			8 => dataHex.Substring(6, 2) + dataHex.Substring(4, 2) + dataHex.Substring(2, 2) + dataHex.Substring(0, 2), 
			4 => dataHex.Substring(2, 2) + dataHex.Substring(0, 2), 
			_ => throw new InvalidDataException($"Byte order is undefined with the number of bytes={length}. The number of valid bytes is: 2, 4, 8 bytes"), 
		};
	}
}
