using NetStudio.Common.IndusCom;

namespace NetStudio.Delta.Models;

public class PacketBase
{
	public byte StationNo { get; set; }

	public byte Function { get; set; }

	public string Memory { get; set; }

	public IpsAddress Address { get; set; }

	public int Quantity { get; set; }

	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }

	public override string ToString()
	{
		return $"Device(StationNo={StationNo}, Function={Function}, Address={Address}, Quantity={Quantity})";
	}
}
