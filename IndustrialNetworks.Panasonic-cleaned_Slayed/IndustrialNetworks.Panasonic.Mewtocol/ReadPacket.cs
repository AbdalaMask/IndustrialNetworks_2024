using System.Collections.Generic;
using NetStudio.Common.Manager;

namespace NetStudio.Panasonic.Mewtocol;

public class ReadPacket : PacketBase
{
	public bool IsBitInWord { get; set; }

	public List<Tag> Tags { get; set; }

	public ReadPacket()
	{
		Tags = new List<Tag>();
	}
}
