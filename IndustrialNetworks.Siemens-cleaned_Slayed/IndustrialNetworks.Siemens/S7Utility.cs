using System;
using System.IO;
using System.Linq;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.Siemens.Models;

namespace NetStudio.Siemens;

public class S7Utility
{
	public static void IncrementAddress(Tag tg)
	{
		decimal offsetAddress = GetOffsetAddress(tg.Address);
		string value = tg.Address.Substring(0, tg.Address.Length - offsetAddress.ToString().Length);
		int num = GetByteAddress(tg.Address);
		if (tg.DataType == DataType.BOOL)
		{
			int indexOfBit = GetIndexOfBit(tg.Address);
			indexOfBit++;
			if (indexOfBit > 7)
			{
				indexOfBit = 0;
				num++;
			}
			tg.Address = $"{value}{num}.{indexOfBit}";
		}
		else
		{
			switch (tg.DataType)
			{
			default:
				num++;
				break;
			case DataType.BYTE:
				num++;
				break;
			case DataType.LINT:
			case DataType.ULINT:
			case DataType.LWORD:
			case DataType.LREAL:
				num += 8;
				break;
			case DataType.INT:
			case DataType.UINT:
			case DataType.WORD:
			case DataType.TIME16:
				num += 2;
				break;
			case DataType.DINT:
			case DataType.UDINT:
			case DataType.DWORD:
			case DataType.REAL:
			case DataType.TIME32:
				num += 4;
				break;
			case DataType.STRING:
				if (tg.Resolution <= 0)
				{
					tg.Resolution = 254;
				}
				else if (tg.Resolution > 254)
				{
					throw new InvalidDataException("Invalid string length. The maximum string length is 254 characters.");
				}
				num += tg.Resolution + 2;
				break;
			}
			tg.Address = $"{value}{num}";
		}
		tg.Name = tg.Address;
	}

