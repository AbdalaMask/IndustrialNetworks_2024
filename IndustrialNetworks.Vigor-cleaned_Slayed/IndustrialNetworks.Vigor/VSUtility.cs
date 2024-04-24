using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.Vigor.Enums;

namespace NetStudio.Vigor;

public static class VSUtility
{
	public static readonly Dictionary<string, DeviceCode> DeviceCodes = new Dictionary<string, DeviceCode>
	{
		{
			"X",
			DeviceCode.ExternalInputX
		},
		{
			"Y",
			DeviceCode.ExternalOutputY
		},
		{
			"M",
			DeviceCode.AuxiliaryRelayM
		},
		{
			"S",
			DeviceCode.StepRelayS
		},
		{
			"SM",
			DeviceCode.SpecialRelayM
		},
		{
			"T_COIL",
			DeviceCode.CoilOfATimerT
		},
		{
			"T_CONTACT",
			DeviceCode.ContactOfATimerT
		},
		{
			"T",
			DeviceCode.TimerT
		},
		{
			"C_COIL",
			DeviceCode.CoilOfACounterC
		},
		{
			"C_CONTACT",
			DeviceCode.ContactOfACounterC
		},
		{
			"C16",
			DeviceCode.Counter16Bit
		},
		{
			"C32",
			DeviceCode.Counter32Bit
		},
		{
			"D",
			DeviceCode.RegisterD
		},
		{
			"SD",
			DeviceCode.SpecialRegisterSD
		},
		{
			"R",
			DeviceCode.RegisterR
		}
	};

	public static bool IsOctal(string memory)
	{
		if (!(memory == "X"))
		{
			return memory == "Y";
		}
		return true;
	}

	public static string GetMemory(string address)
	{
		return address.Substring(0, 1).ToUpper();
	}

	public static int GetWordAddress(string address)
	{
		string memory = GetMemory(address);
		string text = address.Substring(memory.Length, address.Length - memory.Length);
		string s = ((!text.Contains(".")) ? text : text.Split('.')[0]);
		if (IsOctal(memory))
		{
			return Convert.ToInt32(int.Parse(s));
		}
		return int.Parse(s);
	}

	public static int GetBitAddress(string address)
	{
		string memory = GetMemory(address);
		string text = address.Substring(memory.Length);
		string text2 = text ?? "";
		if (text.Contains("."))
		{
			text2 = text.Split('.')[1];
		}
		int num = int.Parse(text2);
		if (IsOctal(memory))
		{
			return Convert.ToInt32(text2, 8);
		}
		return int.Parse(text2);
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

	public static int GetIndexOfWordAddress(Tag startTag, Tag endTag)
	{
		return 2 * (endTag.WordAddress - startTag.WordAddress);
	}

	public static int GetAddress(int wordAddress)
	{
		return int.Parse(wordAddress.ToString(), NumberStyles.HexNumber);
	}

	public static int GetAddress(string address)
	{
		string memory = GetMemory(address);
		string text = address.Substring(memory.Length, address.Length - memory.Length).PadLeft(6, '0');
		return int.Parse(text.Substring(4, 2) + text.Substring(2, 2) + text.Substring(0, 2), NumberStyles.HexNumber);
	}

	public static string GetHexAddress(string address, bool isOctal = false)
	{
		string memory = GetMemory(address);
		string text = address.Substring(memory.Length, address.Length - memory.Length);
		if (isOctal)
		{
			text = Convert.ToString(int.Parse(text), 8);
		}
		string text2 = text.PadLeft(6, '0');
		return text2.Substring(4, 2) + text2.Substring(2, 2) + text2.Substring(0, 2);
	}

	public static string ByteSwap(string IP)
	{
		return IP.Substring(2, 2) + IP.Substring(0, 2);
	}

	public static void IncrementWordAddress(Tag tg)
	{
		string memory = GetMemory(tg.Address);
		string text = tg.Address.Substring(memory.Length);
		if (tg.DataType == DataType.BOOL)
		{
			if (text.Contains("."))
			{
				string[] array = text.Split('.');
				int num = int.Parse(array[0]);
				int num2 = int.Parse(array[1]) + 1;
				if (num2 > 15)
				{
					num2 = 0;
					num++;
				}
				tg.Address = $"{memory}{num}.{num2.ToString("X1")}";
			}
			else
			{
				int value = ((!IsOctal(memory)) ? (int.Parse(text) + 1) : Conversion.IncrementOctal(int.Parse(text)));
				tg.Address = $"{memory}{value}";
			}
			SetTagName(tg);
		}
		else if (!(memory == "C") && !(memory == "T"))
		{
            int num3;
            switch (tg.DataType)
			{
			default:
				throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
			case DataType.INT:
			case DataType.UINT:
			case DataType.WORD:
			case DataType.TIME16:
				num3 = 1;
				break;
			case DataType.DINT:
			case DataType.UDINT:
			case DataType.DWORD:
			case DataType.REAL:
			case DataType.TIME32:
				num3 = 2;
				break;
			}
			int value2 = int.Parse(text) + num3;
			tg.Address = $"{memory}{value2}";
			SetTagName(tg);
		}
		else
		{
			int value3 = int.Parse(text) + 1;
			tg.Address = $"{memory}{value3}";
			SetTagName(tg);
		}
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
			return 16;
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
			if (tg.Address.StartsWith("C"))
			{
				if (GetDeviceCodeByCounter(tg.Address) == DeviceCode.Counter16Bit)
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
			text = ((tg.DataType != 0) ? ((num < 200) ? "C16" : "C32") : "C_COIL");
			break;
		case "T":
			text = ((tg.DataType != 0) ? "T" : "C_COIL");
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

	public static DeviceCode GetDeviceCodeByCounter(string deviceId)
	{
        if (!deviceId.StartsWith("C"))
		{
			throw new InvalidDataException(deviceId + ": This address is not the address of the counter.");
		}
		int wordAddress = GetWordAddress(deviceId);
		if (wordAddress >= 0 && wordAddress <= 199)
		{
			return DeviceCode.Counter16Bit;
		}
		if (wordAddress >= 200 && wordAddress <= 255)
		{
			return DeviceCode.Counter32Bit;
		}
		throw new InvalidDataException(deviceId + ": This address is outside the address range of the counter.");
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
