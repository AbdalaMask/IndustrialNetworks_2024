using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.Panasonic.Mewtocol.Codes;

namespace NetStudio.Panasonic.Mewtocol;

public class MewtocolUtility
{
	public static readonly Dictionary<string, string> DataCodes = new Dictionary<string, string>
	{
		{ "X", "X" },
		{ "Y", "Y" },
		{ "R", "R" },
		{ "L", "L" },
		{ "T", "T" },
		{ "C", "C" },
		{ "WX", "X" },
		{ "DWX", "X" },
		{ "LWX", "X" },
		{ "WY", "Y" },
		{ "DWY", "Y" },
		{ "LWY", "Y" },
		{ "WR", "R" },
		{ "DWR", "R" },
		{ "LWR", "R" },
		{ "WL", "L" },
		{ "DWL", "L" },
		{ "LWL", "L" },
		{ "DT", "D" },
		{ "DDT", "D" },
		{ "LDT", "D" },
		{ "LD", "L" },
		{ "DLD", "L" },
		{ "LLD", "L" },
		{ "FL", "F" },
		{ "DFL", "F" },
		{ "LFL", "F" },
		{ "E", "E" },
		{ "SV", "SV" },
		{ "TSV", "SV" },
		{ "CSV", "SV" },
		{ "EV", "EV" },
		{ "TEV", "EV" },
		{ "CEV", "EV" },
		{ "O", "RM" }
	};

	public static readonly Dictionary<string, AreaCode> Areas = new Dictionary<string, AreaCode>
	{
		{
			"X",
			AreaCode.Contact
		},
		{
			"Y",
			AreaCode.Contact
		},
		{
			"R",
			AreaCode.Contact
		},
		{
			"L",
			AreaCode.Contact
		},
		{
			"T",
			AreaCode.Contact
		},
		{
			"C",
			AreaCode.Contact
		},
		{
			"WX",
			AreaCode.Contact
		},
		{
			"DWX",
			AreaCode.Contact
		},
		{
			"LWX",
			AreaCode.Contact
		},
		{
			"WY",
			AreaCode.Contact
		},
		{
			"DWY",
			AreaCode.Contact
		},
		{
			"LWY",
			AreaCode.Contact
		},
		{
			"WR",
			AreaCode.Contact
		},
		{
			"DWR",
			AreaCode.Contact
		},
		{
			"LWR",
			AreaCode.Contact
		},
		{
			"WL",
			AreaCode.Contact
		},
		{
			"DWL",
			AreaCode.Contact
		},
		{
			"LWL",
			AreaCode.Contact
		},
		{
			"DT",
			AreaCode.Data
		},
		{
			"DDT",
			AreaCode.Data
		},
		{
			"LDT",
			AreaCode.Data
		},
		{
			"LD",
			AreaCode.Data
		},
		{
			"DLD",
			AreaCode.Data
		},
		{
			"LLD",
			AreaCode.Data
		},
		{
			"FL",
			AreaCode.Data
		},
		{
			"DFL",
			AreaCode.Data
		},
		{
			"LFL",
			AreaCode.Data
		},
		{
			"E",
			AreaCode.Data
		},
		{
			"SV",
			AreaCode.SetValue
		},
		{
			"TSV",
			AreaCode.SetValue
		},
		{
			"CSV",
			AreaCode.SetValue
		},
		{
			"EV",
			AreaCode.ElapsedValue
		},
		{
			"TEV",
			AreaCode.ElapsedValue
		},
		{
			"CEV",
			AreaCode.ElapsedValue
		},
		{
			"O",
			AreaCode.OperationMode
		}
	};

