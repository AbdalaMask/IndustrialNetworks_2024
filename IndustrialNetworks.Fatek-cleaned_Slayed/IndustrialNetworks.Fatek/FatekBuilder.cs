using NetStudio.Fatek.Models;

namespace NetStudio.Fatek;

public class FatekBuilder
{
	public class ASCII
	{
		public const char STX = '\u0002';

		public const char ETX = '\u0003';

		public const char EOT = '\u0004';

		public const char ENQ = '\u0005';

		public const char ACK = '\u0006';

		public const char LF = '\n';

		public const char CL = '\f';

		public const char CR = '\r';

		public const char NAK = '\u0015';

		public const char SPACE = ' ';
	}

	public string GetPlcStatusMsg(int stationNo)
	{
		string text = stationNo.ToString("X2");
		text += $"{(byte)64:X2}";
		return "\u0002" + text + CheckSum(text) + "\u0003";
	}

	public string GetPlcControlMsg(int stationNo, ControlCode ctrl)
	{
		string text = stationNo.ToString("X2");
		text += $"{(byte)65:X2}";
		text += $"{(byte)ctrl:X}";
		return "\u0002" + text + CheckSum(text) + "\u0003";
	}

	public string GetSingleDiscreteControlMsg(int stationNo, RunningCode runCode, string discreteNo)
	{
		string text = stationNo.ToString("X2");
		text += $"{(byte)66:X2}";
		text += $"{(byte)runCode:X}";
		text += discreteNo;
		return "\u0002" + text + CheckSum(text) + "\u0003";
	}

	public string GetEnableOrDisableMsg(int stationNo, ushort discreteNo, int numOfPoints)
	{
		string text = stationNo.ToString("X2");
		text += $"{(byte)67:X2}";
		text += $"{numOfPoints:X2}";
		text += discreteNo;
		return "\u0002" + text + CheckSum(text) + "\u0003";
	}

	public string ReadBitsMsg(int stationNo, string discreteNo, int numOfPoints)
	{
		string text = stationNo.ToString("X2");
		text += $"{(byte)68:X2}";
		text += $"{numOfPoints:X2}";
		text += discreteNo;
		return "\u0002" + text + CheckSum(text) + "\u0003";
	}

	public string WriteBitsMsg(int stationNo, string discreteNo, int numOfPoints, string values)
	{
		string text = stationNo.ToString("X2");
		text += $"{(byte)69:X2}";
		text += numOfPoints.ToString("X2");
		text += discreteNo;
		text += values;
		return "\u0002" + text + CheckSum(text) + "\u0003";
	}

	public string ReadRegistersMsg(ReadPacket RP)
	{
		string text = RP.StationNo.ToString("X2");
		text += $"{(byte)70:X2}";
		text += $"{RP.Quantity:X2}";
		text += RP.Address;
		return "\u0002" + text + CheckSum(text) + "\u0003";
	}

	public string WriteRegistersMsg(WritePacket WP)
	{
		string text = WP.StationNo.ToString("X2");
		text += $"{(byte)71:X2}";
		text += WP.Quantity.ToString("X2");
		text += WP.Address;
		text += WP.ValueHex;
		return "\u0002" + text + CheckSum(text) + "\u0003";
	}

	private string CheckSum(string frame)
	{
		uint num = 2u;
		foreach (char c in frame)
		{
			num = (num + (byte)c) % 256;
		}
		return num.ToString("X2");
	}
}
