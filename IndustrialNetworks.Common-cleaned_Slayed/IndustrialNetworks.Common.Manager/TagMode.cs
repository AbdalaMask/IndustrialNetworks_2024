using System.ComponentModel;

namespace NetStudio.Common.Manager;

public enum TagMode
{
	[Description("Read/Write")]
	ReadWrite,
	[Description("Read Only")]
	ReadOnly,
	[Description("Write Only")]
	WriteOnly
}
