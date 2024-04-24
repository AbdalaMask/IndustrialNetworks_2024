namespace NetStudio.Keyence.MC.Serial;

internal sealed class SMCBuilder
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

	public string ReadFormat1(AccessRoute accessRoute, RequestData requestData)
	{
		string empty = string.Empty;
		empty += accessRoute.StationNo.ToString("X2");
		empty += accessRoute.PCNo.ToString("X2");
		empty += requestData.Command;
		empty += requestData.MessageWait.ToString("X");
		empty += requestData.HeadDevice;
		empty += CheckSum(empty);
		return "\u0005" + empty;
	}

	public string ReadFormat2(AccessRoute accessRoute, RequestData requestData)
	{
		string empty = string.Empty;
		empty += accessRoute.StationNo.ToString("X2");
		empty += accessRoute.PCNo.ToString("X2");
		empty += requestData.Command;
		empty += requestData.MessageWait.ToString("X");
		empty += requestData.HeadDevice;
		empty += CheckSum(empty);
		return "\u0005" + empty;
	}

	public string WriteFormat1()
	{
		string empty = string.Empty;
		return "\u0005" + empty;
	}

	public string FrameIDNo(MessageFormat format)
	{
		string result = string.Empty;
		switch (format)
		{
		case MessageFormat.Format2:
			result = "FB";
			break;
		case MessageFormat.Format3:
			result = "F9";
			break;
		case MessageFormat.Format4:
			result = "F8";
			break;
		}
		return result;
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
