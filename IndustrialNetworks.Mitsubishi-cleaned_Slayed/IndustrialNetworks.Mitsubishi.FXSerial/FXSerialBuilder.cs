using NetStudio.Common.DataTypes;

namespace NetStudio.Mitsubishi.FXSerial;

public class FXSerialBuilder
{
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

	public const int SIZE_PACKET = 64;

	private const string READ_BYTES = "0";

	private const string WRITE_BYTES = "1";

	public const string FORCE_ON = "7";

	public const string FORCE_OFF = "8";

	public string ReadBitMsg(ushort startAddress, byte numOfBytes)
	{
		string text = "0";
		text += UINT.ToHex(startAddress, ByteOrder.LittleEndian);
		text += numOfBytes.ToString("X2");
		text += "\u0003";
		return "\u0002" + text + CheckSum(text);
	}

	public string WriteBitMsg(ushort startAddress, string dataHex)
	{
		string text = dataHex;
		text += UINT.ToHex(startAddress, ByteOrder.LittleEndian);
		text += "\u0003";
		return "\u0002" + text + CheckSum(text);
	}

	public string ReadBytesMsg(ushort startAddress, byte numOfBytes)
	{
		string text = "0";
		text += startAddress.ToString("X4");
		text += numOfBytes.ToString("X2");
		text += "\u0003";
		return "\u0002" + text + CheckSum(text);
	}

	public string WriteBytesMsg(ushort startAddress, byte numOfBytes, string dataHex)
	{
		string text = "1";
		text += startAddress.ToString("X4");
		text += numOfBytes.ToString("X2");
		text += dataHex;
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