	public static ValidateResult TagValidate(Tag tg)
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Valid
		};
		tg.Address = tg.Address.ToUpper();
		switch (tg.DataType)
		{
		default:
			validateResult.Status = ValidateStatus.Invalid;
			validateResult.Message = "Data type and Address are not match.";
			break;
		case DataType.BOOL:
			if (tg.Address.StartsWith("DB"))
			{
				if (!tg.Address.Contains(".DBX"))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
			}
			else if (!tg.Address.StartsWith("M") && !tg.Address.StartsWith("I") && !tg.Address.StartsWith("Q"))
			{
				validateResult.Status = ValidateStatus.Invalid;
				validateResult.Message = "Data type and Address are not match.";
			}
			else
			{
				char[] chars = tg.Address.Substring(1).ToCharArray();
				if (HasLetter(validateResult, chars))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
				}
			}
			break;
		case DataType.BYTE:
			if (tg.Address.StartsWith("DB"))
			{
				if (!tg.Address.Contains(".DBB"))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
			}
			else if (!tg.Address.StartsWith("MB") && !tg.Address.StartsWith("IB") && !tg.Address.StartsWith("QB") && !tg.Address.StartsWith("VB"))
			{
				validateResult.Status = ValidateStatus.Invalid;
				validateResult.Message = "Data type and Address are not match.";
			}
			else
			{
				char[] chars3 = tg.Address.Substring(2).ToCharArray();
				if (HasLetter(validateResult, chars3))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
				}
			}
			break;
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
			if (tg.Address.StartsWith("DB"))
			{
				if (!tg.Address.Contains(".DBL"))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
			}
			else if (!tg.Address.StartsWith("ML") && !tg.Address.StartsWith("IL") && !tg.Address.StartsWith("QL") && !tg.Address.StartsWith("VL"))
			{
				validateResult.Status = ValidateStatus.Invalid;
				validateResult.Message = "Data type and Address are not match.";
			}
			else
			{
				char[] chars4 = tg.Address.Substring(2).ToCharArray();
				if (HasLetter(validateResult, chars4))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
				}
			}
			break;
		case DataType.INT:
		case DataType.UINT:
		case DataType.WORD:
		case DataType.TIME16:
			if (tg.Address.StartsWith("DB"))
			{
				if (!tg.Address.Contains(".DBW"))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
			}
			else if (!tg.Address.StartsWith("MW") && !tg.Address.StartsWith("IW") && !tg.Address.StartsWith("QW") && !tg.Address.StartsWith("VW"))
			{
				validateResult.Status = ValidateStatus.Invalid;
				validateResult.Message = "Data type and Address are not match.";
			}
			else
			{
				char[] chars2 = tg.Address.Substring(2).ToCharArray();
				if (HasLetter(validateResult, chars2))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
				}
			}
			break;
		case DataType.DINT:
		case DataType.UDINT:
		case DataType.DWORD:
		case DataType.REAL:
		case DataType.TIME32:
			if (tg.Address.StartsWith("DB"))
			{
				if (!tg.Address.Contains(".DBD"))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
					return validateResult;
				}
			}
			else if (!tg.Address.StartsWith("MD") && !tg.Address.StartsWith("ID") && !tg.Address.StartsWith("QD") && !tg.Address.StartsWith("VD"))
			{
				validateResult.Status = ValidateStatus.Invalid;
				validateResult.Message = "Data type and Address are not match.";
			}
			else
			{
				char[] chars5 = tg.Address.Substring(2).ToCharArray();
				if (HasLetter(validateResult, chars5))
				{
					validateResult.Status = ValidateStatus.Invalid;
					validateResult.Message = "Data type and Address are not match.";
				}
			}
			break;
		case DataType.STRING:
			if (tg.Resolution <= 0)
			{
				tg.Resolution = 254;
			}
			else if (tg.Resolution > 254)
			{
				throw new InvalidDataException("Invalid string length. The maximum string length is 254 characters.");
			}
			if (string.IsNullOrEmpty(tg.Description))
			{
				tg.Description = $"STRING[{tg.Resolution}]";
			}
			break;
		}
		return validateResult;
	}

	private static bool HasLetter(ValidateResult validate, char[] chars)
	{
		bool result = false;
		for (int i = 0; i < chars.Length; i++)
		{
			if (char.IsLetter(chars[i]))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static string GetAddressPrefix(string address)
	{
		char[] array = address.ToCharArray();
		int num = array.Length;
		string text = string.Empty;
		do
		{
			num--;
			if (char.IsLetter(array[num]))
			{
				break;
			}
			ReadOnlySpan<char> readOnlySpan = text;
			char reference = array[num];
			text = string.Concat(readOnlySpan, new ReadOnlySpan<char>(ref reference));
		}
		while (num > 0);
		string text2 = address.Substring(0, address.Length - text.Length);
		if (text2.Contains('.'))
		{
			text2 = text2.Split('.')[0];
		}
		else if (!text2.StartsWith('M') && !text2.StartsWith('I') && !text2.StartsWith('Q'))
		{
			if (text2.StartsWith('V'))
			{
				text2 = "DB1";
			}
		}
		else
		{
			text2 = text2.Substring(0, 1);
		}
		return text2;
	}

	public static decimal GetOffsetAddress(string address)
	{
		char[] array = address.ToCharArray();
		int num = array.Length;
		string text = string.Empty;
		do
		{
			num--;
			if (char.IsLetter(array[num]))
			{
				break;
			}
			ReadOnlySpan<char> readOnlySpan = text;
			char reference = array[num];
			text = string.Concat(readOnlySpan, new ReadOnlySpan<char>(ref reference));
		}
		while (num > 0);
		char[] array2 = text.ToArray();
		Array.Reverse(array2);
		text = new string(array2);
		return decimal.Parse(text);
	}

	public static int GetByteAddress(string address)
	{
		return GetByteAddress(GetOffsetAddress(address));
	}

	public static int GetBitAddress(string address)
	{
		return GetBitAddress(GetOffsetAddress(address));
	}

	public static int GetIndexOfBit(string address)
	{
		decimal offsetAddress = GetOffsetAddress(address);
		return (int)((offsetAddress - Math.Truncate(offsetAddress)) * 10m);
	}

	public static int GetSortAddress(Tag tg)
	{
		int sizeOfDataType = GetSizeOfDataType(tg);
		int bitAddress = GetBitAddress(tg.Address);
		return 8 * sizeOfDataType + bitAddress;
	}

	public static int GetByteAddress(decimal offsetAddress)
	{
		decimal num = offsetAddress - Math.Truncate(offsetAddress);
		return (int)(offsetAddress - num);
	}

	public static int GetBitAddress(decimal offsetAddress)
	{
		decimal num = offsetAddress - Math.Truncate(offsetAddress);
		ushort num2 = (ushort)(offsetAddress - num);
		ushort num3 = (ushort)(num * 10m);
		return 8 * num2 + num3;
	}

	public static Memory GetMemoryByAddress(string address)
	{
        if (address.StartsWith('M'))
		{
			return Memory.Flag;
		}
		if (address.StartsWith('I'))
		{
			return Memory.Input;
		}
		if (address.StartsWith('Q'))
		{
			return Memory.Output;
		}
		if (!address.StartsWith("DB") && !address.StartsWith('V'))
		{
			throw new NotSupportedException(address + ": Data type and Address are not match..");
		}
		return Memory.Datablock;
	}

	public static string GetMemory(string address)
	{
        if (address.StartsWith("DB"))
		{
			int num = address.IndexOf('.');
			return address.Substring(0, num + 4);
		}
		if (char.IsLetter(address[1]))
		{
			return address.Substring(0, 2);
		}
		return address.Substring(0, 1);
	}

	public static decimal GetOffset(string address)
	{
		string memory = GetMemory(address);
		return decimal.Parse(address.Substring(memory.Length));
	}

	public static ushort GetSizeOfDataType(Tag tg)
	{
        switch (tg.DataType)
		{
		default:
			return 1;
		case DataType.BOOL:
			return 1;
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
		case DataType.STRING:
			if (tg.Resolution <= 0)
			{
				tg.Resolution = 254;
			}
			return tg.Resolution;
		}
	}

	public static byte[] Sort(byte[] values)
	{
		int num = values.Length;
		return num switch
		{
			8 => new byte[8]
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
			4 => new byte[4]
			{
				values[3],
				values[2],
				values[1],
				values[0]
			}, 
			2 => new byte[2]
			{
				values[1],
				values[0]
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
}
