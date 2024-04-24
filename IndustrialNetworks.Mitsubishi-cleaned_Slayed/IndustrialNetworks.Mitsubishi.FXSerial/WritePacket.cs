namespace NetStudio.Mitsubishi.FXSerial;

public sealed class WritePacket : PacketBase
{
	public bool IsBit { get; set; }

	public ushort StartAddress { get; set; }

	public string ValueHex { get; set; }
}
