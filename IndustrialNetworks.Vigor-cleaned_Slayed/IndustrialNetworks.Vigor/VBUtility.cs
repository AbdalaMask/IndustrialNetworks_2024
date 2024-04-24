using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.Vigor.Enums;

namespace NetStudio.Vigor;

public class VBUtility
{
	public static Dictionary<string, string> Memories = new Dictionary<string, string>
	{
		{ "X", "0000" },
		{ "Y", "0040" },
		{ "S", "0300" },
		{ "M", "0080" },
		{ "SM", "03E0" },
		{ "D", "1C00" },
		{ "SD", "1600" },
		{ "T", "1400" },
		{ "T_CONTACT", "0380" },
		{ "T_COIL", "0780" },
		{ "C16", "1800" },
		{ "C32", "1A00" },
		{ "C_CONTACT", "03A0" },
		{ "C_COIL", "07A0" }
	};

	public static Dictionary<string, string> Registers = new Dictionary<string, string>
	{
		{ "X: Input Relay", "0000" },
		{ "Y: Output Relay", "0040" },
		{ "S: Step Relay", "0300" },
		{ "M: Aux Relay", "0080" },
		{ "M: Special Relay", "03E0" },
		{ "D: General Register", "1C00" },
		{ "D: Special Register", "1600" },
		{ "T: Current Value of Timer", "1400" },
		{ "C: Current Value of 16bits Counter", "1800" },
		{ "C: Current Value of 32bits Counter", "1A00" }
	};

	public static Dictionary<string, string> Bits = new Dictionary<string, string>
	{
		{ "X: Input Relay", "0000" },
		{ "Y: Output Relay", "0040" },
		{ "S: Step Relay", "0300" },
		{ "M: Aux Relay", "0080" },
		{ "M: Special Relay", "03E0" },
		{ "T: Timer Contact", "0380" },
		{ "T: Timer Coil", "0780" },
		{ "C: Counter Contact", "03A0" },
		{ "C: Counter Coil", "07A0" }
	};

	public static void Validate(string statusCode)
	{
		switch (statusCode)
		{
		default:
			throw new InvalidDataException("An unknown error.");
		case "28":
			throw new InvalidDataException("Address out of range.");
		case "14":
			throw new InvalidDataException("Stop, parity error, frame error, overrun.");
		case "12":
			throw new InvalidDataException("Command undifine.");
		case "11":
			throw new InvalidDataException("Check sum error.");
		case "10":
			throw new InvalidDataException("ASCII Code error.");
		}
	}

