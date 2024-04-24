using System.ComponentModel;

namespace NetStudio.Panasonic.Mewtocol.Codes;

public enum OperationCode
{
	[Description("NORMAL")]
	N,
	[Description("PROGRAM")]
	R,
	[Description("RUN")]
	P
}
