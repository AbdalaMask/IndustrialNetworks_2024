using System.ComponentModel;

namespace NetStudio.Common.Alarms;

public enum LimitMode
{
	[Description("None")]
	None,
	[Description("Lower")]
	Lower,
	[Description("Equal")]
	Equal,
	[Description("Higher")]
	Higher
}
