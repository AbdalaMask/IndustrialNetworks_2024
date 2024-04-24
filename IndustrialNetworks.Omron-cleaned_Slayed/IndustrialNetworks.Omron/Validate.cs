using System;
using System.Text;
using NetStudio.Common.DataTypes;

namespace NetStudio.Omron;

public class Validate
{
	public static void FinsTcp(byte code)
	{
		switch (code)
		{
		default:
			throw new Exception($"{code}: Unknown.");
		case 20:
			throw new Exception($"{code}: All Connections are in Use.");
		case 21:
			throw new Exception($"{code}: The Specified Node is already Connected.");
		case 22:
			throw new Exception($"{code}: Attempt to Access a Protected Node from an Unspecified IP Address.");
		case 23:
			throw new Exception($"{code}: The Client FINS Node Address is out of Range.");
		case 24:
			throw new Exception($"{code}: The same FINS Node Address is being used by the Client and Server.");
		case 25:
			throw new Exception($"{code}: All the Node Addresses Available for Allocation have been Used.");
		case 1:
			throw new Exception($"{code}: The FINS Identifier (ASCII Code) was Invalid.");
		case 2:
			throw new Exception($"{code}: The Data Length is too Long.");
		case 3:
			throw new Exception($"{code}: The Command is not Supported.");
		}
	}

	public void EndCode(string code)
	{
		if (code == null)
		{
			return;
		}
		int length = code.Length;
		if (length != 2)
		{
			return;
		}
		switch (code[1])
		{
		case '0':
			if (!(code == "20"))
			{
				_ = code == "00";
				break;
			}
			throw new Exception("Could not create I/O table.");
		case '1':
			if (!(code == "01"))
			{
				if (!(code == "21"))
				{
					break;
				}
				throw new Exception("Not executable due to CPU Unit CPU error.");
			}
			throw new Exception("Not executable in RUN mode.");
		case '2':
			if (!(code == "02"))
			{
				break;
			}
			throw new Exception("Not executable in MONITOR mode.");
		case '3':
			switch (code)
			{
			case "A3":
				throw new Exception("Aborted due to FCS error in transmission data.");
			case "23":
				throw new Exception("User memory protected.");
			case "13":
				throw new Exception("FCS error.");
			case "03":
				throw new Exception("UM write-protected.");
			}
			break;
		case '4':
			switch (code)
			{
			case "A4":
				throw new Exception("Aborted due to format error in transmission data.");
			case "14":
				throw new Exception("Format error.");
			case "04":
				throw new Exception("Address over.");
			}
			break;
		case '5':
			if (!(code == "15"))
			{
				if (!(code == "A5"))
				{
					break;
				}
				throw new Exception("Aborted due to entry number data\r\nerror in transmission data.");
			}
			throw new Exception("Entry number data error.");
		case '6':
			if (!(code == "16"))
			{
				break;
			}
			throw new Exception("Command not supported.");
		case '8':
			if (!(code == "18"))
			{
				if (!(code == "A8"))
				{
					break;
				}
				throw new Exception("Aborted due to frame length error in\r\ntransmission data.");
			}
			throw new Exception("Frame length error.");
		case '9':
			if (!(code == "19"))
			{
				break;
			}
			throw new Exception("Not executable.");
		case 'B':
			if (!(code == "0B"))
			{
				break;
			}
			throw new Exception("Not executable in PROGRAM mode.");
		case '7':
		case ':':
		case ';':
		case '<':
		case '=':
		case '>':
		case '?':
		case '@':
		case 'A':
			break;
		}
	}

	public string CpuUnitErrors(BOOL[] bcd_errors)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if ((bool)bcd_errors[5])
		{
			stringBuilder.AppendLine("Battery error (A40204).");
		}
		if ((bool)bcd_errors[6])
		{
			stringBuilder.AppendLine("Special I/O Unit error (OR of A40206 and A40207).");
		}
		if ((bool)bcd_errors[7])
		{
			stringBuilder.AppendLine("FAL generated (A40215).");
		}
		if ((bool)bcd_errors[8])
		{
			stringBuilder.AppendLine("Memory error (A40115).");
		}
		if ((bool)bcd_errors[10])
		{
			stringBuilder.AppendLine("I/O bus error (A40114).");
		}
		if ((bool)bcd_errors[14])
		{
			stringBuilder.AppendLine("No end instruction error (FALS) (A40109 Program error).");
		}
		if ((bool)bcd_errors[15])
		{
			stringBuilder.AppendLine("System error (FALS) (A40106)");
		}
		if ((bool)bcd_errors[27])
		{
			stringBuilder.AppendLine("I/O verify error (A40209).");
		}
		if ((bool)bcd_errors[28])
		{
			stringBuilder.AppendLine("Cycle time overrun (A40108).");
		}
		if ((bool)bcd_errors[29])
		{
			stringBuilder.AppendLine("Number duplication (A40113)");
		}
		if ((bool)bcd_errors[30])
		{
			stringBuilder.AppendLine("I/O setting error (A40110).");
		}
		if ((bool)bcd_errors[31])
		{
			stringBuilder.AppendLine("SYSMAC BUS error (A40205).");
		}
		return stringBuilder.ToString();
	}
}
