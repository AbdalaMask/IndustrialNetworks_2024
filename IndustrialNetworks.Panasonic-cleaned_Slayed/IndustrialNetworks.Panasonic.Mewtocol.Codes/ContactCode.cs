using System.ComponentModel;

namespace NetStudio.Panasonic.Mewtocol.Codes;

public enum ContactCode
{
	[Description("External Input X")]
	X,
	[Description("External Output Y")]
	Y,
	[Description("Internal Relay R")]
	R,
	[Description("Link Relay L")]
	L,
	[Description("Timer T")]
	T,
	[Description("Counter C")]
	C
}
