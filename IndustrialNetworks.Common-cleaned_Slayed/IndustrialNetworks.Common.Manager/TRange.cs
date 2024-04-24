using NetStudio.Common.IndusCom;

namespace NetStudio.Common.Manager;

public class TRange
{
	public Tag Template { get; set; }

	public string Memory { get; set; }

	public decimal Offset { get; set; }

	public int Quantity { get; set; } = 16;


	public int Size { get; set; } = 1;


	public IpsProtocolType Protocol { get; set; }
}
