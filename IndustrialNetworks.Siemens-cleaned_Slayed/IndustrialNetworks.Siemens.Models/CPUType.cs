using System.ComponentModel;

namespace NetStudio.Siemens.Models;

public enum CPUType
{
	[Description("S7-200")]
	S7200,
	[Description("S7-200 Smart")]
	S7200Smart,
	[Description("S7-300")]
	S7300,
	[Description("S7-400")]
	S7400,
	[Description("S7-1200")]
	S71200,
	[Description("S7-1500")]
	S71500,
	[Description("Win LC")]
	WinLC
}
