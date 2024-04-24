using System.ComponentModel;

namespace NetStudio.IPS.Entity;

public enum EditMode
{
	[Description("None")]
	None,
	[Description("Add New: Normal")]
	AddNew,
	[Description("Add New: Continuous")]
	Continuous,
	[Description("Edit")]
	Edit,
	[Description("Copy")]
	Copy
}
