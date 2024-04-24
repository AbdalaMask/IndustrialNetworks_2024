using System.ComponentModel;

namespace NetStudio.Vigor.Enums;

public enum DeviceCode
{
	[Description("90H: External Input X")]
	ExternalInputX = 144,
	[Description("91H: External Output Y")]
	ExternalOutputY = 145,
	[Description("92H: Auxiliary Relay M")]
	AuxiliaryRelayM = 146,
	[Description("93H: Step Relay S")]
	StepRelayS = 147,
	[Description("94H: Special Relay M")]
	SpecialRelayM = 148,
	[Description("95H: Register D’s Bit D.b")]
	RegisterDsBitDb = 149,
	[Description("97H: Register R’s Bit R.b")]
	RegisterRsBitRb = 151,
	[Description("98H: Coil of a timer T")]
	CoilOfATimerT = 152,
	[Description("99H: Contact of a timer T")]
	ContactOfATimerT = 153,
	[Description("9CH: Coil of a counter C")]
	CoilOfACounterC = 156,
	[Description("9DH: Contact of a counter C")]
	ContactOfACounterC = 157,
	[Description("A0H: Register D(content value)")]
	RegisterD = 160,
	[Description("A1H: Special Register SD(content value)")]
	SpecialRegisterSD = 161,
	[Description("A2H: Register R(content value)")]
	RegisterR = 162,
	[Description("A8H: Timer T (present value)")]
	TimerT = 168,
	[Description("ACH: 16-bit Counter C (current value)")]
	Counter16Bit = 172,
	[Description("ADH: 32-bit Counter C (current value)")]
	Counter32Bit = 173
}
