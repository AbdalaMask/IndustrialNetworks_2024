using System.ComponentModel;

namespace NetStudio.Omron.Models;

public enum Mode
{
	[Description("PROGRAM")]
	PROGRAM,
	[Description("RUN")]
	RUN,
	[Description("MONITOR")]
	MONITOR
}
