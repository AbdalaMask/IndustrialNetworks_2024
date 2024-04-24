namespace NetStudio.Mitsubishi.Dedicated;

public class PacketBase
{
	public int StationNo { get; set; }

	public int PlcNo { get; set; } = 255;


	public Command Command { get; set; } = Command.WW;


	public byte MWT { get; set; }

	public string Address { get; set; }

	public int Quantity { get; set; }

	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }
}
