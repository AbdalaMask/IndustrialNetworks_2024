using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.Mitsubishi.Models;

namespace NetStudio.Mitsubishi.MC;

public sealed class MCUtility
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

	public static bool IsHexadecimal(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "ZR";
		case "X":
		case "DX":
		case "Y":
		case "DY":
		case "B":
		case "SB":
		case "W":
		case "SW":
			return true;
		}
	}

	public static bool IsBitMemory(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "LCC";
		case "X":
		case "DX":
		case "Y":
		case "DY":
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
		case "LTS":
		case "LTC":
		case "STS":
		case "STC":
		case "LSTS":
		case "LSTC":
		case "CS":
		case "CC":
		case "LCS":
			return true;
		}
	}

	public static string GetMemory(Tag tg)
	{
		string text = tg.Address.Substring(0, 1);
		if (!IsHexadecimal(text))
		{
			text = string.Join("", from char_0 in tg.Address
				where char.IsLetter(char_0)
				select (char_0));
		}
		if (!DeviceCodes.ContainsKey(text))
		{
			throw new NotSupportedException(text + ": This data type is not supported.");
		}
		return text;
	}

	public static MitDevice ParseAddress(Tag tg)
	{
		MitDevice mitDevice = new MitDevice();
		string memory = GetMemory(tg);
		string text = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
		mitDevice.HeaderDeviceNumber = (IsHexadecimal(memory) ? Convert.ToInt32(text, 16) : int.Parse(text));
		mitDevice.DeviceCode = DeviceCodes[memory];
		return mitDevice;
	}

	public static byte GetDeviceCode(Tag tg)
	{
		string memory = GetMemory(tg);
		return DeviceCodes[memory];
	}

	public static int GetWordAddress(Tag tg)
	{
		string memory = GetMemory(tg);
		string text = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
		int num = (IsHexadecimal(memory) ? Convert.ToInt32(text, 16) : int.Parse(text));
		if (IsBitMemory(memory))
		{
			num /= 16;
		}
		return num;
	}

	public static int GetHeaderDeviceNumber(Tag tg)
	{
		string memory = GetMemory(tg);
		string text = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
		if (!IsHexadecimal(memory))
		{
			return int.Parse(text);
		}
		return Convert.ToInt32(text, 16);
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

	public static string IncrementWordAddress(Device device_0, Tag tg)
	{
		string memory = GetMemory(tg);
		int num = GetSizeOfDataType(tg);
		if (tg.DataType == DataType.STRING)
		{
			num = tg.Resolution;
		}
		string value = tg.Address.Substring(memory.Length, tg.Address.Length - memory.Length);
		string text2 = (tg.Address = (IsBitMemory(memory) ? ((!IsHexadecimal(memory)) ? (memory + ((tg.DataType != 0) ? (Convert.ToInt32(value) + num * 16) : (Convert.ToInt32(value) + num))) : (memory + ((tg.DataType != 0) ? (Convert.ToInt32(value, 16) + num * 16) : (Convert.ToInt32(value, 16) + num)).ToString("X"))) : ((!IsHexadecimal(memory)) ? (memory + (Convert.ToInt32(value) + num)) : (memory + (Convert.ToInt32(value, 16) + num).ToString("X")))));
		SetTagName(tg);
		return text2.ToUpper();
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
		string text = ((!IsHexadecimal(tg.Address.Substring(0, 1))) ? string.Join("", from char_0 in tg.Address
			where char.IsLetter(char_0)
			select (char_0)) : tg.Address.Substring(0, 1));
		if (!DeviceCodes.ContainsKey(text))
		{
			validateResult.Message = text + ": This data type is not supported.";
		}
		if (string.IsNullOrEmpty(validateResult.Message))
		{
			string text2 = tg.Address.Substring(text.Length, tg.Address.Length - text.Length);
			bool num = tg.Address == tg.Name;
			if (IsHexadecimal(tg.Address.Substring(0, 1)))
			{
				tg.Address = text + Convert.ToInt32(text2, 16).ToString("X");
			}
			else
			{
				tg.Address = text + int.Parse(text2);
			}
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

	public static string CheckErrorCodes(int errorCode)
	{
		string result = string.Empty;
		if (errorCode >= 16384 && errorCode <= 20479)
		{
			result = "Errors detected by the CPU module(Errors occurred in other than MC protocol communication).";
		}
		else if (errorCode >= 49233 && errorCode <= 49236)
		{
			result = "The number of read or write points is outside the allowable range.";
		}
		else
		{
			switch (errorCode)
			{
			case 49232:
				result = "When \"Communication Data Code\" is set to ASCII Code, ASCII code data that cannot be converted to binary were received.";
				break;
			case 49238:
				result = "The read or write request exceeds the maximum address.";
				break;
			case 49240:
				result = "The request data length after ASCII-to-binary conversion does not match the data size of the character area (a part of text data).";
				break;
			case 49241:
				result = "The command and/or subcommand are specified incorrectly.";
				break;
			case 49243:
				result = "The CPU module cannot read data from or write data to the specified device.";
				break;
			case 49244:
				result = "The request data is incorrect. (e.g. reading or writing data in units of bits from or to a word device).";
				break;
			case 49245:
				result = "No monitor registration";
				break;
			case 49247:
				result = "The request cannot be executed to the CPU module.";
				break;
			case 49248:
				result = "The request data is incorrect. (ex. incorrect specification of data for bit devices)";
				break;
			case 49249:
				result = "The request data length does not match the number of data in the character area (a part of text data).";
				break;
			case 85:
				result = "Although online change is disabled, the connected device requested the RUN-state CPU module for data writing.";
				break;
			case 80:
				result = "A code other than specified ones is set to command/response type of subheader.";
				break;
			case 49264:
				result = "The device memory extension cannot be specified for the target station.";
				break;
			case 49263:
				result = "The CPU module received a request message in ASCII format The CPU module received a request message in ASCII format received it in binary format when the setting is set to ASCII Code.";
				break;
			case 49664:
				result = "The remote password is incorrect.";
				break;
			case 49665:
				result = "The port used for communication is locked with the remote password. Or, because of the remote password lock status with \"Communication Data Code\" set to ASCII Code, the subcommand and later part cannot be converted to a binary code.";
				break;
			case 49668:
				result = "The connected device is different from the one that requested for unlock processing of the remote password.";
				break;
			case 49333:
				result = "The CPU module cannot handle the data specified.";
				break;
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

	public static BOOL[] BytesToBools(byte[] bytes)
	{
		BOOL[] array = new BOOL[2 * bytes.Length];
		int i = 0;
		int num = 0;
		for (; i < bytes.Length; i++)
		{
			string text = bytes[i].ToString("X2");
			array[num] = text[0] == '1';
			if (num < array.Length - 1)
			{
				array[++num] = text[1] == '1';
			}
			num++;
		}
		return array;
	}

	public static byte[] BoolsToBytes(BOOL[] values)
	{
		if (values.Length % 2 != 0)
		{
			Array.Resize(ref values, values.Length + 1);
		}
		byte[] array = new byte[values.Length / 2];
		int num = 0;
		for (int i = 0; i < values.Length; i += 2)
		{
			array[num] = Convert.ToByte($"{Convert.ToByte(values[i])}{Convert.ToByte(values[i + 1])}", 16);
			num++;
		}
		return array;
	}

	public static byte[] BoolsToBytes(bool[] values)
	{
		if (values.Length % 2 != 0)
		{
			Array.Resize(ref values, values.Length + 1);
		}
		byte[] array = new byte[values.Length / 2];
		int num = 0;
		for (int i = 0; i < values.Length; i += 2)
		{
			array[num] = Convert.ToByte($"{Convert.ToByte(values[i])}{Convert.ToByte(values[i + 1])}", 16);
			num++;
		}
		return array;
	}
}
