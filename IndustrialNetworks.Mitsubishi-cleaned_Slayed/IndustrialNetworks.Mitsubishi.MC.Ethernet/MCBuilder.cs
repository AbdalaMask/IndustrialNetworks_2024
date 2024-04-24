namespace NetStudio.Mitsubishi.MC.Ethernet;

internal sealed class MCBuilder
{
	public byte[] ReadMC1EMsg(ReadPacket RP)
	{
		return new byte[21]
		{
			1,
			255,
			0,
			0,
			(byte)RP.WordAddress,
			(byte)(RP.WordAddress >> 8),
			(byte)(RP.WordAddress >> 16),
			(byte)(RP.WordAddress >> 24),
			32,
			68,
			(byte)RP.Quantity,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};
	}

	public byte[] ReadMC3EMsg(ReadPacket RP)
	{
		return new byte[21]
		{
			80,
			0,
			0,
			255,
			255,
			3,
			RP.StationNo,
			12,
			0,
			0,
			0,
			1,
			4,
			RP.IsBit ? ((byte)1) : ((byte)0),
			0,
			(byte)RP.WordAddress,
			(byte)(RP.WordAddress >> 8),
			(byte)(RP.WordAddress >> 16),
			RP.DeviceCode,
			(byte)RP.Quantity,
			(byte)(RP.Quantity >> 8)
		};
	}

	public byte[] WriteMC3EMsg(WritePacket WP)
	{
		byte[] array = new byte[21 + WP.NumOfBytes];
		array[0] = 80;
		array[1] = 0;
		array[2] = 0;
		array[3] = byte.MaxValue;
		array[4] = byte.MaxValue;
		array[5] = 3;
		array[6] = WP.StationNo;
		int num = array.Length - 9;
		array[7] = (byte)num;
		array[8] = (byte)(num >> 8);
		array[9] = 0;
		array[10] = 0;
		array[11] = 1;
		array[12] = 20;
		array[13] = (WP.IsBit ? ((byte)1) : ((byte)0));
		array[14] = 0;
		array[15] = (byte)WP.WordAddress;
		array[16] = (byte)(WP.WordAddress >> 8);
		array[17] = (byte)(WP.WordAddress >> 16);
		array[18] = WP.DeviceCode;
		array[19] = (byte)WP.Quantity;
		array[20] = (byte)(WP.Quantity >> 8);
		for (int i = 0; i < WP.Data.Length; i++)
		{
			array[21 + i] = WP.Data[i];
		}
		return array;
	}

	public byte[] ReadMC4EMsg(ReadPacket RP)
	{
		return new byte[25]
		{
			84,
			0,
			1,
			0,
			0,
			0,
			0,
			255,
			255,
			3,
			RP.StationNo,
			12,
			0,
			0,
			0,
			1,
			4,
			RP.IsBit ? ((byte)1) : ((byte)0),
			0,
			(byte)RP.WordAddress,
			(byte)(RP.WordAddress >> 8),
			(byte)(RP.WordAddress >> 16),
			RP.DeviceCode,
			(byte)RP.Quantity,
			(byte)(RP.Quantity >> 8)
		};
	}

	public byte[] WriteMC4EMsg(WritePacket WP)
	{
		byte[] array = new byte[25 + WP.NumOfBytes];
		array[0] = 84;
		array[1] = 0;
		array[2] = 1;
		array[3] = 0;
		array[4] = 0;
		array[5] = 0;
		array[6] = 0;
		array[7] = byte.MaxValue;
		array[8] = byte.MaxValue;
		array[9] = 3;
		array[10] = WP.StationNo;
		int num = array.Length - 13;
		array[11] = (byte)num;
		array[12] = (byte)(num >> 8);
		array[13] = 0;
		array[14] = 0;
		array[15] = 1;
		array[16] = 20;
		array[17] = (WP.IsBit ? ((byte)1) : ((byte)0));
		array[18] = 0;
		array[19] = (byte)WP.WordAddress;
		array[20] = (byte)(WP.WordAddress >> 8);
		array[21] = (byte)(WP.WordAddress >> 16);
		array[22] = WP.DeviceCode;
		array[23] = (byte)WP.Quantity;
		array[24] = (byte)(WP.Quantity >> 8);
		for (int i = 0; i < WP.Data.Length; i++)
		{
			array[25 + i] = WP.Data[i];
		}
		return array;
	}
}
