namespace NetStudio.Vigor;

public class VBBuilder
{
	public class Command
	{
		public const string ReadContinueData = "51";

		public const string WriteContinueData = "61";

		public const string ForceContactON = "70";

		public const string ForceContactOFF = "71";
	}

	protected class ASCII
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

	protected const string OK = "00";

	public string ReadMsg(int stationNo, int startAddress, int numOfBytes)
	{
		string text = stationNo.ToString("X2");
		text += "51";
		text += startAddress.ToString("X4");
		text += numOfBytes.ToString("X2");
		text += "\u0003";
		return "\u0002" + text + CheckSum(text);
	}

	public string WriteMsg(int stationNo, int startAddress, byte[] values)
	{
		string text = stationNo.ToString("X2");
		text += "61";
		text += startAddress.ToString("X4");
		text += values.Length.ToString("X2");
		foreach (byte b in values)
		{
			text += b.ToString("X2");
		}
		text += "\u0003";
		return "\u0002" + text + CheckSum(text);
	}

	public string WriteMsg(int stationNo, int startAddress, string dataHex)
	{
		string text = stationNo.ToString("X2");
		text += "61";
		text += startAddress.ToString("X4");
		text += dataHex;
		text += "\u0003";
		return "\u0002" + text + CheckSum(text);
	}

	public string WriteBitMsg(int stationNo, int startAddress, bool value)
	{
		string text = stationNo.ToString("X2");
		text += (value ? "70" : "71");
		text += startAddress.ToString("X4");
		text += "\u0003";
		return "\u0002" + text + CheckSum(text);
	}

	public string WriteBitMsg(int stationNo, int startAddress, string dataHex)
	{
		string text = stationNo.ToString("X2");
		text += dataHex;
		text += startAddress.ToString("X4");
		text += "\u0003";
		return "\u0002" + text + CheckSum(text);
	}

	private string CheckSum(string frame)
	{
		uint num = 0u;
		foreach (char c in frame)
		{
			num = (num + (byte)c) % 256;
		}
		return num.ToString("X2");
	}
}
