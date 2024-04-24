namespace NetStudio.Siemens.Models;

public class PacketBase
{
	public bool IsBit { get; set; }

	public int DBNumber { get; set; }

	public Memory Memory { get; set; } = Memory.Flag;


	public decimal Address { get; set; }

	public int Quantity { get; set; }

	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }

	public override string ToString()
	{
		return $"DBNumber: {DBNumber}, Memory: {Memory}, Start address: {Address}, Quantity: {Quantity}";
	}
}
