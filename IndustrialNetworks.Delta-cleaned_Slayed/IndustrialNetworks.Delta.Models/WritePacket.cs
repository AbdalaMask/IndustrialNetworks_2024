namespace NetStudio.Delta.Models;

public class WritePacket : PacketBase
{
	public string ValueHex { get; set; }

	public byte[] DataDec { get; set; }
}
