using NetStudio.Vigor.Enums;

namespace NetStudio.Vigor;

public class PacketBase
{
	public int StationNo { get; set; }

	public string Memory { get; set; }

	public string Address { get; set; }

	public int WordAddress { get; set; }

	public int Quantity { get; set; }

	public int NumOfBytes { get; set; }

	public DeviceCode DeviceCode { get; set; }

	public FunctionCode Function { get; set; }

	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }
}
