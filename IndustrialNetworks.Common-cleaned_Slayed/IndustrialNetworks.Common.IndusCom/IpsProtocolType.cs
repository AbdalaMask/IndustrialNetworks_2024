using System.ComponentModel;

namespace NetStudio.Common.IndusCom;

public enum IpsProtocolType
{
	[Description("TCP/IP")]
	S7_TCP,
	[Description("MPI")]
	S7_MPI,
	[Description("PPI")]
	S7_PPI,
	[Description("ASCII Protocol")]
	ASCII_PROTOCOL,
	[Description("Cnet: XGT Protocol")]
	CNET_XGT_PROTOCOL,
	[Description("FEnet: XGT Protocol")]
	FENET_XGT_PROTOCOL,
	[Description("Mewtocol Protocol")]
	MEWTOCOL_PROTOCOL,
	[Description("MC Protocol")]
	MISUBISHI_MC_PROTOCOL,
	[Description("SLMP Protocol")]
	MISUBISHI_SLMP_PROTOCOL,
	[Description("Dedicated Protocol: Format1")]
	DEDICATED1_PROTOCOL,
	[Description("Dedicated Protocol: Format4")]
	DEDICATED4_PROTOCOL,
	[Description("FX Serial Protocol")]
	FX_SERIAL_PROTOCOL,
	[Description("Fins TCP/IP")]
	FINS_TCP_PROTOCOL,
	[Description("Fins UDP")]
	FINS_UDP_PROTOCOL,
	[Description("Host Link")]
	HOSTLINK_FINS_PROTOCOL,
	[Description("Host Link: C-Mode")]
	HOSTLINK_CMODE_PROTOCOL,
	[Description("Modbus TCP/IP")]
	MODBUS_TCP,
	[Description("Modbus RTU")]
	MODBUS_RTU,
	[Description("Modbus ASCII")]
	MODBUS_ASCII,
	[Description("VS-Protocol")]
	VS_PROTOCOL,
	[Description("VB/VH-Protocol")]
	VB_PROTOCOL,
	[Description("Fatek Protocol")]
	FATEK_PROTOCOL,
	[Description("Modbus ASCII")]
	DELTA_ASCII,
	[Description("Modbus RTU")]
	DELTA_RTU,
	[Description("Modbus TCP/IP")]
	DELTA_TCP,
	[Description("MC Protocol")]
	KEYENCE_MC_PROTOCOL,
	[Description("Ethernet/IP")]
	KEYENCE_ETHERNET_IP_PROTOCOL
}
