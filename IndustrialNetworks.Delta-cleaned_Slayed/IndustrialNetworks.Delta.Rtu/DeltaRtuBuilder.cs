using System;
using NetStudio.Common.Manager;
using NetStudio.Delta.Models;

namespace NetStudio.Delta.Rtu;

public class DeltaRtuBuilder : BaseBuilder
{
	public byte[] ReadMessage(byte stationNo, int address, byte func, int quantity)
	{
		byte[] array = new byte[8]
		{
			stationNo,
			func,
			(byte)(address >> 8),
			(byte)address,
			(byte)(quantity >> 8),
			(byte)quantity,
			0,
			0
		};
		byte[] array2 = CRC(array);
		array[6] = array2[0];
		array[7] = array2[1];
		return array;
	}

	public byte[] WriteMessage(byte stationNo, int address, byte func, byte[] values)
	{
		byte[] array = new byte[6 + values.Length];
		array[0] = stationNo;
		array[1] = func;
		array[2] = (byte)(address >> 8);
		array[3] = (byte)address;
		for (int i = 0; i < values.Length; i++)
		{
			array[i + 4] = values[i];
		}
		byte[] array2 = CRC(array);
		array[^2] = array2[0];
		array[^1] = array2[1];
		return array;
	}

	public byte[] WriteMessage(byte stationNo, int address, byte func, int quantity, byte[] values)
	{
		int num = values.Length;
		byte[] array = new byte[9 + num];
		array[0] = stationNo;
		array[1] = func;
		array[2] = (byte)(address >> 8);
		array[3] = (byte)address;
		array[4] = (byte)(quantity >> 8);
		array[5] = (byte)quantity;
		array[6] = (byte)num;
		for (int i = 0; i < num; i++)
		{
			array[i + 7] = values[i];
		}
		byte[] array2 = CRC(array);
		array[^2] = array2[0];
		array[^1] = array2[1];
		return array;
	}

	public byte[] CRC(byte[] data)
	{
		 
		int num = 65535;
		byte[] array = new byte[2];
		for (int i = 0; i < data.Length - 2; i++)
		{
			num ^= data[i];
			for (int j = 0; j < 8; j++)
			{
				ushort num2 = (ushort)(num & 1);
				num = (num >> 1) & 0x7FFF;
				if (num2 == 1)
				{
					num ^= 0xA001;
				}
			}
		}
		array[1] = (byte)((uint)(num >> 8) & 0xFFu);
		array[0] = (byte)((uint)num & 0xFFu);
		return array;
	}
}
