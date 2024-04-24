using System.ComponentModel;

namespace NetStudio.Common.Historiant;

public enum CycleUnit
{
	[Description("Millisecond")]
	Millisecond,
	[Description("Seconds")]
	Seconds,
	[Description("Minutes")]
	Minutes,
	[Description("Hours")]
	Hours,
	[Description("Days")]
	Days
}
