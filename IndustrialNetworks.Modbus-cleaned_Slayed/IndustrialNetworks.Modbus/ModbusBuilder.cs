namespace NetStudio.Modbus;

public class ModbusBuilder
{
	public const byte FUNC_01 = 1;

	public const byte FUNC_02 = 2;

	public const byte FUNC_03 = 3;

	public const byte FUNC_04 = 4;

	public const byte FUNC_05 = 5;

	public const byte FUNC_06 = 6;

	public const byte FUNC_15 = 15;

	public const byte FUNC_16 = 16;

	protected string GetErrorMessage(byte errorCode)
	{
		return errorCode switch
		{
			32 => "Initiated transaction forgotten by slave device.", 
			1 => "Illegal Function.", 
			2 => "Illegal data address.", 
			3 => "Illegal Data Value.", 
			4 => "Failure In Associated Device.", 
			5 => "Acknowledge.", 
			6 => "Slave Device Busy.", 
			7 => "NAK – Negative Acknowledgement.", 
			8 => "Memory Parity Error.", 
			10 => "Gateway Path Unavailable.", 
			11 => "Gateway Target Device Failed to respond.", 
			128 => "Unexpected response received.", 
			64 => "Unexpected master output path received.", 
			_ => "An unknown error.", 
		};
	}
}
