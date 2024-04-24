using System.ComponentModel;

namespace NetStudio.Common.Manager;

public enum Manufacturer
{
	[Description("Popular Protocol")]
	IPC,
	[Description("Siemens Automation")]
	SIEMENS,
	[Description("Schneider Electric")]
	SCHNEIDER,
	[Description("Mitsubishi Electric")]
	MITSUBISHI,
	[Description("Omron Automation")]
	OMRON,
	[Description("Panasonic Automation")]
	PANASONIC,
	[Description("LS Electric")]
	LS,
	[Description("Delta Electronics")]
	DELTA,
	[Description("Fatek Automation")]
	FATEK,
	[Description("Vigor Electric")]
	VIGOR,
	[Description("Keyence Corporation")]
	KEYENCE
}
