namespace NetStudio.Omron.Models;

public class WritePacket : PacketBase
{
	public byte MemoryAreaCode { get; set; }

	public string wordAddress { get; set; }

	public int bitAddress { get; set; }

	public byte[] Values { get; set; }

	public string ValueHex { get; set; }
}
