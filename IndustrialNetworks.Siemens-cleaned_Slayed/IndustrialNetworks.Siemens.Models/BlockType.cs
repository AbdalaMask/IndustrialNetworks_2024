using System.ComponentModel;

namespace NetStudio.Siemens.Models;

public enum BlockType
{
	[Description("Organization block")]
	OB = 56,
	[Description("Data block")]
	DB = 65,
	[Description("System Data block")]
	SDB = 66,
	[Description("Function")]
	FC = 67,
	[Description("System Function")]
	SFC = 68,
	[Description("Function block")]
	FB = 69,
	[Description("System Function Block")]
	SFB = 70
}
