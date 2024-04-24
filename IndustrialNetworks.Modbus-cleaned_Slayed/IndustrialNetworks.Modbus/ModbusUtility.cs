using System;
using System.Collections.Generic;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;

namespace NetStudio.Modbus;

public static class ModbusUtility
{
	public static void IncrementAddress(Tag tg)
	{
		IpsAddress ipsAddress = new IpsAddress();
		if (tg.Address.Contains("."))
		{
			string[] array = tg.Address.Split('.');
			ipsAddress.WordAddress = int.Parse(array[0]);
			ipsAddress.BitAddress = int.Parse(array[1]);
		}
		else
		{
			ipsAddress.WordAddress = int.Parse(tg.Address);
		}
		int num = 0;
		switch (tg.DataType)
		{
		case DataType.BOOL:
			num++;
			break;
		default:
			throw new NotSupportedException("This data type is not supported.");
		case DataType.LINT:
		case DataType.ULINT:
		case DataType.LWORD:
		case DataType.LREAL:
			num += 4;
			break;
		case DataType.INT:
		case DataType.UINT:
		case DataType.WORD:
		case DataType.TIME16:
			num++;
			break;
		case DataType.DINT:
		case DataType.UDINT:
		case DataType.DWORD:
		case DataType.REAL:
		case DataType.TIME32:
			num += 2;
			break;
		case DataType.STRING:
			num += 16;
			break;
		}
		if (tg.Address.Contains("."))
		{
			ipsAddress.BitAddress++;
			if (ipsAddress.BitAddress > 15)
			{
				ipsAddress.BitAddress = 0;
				ipsAddress.WordAddress++;
			}
			tg.Address = $"{ipsAddress.WordAddress}.{ipsAddress.BitAddress}";
		}
		else
		{
			ipsAddress.WordAddress += num;
			tg.Address = $"{ipsAddress.WordAddress}";
		}
		tg.Name = tg.Address;
	}

	public static ValidateResult TagValidate(Tag tg)
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid
		};
		if (tg.Address.Contains("."))
		{
			if (tg.DataType != 0)
			{
				validateResult.Message = "This address type is not supported.";
			}
			else
			{
				tg.Mode = TagMode.ReadOnly;
				if (int.Parse(tg.Address.Split('.')[1]) > 15)
				{
					validateResult.Message = "This address type is not supported.";
				}
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

	public static IpsAddress GetAddress(decimal address, int baseAddress, bool isBit = false)
	{
		IpsAddress ipsAddress = new IpsAddress();
		if ($"{address}".Contains("."))
		{
			string[] array = $"{address}".Split('.');
			ipsAddress.WordAddress = int.Parse(array[0]);
			ipsAddress.BitAddress = int.Parse(array[1]);
		}
		else
		{
			ipsAddress.WordAddress = int.Parse($"{address}");
		}
		if (ipsAddress.WordAddress >= 1 && ipsAddress.WordAddress <= 9999)
		{
			ipsAddress.WordAddress -= baseAddress;
			return ipsAddress;
		}
		if (ipsAddress.WordAddress >= 10001 && ipsAddress.WordAddress <= 19999)
		{
			ipsAddress.WordAddress -= 10000 + baseAddress;
			return ipsAddress;
		}
		if (ipsAddress.WordAddress >= 100001 && ipsAddress.WordAddress <= 199999)
		{
			ipsAddress.WordAddress -= 100000 + baseAddress;
			return ipsAddress;
		}
		if (ipsAddress.WordAddress >= 30001 && ipsAddress.WordAddress <= 39999)
		{
			ipsAddress.WordAddress -= 30000 + baseAddress;
			return ipsAddress;
		}
		if (ipsAddress.WordAddress >= 300001 && ipsAddress.WordAddress <= 399999)
		{
			ipsAddress.WordAddress -= 300000 + baseAddress;
			return ipsAddress;
		}
		if (isBit)
		{
			ipsAddress.WordAddress -= baseAddress;
			return ipsAddress;
		}
		if (ipsAddress.WordAddress >= 40001 && ipsAddress.WordAddress <= 49999)
		{
			ipsAddress.WordAddress -= 40000 + baseAddress;
			return ipsAddress;
		}
		if (ipsAddress.WordAddress < 400001 || ipsAddress.WordAddress > 465535)
		{
			throw new Exception($"Invalid modbus address: {address}");
		}
		ipsAddress.WordAddress -= 400000 + baseAddress;
		return ipsAddress;
	}

	public static List<ReadPacket> CreatePackage<T>(byte slaveId, ushort startAddress, ushort quantity, int packageSize = 120) where T : struct
	{
		Type typeFromHandle = typeof(T);
		if (!(typeFromHandle == typeof(INT)) && !(typeFromHandle == typeof(UINT)) && !(typeFromHandle == typeof(WORD)))
		{
			if (!(typeFromHandle == typeof(DINT)) && !(typeFromHandle == typeof(UDINT)) && !(typeFromHandle == typeof(DWORD)) && !(typeFromHandle == typeof(REAL)))
			{
				if (!(typeFromHandle == typeof(LINT)) && !(typeFromHandle == typeof(ULINT)) && !(typeFromHandle == typeof(LWORD)) && !(typeFromHandle == typeof(LREAL)))
				{
					throw new NotSupportedException();
				}
				packageSize /= 4;
			}
			else
			{
				packageSize /= 2;
			}
		}
		else
		{
			packageSize = packageSize;
		}
		List<ReadPacket> list = new List<ReadPacket>();
		int num = quantity / packageSize;
		for (int i = 0; i < num; i++)
		{
			ushort address = (ushort)(startAddress + i * packageSize);
			ReadPacket item = new ReadPacket
			{
				StationNo = slaveId,
				Address = address,
				Quantity = (ushort)packageSize
			};
			list.Add(item);
		}
		int num2 = quantity % packageSize;
		if (num2 > 0)
		{
			ReadPacket item2 = new ReadPacket
			{
				StationNo = slaveId,
				Address = (ushort)(startAddress + num * packageSize),
				Quantity = (ushort)num2
			};
			list.Add(item2);
		}
		return list;
	}
}
