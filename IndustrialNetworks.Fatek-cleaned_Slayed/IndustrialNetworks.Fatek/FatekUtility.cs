using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;

namespace NetStudio.Fatek;

public class FatekUtility
{
	public static readonly Dictionary<string, string> Classifies = new Dictionary<string, string>
	{
		{ "X", "X" },
		{ "WX", "X" },
		{ "DWX", "X" },
		{ "Y", "Y" },
		{ "WY", "Y" },
		{ "DWY", "Y" },
		{ "M", "M" },
		{ "WM", "M" },
		{ "DWM", "M" },
		{ "S", "S" },
		{ "WS", "S" },
		{ "DWS", "S" },
		{ "C", "C" },
		{ "WC", "C" },
		{ "DWC", "C" },
		{ "T", "T" },
		{ "WT", "T" },
		{ "DWT", "T" },
		{ "R", "R" },
		{ "DR", "R" },
		{ "D", "D" },
		{ "DD", "D" },
		{ "TMR", "TMR" },
		{ "CTR", "CTR" },
		{ "RT", "RT" },
		{ "DRT", "RT" },
		{ "RC", "RC" },
		{ "DRC", "RC" }
	};

	public static readonly Dictionary<string, string> Memories = new Dictionary<string, string>
	{
		{ "X", "X" },
		{ "WX", "WX" },
		{ "DWX", "DWX" },
		{ "Y", "Y" },
		{ "WY", "WY" },
		{ "DWY", "DWY" },
		{ "M", "M" },
		{ "WM", "WM" },
		{ "DWM", "DWM" },
		{ "S", "S" },
		{ "WS", "WS" },
		{ "DWS", "DWS" },
		{ "C", "C" },
		{ "WC", "WC" },
		{ "DWC", "DWC" },
		{ "T", "T" },
		{ "WT", "WT" },
		{ "DWT", "DWT" },
		{ "R", "R" },
		{ "DR", "DR" },
		{ "D", "D" },
		{ "DD", "DD" },
		{ "TMR", "TMR" },
		{ "CTR", "CTR" },
		{ "RT", "RT" },
		{ "DRT", "DRT" },
		{ "RC", "RC" },
		{ "DRC", "DRC" }
	};

	public static readonly Dictionary<string, string> Errors = new Dictionary<string, string>
	{
		{ "2", "Error code 2: Illegal value." },
		{ "4", "Error code 4: Illegal format, or communication command can not execute." },
		{ "5", "Error code 5: Can not run（Ladder Checksum error when run PLC)." },
		{ "6", "Error code 6: Can not run（PLC ID≠Ladder ID when run PLC." },
		{ "7", "Error code 7: Can not run（Snytax check error when run PLC)." },
		{ "9", "Error code 9: Can not run（Function not supported)." },
		{ "10", "Error code A: Illegal address." }
	};

