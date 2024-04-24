using System.Collections.Generic;
using NetStudio.Common.Manager;

namespace NetStudio.Mitsubishi.FXSerial;

public sealed class ReadPacket : PacketBase
{
	public int NumOfchars => 2 * base.NumOfBytes;

	public string SendMsg { get; set; }

	public List<Tag> Tags { get; set; }

	public ReadPacket()
	{
		Tags = new List<Tag>();
	}
}
