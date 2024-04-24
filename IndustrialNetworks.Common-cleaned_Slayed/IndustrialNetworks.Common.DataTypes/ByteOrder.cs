using System.ComponentModel;

namespace NetStudio.Common.DataTypes;

public enum ByteOrder
{
	[Description("Big endian")]
	BigEndian,
	[Description("Little endian")]
	LittleEndian,
	[Description("Big endian byte swap")]
	BigEndianByteSwap,
	[Description("Little endian byte swap")]
	LittleEndianByteSwap
}
