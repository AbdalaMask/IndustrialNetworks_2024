using System.Collections.Generic;
using NetStudio.Omron.Models;

namespace NetStudio.Omron.HostLink;

public class HostLinkFinsBuilder : BaseBuilder
{
	public static class Command
	{
		public const string MEMORY_AREA_READ = "0101";

		public const string MEMORY_AREA_WRITE = "0102";

		public const string MEMORY_AREA_FILL = "0103";

		public const string MULTIPLE_MEMORY_AREA_READ = "0104";

		public const string MEMORY_AREA_TRANSFER = "0105";

		public const string RUN_MODE = "0401";

		public const string STOP_MODE = "0402";

		public const string READ_CPU_STATUS = "0601";
	}

	public const char CHAR_START = '@';

	public const string DELIMITER = "\r";

	private const string HEADER_CODE = "FA";

	public static readonly Dictionary<string, string> BitMemoryAreaCode = new Dictionary<string, string>
	{
		{ "SYS", "SYS" },
		{ "CIO", "30" },
		{ "W", "31" },
		{ "H", "32" },
		{ "A", "33" },
		{ "D", "02" },
		{ "E", "20" },
		{ "TK", "06" },
		{ "CIO_FS", "70" },
		{ "W_FS", "71" },
		{ "H_FS", "72" },
		{ "T", "09" },
		{ "C", "09" },
		{ "T_FS", "49" },
		{ "C_FS", "49" }
	};

	public static readonly Dictionary<string, string> WordMemoryAreaCode = new Dictionary<string, string>
	{
		{ "SYS", "SYS" },
		{ "CIO", "B0" },
		{ "W", "B1" },
		{ "H", "B2" },
		{ "A", "B3" },
		{ "D", "82" },
		{ "E", "A0" },
		{ "TK", "46" },
		{ "CIO_FS", "F0" },
		{ "W_FS", "F1" },
		{ "H_FS", "F2" },
		{ "T", "89" },
		{ "C", "89" },
		{ "IR", "DC" },
		{ "DR", "BC" }
	};

	private string RWT { get; set; } = "0";


	private string ICF { get; set; } = "00";


	private string RSV { get; set; } = "00";


	private string GCT { get; set; } = "00";


	private string DNA { get; set; } = "00";


	private string DA1 { get; set; } = "00";


	private string DA2 { get; set; } = "00";


	private string SNA { get; set; } = "00";


	private string SA1 { get; set; } = "00";


	private string SA2 { get; set; } = "00";


	private string SID { get; set; } = "00";


	public string ReadMsg(int unitNo, string memoryAreaCode, string addressArray)
	{
		string text = "@";
		text += unitNo.ToString("D2");
		text += "FA";
		text += RWT;
		text += ICF;
		text += DA2;
		text += SA2;
		text += SID;
		text += "0104";
		text += memoryAreaCode;
		text += addressArray;
		text += FCS(text);
		return text + "*\r";
	}

	public string ReadMsg(int unitNo, string memoryAreaCode, int wordAddress, int bitAddress, int numOfElements)
	{
		string text = "@";
		text += unitNo.ToString("D2");
		text += "FA";
		text += RWT;
		text += ICF;
		text += DA2;
		text += SA2;
		text += SID;
		text += "0101";
		text += memoryAreaCode;
		text += wordAddress.ToString("X4");
		text += bitAddress.ToString("X2");
		text += numOfElements.ToString("X4");
		text += FCS(text);
		return text + "*\r";
	}

	public string WriteMsg(int unitNo, string memoryAreaCode, int wordAddress, int bitAddress, int numOfElements, string values)
	{
		string text = "@";
		text += unitNo.ToString("D2");
		text += "FA";
		text += RWT;
		text += ICF;
		text += DA2;
		text += SA2;
		text += SID;
		text += "0102";
		text += memoryAreaCode;
		text += wordAddress.ToString("X4");
		text += bitAddress.ToString("X2");
		text += numOfElements.ToString("X4");
		text += values;
		text += FCS(text);
		return text + "*\r";
	}

	public string OperationModeMsg(int unitNo, Mode mode)
	{
		string text = "@";
		text += unitNo.ToString("D2");
		text += "FA";
		text += RWT;
		text += ICF;
		text += DA2;
		text += SA2;
		text += SID;
		switch (mode)
		{
		case Mode.PROGRAM:
			text += "0402";
			text += "FFFF";
			break;
		case Mode.RUN:
			text += "0401";
			text += "FFFF";
			text += "04";
			break;
		case Mode.MONITOR:
			text += "0401";
			text += "FFFF";
			text += "02";
			break;
		}
		text += FCS(text);
		return text + "*\r";
	}

	public string ReadOperationModeMsg(int unitNo)
	{
		string text = "@";
		text += unitNo.ToString("D2");
		text += "FA";
		text += RWT;
		text += ICF;
		text += DA2;
		text += SA2;
		text += SID;
		text += "0601";
		text += FCS(text);
		return text + "*\r";
	}
}
