namespace NetStudio.Modbus;

public class WritePacket : PacketBase
{
	public string ValueHex { get; set; }

	public byte[] DataDec { get; set; }
}
