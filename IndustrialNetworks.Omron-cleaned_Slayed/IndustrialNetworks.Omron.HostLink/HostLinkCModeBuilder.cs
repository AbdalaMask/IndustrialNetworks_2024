using System.Collections.Generic;

namespace NetStudio.Omron.HostLink;

public class HostLinkCModeBuilder : BaseBuilder
{
	public static readonly Dictionary<string, string> HeaderCodesForRead = new Dictionary<string, string>
	{
		{ "I", "RR" },
		{ "Q", "RR" },
		{ "A", "RJ" },
		{ "T", "RC" },
		{ "C", "RC" },
		{ "D", "RD" },
		{ "E", "RE" },
		{ "H", "RH" },
		{ "W", "W" },
		{ "DR", "DR" },
		{ "IR", "IR" },
		{ "TK", "TK" },
		{ "TR", "TR" }
	};

	public static readonly Dictionary<string, string> HeaderCodesForWrite = new Dictionary<string, string>
	{
		{ "I", "WR" },
		{ "Q", "WR" },
		{ "A", "WJ" },
		{ "T", "WC" },
		{ "C", "WC" },
		{ "D", "WD" },
		{ "E", "WE" },
		{ "H", "WH" }
	};

	public static readonly Dictionary<string, string> AreaClassifications = new Dictionary<string, string>
	{
		{ "I", "CIO " },
		{ "Q", "CIO " },
		{ "L", "LR  " },
		{ "W", "WR  " },
		{ "H", "HR  " },
		{ "T", "TIM " },
		{ "C", "CNT " }
	};

	public string ReadMsg(int unitNo, string header, string text = "")
	{
		string text2 = "@";
		text2 += unitNo.ToString("D2");
		text2 += header;
		text2 += text;
		text2 += FCS(text2);
		return text2 + "*\r";
	}

	public string WriteMsg(int unitNo, string header, string text = "")
	{
		string text2 = "@";
		text2 += unitNo.ToString("D2");
		text2 += header;
		text2 += text;
		text2 += FCS(text2);
		return text2 + "*\r";
	}

	public string ForceSetMsg(int unitNo, string header, string classification, string text)
	{
		string text2 = "@";
		text2 += unitNo.ToString("D2");
		text2 += header;
		text2 += classification;
		text2 += text;
		text2 += FCS(text2);
		return text2 + "*\r";
	}

	public string GetModelMsg(string modelCode)
	{
		return modelCode switch
		{
			"0A" => "C500F", 
			"0B" => "C120F", 
			"0E" => "C2000", 
			"09" => "C250F", 
			"40" => "CVM1-CPU01-E", 
			"30" => "CS/CJ", 
			"20" => "CV500", 
			"10" => "C1000H", 
			"41" => "CVM1-CPU11-E", 
			"21" => "CV1000", 
			"11" => "C2000H/CQM1/CPM1", 
			"01" => "C250", 
			"42" => "CVM1-CPU21-E", 
			"22" => "CV2000", 
			"12" => "C20H/C28H/C40H, C200H, C200HS, C200HX/HG/HE (-ZE)", 
			"02" => "C500", 
			"03" => "C120/C50", 
			_ => "Unknow", 
		};
	}
}
