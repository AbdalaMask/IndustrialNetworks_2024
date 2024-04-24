using System.ComponentModel;

namespace NetStudio.Common.Historiant;

public enum StorageType
{
	[Description("CSV file")]
	CSV,
	[Description("Database")]
	Database,
	[Description("Text file")]
	Text
}