	public static bool IsHexadecimal(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "L";
		case "X":
		case "Y":
		case "R":
			return true;
		}
	}

	public static bool IsBitMemory(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "C";
		case "X":
		case "Y":
		case "R":
		case "L":
		case "T":
			return true;
		}
	}

	public static bool IsBitInWord(Tag tg)
	{
		return IsContactArea(GetMemory(tg));
	}

	public static bool IsBitInWord(string memory)
	{
		return IsContactArea(memory);
	}

	public static bool IsContactArea(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "DWR";
		case "X":
		case "Y":
		case "R":
		case "L":
		case "T":
		case "C":
		case "WX":
		case "WY":
		case "WR":
		case "DWX":
		case "DWY":
			return true;
		}
	}

	public static bool IsDataArea(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "LFL";
		case "DT":
		case "DDT":
		case "LDT":
		case "LD":
		case "DLD":
		case "LLD":
		case "FL":
		case "DFL":
			return true;
		}
	}

	public static string GetMemory(Tag tg)
	{
		if (tg.DataType == DataType.BOOL)
		{
			return tg.Address.Substring(0, 1).ToUpper();
		}
		return string.Join("", from char_0 in tg.Address
			where char.IsLetter(char_0)
			select (char_0)).ToUpper();
	}

	public static int GetFullAddress(Tag tg)
	{
		if (tg.DataType == DataType.BOOL)
		{
			return Convert.ToUInt16(tg.Address.Substring(1), 16);
		}
		return int.Parse(string.Join("", from char_0 in tg.Address
			where char.IsDigit(char_0)
			select (char_0)));
	}

	public static int GetWordAddress(Tag tg)
	{
		string memory = GetMemory(tg);
		string text;
		if (tg.DataType == DataType.BOOL)
		{
			text = tg.Address.Substring(1);
			if (text.Length == 1)
			{
				return 0;
			}
			return int.Parse(text.Substring(0, text.Length - 1));
		}
		text = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
		return int.Parse(text);
	}

	public static int GetAddress(Tag tg)
	{
		string memory = GetMemory(tg);
		string text;
		if (tg.DataType == DataType.BOOL)
		{
			text = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
			if (IsHexadecimal(memory))
			{
				return Convert.ToInt32(text.Substring(text.Length - 1, 1), 16);
			}
		}
		else
		{
			text = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
		}
		return int.Parse(text);
	}

	public static int GetIndexOfWordAddress(Tag startTag, Tag endTag)
	{
		int wordAddress = GetWordAddress(startTag);
		int wordAddress2 = GetWordAddress(endTag);
		if (wordAddress2 < wordAddress)
		{
			throw new Exception("Invalid FP address: " + endTag.Address);
		}
		return wordAddress2 - wordAddress;
	}

	public static void IncrementWordAddress(Tag tg)
	{
        int num;
        int num2;
        switch (tg.DataType)
        {
            case DataType.BOOL:
                IncrementBitAddress(tg);
                SetTagName(tg);
                return;
            default:
                throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
            case DataType.INT:
            case DataType.UINT:
            case DataType.WORD:
            case DataType.TIME16:
                num = 2;
                num2 = 1;
                break;
            case DataType.DINT:
            case DataType.UDINT:
            case DataType.DWORD:
            case DataType.REAL:
            case DataType.TIME32:
                num2 = 2;
                num = 3;
                break;
            case DataType.STRING:
                num = 2;
                num2 = ((tg.Resolution % 2 == 0) ? ((ushort)(2 + tg.Resolution / 2)) : ((ushort)(2 + (tg.Resolution + 1) / 2)));
                break;
        }
        string text = tg.Address.Substring(0, num);
		int value = int.Parse(tg.Address.Substring(text.Length)) + num2;
		tg.Address = $"{text}{value}";
		SetTagName(tg);
	}

	public static void IncrementBitAddress(Tag tg)
	{
		string text = tg.Address.Substring(0, 1);
		string text2 = tg.Address.Substring(1, tg.Address.Length - 1).PadLeft(4, '0');
		ushort num = ushort.Parse(text2.Substring(0, text2.Length - 1));
		ushort num2 = Convert.ToByte(text2.Last().ToString(), 16);
		num2++;
		if (num2 >= 16)
		{
			num++;
			num2 = 0;
		}
		if (num == 0)
		{
			tg.Address = text + num2.ToString("X");
			return;
		}
		tg.Address = text + $"{num}{num2.ToString("X")}";
	}

	public static ValidateResult TagValidate(Tag tg)
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid
		};
		string memory = GetMemory(tg);
		if (!Areas.ContainsKey(memory))
		{
			validateResult.Message = "This address type is not supported.";
		}
		else
		{
			switch (tg.DataType)
			{
			case DataType.BOOL:
				if (tg.Address.Length <= 1)
				{
					validateResult.Message = "Data type and Address are not match.\n\rValid address example: Y0, Y2E, X0, X1, X0F,...";
				}
				if (!IsBitMemory(tg.Address.Substring(0, 1)))
				{
					validateResult.Message = "Data type and Address are not match.";
				}
				break;
			default:
				throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
			case DataType.DINT:
			case DataType.UDINT:
			case DataType.DWORD:
			case DataType.REAL:
			case DataType.TIME32:
				if (tg.Address.Length < 4 || memory.Length != 3)
				{
					validateResult.Message = "Data type and Address are not match.\n\rValid address example: DDT0, DDT2, DLD0, CD0, FD0,...";
				}
				break;
			case DataType.INT:
			case DataType.UINT:
			case DataType.WORD:
			case DataType.TIME16:
			case DataType.STRING:
				if (tg.Address.Length < 3 || memory.Length != 2)
				{
					validateResult.Message = "Data type and Address are not match.\n\rValid address example: DT0, DT1, WY4, WX0, DL0, DL1,...";
				}
				break;
			}
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

	public static bool IsSetValueArea(string memory)
	{
		return memory == "SV";
	}

	public static bool IselapsedValueArea(string memory)
	{
		return memory == "EV";
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
			if (tg.Resolution % 2 != 0)
			{
				return (ushort)(2 + (tg.Resolution + 1) / 2);
			}
			return (ushort)(2 + tg.Resolution / 2);
		}
	}

	public static KeyValuePair<string, string> ConvertToWordAddress(Tag tg)
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (tg.DataType == DataType.BOOL)
		{
			empty = tg.Address.Substring(0, 1);
			empty2 = tg.Address.Substring(1);
			empty2 = ((empty2.Length != 1) ? empty2.Substring(0, empty2.Length - 1) : "0");
		}
		else
		{
			empty = string.Join("", from char_0 in tg.Address
				where char.IsLetter(char_0)
				select (char_0));
			empty2 = string.Join("", from char_0 in tg.Address
				where char.IsDigit(char_0)
				select (char_0));
		}
		if (!DataCodes.ContainsKey(empty))
		{
			throw new NotSupportedException("This address type is not supported.");
		}
		return new KeyValuePair<string, string>(empty, empty2);
	}

	public static string Sort(string dataHex)
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
