using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;

namespace NetStudio.LS;

public static class XgtUtility
{
	public static readonly Dictionary<string, string> Memories = new Dictionary<string, string>
	{
		{ "P", "P" },
		{ "M", "M" },
		{ "L", "L" },
		{ "K", "K" },
		{ "C", "C" },
		{ "T", "T" },
		{ "F", "F" },
		{ "I", "I" },
		{ "Q", "Q" },
		{ "U", "U" },
		{ "D", "D" },
		{ "S", "S" },
		{ "W", "W" },
		{ "R", "R" }
	};

	public static bool IsBitMemory(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "UL";
		case "PX":
		case "PB":
		case "PW":
		case "PD":
		case "PL":
		case "MX":
		case "MB":
		case "MW":
		case "MD":
		case "ML":
		case "LX":
		case "LB":
		case "LW":
		case "LD":
		case "LL":
		case "KX":
		case "KB":
		case "KW":
		case "KD":
		case "KL":
		case "CX":
		case "CB":
		case "CW":
		case "CD":
		case "CL":
		case "TX":
		case "TB":
		case "TW":
		case "TD":
		case "TL":
		case "FX":
		case "FB":
		case "FW":
		case "FD":
		case "FL":
		case "IX":
		case "IB":
		case "IW":
		case "ID":
		case "IL":
		case "QX":
		case "QB":
		case "QW":
		case "QD":
		case "QL":
		case "UX":
		case "UB":
		case "UW":
		case "UD":
			return true;
		}
	}

	public static bool IsWordMemory(string memory)
	{
		switch (memory)
		{
		default:
			return memory == "RL";
		case "DW":
		case "DD":
		case "DL":
		case "WW":
		case "WD":
		case "WL":
		case "RW":
		case "RD":
			return true;
		}
	}

	public static string GetMemory(Tag tg)
	{
		return tg.Address.Substring(0, 1);
	}

	public static decimal GetOffset(Tag tg)
	{
		string memory = GetMemory(tg);
		return decimal.Parse(tg.Address.Substring(memory.Length));
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
			return 16;
		}
	}

	public static ushort GetIECSizeOfDataType(Tag tg)
	{
        ushort num;
        switch (tg.DataType)
		{
		default:
			num = 1;
			break;
		case DataType.BOOL:
			num = 1;
			if (tg.Address.ToUpper()[2] != 'X')
			{
				throw new Exception("Data type and Address are not match.");
			}
			break;
		case DataType.BYTE:
			num = 1;
			if (tg.Address.ToUpper()[2] != 'B')
			{
				throw new Exception("Data type and Address are not match.");
			}
			break;
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
			num = 4;
			if (tg.Address.ToUpper()[2] != 'L')
			{
				throw new Exception("Data type and Address are not match.");
			}
			break;
		case DataType.INT:
		case DataType.UINT:
		case DataType.WORD:
		case DataType.TIME16:
			num = 1;
			if (tg.Address.ToUpper()[2] != 'W')
			{
				throw new Exception("Data type and Address are not match.");
			}
			break;
		case DataType.DINT:
		case DataType.UDINT:
		case DataType.DWORD:
		case DataType.REAL:
		case DataType.TIME32:
			num = 2;
			if (tg.Address.ToUpper()[2] != 'D')
			{
				throw new Exception("Data type and Address are not match.");
			}
			break;
		case DataType.STRING:
			num = 16;
			break;
		}
		return num;
	}

	public static ValidateResult TagValidate(Device device_0, Tag tg)
	{
		if (device_0.DeviceType == 2)
		{
			return IECTagValidate(tg);
		}
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid
		};
		tg.Address.ToUpper();
		if (Memories.ContainsKey(tg.Address.Substring(0, 1)) && !char.IsLetter(tg.Address[1]))
		{
			if (tg.DataType == DataType.BOOL)
			{
				if (tg.Address.Length < 3)
				{
					tg.Address = $"{tg.Address[0]}{tg.Address.Substring(1, tg.Address.Length - 1).PadLeft(2, '0')}";
				}
			}
			else if (tg.DataType == DataType.STRING)
			{
				tg.Resolution = 32;
			}
			validateResult.Status = ValidateStatus.Valid;
			try
			{
				GetSizeOfDataType(tg);
			}
			catch (Exception ex)
			{
				validateResult.Status = ValidateStatus.Invalid;
				validateResult.Message = ex.Message;
			}
			return validateResult;
		}
		validateResult.Status = ValidateStatus.Invalid;
		validateResult.Message = "This address type is not supported.\n\rExamples of valid addresses: P00, P20, I02, Q01, D0, M000B, M100C, F0, L0, K0,...for all data types.";
		return validateResult;
	}

	public static ValidateResult IECTagValidate(Tag tg)
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid
		};
		tg.Address.ToUpper();
		if (tg.Address.StartsWith("%") && Memories.ContainsKey(tg.Address.Substring(1, 1)) && !char.IsNumber(tg.Address[2]) && !char.IsLetter(tg.Address[3]))
		{
			if (tg.DataType == DataType.STRING)
			{
				tg.Resolution = 32;
			}
			validateResult.Status = ValidateStatus.Valid;
			try
			{
				GetIECSizeOfDataType(tg);
			}
			catch (Exception ex)
			{
				validateResult.Status = ValidateStatus.Invalid;
				validateResult.Message = ex.Message;
			}
			return validateResult;
		}
		validateResult.Status = ValidateStatus.Invalid;
		validateResult.Message = "This address type is not supported.\n\rExamples of valid addresses: %MX0, %MB50, %MW4, %MD4, %MD56, %ML79, %IX0.0.0, %QX0.0.1, %QW0.0.0,...";
		return validateResult;
	}

	public static void IncrementWordAddress(Device device_0, Tag tg)
	{
		if (device_0.DeviceType == 2)
		{
			IncrementIECWordAddress(tg);
			return;
		}
		string text = tg.Address.Substring(0, 1);
		string text2 = tg.Address.Substring(text.Length, tg.Address.Length - text.Length);
		if (tg.DataType == DataType.BOOL)
		{
			if (tg.Address[0] != 'P' && tg.Address[0] != 'M' && tg.Address[0] != 'L' && tg.Address[0] != 'K' && tg.Address[0] != 'F')
			{
				ushort num = ushort.Parse(text2);
				tg.Address = text + $"{++num}";
			}
			else
			{
				ushort num2 = ushort.Parse(text2.Substring(0, text2.Length - 1));
				ushort num3 = Convert.ToByte(text2.Last().ToString(), 16);
				num3++;
				if (num3 >= 16)
				{
					num2++;
					num3 = 0;
				}
				tg.Address = text + $"{num2}{num3.ToString("X")}";
			}
		}
		else
		{
            ushort num4;
            switch (tg.DataType)
			{
			case DataType.BOOL:
				tg.Address = IncrementBitAddress(tg.Address);
				SetTagName(tg);
				return;
			default:
				throw new NotSupportedException();
			case DataType.LINT:
			case DataType.ULINT:
			case DataType.LWORD:
			case DataType.LREAL:
				num4 = 4;
				break;
			case DataType.INT:
			case DataType.UINT:
			case DataType.WORD:
			case DataType.TIME16:
				num4 = 1;
				break;
			case DataType.DINT:
			case DataType.UDINT:
			case DataType.DWORD:
			case DataType.REAL:
			case DataType.TIME32:
				num4 = 2;
				break;
			case DataType.STRING:
				num4 = 16;
				break;
			}
			ushort num5 = ushort.Parse(text2);
			tg.Address = text + $"{num5 += num4}";
		}
		SetTagName(tg);
	}

	public static void IncrementIECWordAddress(Tag tg)
	{
        string text = tg.Address.Substring(0, 3);
        string text2 = tg.Address.Substring(text.Length, tg.Address.Length - text.Length);
		string[] array = text2.Split('.');
        int num;
        int num2;
        int num3;
        ushort num4;
        if (tg.DataType == DataType.BOOL)
        {
            if (tg.Address[1] != 'Q' && tg.Address[1] != 'I')
            {
                if (tg.Address[1] == 'U')
                {
                    if (array.Length == 3)
                    {
                        num = int.Parse(array[0]);
                        num2 = int.Parse(array[1]);
                        num3 = int.Parse(array[2]);
                        if (num3 < 511)
                        {
                            num3++;
                        }
                        else
                        {
                            num3 = 0;
                            if (num2 < 10)
                            {
                                num2++;
                            }
                            else
                            {
                                num2 = 0;
                                if (num != 0)
                                {
                                    throw new Exception("Invalid address.");
                                }
                                num++;
                            }
                        }
                        tg.Address = text + $"{num}.{num2}.{num3}";
                    }
                }
                else
                {
                    num4 = ushort.Parse(text2);
                    tg.Address = text + $"{++num4}";
                }
            }
            else if (array.Length == 3)
            {
                num = int.Parse(array[0]);
                num2 = int.Parse(array[1]);
                num3 = int.Parse(array[2]);
                if (num3 < 63)
                {
                    num3++;
                }
                else
                {
                    num3 = 0;
                    if (num2 < 15)
                    {
                        num2++;
                    }
                    else
                    {
                        num2 = 0;
                        if (num != 0)
                        {
                            throw new Exception("Invalid address.");
                        }
                        num++;
                    }
                }
                tg.Address = text + $"{num}.{num2}.{num3}";
            }
        }
        else
        {
            switch (tg.DataType)
            {
                default:
                    throw new NotSupportedException();
                case DataType.BYTE:
                    if (tg.Address[1] != 'Q' && tg.Address[1] != 'I')
                    {
                        if (tg.Address[1] == 'U')
                        {
                            if (array.Length != 3)
                            {
                                break;
                            }
                            num = int.Parse(array[0]);
                            num2 = int.Parse(array[1]);
                            num3 = int.Parse(array[2]);
                            if (num3 < 63)
                            {
                                num3++;
                            }
                            else
                            {
                                num3 = 0;
                                if (num2 < 10)
                                {
                                    num2++;
                                }
                                else
                                {
                                    num2 = 0;
                                    if (num != 0)
                                    {
                                        throw new Exception("Invalid address.");
                                    }
                                    num++;
                                }
                            }
                            tg.Address = text + $"{num}.{num2}.{num3}";
                        }
                        else
                        {
                            num4 = ushort.Parse(text2);
                            tg.Address = text + $"{++num4}";
                        }
                    }
                    else
                    {
                        if (array.Length != 3)
                        {
                            break;
                        }
                        num = int.Parse(array[0]);
                        num2 = int.Parse(array[1]);
                        num3 = int.Parse(array[2]);
                        if (num3 < 7)
                        {
                            num3++;
                        }
                        else
                        {
                            num3 = 0;
                            if (num2 < 15)
                            {
                                num2++;
                            }
                            else
                            {
                                num2 = 0;
                                if (num != 0)
                                {
                                    throw new Exception("Invalid address.");
                                }
                                num++;
                            }
                        }
                        tg.Address = text + $"{num}.{num2}.{num3}";
                    }
                    break;
                case DataType.LINT:
                case DataType.ULINT:
                case DataType.LWORD:
                case DataType.LREAL:
                    if (tg.Address[1] != 'Q' && tg.Address[1] != 'I')
                    {
                        if (tg.Address[1] == 'U')
                        {
                            if (array.Length != 3)
                            {
                                break;
                            }
                            num = int.Parse(array[0]);
                            num2 = int.Parse(array[1]);
                            num3 = int.Parse(array[2]);
                            if (num3 < 7)
                            {
                                num3++;
                            }
                            else
                            {
                                num3 = 0;
                                if (num2 < 10)
                                {
                                    num2++;
                                }
                                else
                                {
                                    num2 = 0;
                                    if (num != 0)
                                    {
                                        throw new Exception("Invalid address.");
                                    }
                                    num++;
                                }
                            }
                            tg.Address = text + $"{num}.{num2}.{num3}";
                        }
                        else
                        {
                            num4 = ushort.Parse(text2);
                            tg.Address = text + $"{++num4}";
                        }
                    }
                    else
                    {
                        if (array.Length != 3)
                        {
                            break;
                        }
                        num = int.Parse(array[0]);
                        num2 = int.Parse(array[1]);
                        num3 = int.Parse(array[2]);
                        num3 = 0;
                        if (num2 < 15)
                        {
                            num2++;
                        }
                        else
                        {
                            num2 = 0;
                            if (num != 0)
                            {
                                throw new Exception("Invalid address.");
                            }
                            num++;
                        }
                        tg.Address = text + $"{num}.{num2}.{num3}";
                    }
                    break;
                case DataType.INT:
                case DataType.UINT:
                case DataType.WORD:
                case DataType.TIME16:
                    if (tg.Address[1] != 'Q' && tg.Address[1] != 'I')
                    {
                        if (tg.Address[1] == 'U')
                        {
                            if (array.Length != 3)
                            {
                                break;
                            }
                            num = int.Parse(array[0]);
                            num2 = int.Parse(array[1]);
                            num3 = int.Parse(array[2]);
                            if (num3 < 31)
                            {
                                num3++;
                            }
                            else
                            {
                                num3 = 0;
                                if (num2 < 10)
                                {
                                    num2++;
                                }
                                else
                                {
                                    num2 = 0;
                                    if (num != 0)
                                    {
                                        throw new Exception("Invalid address.");
                                    }
                                    num++;
                                }
                            }
                            tg.Address = text + $"{num}.{num2}.{num3}";
                        }
                        else
                        {
                            num4 = ushort.Parse(text2);
                            tg.Address = text + $"{++num4}";
                        }
                    }
                    else
                    {
                        if (array.Length != 3)
                        {
                            break;
                        }
                        num = int.Parse(array[0]);
                        num2 = int.Parse(array[1]);
                        num3 = int.Parse(array[2]);
                        if (num3 < 3)
                        {
                            num3++;
                        }
                        else
                        {
                            num3 = 0;
                            if (num2 < 15)
                            {
                                num2++;
                            }
                            else
                            {
                                num2 = 0;
                                if (num != 0)
                                {
                                    throw new Exception("Invalid address.");
                                }
                                num++;
                            }
                        }
                        tg.Address = text + $"{num}.{num2}.{num3}";
                    }
                    break;
                case DataType.DINT:
                case DataType.UDINT:
                case DataType.DWORD:
                case DataType.REAL:
                case DataType.TIME32:
                    if (tg.Address[1] != 'Q' && tg.Address[1] != 'I')
                    {
                        if (tg.Address[1] == 'U')
                        {
                            if (array.Length != 3)
                            {
                                break;
                            }
                            num = int.Parse(array[0]);
                            num2 = int.Parse(array[1]);
                            num3 = int.Parse(array[2]);
                            if (num3 < 15)
                            {
                                num3++;
                            }
                            else
                            {
                                num3 = 0;
                                if (num2 < 10)
                                {
                                    num2++;
                                }
                                else
                                {
                                    num2 = 0;
                                    if (num != 0)
                                    {
                                        throw new Exception("Invalid address.");
                                    }
                                    num++;
                                }
                            }
                            tg.Address = text + $"{num}.{num2}.{num3}";
                        }
                        else
                        {
                            num4 = ushort.Parse(text2);
                            tg.Address = text + $"{++num4}";
                        }
                    }
                    else
                    {
                        if (array.Length != 3)
                        {
                            break;
                        }
                        num = int.Parse(array[0]);
                        num2 = int.Parse(array[1]);
                        num3 = int.Parse(array[2]);
                        if (num3 < 1)
                        {
                            num3++;
                        }
                        else
                        {
                            num3 = 0;
                            if (num2 < 15)
                            {
                                num2++;
                            }
                            else
                            {
                                num2 = 0;
                                if (num != 0)
                                {
                                    throw new Exception("Invalid address.");
                                }
                                num++;
                            }
                        }
                        tg.Address = text + $"{num}.{num2}.{num3}";
                    }
                    break;
                case DataType.STRING:
                    num4 = ushort.Parse(text2);
                    tg.Address = text + $"{num4 += 16}";
                    break;
            }
        }
        SetTagName(tg);
	}

	public static string IncrementBitAddress(string address)
	{
		string text = address.Substring(0, 1);
		string text2 = address.Substring(text.Length, address.Length - text.Length);
		ushort num = ushort.Parse(text2.Substring(0, text2.Length - 1));
		ushort num2 = Convert.ToByte(text2.Last().ToString(), 16);
		num2++;
		if (num2 >= 16)
		{
			num++;
			num2 = 0;
		}
		return text + $"{num}{num2.ToString("X")}";
	}

	public static int GetIndexOfWordAddress(Tag startTag, Tag endTag)
	{
		int num = 1;
		int num2 = 1;
		string text = startTag.Address;
		string text2 = endTag.Address;
		if (startTag.DataType == DataType.BOOL && (text[0] == 'Q' || text[0] == 'P' || text[0] == 'M' || text[0] == 'L' || text[0] == 'K' || text[0] == 'F'))
		{
			num = 2;
			if (text.Length < 3)
			{
				text = text.Insert(1, "0");
			}
		}
		if (endTag.DataType == DataType.BOOL && (text[0] == 'Q' || text2[0] == 'P' || text2[0] == 'M' || text2[0] == 'L' || text2[0] == 'K' || text2[0] == 'F'))
		{
			num2 = 2;
			if (text2.Length < 3)
			{
				text2 = text2.Insert(1, "0");
			}
		}
		int num3 = int.Parse(text.Substring(1, text.Length - num));
		int num4 = int.Parse(text2.Substring(1, text2.Length - num2));
		if (num4 < num3)
		{
			throw new Exception("Invalid LS-PLC address: " + endTag.Address);
		}
		return num4 - num3;
	}

	public static int GetWordAddress(Tag tg)
	{
		if (tg.DataType == DataType.BOOL)
		{
			if (tg.Address[0] == 'I' || tg.Address[0] == 'Q')
			{
				return int.Parse(tg.Address.Substring(1, tg.Address.Length - 1)) / 16;
			}
			if (tg.Address[0] == 'P' || tg.Address[0] == 'M' || tg.Address[0] == 'L' || tg.Address[0] == 'K' || tg.Address[0] == 'F')
			{
				return int.Parse(tg.Address.Substring(1, tg.Address.Length - 2));
			}
		}
		return int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
	}

	public static int GetIECWordAddress(Tag tg)
	{
		if (tg.DataType == DataType.BOOL)
		{
			if (tg.Address[0] == 'I' || tg.Address[0] == 'Q')
			{
				return int.Parse(tg.Address.Substring(1, tg.Address.Length - 1)) / 16;
			}
			if (tg.Address[0] == 'P' || tg.Address[0] == 'M' || tg.Address[0] == 'L' || tg.Address[0] == 'K' || tg.Address[0] == 'F')
			{
				return int.Parse(tg.Address.Substring(1, tg.Address.Length - 2));
			}
		}
		return int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
	}

	public static int GetByteAddress(Tag tg)
	{
		if (tg.DataType == DataType.BOOL)
		{
			if (tg.Address[0] == 'I' || tg.Address[0] == 'Q')
			{
				return int.Parse(tg.Address.Substring(1, tg.Address.Length - 1)) / 8;
			}
			if (tg.Address[0] == 'P' || tg.Address[0] == 'M' || tg.Address[0] == 'L' || tg.Address[0] == 'K' || tg.Address[0] == 'F')
			{
				return int.Parse(tg.Address.Substring(1, tg.Address.Length - 2)) * 2;
			}
		}
		return 2 * int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
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

	public static string GetAddressOfQorI(Tag tg)
	{
		string value = tg.Address.Substring(0, 1);
		int num = int.Parse(tg.Address.Substring(1, tg.Address.Length - 1));
		int num2 = num / 16;
		int value2 = num % 16;
		return $"{value}{num2 / 8}.{num2 % 8}.{value2}";
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

	public static byte[] Sort(byte[] data)
	{
		int num = data.Length;
		return num switch
		{
			8 => new byte[8]
			{
				data[0],
				data[1],
				data[2],
				data[3],
				data[4],
				data[5],
				data[6],
				data[7]
			}, 
			4 => new byte[4]
			{
				data[0],
				data[1],
				data[2],
				data[3]
			}, 
			2 => new byte[2]
			{
				data[0],
				data[1]
			}, 
			_ => throw new InvalidDataException($"Byte order is undefined with the number of bytes={num}. The number of valid bytes is: 2, 4, 8 bytes"), 
		};
	}
}
