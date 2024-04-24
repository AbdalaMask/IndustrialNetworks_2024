using System.ComponentModel;

namespace NetStudio.Common.Historiant;

public enum LoggingLimit
{
	[Description("Within deadband")]
	WithinDeadband,
	[Description("Outside deadband")]
	OutsideDeadband
}