	public static bool IsBitMemory(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "DWC";
		case "X":
		case "WX":
		case "DWX":
		case "Y":
		case "WY":
		case "DWY":
		case "M":
		case "WM":
		case "DWM":
		case "S":
		case "WS":
		case "DWS":
		case "T":
		case "WT":
		case "DWT":
		case "C":
		case "WC":
			return true;
		}
	}

	public static bool IsWord4Memory(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "DRC";
		case "RT":
		case "DRT":
		case "RC":
			return true;
		}
	}

	public static bool IsWord5Memory(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "DR";
		case "D":
		case "R":
		case "DD":
			return true;
		}
	}

	public static string GetClassify(Tag tg)
	{
		string text = string.Join("", from char_0 in tg.Address
			where char.IsLetter(char_0)
			select (char_0));
		return Classifies[text.ToUpper()];
	}

	public static string GetMemory(Tag tg)
	{
		return string.Join("", from char_0 in tg.Address
			where char.IsLetter(char_0)
			select (char_0)).ToUpper();
	}

	public static ValidateResult TagValidate(Tag tg)
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid
		};
		string memory = GetMemory(tg);
		string text = tg.Address.Substring(memory.Length);
		if (!Memories.ContainsKey(memory))
		{
			validateResult.Status = ValidateStatus.Invalid;
			validateResult.Message = "This address type is not supported.\n\rExamples of valid addresses: X0000, WX0000, DWX0000, Y0001, WY0000 ,DWY0000, R12345, DR00000, D01346, DD00000, M0003, WM0000, DWM0000, S0005, WS0000, DWS0000, T0001, WT0000, DWT0000, C0002, WC0000, DWC0000, RT0001, DRT0000, RC0002, DRC0000...for all data types.";
			return validateResult;
		}
		validateResult.Status = ValidateStatus.Valid;
		int sizeOfDataType;
		try
		{
			sizeOfDataType = GetSizeOfDataType(tg);
		}
		catch (Exception ex)
		{
			validateResult.Status = ValidateStatus.Invalid;
			validateResult.Message = ex.Message;
			return validateResult;
		}
		if (IsBitMemory(memory))
		{
			if (tg.DataType != 0 && tg.DataType != DataType.TIME16)
			{
				if (sizeOfDataType + 1 != memory.Length)
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
				if (!Memories.ContainsKey(memory))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
			}
			tg.Address = memory + tg.Address.Substring(memory.Length).PadLeft(4, '0');
		}
		else
		{
			if (tg.DataType == DataType.BOOL)
			{
				validateResult.Status = ValidateStatus.Invalid;
				validateResult.Message = "Data type and Address are not match.";
				return validateResult;
			}
			if (!Memories.ContainsKey(memory))
			{
				validateResult.Status = ValidateStatus.Invalid;
				validateResult.Message = "Data type and Address are not match.";
				return validateResult;
			}
			if (IsWord4Memory(memory))
			{
				if (sizeOfDataType + 1 != memory.Length)
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
				if (text.Length < 1 || text.Length > 4)
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
			}
			if (IsWord5Memory(memory))
			{
				if (sizeOfDataType != memory.Length)
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
				if (text.Length < 1 || text.Length > 5)
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
			}
			else if (sizeOfDataType + 1 != memory.Length)
			{
				validateResult.Status = ValidateStatus.Invalid;
				validateResult.Message = "Data type and Address are not match.";
				return validateResult;
			}
			if (!(memory == "D") && !(memory == "R"))
			{
				tg.Address = memory + text.PadLeft(4, '0');
			}
			else
			{
				tg.Address = memory + text.PadLeft(5, '0');
			}
		}
		return validateResult;
	}

	public static void IncrementWordAddress(Tag tg)
	{
		string memory = GetMemory(tg);
		ushort num = ushort.Parse(tg.Address.Substring(memory.Length));
		if (IsBitMemory(memory))
		{
			if (tg.DataType == DataType.BOOL)
			{
				tg.Address = memory + $"{++num}".PadLeft(4, '0');
			}
			else
			{
				switch (tg.DataType)
				{
				default:
					throw new NotSupportedException();
				case DataType.INT:
				case DataType.UINT:
				case DataType.WORD:
				case DataType.TIME16:
					if (!(memory == "R") && !(memory == "D"))
					{
						tg.Address = memory + $"{num += 16}".PadLeft(4, '0');
					}
					else
					{
						tg.Address = memory + $"{num += 16}".PadLeft(5, '0');
					}
					break;
				case DataType.DINT:
				case DataType.UDINT:
				case DataType.DWORD:
				case DataType.REAL:
				case DataType.TIME32:
					if (!(memory == "R") && !(memory == "D"))
					{
						tg.Address = memory + $"{num += 32}".PadLeft(4, '0');
					}
					else
					{
						tg.Address = memory + $"{num += 32}".PadLeft(5, '0');
					}
					break;
				}
			}
		}
		else
		{
			ushort num2 = 1;
			switch (tg.DataType)
			{
			default:
				throw new NotSupportedException();
			case DataType.LINT:
			case DataType.ULINT:
			case DataType.LWORD:
			case DataType.LREAL:
				num2 = 4;
				break;
			case DataType.INT:
			case DataType.UINT:
			case DataType.WORD:
			case DataType.TIME16:
				num2 = 1;
				break;
			case DataType.DINT:
			case DataType.UDINT:
			case DataType.DWORD:
			case DataType.REAL:
			case DataType.TIME32:
				num2 = 2;
				break;
			case DataType.BOOL:
				break;
			}
			if (!(memory == "R") && !(memory == "D"))
			{
				tg.Address = memory + $"{num += num2}".PadLeft(4, '0');
			}
			else
			{
				tg.Address = memory + $"{num += num2}".PadLeft(5, '0');
			}
		}
		SetTagName(tg);
	}

	public static int GetSizeOfDataType(Tag tg)
	{
        switch (tg.DataType)
		{
		case DataType.BOOL:
			return 1;
		default:
			throw new NotSupportedException($"{tg.DataType}: " + "This data type is not supported.");
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
			return 4;
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

	public static int GetWordAddress(Tag tg)
	{
		string memory = GetMemory(tg);
		if (IsBitMemory(GetMemory(tg)))
		{
			return int.Parse(tg.Address.Substring(memory.Length)) / 16;
		}
		if (tg.DataType == DataType.BYTE)
		{
			return int.Parse(tg.Address.Substring(memory.Length)) / 8;
		}
		return int.Parse(tg.Address.Substring(memory.Length));
	}

	public static int GetIndexOfWordAddress(Tag startTag, Tag endTag)
	{
		return endTag.WordAddress - startTag.WordAddress;
	}

	public static int GetIndexOfBitAddress(Tag tg)
	{
		string memory = GetMemory(tg);
		return int.Parse(tg.Address.Substring(memory.Length)) % 16;
	}

	private static void SetTagName(Tag tg)
	{
		tg.Name = tg.Address;
		switch (tg.DataType)
		{
		case DataType.BOOL:
		case DataType.INT:
		case DataType.UINT:
		case DataType.WORD:
		case DataType.DINT:
		case DataType.UDINT:
		case DataType.DWORD:
		case DataType.REAL:
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
		case DataType.TIME16:
		case DataType.TIME32:
			return;
		}
		throw new NotSupportedException($"{tg.DataType}: " + "This data type is not supported.");
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
}
