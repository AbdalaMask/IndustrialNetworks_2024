using System.ComponentModel;

namespace NetStudio.Common.IndusCom;

public enum ConnectionType
{
	[Description("Ethernet")]
	Ethernet,
	[Description("Serial Port")]
	Serial
}
