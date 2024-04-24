using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;

namespace NetStudio.Mitsubishi.Dedicated;

public static class DedicatedUtility
{
	public static Dictionary<string, byte> DeviceCodes = new Dictionary<string, byte>
	{
		{ "X", 156 },
		{ "Y", 157 },
		{ "M", 144 },
		{ "SM", 145 },
		{ "L", 146 },
		{ "F", 147 },
		{ "V", 148 },
		{ "S", 152 },
		{ "B", 160 },
		{ "SB", 161 },
		{ "DX", 162 },
		{ "DY", 163 },
		{ "D", 168 },
		{ "SD", 169 },
		{ "R", 175 },
		{ "ZR", 176 },
		{ "W", 180 },
		{ "SW", 181 },
		{ "TC", 192 },
		{ "TS", 193 },
		{ "TN", 194 },
		{ "CC", 195 },
		{ "CS", 196 },
		{ "CN", 197 },
		{ "SC", 198 },
		{ "SS", 199 },
		{ "SN", 200 },
		{ "Z", 204 }
	};

	public static string GetMemory(Tag tg)
	{
		string text = string.Join("", from char_0 in tg.Address.Substring(0, 2)
			where char.IsLetter(char_0)
			select (char_0));
		if (!DeviceCodes.ContainsKey(text))
		{
			throw new NotSupportedException(text + ": This data type is not supported.");
		}
		return text;
	}

	public static int GetWordAddress(Tag tg)
	{
		string memory = GetMemory(tg);
		string value = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
		if (!IsBitMemory(memory))
		{
			if (IsHexadecimal(memory))
			{
				return Convert.ToInt32(value, 16);
			}
			return Convert.ToInt32(value);
		}
		int num = (IsHexadecimal(memory) ? Convert.ToInt32(value, 16) : ((!IsOctal(memory)) ? Convert.ToInt32(value) : Convert.ToInt32(value, 8)));
		return num / 16;
	}

	public static int GetBitAddress(Tag tg)
	{
		string memory = GetMemory(tg);
		string text = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
		int num = (IsHexadecimal(memory) ? Convert.ToInt32(text, 16) : ((!IsOctal(memory)) ? int.Parse(text) : Convert.ToInt32(text, 8)));
		return num % 16;
	}

	public static int GetSizeOfDataType(Tag tg)
	{
        switch (tg.DataType)
		{
		default:
			throw new NotSupportedException($"{tg.DataType}: " + "This data type is not supported.");
		case DataType.BOOL:
			return 1;
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
			return 4;
		case DataType.BYTE:
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
			return 64;
		}
	}

	public static int GetIndexOfWordAddress(Tag startTag, Tag endTag)
	{
		if (startTag.WordAddress > endTag.WordAddress)
		{
			throw new InvalidDataException("End tag: Invalid address.");
		}
		return endTag.WordAddress - startTag.WordAddress;
	}

	public static string IncrementWordAddress(Tag tg)
	{
		string memory = GetMemory(tg);
		int num = GetSizeOfDataType(tg);
		if (tg.DataType == DataType.STRING)
		{
			num = tg.Resolution;
		}
		string value = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
		string text;
		if (!IsBitMemory(memory))
		{
			text = ((!IsHexadecimal(memory)) ? (memory + (Convert.ToInt32(value) + num).ToString("D4")) : (memory + (Convert.ToInt32(value, 16) + num).ToString("X4")));
		}
		else if (IsHexadecimal(memory))
		{
			text = memory + ((tg.DataType != 0) ? (Convert.ToInt32(value, 16) + num * 16) : (Convert.ToInt32(value, 16) + num)).ToString("X4");
		}
		else if (IsOctal(memory))
		{
			int num2 = ((tg.DataType != 0) ? (Convert.ToInt32(value, 8) + num * 16) : (Convert.ToInt32(value, 8) + num));
			text = memory + Convert.ToString(num2, 8).PadLeft(4, '0');
		}
		else
		{
			text = memory + ((tg.DataType != 0) ? (Convert.ToInt32(value) + num * 16) : (Convert.ToInt32(value) + num)).ToString("D4");
		}
		tg.Address = text;
		SetTagName(tg);
		return text.ToUpper();
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

	public static ValidateResult TagValidate(Tag tg)
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid
		};
		tg.Address = (tg.Address ?? "").ToUpper();
		string memory = GetMemory(tg);
		if (!DeviceCodes.ContainsKey(memory))
		{
			validateResult.Message = memory + ": This data type is not supported.";
		}
		if (string.IsNullOrEmpty(validateResult.Message))
		{
			string text = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
			bool num = tg.Address == tg.Name;
			tg.Address = memory + ((text.Length < 4) ? text.PadLeft(4, '0') : text);
			tg.Address.ToUpper();
			if (num)
			{
				tg.Name = tg.Address;
			}
			validateResult.Status = ValidateStatus.Valid;
		}
		else
		{
			validateResult.Status = ValidateStatus.Invalid;
		}
		try
		{
			int sizeOfDataType = GetSizeOfDataType(tg);
			if (tg.DataType == DataType.STRING)
			{
				if (tg.Resolution % 2 != 0)
				{
					tg.Resolution++;
				}
				if (tg.Resolution < 2)
				{
					tg.Resolution = (ushort)sizeOfDataType;
				}
				else if (tg.Resolution > 1024)
				{
					tg.Resolution = 1024;
				}
			}
		}
		catch (Exception ex)
		{
			validateResult.Status = ValidateStatus.Invalid;
			validateResult.Message = ex.Message;
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

	public static bool IsHexadecimal(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "SW";
		case "B":
		case "SB":
		case "W":
			return true;
		}
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
		case "TS":
		case "TC":
			return true;
		}
	}
}
