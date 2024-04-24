using System;
using System.Collections.Generic;
using NetStudio.Common;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;

namespace NetStudio.LS.Xgt.Cnet;

public class XgtBuilder
{
	private const ushort NumberOfBlocks = 1;

	public static readonly Dictionary<STRING, STRING> Errors = new Dictionary<STRING, STRING>
	{
		{ "ReadDataFailed", "Read data failed" },
		{ "0003", "Number Of Blocks Exceeded: Number of blocks exceeds 16 at Individual Read/Write Request." },
		{ "0004", "Variable Length Error: Variable Length exceeds the max. size of 16." },
		{ "0007", "DataType Error: Other data type than X,B,W,D,L received." },
		{ "0011", "Data Error: Data length area information incorrect. In case % is unavailable to start with. Variable’s area value wrong. Other value is written for Bit Write than 00 or 01." },
		{ "0090", "Monitor Execution Error: Unregistered monitor execution requested." },
		{ "0190", "Monitor Execution Error: Reg. No. range exceeded." },
		{ "0290", "Monitor Register Error: Reg. No. range exceeded." },
		{ "1132", "Device Memory Error: Other letter than applicable device is input." },
		{ "1232", "Data Size Error: Request exceeds the max range of 60 Words to read or write at a time." },
		{ "1983", "Data Size Error: Request exceeds the max range of 1024 data to read or write at a time." },
		{ "1234", "Extra Frame Error:  Unnecessary details exist as added." },
		{ "1332", "DataType Discordant: All the blocks shall be requested of the identical data type in the case of Individual Read/Write." },
		{ "1432", "Data Value Error: Data value unavailable to convert to Hex." },
		{ "7132", "Variable Request Area Exceeded: Request exceeds the area each device supports." }
	};

	public ByteOrder ByteOrder { get; set; } = ByteOrder.LittleEndian;


	public string ReadDirectVariableIndividually(ReadPacket  RP)
	{
		string text = $"{5}";
		text += RP.StationNo.ToString("X2");
		text += "r";
		text += "SS";
		text += ((ushort)1).ToString("X2");
		text += ((ushort)RP.Address.Length).ToString("X2");
		text += RP.Address;
		text += $"{4}";
		return text + CalculateBCC(text);
	}

	public string WritingDirectVariableIndividually(WritePacket WP)
	{
		string text = $"{5}";
		text += WP.StationNo.ToString("X2");
		text += "w";
		text += "SS";
		text += ((ushort)1).ToString("X2");
		text += ((ushort)WP.Address.Length).ToString("X2");
		text += WP.Address;
		text += WP.ValueHex;
		text += $"{4}";
		return text + CalculateBCC(text);
	}

	public string ReadingDirecVariableContinuously(ReadPacket RP)
	{
		string text = $"{5}";
		text += RP.StationNo.ToString("X2");
		text += "r";
		text += "SB";
		text += ((ushort)RP.Address.Length).ToString("X2");
		text += RP.Address;
		text += RP.Quantity.ToString("X2");
		text += $"{4}";
		return text + CheckSum(text);
	}

	public string WritingTheDirectVariableContinuously(WritePacket WP)
	{
		if (!WP.Address.StartsWith("%"))
		{
			WP.Address = "%" + WP.Address;
		}
		string text = $"{5}";
		text += WP.StationNo.ToString("X2");
		text += "w";
		text += "SB";
		text += ((ushort)WP.Address.Length).ToString("X2");
		text += WP.Address;
		text += WP.Quantity.ToString("X2");
		text += WP.ValueHex;
		text += $"{4}";
		return text + CheckSum(text);
	}

	protected byte[] CalculateBCC(List<byte> frame)
	{
	 
		uint num = 0u;
		foreach (byte item in frame)
		{
			num += item;
		}
		string text = num.ToString("X2").Right(2);
		return new byte[2]
		{
			(byte)text[0],
			(byte)text[1]
		};
	}

	protected string CalculateBCC(string frame)
	{
	 
		uint num = 0u;
		for (int i = 0; i < frame.Length; i++)
		{
			byte b = (byte)frame[i];
			num += b;
		}
		string text = num.ToString("X");
		return text.Substring(text.Length - 2);
	}

	protected string CheckSum(string frame)
	{
		 
		uint num = 0u;
		foreach (char c in frame)
		{
			num = (num + (byte)c) % 256;
		}
		return num.ToString("X2");
	}

	protected UINT GetAddressByDeviceName(string deviceName, bool addressIsHexa)
	{
		string text = deviceName.Substring(0, 1);
		string value = deviceName.Substring(text.Length, deviceName.Length - text.Length);
		if (!addressIsHexa)
		{
			return UINT.Parse(value, ByteOrder, TypeStyles.Decimal);
		}
		return UINT.Parse(value, ByteOrder);
	}
}
