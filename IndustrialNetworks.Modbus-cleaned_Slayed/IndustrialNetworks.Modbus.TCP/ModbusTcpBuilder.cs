using System;
using NetStudio.Common.Manager;

namespace NetStudio.Modbus.TCP;

public class ModbusTcpBuilder : ModbusBuilder
{
	public byte[] ReadMessage(int y, byte slaveAddr, byte func, int address, int quantity)
	{
		 
		return new byte[12]
		{
			(byte)(y >> 8),
			(byte)y,
			0,
			0,
			0,
			6,
			slaveAddr,
			func,
			(byte)(address >> 8),
			(byte)address,
			(byte)(quantity >> 8),
			(byte)quantity
		};
	}

	protected byte[] WriteMessage(int y, byte slaveAddr, int address, byte func, byte[] values)
	{
		 
		int num = values.Length;
		byte[] array = new byte[10 + num];
		array[0] = (byte)(y >> 8);
		array[1] = (byte)y;
		array[5] = (byte)(4 + num);
		array[6] = slaveAddr;
		array[7] = func;
		array[8] = (byte)(address >> 8);
		array[9] = (byte)address;
		for (int i = 0; i < num; i++)
		{
			array[i + 10] = values[i];
		}
		return array;
	}

	protected byte[] WriteMultipleMessage(int y, byte slaveAddr, int address, byte func, int numberOfData, byte[] values)
	{
		 
		int num = values.Length;
		byte[] array = new byte[13 + num];
		array[0] = (byte)(y >> 8);
		array[1] = (byte)y;
		array[5] = (byte)(7 + num);
		array[6] = slaveAddr;
		array[7] = func;
		array[8] = (byte)(address >> 8);
		array[9] = (byte)address;
		array[10] = (byte)(numberOfData >> 8);
		array[11] = (byte)numberOfData;
		array[12] = (byte)num;
		for (int i = 0; i < num; i++)
		{
			array[i + 13] = values[i];
		}
		return array;
	}
}