	public static string GetSpecialMemory(Tag tg)
	{
		string text = tg.Address.Substring(0, 1).ToUpper();
        int num;
        switch (text)
		{
		case "M":
			num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			if (num >= 9000)
			{
				text = "SM";
			}
			break;
		case "C":
			num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			text = ((tg.DataType != 0) ? ((num < 200) ? "C16" : "C32") : "C_CONTACT");
			break;
		case "T":
			text = ((tg.DataType != 0) ? "T" : "T_CONTACT");
			break;
		case "D":
			num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			if (num >= 9000)
			{
				text = "SD";
			}
			break;
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
			if (num >= 9000)
			{
				text = "SM";
				offset = 9000;
			}
			break;
		case "C":
			num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			if (tg.DataType == DataType.BOOL)
			{
				text = "C_CONTACT";
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
			text = ((tg.DataType != 0) ? "T" : "T_CONTACT");
			break;
		case "D":
			num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			if (num >= 9000)
			{
				text = "SD";
				offset = 9000;
			}
			break;
		}
		return text;
	}

	public static int GetSpecialAddress(string memory, int address)
	{
		int result = address;
		switch (memory)
		{
		case "C32":
			result = address - 200;
			break;
		case "SD":
		case "SM":
			result = address - 9000;
			break;
		case "D":
		case "M":
			if (address >= 9000)
			{
				result = address - 9000;
			}
			break;
		}
		return result;
	}

	public static int GetByteAddress(Tag tg)
	{
		string specialMemory = GetSpecialMemory(tg);
		string text = tg.Address.Substring(0, 1);
		int address = int.Parse(tg.Address.Substring(text.Length, tg.Address.Length - text.Length));
		int num = int.Parse(Memories[specialMemory], NumberStyles.HexNumber);
		if (tg.DataType == DataType.BOOL)
		{
			return num + GetSpecialAddress(text, address) / 8;
		}
		int sizeOfDataType = GetSizeOfDataType(tg.DataType);
		return num + sizeOfDataType * GetSpecialAddress(text, address);
	}

	public static int GetBitAddress(Tag tg)
	{
		string specialMemory = GetSpecialMemory(tg);
		string text = tg.Address.Substring(0, 1);
		int num = int.Parse(Memories[specialMemory], NumberStyles.HexNumber);
		int num2 = int.Parse(tg.Address.Substring(text.Length, tg.Address.Length - text.Length));
		int num3 = num2 / 8;
		int num4 = num2 % 8;
		return 8 * (num + num3) + num4;
	}

	public static ushort GetSizeOfDataType(DataType dataType)
	{
        switch (dataType)
		{
		default:
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
		case DataType.STRING:
			throw new NotSupportedException("This data type is not supported.");
		}
	}

	public static int GetIndexOfBitAddress(Tag startTag, Tag endTag)
	{
		int num = startTag.BitAddress % 8;
		return endTag.BitAddress - startTag.BitAddress + num;
	}

	public static int GetIndexOfByteAddress(Tag startTag, Tag endTag)
	{
		return endTag.ByteAddress - startTag.ByteAddress;
	}

	public static int GetIndexOfWordAddress(Tag startTag, Tag endTag)
	{
		return endTag.WordAddress - startTag.WordAddress;
	}

	public static ValidateResult TagValidate(Tag tg)
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid
		};
		if (!string.IsNullOrEmpty(tg.Address) && !string.IsNullOrWhiteSpace(tg.Address))
		{
			tg.Address = tg.Address.ToUpper();
			switch (tg.DataType)
			{
			default:
				throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
			case DataType.BOOL:
			case DataType.INT:
			case DataType.UINT:
			case DataType.WORD:
			case DataType.DINT:
			case DataType.UDINT:
			case DataType.DWORD:
			case DataType.REAL:
			case DataType.TIME16:
			case DataType.TIME32:
				if (tg.Address.StartsWith("C") && tg.DataType != 0)
				{
					if (VSUtility.GetDeviceCodeByCounter(tg.Address) == DeviceCode.Counter16Bit)
					{
						tg.DataType = DataType.INT;
						if (string.IsNullOrEmpty(tg.Description) || string.IsNullOrWhiteSpace(tg.Description))
						{
							tg.Description = "16-bit Counter. Counting direction: Up only. Available set value: 1 to 32,767.";
						}
					}
					else
					{
						tg.DataType = DataType.DINT;
						if (string.IsNullOrEmpty(tg.Description) || string.IsNullOrWhiteSpace(tg.Description) || tg.Description.Contains("16-bit"))
						{
							tg.Description = "32-bit Counter. Counting direction: Up or Down(Selectable or control by inputs.). Available set value: -2,147,483,648 to +2,147,483,647.";
						}
					}
					tg.Mode = TagMode.ReadOnly;
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
		}
		return validateResult;
	}

	public static void IncrementWordAddress(Tag tg)
	{
        string text = tg.Address.Substring(0, 1).ToUpper();
        string text2 = tg.Address.Substring(text.Length, tg.Address.Length - text.Length);
        string text3 = !text2.Contains(".") ? text2 : text2.Split('.')[0];
        int num2 = int.Parse(text3);
		if (text == "C")
		{
			tg.Address = $"{text}{++num2}";
			SetTagName(tg);
			return;
		}
        int num;
        switch (tg.DataType)
        {
            case DataType.BOOL:
                {
                    if (!(text == "D") && !(text == "R"))
                    {
                        tg.Address = $"{text}{++num2}";
                        SetTagName(tg);
                        return;
                    }
                    string s = "0";
                    if (text2.Contains("."))
                    {
                        s = text2.Split('.')[1];
                    }
                    int num3 = int.Parse(s);
                    num3++;
                    if (num3 > 15)
                    {
                        num3 = 0;
                        num2++;
                    }
                    tg.Address = $"{text}{num2}.{num3.ToString("X1")}";
                    SetTagName(tg);
                    return;
                }
            default:
                throw new NotSupportedException($"{tg.DataType}: This data type is not supported");
            case DataType.BYTE:
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
        }
        num2 += num;
		tg.Address = $"{text}{num2}";
		SetTagName(tg);
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

	public static ushort GetResolution(string address)
	{
		ushort result = 1;
		int num = int.Parse(address.Substring(1, address.Length - 1));
		if ((num >= 0 && num <= 199) || (num >= 250 && num <= 255))
		{
			result = 100;
		}
		else if (num >= 200 && num <= 245)
		{
			result = 10;
		}
		return result;
	}

	public static ushort GetResolution(Tag tg)
	{
		ushort result = 1;
		if (GetSpecialMemory(tg) == "T")
		{
			int num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
			if ((num >= 0 && num <= 199) || (num >= 250 && num <= 255))
			{
				result = 100;
			}
			else if (num >= 200 && num <= 245)
			{
				result = 10;
			}
		}
		return result;
	}

	public static byte[] Sort(byte[] values)
	{
		int num = values.Length;
		return num switch
		{
			8 => new byte[8]
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
			4 => new byte[4]
			{
				values[0],
				values[1],
				values[2],
				values[3]
			}, 
			2 => new byte[2]
			{
				values[0],
				values[1]
			}, 
			_ => throw new InvalidDataException($"Byte order is undefined with the number of bytes={num}. The number of valid bytes is: 2, 4, 8 bytes"), 
		};
	}
}
