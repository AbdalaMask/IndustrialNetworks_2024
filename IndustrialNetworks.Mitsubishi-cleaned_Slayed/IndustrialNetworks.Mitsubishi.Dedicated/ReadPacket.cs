using System.Collections.Generic;
using NetStudio.Common.Manager;

namespace NetStudio.Mitsubishi.Dedicated;

public sealed class ReadPacket : PacketBase
{
	public int NumOfBytes => 2 * base.Quantity;

	public int NumOfchars => 2 * NumOfBytes;

	public int WordAddress { get; set; }

	public string SendMsg { get; set; }

	public List<Tag> Tags { get; set; }

	public ReadPacket()
	{
		Tags = new List<Tag>();
	}
}
