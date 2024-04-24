namespace NetStudio.Panasonic.Mewtocol;

public class PacketBase
{
	public ushort StationNo { get; set; }

	public string Memory { get; set; }

	public string Address { get; set; }

	public int Quantity { get; set; }

	public string SendMsg { get; set; }

	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }
}
