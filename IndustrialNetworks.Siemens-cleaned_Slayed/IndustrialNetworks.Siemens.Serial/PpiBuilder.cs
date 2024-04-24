using System.Collections.Generic;
using NetStudio.Common.DataTypes;
using NetStudio.Siemens.Models;

namespace NetStudio.Siemens.Serial;

public class PpiBuilder : CheckSum
{
	public byte[] ReadDataMessage(ReadPacket RP)
	{
		List<byte> list = new List<byte>
		{
			104, 27, 27, 104, 2, 0, 124, 50, 1, 0,
			0, 2, 0, 0, 14, 0, 0, 4, 1, 18,
			10, 16
		};
		if (RP.Memory != Memory.Counter && RP.Memory != Memory.Timer)
		{
			list.Add(2);
			list.AddRange(UINT.ToBytes((ushort)RP.Quantity));
			list.AddRange(UINT.ToBytes((ushort)RP.DBNumber));
			list.Add((byte)RP.Memory);
			int byteAddress = S7Utility.GetByteAddress(RP.Address);
			byte[] array = DINT.ToBytes(8 * byteAddress);
			for (int i = 1; i < 4; i++)
			{
				list.Add(array[i]);
			}
		}
		else
		{
			list.Add((byte)RP.Memory);
			list.AddRange(UINT.ToBytes((ushort)RP.Quantity));
			list.AddRange(UINT.ToBytes((ushort)RP.DBNumber));
			list.Add((byte)RP.Memory);
			int byteAddress2 = S7Utility.GetByteAddress(RP.Address);
			byte[] array2 = DINT.ToBytes(8 * byteAddress2);
			for (int j = 1; j < 4; j++)
			{
				list.Add(array2[j]);
			}
		}
		byte b2 = (list[1] = (byte)(list.Count - 4));
		list[2] = b2;
		list.Add(FCS(list.GetRange(4, b2).ToArray()));
		list.Add(22);
		return list.ToArray();
	}
}
