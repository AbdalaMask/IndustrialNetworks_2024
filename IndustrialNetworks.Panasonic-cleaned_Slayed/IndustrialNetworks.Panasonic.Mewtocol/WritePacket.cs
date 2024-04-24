using NetStudio.Panasonic.Mewtocol.Codes;

namespace NetStudio.Panasonic.Mewtocol;

public class WritePacket : PacketBase
{
	public bool IsBit { get; set; }

	public AreaCode AreaCode { get; set; }
}
