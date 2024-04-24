namespace NetStudio.Fatek.Models;

public class PacketBase
{
	public bool IsBit { get; set; }

	public ushort StationNo { get; set; }

	public string Address { get; set; }

	public int Quantity { get; set; }

	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }
}
