using System;
using System.Collections.Generic;
using System.Linq;
using NetStudio.Omron.Models;

namespace NetStudio.Omron.Fins;

public class FinsBuilder : BaseBuilder
{
	public static class FINSCommand
	{
		public static readonly byte[] MEMORY_AREA_READ = new byte[2] { 1, 1 };

		public static readonly byte[] MEMORY_AREA_WRITE = new byte[2] { 1, 2 };

		public static readonly byte[] MEMORY_AREA_FILL = new byte[2] { 1, 3 };

		public static readonly byte[] MULTIPLE_MEMORY_AREA_READ = new byte[2] { 1, 4 };

		public static readonly byte[] MEMORY_AREA_TRANSFER = new byte[2] { 1, 5 };

		public static readonly byte[] RUN_MODE = new byte[2] { 4, 1 };

		public static readonly byte[] STOP_MODE = new byte[2] { 4, 2 };

		public static readonly byte[] READ_CPU_STATUS = new byte[2] { 6, 1 };
	}

	protected byte HEADER_CODE = 15;

	public static readonly Dictionary<string, byte> BitMemoryAreaCode = new Dictionary<string, byte>
	{
		{ "CIO", 48 },
		{ "W", 49 },
		{ "H", 50 },
		{ "A", 51 },
		{ "D", 2 },
		{ "E", 32 },
		{ "TK", 6 },
		{ "CIO_FS", 112 },
		{ "W_FS", 113 },
		{ "H_FS", 114 },
		{ "T", 9 },
		{ "C", 9 },
		{ "T_FS", 73 },
		{ "C_FS", 73 }
	};

	public static readonly Dictionary<string, byte> WordMemoryAreaCode = new Dictionary<string, byte>
	{
		{ "CIO", 176 },
		{ "W", 177 },
		{ "H", 178 },
		{ "A", 179 },
		{ "D", 130 },
		{ "E", 160 },
		{ "TK", 70 },
		{ "CIO_FS", 240 },
		{ "W_FS", 241 },
		{ "H_FS", 242 },
		{ "T", 137 },
		{ "C", 137 },
		{ "IR", 220 },
		{ "DR", 188 }
	};

	public byte[] FS_HEADER = new byte[20]
	{
		70, 73, 78, 83, 0, 0, 0, 12, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0
	};

	protected byte RWT { get; set; }

	protected byte ICF { get; set; } = 128;


	protected byte RSV { get; set; }

	protected byte GCT { get; set; } = 2;


	protected byte DNA { get; set; }

	protected byte DA1 { get; set; } = 1;


	protected byte DA2 { get; set; }

	private byte SNA { get; set; }

	protected byte SA1 { get; set; } = 1;


	protected byte SA2 { get; set; }

	protected byte SID { get; set; }

	public byte[] OnInitializeTcpMsg(byte[] message)
	{
		List<byte> list = new List<byte>();
		list.AddRange(new byte[4] { 70, 73, 78, 83 });
		uint value = (uint)(8 + message.Length);
		list.AddRange(BitConverter.GetBytes(value).Reverse());
		list.AddRange(new byte[4]);
		list.AddRange(new byte[4]);
		list.AddRange(message);
		return list.ToArray();
	}

	public byte[] ReadTcpMsg(byte memoryAreaCode, int wordAddress, int bitAddress, int numOfElements)
	{
		List<byte> list = new List<byte>();
		list.AddRange(new byte[4] { 70, 73, 78, 83 });
		list.AddRange(BitConverter.GetBytes(26u).Reverse());
		list.AddRange(new byte[4] { 0, 0, 0, 2 });
		list.AddRange(new byte[4]);
		list.Add(128);
		list.Add(RSV);
		list.Add(2);
		list.Add(DNA);
		list.Add(DA1);
		list.Add(DA2);
		list.Add(SNA);
		list.Add(SA1);
		list.Add(SA2);
		list.Add(SID);
		list.AddRange(FINSCommand.MEMORY_AREA_READ);
		list.Add(memoryAreaCode);
		list.Add((byte)(wordAddress >> 8));
		list.Add((byte)wordAddress);
		list.Add((byte)bitAddress);
		list.Add((byte)(numOfElements >> 8));
		list.Add((byte)numOfElements);
		return list.ToArray();
	}

