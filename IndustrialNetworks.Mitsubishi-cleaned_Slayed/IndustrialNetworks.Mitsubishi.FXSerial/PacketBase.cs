namespace NetStudio.Mitsubishi.FXSerial;

public class PacketBase
{
	public string Memory { get; set; }

	public string Address { get; set; }

	public int NumOfBytes { get; set; }

	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }
}
