using System.Collections.Generic;
using NetStudio.Common.Manager;

namespace NetStudio.Mitsubishi.MC;

public sealed class ReadPacket : PacketBase
{
	public byte[] SendBytes { get; set; }

	public List<Tag> Tags { get; set; }

	public ReadPacket()
	{
		Tags = new List<Tag>();
	}
}
