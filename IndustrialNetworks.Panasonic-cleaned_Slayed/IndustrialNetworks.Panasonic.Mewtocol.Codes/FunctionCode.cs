using System.ComponentModel;

namespace NetStudio.Panasonic.Mewtocol.Codes;

public enum FunctionCode
{
	[Description("RCS: Specifies only one point")]
	RCS,
	[Description("RCP: Specifies multiple contacts")]
	RCP,
	[Description("RCC: Specifies a range in word units")]
	RCC,
	[Description("WC: Turns contacts on and off")]
	WC,
	[Description("WCS: Specifies only one point")]
	WCS,
	[Description("WCP: Specifies multiple contacts")]
	WCP,
	[Description("WCC: Specifies a range in word units")]
	WCC,
	[Description("RD: Reads the contents of a data area")]
	RD,
	[Description("WD: Writes data to a data area")]
	WD,
	[Description("RS: Reads the value set for a timer/counter")]
	RS,
	[Description("WS: Writes a timer/counter setting value")]
	WS,
	[Description("RK: Reads the timer/counter elapsed value")]
	RK,
	[Description("WK: Writes the timer/counter elapsed value")]
	WK,
	[Description("MC: Registers the contact to be monitored")]
	MC,
	[Description("MD: Registers the data to be monitored")]
	MD,
	[Description("MG: Monitors a registered contact or data")]
	MG,
	[Description("SC: Embeds the area of a specified range in a 16-point on and off pattern")]
	SC,
	[Description("SD: Writes the same contents to the data area of a specified range")]
	SD,
	[Description("RR: Reads the contents of a system register")]
	RR,
	[Description("WR: Specifies the contents of a system register")]
	WR,
	[Description("RT: Reads the specifications of the programmable controller and error codes if an error occurs")]
	RT,
	[Description("RM: Switches the operation mode of the programmable controller")]
	RM,
	[Description("AB: Aborts communication")]
	AB
}
