using System.ComponentModel;

namespace NetStudio.Mitsubishi.Models;

public enum CPUType
{
	[Description("FX-CPU")]
	FXCPU,
	[Description("FX5-CPU")]
	FX5CPU,
	[Description("Q-CPU (Q Mode)")]
	QCPU,
	[Description("L-CPU")]
	LCPU,
	[Description("R-CPU")]
	RCPU,
	[Description("NC-CPU")]
	NCCPU,
	[Description("Other")]
	OTHER
}
