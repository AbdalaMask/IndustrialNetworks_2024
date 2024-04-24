using System;
using System.Collections.Generic;
using System.Linq;
using NetStudio.Common.DataTypes;

namespace NetStudio.LS.Xgt.FEnet;

public class XgtBuilder
{
	private const ushort NumberOfBlocks = 1;

	private readonly byte[] LSIS_XGT = new byte[10] { 76, 83, 73, 83, 45, 88, 71, 84, 0, 0 };

	private readonly byte[] LGIS_GLOFA = new byte[10] { 76, 71, 73, 83, 45, 71, 76, 79, 70, 65 };

	private readonly byte[] MASTER_K = new byte[10] { 77, 65, 83, 84, 69, 82, 45, 75, 0, 0 };

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


	public byte[] ReadDirectVariableIndividually(ReadPacket RP)
	{
		ushort num = (ushort)RP.Address.Length;
		byte[] bytes = BitConverter.GetBytes(num);
		byte[] bytes2 = BitConverter.GetBytes((ushort)(10 + num));
		List<byte> list = new List<byte>();
		switch (RP.CompanyID)
		{
		default:
			list.AddRange(LSIS_XGT);
			break;
		case CompanyID.LGIS_GLOFA:
			list.AddRange(LGIS_GLOFA);
			break;
		case CompanyID.MASTER_K:
			list.AddRange(MASTER_K);
			break;
		}
		list.AddRange(LSIS_XGT);
		list.AddRange(new byte[2]);
		list.Add((byte)RP.CPUInfo);
		list.Add(51);
		list.AddRange(new byte[2] { 0, 1 });
		list.AddRange(new byte[2]
		{
			bytes2[0],
			bytes2[1]
		});
		list.Add(RP.FEnetPosition);
		list.Add(0);
		list.AddRange(Command.Read);
		list.AddRange(FEDataType.Continuous);
		list.AddRange(new byte[2]);
		list.AddRange(new byte[2] { 1, 0 });
		list.AddRange(new byte[2]
		{
			bytes[0],
			bytes[1]
		});
		list.AddRange(RP.Address.Select((char char_0) => (byte)char_0));
		return list.ToArray();
	}

	public byte[] WritingDirectVariableIndividually(WritePacket WP)
	{
		ushort num = (ushort)WP.Address.Length;
		byte[] bytes = BitConverter.GetBytes(num);
		byte[] bytes2 = BitConverter.GetBytes((ushort)(12 + num + WP.Values.Length));
		byte[] bytes3 = BitConverter.GetBytes((ushort)WP.Values.Length);
		List<byte> list = new List<byte>();
		switch (WP.CompanyID)
		{
		default:
			list.AddRange(LSIS_XGT);
			break;
		case CompanyID.LGIS_GLOFA:
			list.AddRange(LGIS_GLOFA);
			break;
		case CompanyID.MASTER_K:
			list.AddRange(MASTER_K);
			break;
		}
		list.AddRange(new byte[2]);
		list.Add((byte)WP.CPUInfo);
		list.Add(51);
		list.AddRange(new byte[2]);
		list.AddRange(new byte[2]
		{
			bytes2[0],
			bytes2[1]
		});
		list.Add(WP.FEnetPosition);
		list.Add(0);
		list.AddRange(Command.Write);
		list.AddRange(WP.DataType);
		list.AddRange(new byte[2]);
		list.AddRange(new byte[2] { 1, 0 });
		list.AddRange(new byte[2]
		{
			bytes[0],
			bytes[1]
		});
		list.AddRange(WP.Address.Select((char char_0) => (byte)char_0));
		list.AddRange(new byte[2]
		{
			bytes3[0],
			bytes3[1]
		});
		list.AddRange(WP.Values);
		return list.ToArray();
	}

	public byte[] ReadingDirecVariableContinuously(ReadPacket RP)
	{
		ushort num = (ushort)RP.Address.Length;
		byte[] bytes = BitConverter.GetBytes(num);
		byte[] bytes2 = BitConverter.GetBytes((ushort)(12 + num));
		byte[] bytes3 = BitConverter.GetBytes((ushort)(2 * RP.Quantity));
		List<byte> list = new List<byte>();
		switch (RP.CompanyID)
		{
		default:
			list.AddRange(LSIS_XGT);
			break;
		case CompanyID.LGIS_GLOFA:
			list.AddRange(LGIS_GLOFA);
			break;
		case CompanyID.MASTER_K:
			list.AddRange(MASTER_K);
			break;
		}
		list.AddRange(new byte[2]);
		list.Add((byte)RP.CPUInfo);
		list.Add(51);
		list.AddRange(new byte[2]);
		list.AddRange(new byte[2]
		{
			bytes2[0],
			bytes2[1]
		});
		list.Add(RP.FEnetPosition);
		list.Add(0);
		list.AddRange(Command.Read);
		list.AddRange(RP.DataType);
		list.AddRange(new byte[2]);
		list.AddRange(new byte[2] { 1, 0 });
		list.AddRange(new byte[2]
		{
			bytes[0],
			bytes[1]
		});
		list.AddRange(RP.Address.Select((char char_0) => (byte)char_0));
		list.AddRange(new byte[2]
		{
			bytes3[0],
			bytes3[1]
		});
		return list.ToArray();
	}

	public byte[] WritingTheDirectVariableContinuously(WritePacket WP)
	{
		ushort num = (ushort)WP.Address.Length;
		byte[] bytes = BitConverter.GetBytes(num);
		byte[] bytes2 = BitConverter.GetBytes((ushort)(12 + num + WP.Values.Length));
		byte[] bytes3 = BitConverter.GetBytes((ushort)WP.Values.Length);
		List<byte> list = new List<byte>();
		switch (WP.CompanyID)
		{
		default:
			list.AddRange(LSIS_XGT);
			break;
		case CompanyID.LGIS_GLOFA:
			list.AddRange(LGIS_GLOFA);
			break;
		case CompanyID.MASTER_K:
			list.AddRange(MASTER_K);
			break;
		}
		list.AddRange(new byte[2]);
		list.Add((byte)WP.CPUInfo);
		list.Add(51);
		list.AddRange(new byte[2]);
		list.AddRange(new byte[2]
		{
			bytes2[0],
			bytes2[1]
		});
		list.Add(WP.FEnetPosition);
		list.Add(0);
		list.AddRange(Command.Write);
		list.AddRange(WP.DataType);
		list.AddRange(new byte[2]);
		list.AddRange(new byte[2] { 1, 0 });
		list.AddRange(new byte[2]
		{
			bytes[0],
			bytes[1]
		});
		list.AddRange(WP.Address.Select((char char_0) => (byte)char_0));
		list.AddRange(new byte[2]
		{
			bytes3[0],
			bytes3[1]
		});
		list.AddRange(WP.Values);
		return list.ToArray();
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
