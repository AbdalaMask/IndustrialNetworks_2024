using System.ComponentModel;

namespace NetStudio.Vigor.Enums;

public enum FunctionCode
{
	[Description("20H: Word Device Read")]
	WordDeviceRead = 32,
	[Description("28H: Word Device Write")]
	WordDeviceWrite = 40,
	[Description("21H: Bit Device Read")]
	BitDeviceRead = 33,
	[Description("29H: Bit Device Write")]
	BitDeviceWrite = 41
}
