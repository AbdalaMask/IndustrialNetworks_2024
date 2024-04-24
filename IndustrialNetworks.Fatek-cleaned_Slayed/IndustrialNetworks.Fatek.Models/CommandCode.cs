namespace NetStudio.Fatek.Models;

public enum CommandCode
{
	PLC_Status = 64,
	PLC_Control,
	Single_Discrete_Control,
	Read_Enable_Disable,
	Read_Continuous_Discrete,
	Write_Continuous_Discrete,
	Read_Continuous_Registers,
	Write_Continuous_Registers
}
