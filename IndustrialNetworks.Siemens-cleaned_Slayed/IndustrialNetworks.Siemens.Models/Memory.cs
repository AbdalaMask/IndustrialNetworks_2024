using System.ComponentModel;

namespace NetStudio.Siemens.Models;

public enum Memory
{
	[Description("Periphery")]
	Periphery = 0,
	[Description("Counter")]
	Counter = 28,
	[Description("Timer")]
	Timer = 29,
	[Description("Input")]
	Input = 129,
	[Description("Output")]
	Output = 130,
	[Description("Flag")]
	Flag = 131,
	[Description("Datablock")]
	Datablock = 132,
	[Description("Instance Data block")]
	InstanceDatablock = 133,
	[Description("Local Data")]
	LocalData = 134,
	[Description("Local data of previous block")]
	LocalDataOfPreviousBlock = 135
}
