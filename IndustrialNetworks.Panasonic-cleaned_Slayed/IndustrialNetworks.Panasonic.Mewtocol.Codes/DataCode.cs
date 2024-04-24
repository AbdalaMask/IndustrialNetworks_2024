using System.ComponentModel;

namespace NetStudio.Panasonic.Mewtocol.Codes;

public enum DataCode
{
	[Description("Data register DT")]
	D,
	[Description("Link data register LD")]
	L,
	[Description("File register FL")]
	F
}