	public byte[] WriteTcpMsg(byte memoryAreaCode, int wordAddress, int bitAddress, int numOfElements, byte[] values)
	{
		List<byte> list = new List<byte>();
		list.AddRange(new byte[4] { 70, 73, 78, 83 });
		uint value = (uint)(26 + values.Length);
		list.AddRange(BitConverter.GetBytes(value).Reverse());
		list.AddRange(new byte[4] { 0, 0, 0, 2 });
		list.AddRange(new byte[4]);
		list.Add(ICF);
		list.Add(RSV);
		list.Add(GCT);
		list.Add(DNA);
		list.Add(DA1);
		list.Add(DA2);
		list.Add(SNA);
		list.Add(SA1);
		list.Add(SA2);
		list.Add(SID);
		list.AddRange(FINSCommand.MEMORY_AREA_WRITE);
		list.Add(memoryAreaCode);
		list.Add((byte)(wordAddress >> 8));
		list.Add((byte)wordAddress);
		list.Add((byte)bitAddress);
		list.Add((byte)(numOfElements >> 8));
		list.Add((byte)numOfElements);
		list.AddRange(values);
		return list.ToArray();
	}

	public byte[] ReadUdpMsg(byte memoryAreaCode, int wordAddress, int bitAddress, int numOfElements)
	{
		List<byte> list = new List<byte>();
		list.Add(128);
		list.Add(RSV);
		list.Add(2);
		list.Add(DNA);
		list.Add(DA1);
		list.Add(DA2);
		list.Add(SNA);
		list.Add(SA1);
		list.Add(SA2);
		list.Add(SID);
		list.AddRange(FINSCommand.MEMORY_AREA_READ);
		list.Add(memoryAreaCode);
		list.Add((byte)(wordAddress >> 8));
		list.Add((byte)wordAddress);
		list.Add((byte)bitAddress);
		list.Add((byte)(numOfElements >> 8));
		list.Add((byte)numOfElements);
		return list.ToArray();
	}

	public byte[] WriteUdpMsg(byte memoryAreaCode, int wordAddress, int bitAddress, int numOfElements, byte[] values)
	{
		List<byte> list = new List<byte>();
		list.Add(ICF);
		list.Add(RSV);
		list.Add(GCT);
		list.Add(DNA);
		list.Add(DA1);
		list.Add(DA2);
		list.Add(SNA);
		list.Add(SA1);
		list.Add(SA2);
		list.Add(SID);
		list.AddRange(FINSCommand.MEMORY_AREA_WRITE);
		list.Add(memoryAreaCode);
		list.Add((byte)(wordAddress >> 8));
		list.Add((byte)wordAddress);
		list.Add((byte)bitAddress);
		list.Add((byte)(numOfElements >> 8));
		list.Add((byte)numOfElements);
		list.AddRange(values);
		return list.ToArray();
	}

	public string OperationModeMsg(int unitNo, Mode mode)
	{
		string text = "@";
		text += unitNo.ToString("D2");
		text += HEADER_CODE;
		text += RWT;
		text += ICF;
		text += DA2;
		text += SA2;
		text += SID;
		switch (mode)
		{
		case Mode.PROGRAM:
			text += FINSCommand.STOP_MODE;
			text += "FFFF";
			break;
		case Mode.RUN:
			text += FINSCommand.RUN_MODE;
			text += "FFFF";
			text += "04";
			break;
		case Mode.MONITOR:
			text += FINSCommand.RUN_MODE;
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
		text += HEADER_CODE;
		text += RWT;
		text += ICF;
		text += DA2;
		text += SA2;
		text += SID;
		text += FINSCommand.READ_CPU_STATUS;
		text += FCS(text);
		return text + "*\r";
	}
}
