namespace NetStudio.Omron.Models;

public class PacketBase
{
	public ushort StationNo { get; set; }

	public string Memory { get; set; }

	public string Address { get; set; }

	public int NumOfWords { get; set; }

	public int NumOfchars => 4 * NumOfWords;

	public int NumOfRecvBytes { get; set; }

	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }
}
