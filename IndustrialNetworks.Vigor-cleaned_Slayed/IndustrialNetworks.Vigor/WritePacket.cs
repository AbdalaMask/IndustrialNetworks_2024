namespace NetStudio.Vigor;

public sealed class WritePacket : PacketBase
{
	public bool IsBit { get; set; }

	public int ByteAddress { get; set; }

	public int BitAddress { get; set; }

	public byte[] ValueDec { get; set; }

	public string ValueHex { get; set; }
}
