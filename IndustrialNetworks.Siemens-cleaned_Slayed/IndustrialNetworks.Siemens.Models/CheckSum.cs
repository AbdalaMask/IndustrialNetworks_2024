using System.Collections.Generic;

namespace NetStudio.Siemens.Models;

public class CheckSum
{
	public byte CRC(byte[] data)
	{
		byte b = 0;
		foreach (byte b2 in data)
		{
			b ^= b2;
		}
		return b;
	}

	public byte FCS(byte[] data)
	{
		byte b = 0;
		foreach (byte b2 in data)
		{
			b = (byte)((b + b2) % 256);
		}
		return b;
	}

	public byte[] AddCRC(byte[] data)
	{
		byte item = CRC(data);
		return new List<byte>(data) { item }.ToArray();
	}

	public static byte[] AddOnDLE(byte[] data)
	{
		List<byte> list = new List<byte>();
		foreach (byte b in data)
		{
			list.Add(b);
			if (b == 16)
			{
				list.Add(b);
			}
		}
		return list.ToArray();
	}

	public static byte[] SubOnDLE(byte[] data)
	{
		List<byte> list = new List<byte>();
		byte b = 0;
		foreach (byte b2 in data)
		{
			if (b == 16 && b2 == 16)
			{
				b = 0;
				continue;
			}
			list.Add(b2);
			b = b2;
		}
		return list.ToArray();
	}
}
