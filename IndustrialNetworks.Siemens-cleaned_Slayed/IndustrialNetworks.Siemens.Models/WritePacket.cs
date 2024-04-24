namespace NetStudio.Siemens.Models;

public sealed class WritePacket : PacketBase
{
	public byte[] Data { get; set; }
}
