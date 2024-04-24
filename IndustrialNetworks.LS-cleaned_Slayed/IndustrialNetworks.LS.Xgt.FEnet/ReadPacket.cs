using System.Collections.Generic;
using NetStudio.Common.Manager;

namespace NetStudio.LS.Xgt.FEnet;

public class ReadPacket : PacketBase
{
	public byte[] SendBytes { get; set; }

	public int RcvDataCout { get; set; }

	public List<Tag> Tags { get; set; }

	public ReadPacket()
	{
		Tags = new List<Tag>();
	}
}
