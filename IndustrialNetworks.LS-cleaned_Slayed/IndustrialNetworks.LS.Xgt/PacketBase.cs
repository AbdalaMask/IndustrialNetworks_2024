using NetStudio.LS.Xgt.FEnet;

namespace NetStudio.LS.Xgt;

public class PacketBase
{
	public CompanyID CompanyID { get; set; }

	public CPUInfo CPUInfo { get; set; } = CPUInfo.XGB_MK;


	public byte FEnetPosition { get; set; }

	public bool IsBit { get; set; }

	public ushort StationNo { get; set; }

	public string Address { get; set; }

	public int Quantity { get; set; }

	public byte[] DataType { get; set; } = FEDataType.Continuous;


	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }
}
