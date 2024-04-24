using System.ComponentModel;

namespace NetStudio.Common.AsrsLink;

public enum OperatingMode
{
	[Description("None")]
	None,
	[Description("All")]
	All,
	[Description("Write to Controller")]
	WriteToController,
	[Description("Write to Database")]
	WriteToSQL
}
