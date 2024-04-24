using System.Collections.Generic;

namespace NetStudio.Mitsubishi.Dedicated;

internal sealed class DedicatedBuilder
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

	private ControlProcedure controlProcedure;

	public Dictionary<string, string> ErrorCodes = new Dictionary<string, string>
	{
		{ "02", "Sum check error." },
		{ "03", "Protocol error." },
		{ "06", "Character area error." },
		{ "07", "Character error." },
		{ "0A", "PLC number error." },
		{ "10", "PLC number error." },
		{ "18", "Remote control error." }
	};

	public DedicatedBuilder(ControlProcedure controlProcedure_0)
	{
		controlProcedure = controlProcedure_0;
	}

	public string ReadMsg(ReadPacket RP)
	{
		string empty = string.Empty;
		empty += RP.StationNo.ToString("X2");
		empty += RP.PlcNo.ToString("X2");
		empty += RP.Command;
		empty += RP.MWT.ToString("X");
		empty += RP.Address;
		empty += RP.Quantity.ToString("X2");
		empty += CheckSum(empty);
		if (controlProcedure == ControlProcedure.Format4)
		{
			empty += "\r";
			empty += "\n";
		}
		return "\u0005" + empty;
	}

	public string WriteMsg(WritePacket WP)
	{
		string empty = string.Empty;
		empty += WP.StationNo.ToString("X2");
		empty += WP.PlcNo.ToString("X2");
		empty += WP.Command;
		empty += WP.MWT.ToString("X");
		empty += WP.Address;
		empty += WP.Quantity.ToString("X2");
		empty += WP.ValueHex;
		empty += CheckSum(empty);
		if (controlProcedure == ControlProcedure.Format4)
		{
			empty += "\r";
			empty += "\n";
		}
		return "\u0005" + empty;
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
