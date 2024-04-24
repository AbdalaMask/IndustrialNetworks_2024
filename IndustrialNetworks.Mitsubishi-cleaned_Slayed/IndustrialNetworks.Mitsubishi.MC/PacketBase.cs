namespace NetStudio.Mitsubishi.MC;

public class PacketBase
{
	public byte StationNo { get; set; }

	public bool IsBit { get; set; }

	public byte DeviceCode { get; set; }

	public string Address { get; set; }

	public int WordAddress { get; set; }

	public int Quantity { get; set; }

	public int NumOfBytes
	{
		get
		{
			if (IsBit)
			{
				return Quantity / 2 + ((Quantity % 2 != 0) ? 1 : 0);
			}
			return 2 * Quantity;
		}
	}

	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }
}
