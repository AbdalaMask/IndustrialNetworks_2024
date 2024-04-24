using System.ComponentModel;

namespace NetStudio.Common.AsrsLink;

public enum AsrsTrigger
{
	[Description("None")]
	None,
	[Description("Positive")]
	Positive,
	[Description("Negative")]
	Negative,
	[Description("Directive")]
	Directive,
	[Description("Upon change")]
	UponChange
}
