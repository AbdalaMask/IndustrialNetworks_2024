using System.Collections.Generic;
using NetStudio.Common.Manager;

namespace NetStudio.Modbus;

public class ReadPacket : PacketBase
{
	public string SendMsg { get; set; }

	public byte[] SendBytes { get; set; }

	public List<Tag> Tags { get; set; }

	public ReadPacket()
	{
		Tags = new List<Tag>();
	}
}
