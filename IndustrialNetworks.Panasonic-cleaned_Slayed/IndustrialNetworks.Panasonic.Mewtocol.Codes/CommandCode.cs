using System.ComponentModel;

namespace NetStudio.Panasonic.Mewtocol.Codes;

public enum CommandCode
{
	[Description("Writing to a data area")]
	WDT = 80,
	[Description("Reading from a data area")]
	RDT,
	[Description("Writing of contact information")]
	WCI,
	[Description("Reading of contact information")]
	RCI
}
