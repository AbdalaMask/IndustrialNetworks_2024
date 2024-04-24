using System.ComponentModel;

namespace NetStudio.Common.Historiant;

public enum LoggingMode
{
	[Description("Cyclic")]
	Cyclic,
	[Description("On change")]
	OnChange
}
