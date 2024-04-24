namespace NetStudio.Delta.Models;

public class BaseBuilder
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
			1 => "01/0x01: Illegal Function.", 
			2 => "02/0x02: Illegal Data Address.", 
			3 => "03/0x03: Illegal Data Value.", 
			4 => "04/0x04: Failure In Associated Device.", 
			5 => "05/0x05: Acknowledge.", 
			6 => "06/0x06: Slave Device Busy.", 
			7 => "07/0x07: NAK – Negative Acknowledgement.", 
			8 => "08/0x08: Memory Parity Error.", 
			10 => "10/0x0A: Gateway Path Unavailable.", 
			11 => "11/0x0B: Gateway Target Device Failed to respond.", 
			128 => "Unexpected response received.", 
			64 => "Unexpected master output path received.", 
			_ => "An unknown error.", 
		};
	}
}
