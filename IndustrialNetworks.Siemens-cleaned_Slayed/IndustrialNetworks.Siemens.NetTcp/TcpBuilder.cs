using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetStudio.Common.DataTypes;
using NetStudio.Siemens.Models;

namespace NetStudio.Siemens.NetTcp;

internal class TcpBuilder
{
	public static readonly byte FUNC_READ = 4;

	public static readonly byte FUNC_WRITE = 5;

	public static readonly byte[] TPKT = new byte[4] { 3, 0, 0, 0 };

	public static readonly byte[] ISO_COTP = new byte[4] { 3, 0, 0, 22 };

	public static readonly byte ConnectRequest = 244;

	public static readonly byte[] DestinationReference = new byte[2];

	public static readonly byte[] SourceReference = new byte[2] { 0, 46 };

	public static readonly byte TPDU = 192;

	public static readonly byte SourceTasp = 193;

	public static readonly byte DestinationTasp = 194;

	private byte[] IsoOverTcp { get; } = new byte[7] { 3, 0, 0, 0, 2, 240, 128 };


	private byte[] Header { get; } = new byte[10] { 50, 1, 0, 0, 0, 0, 0, 8, 0, 0 };


	public static byte[] GetOperationModeMessage(OperationMode mode)
	{
		List<byte> list = new List<byte>(new byte[7] { 3, 0, 0, 0, 2, 240, 128 });
		switch (mode)
		{
		default:
			throw new NotSupportedException($"{mode}: This mode of operation is not supported");
		case OperationMode.START:
			list.AddRange(new byte[21]
			{
				50, 1, 0, 0, 3, 0, 0, 20, 0, 0,
				40, 0, 0, 0, 0, 0, 0, 253, 0, 0,
				9
			});
			break;
		case OperationMode.STOP:
			list.AddRange(new byte[17]
			{
				50, 1, 0, 0, 3, 0, 0, 16, 0, 0,
				41, 0, 0, 0, 0, 0, 9
			});
			break;
		case OperationMode.RUN:
			list.AddRange(new byte[26]
			{
				50, 7, 0, 0, 97, 0, 0, 8, 0, 8,
				0, 1, 18, 4, 17, 68, 1, 0, 255, 9,
				0, 4, 4, 36, 0, 0
			});
			break;
		}
		list.AddRange(Encoding.Default.GetBytes("P_PROGRAM"));
		byte[] array = UINT.ToBytes((ushort)list.Count);
		list[2] = array[0];
		list[3] = array[1];
		return list.ToArray();
	}

	public static byte[] GetConnectionRequestMessage(Tsap tsap_src, Tsap tsap_des)
	{
		byte[] obj = new byte[22]
		{
			3, 0, 0, 22, 17, 224, 0, 0, 0, 46,
			0, 192, 1, 10, 192, 2, 0, 0, 0, 2,
			0, 0
		};
		obj[16] = tsap_src.FirstByte;
		obj[17] = tsap_src.SecondByte;
		obj[18] = DestinationTasp;
		obj[20] = tsap_des.FirstByte;
		obj[21] = tsap_des.SecondByte;
		return obj;
	}

	public byte[] GetConnectionSetupMessage()
	{
		return new byte[25]
		{
			3, 0, 0, 25, 2, 240, 128, 50, 1, 0,
			0, 0, 0, 0, 8, 0, 0, 240, 0, 0,
			16, 0, 16, 3, 192
		};
	}

	public byte[] ReadDataMessage(ReadPacket RP)
	{
		byte[] obj = new byte[19]
		{
			3, 0, 0, 0, 2, 240, 128, 50, 1, 0,
			0, 2, 0, 0, 0, 0, 0, 4, 0
		};
		obj[14] = Convert.ToByte(14);
		obj[18] = 1;
		List<byte> list = new List<byte>(obj);
		list.AddRange(new byte[4] { 18, 10, 16, 2 });
		byte[] collection = S7Utility.Sort(BitConverter.GetBytes((ushort)RP.Quantity));
		list.AddRange(collection);
		byte[] collection2 = S7Utility.Sort(BitConverter.GetBytes((ushort)RP.DBNumber));
		list.AddRange(collection2);
		list.Add((byte)RP.Memory);
		int byteAddress = S7Utility.GetByteAddress(RP.Address);
		byte[] array = S7Utility.Sort(BitConverter.GetBytes(8 * byteAddress));
		for (int i = 1; i < 4; i++)
		{
			list.Add(array[i]);
		}
		byte[] array2 = S7Utility.Sort(BitConverter.GetBytes((ushort)list.Count));
		list[2] = array2[0];
		list[3] = array2[1];
		return list.ToArray();
	}

	public byte[] WriteDataMessage(WritePacket WP)
	{
		byte[] array = S7Utility.Sort(BitConverter.GetBytes((ushort)(WP.Data.Length + 4)));
		byte[] obj = new byte[19]
		{
			3, 0, 0, 0, 2, 240, 128, 50, 1, 0,
			0, 2, 0, 0, 14, 0, 0, 5, 0
		};
		obj[15] = array[0];
		obj[16] = array[1];
		obj[18] = 1;
		List<byte> list = new List<byte>(obj);
		list.AddRange(new byte[4] { 18, 10, 16, 2 });
		byte[] collection = S7Utility.Sort(BitConverter.GetBytes((ushort)WP.Data.Length));
		list.AddRange(collection);
		byte[] collection2 = S7Utility.Sort(BitConverter.GetBytes((ushort)WP.DBNumber));
		list.AddRange(collection2);
		list.Add((byte)WP.Memory);
		int byteAddress = S7Utility.GetByteAddress(WP.Address);
		byte[] array2 = S7Utility.Sort(BitConverter.GetBytes(8 * byteAddress));
		for (int i = 1; i < 4; i++)
		{
			list.Add(array2[i]);
		}
		list.AddRange(new byte[2] { 0, 4 });
		WP.Quantity = WP.Data.Length;
		byte[] collection3 = S7Utility.Sort(BitConverter.GetBytes((ushort)(8 * WP.Quantity)));
		list.AddRange(collection3);
		list.AddRange(WP.Data);
		byte[] array3 = S7Utility.Sort(BitConverter.GetBytes((ushort)list.Count));
		list[2] = array3[0];
		list[3] = array3[1];
		string.Join(" ", list.Select((byte byte_0) => byte_0));
		return list.ToArray();
	}

	public byte[] WriteBitMessage(WritePacket WP)
	{
		byte b = (byte)WP.Memory;
		int bitAddress = S7Utility.GetBitAddress(WP.Address);
		byte[] array = S7Utility.Sort(BitConverter.GetBytes((ushort)WP.DBNumber));
		byte[] array2 = S7Utility.Sort(BitConverter.GetBytes(bitAddress));
		byte[] array3 = S7Utility.Sort(BitConverter.GetBytes((ushort)WP.Data.Length));
		byte[] array4 = S7Utility.Sort(BitConverter.GetBytes((ushort)(WP.Data.Length + 4)));
		byte[] obj = new byte[36]
		{
			3, 0, 0, 0, 2, 240, 128, 50, 1, 0,
			0, 2, 0, 0, 14, 0, 0, 5, 1, 18,
			10, 16, 1, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 3, 0, 0, 0
		};
		obj[15] = array4[0];
		obj[16] = array4[1];
		obj[23] = array3[0];
		obj[24] = array3[1];
		obj[25] = array[0];
		obj[26] = array[1];
		obj[27] = b;
		obj[28] = array2[1];
		obj[29] = array2[2];
		obj[30] = array2[3];
		obj[33] = array3[0];
		obj[34] = array3[1];
		obj[35] = WP.Data[0];
		List<byte> list = new List<byte>(obj);
		byte[] array5 = S7Utility.Sort(BitConverter.GetBytes((ushort)list.Count));
		list[2] = array5[0];
		list[3] = array5[1];
		return list.ToArray();
	}

	public byte[] WriteDataMessage(WritePacket WP, byte[] data)
	{
		byte[] array = S7Utility.Sort(BitConverter.GetBytes((ushort)WP.DBNumber));
		byte b = (byte)WP.Memory;
		int byteAddress = S7Utility.GetByteAddress(WP.Address);
		byte[] array2 = S7Utility.Sort(BitConverter.GetBytes(8 * byteAddress));
		byte[] array3 = S7Utility.Sort(BitConverter.GetBytes((ushort)(8 * WP.Data.Length)));
		byte[] array4 = S7Utility.Sort(BitConverter.GetBytes((ushort)WP.Data.Length));
		byte[] array5 = S7Utility.Sort(BitConverter.GetBytes((ushort)(WP.Data.Length + 4)));
		byte[] obj = new byte[35]
		{
			3, 0, 0, 0, 2, 240, 128, 50, 1, 0,
			0, 2, 0, 0, 14, 0, 0, 5, 1, 18,
			10, 16, 2, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 4, 0, 0
		};
		obj[15] = array5[0];
		obj[16] = array5[1];
		obj[23] = array4[0];
		obj[24] = array4[1];
		obj[25] = array[0];
		obj[26] = array[1];
		obj[27] = b;
		obj[28] = array2[1];
		obj[29] = array2[2];
		obj[30] = array2[3];
		obj[33] = array3[0];
		obj[34] = array3[1];
		List<byte> list = new List<byte>(obj);
		list.AddRange(WP.Data);
		byte[] array6 = S7Utility.Sort(BitConverter.GetBytes((ushort)list.Count));
		list[2] = array6[0];
		list[3] = array6[1];
		string.Join(" ", list.Select((byte byte_0) => byte_0));
		return list.ToArray();
	}

	public static byte[] GetResetAllMessage()
	{
		return new List<byte>(new byte[50]
		{
			50, 7, 0, 0, 4, 0, 0, 12, 0, 28,
			0, 1, 18, 8, 18, 65, 12, 0, 0, 0,
			0, 0, 255, 9, 0, 24, 0, 20, 0, 0,
			0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
			0, 1, 0, 1, 0, 1, 0, 1, 0, 0
		}).ToArray();
	}

	public static byte[] GetReadClockMessage()
	{
		return new List<byte>(new byte[22]
		{
			50, 7, 0, 0, 13, 0, 0, 8, 0, 4,
			0, 1, 18, 4, 17, 71, 1, 0, 10, 0,
			0, 0
		}).ToArray();
	}

	public static byte[] GetWriteClockMessage(DateTime dateTime)
	{
		byte[] collection = Conversion.ToBytes(dateTime);
		List<byte> list = new List<byte>(new byte[23]
		{
			50, 7, 0, 0, 12, 0, 0, 8, 0, 14,
			0, 1, 18, 4, 17, 71, 2, 0, 255, 9,
			0, 10, 0
		});
		list.AddRange(collection);
		return list.ToArray();
	}

	public string GetErrorMessage(byte[] exceptions)
	{
		string result = string.Empty;
		switch (exceptions[8])
		{
		case 1:
			result = GetErrorMessage(exceptions[9], exceptions[10]);
			break;
		case 2:
			result = GetErrorMessage(exceptions[17], exceptions[18]);
			break;
		case 3:
			result = GetErrorMessage(exceptions[17], exceptions[18]);
			break;
		case 7:
			result = GetErrorMessage(exceptions[9], exceptions[10]);
			break;
		}
		return result;
	}

	private string GetErrorMessage(byte byteHigh, byte byteLow)
	{
		int num = byteHigh * 256 + byteLow;
		string result = string.Empty;
		switch (num)
		{
		case 257:
			result = "Communication link not available";
			break;
		case 1:
			result = "Hardware fault";
			break;
		case 3:
			result = "Object access not allowed: occurs when access to timer and counter data type is set to signed integer and not BCD";
			break;
		case 4:
			result = "Not context";
			break;
		case 5:
			result = "Address out of range: occurs when requesting an address within a data block that does not exist or is out of range";
			break;
		case 6:
			result = "Address out of range";
			break;
		case 7:
			result = "Write data size mismatch";
			break;
		case 10:
			result = "Object does not exist: occurs when trying to request a data block that does not exist";
			break;
		case 268:
			result = "Data does not exist or is locked";
			break;
		case 266:
			result = "Negative acknowledge / time out error";
			break;
		case 770:
			result = "Block size is too small";
			break;
		case 512:
			result = "Unknown error";
			break;
		case 513:
			result = "Wrong interface specified";
			break;
		case 514:
			result = "Too many interfaces";
			break;
		case 515:
			result = "Interface already initialized";
			break;
		case 516:
			result = "Interface already initialized with another connection";
			break;
		case 517:
			result = "Interface not initialized; this may be due to an invalid MPI address (local or remote ID) or the PLC is not communicating on the MPI network";
			break;
		case 518:
			result = "Can't set handle";
			break;
		case 519:
			result = "Data segment isn’t locked";
			break;
		case 521:
			result = "Data field incorrect";
			break;
		case 787:
			result = "Wrong MPI baud rate selected";
			break;
		case 788:
			result = "Highest MPI address is wrong";
			break;
		case 789:
			result = "Address already exists";
			break;
		case 794:
			result = "Not connected to MPI network";
			break;
		case 795:
			result = "-";
			break;
		case 771:
			result = "Block boundary exceeded";
			break;
		case 897:
			result = "Hardware error";
			break;
		case 800:
			result = "Hardware error";
			break;
		case 32768:
			result = "Interface Is busy";
			break;
		case 16385:
			result = "Communication link unknown";
			break;
		case 16386:
			result = "Communication link not available";
			break;
		case 16387:
			result = "MPI communication in progress";
			break;
		case 16388:
			result = "MPI connection down; this may be due to an invalid MPI address (local or remote ID) or the PLC is not communicating on the MPI network";
			break;
		case 33025:
			result = "Hardware error";
			break;
		case 33027:
			result = "Access to object not permitted";
			break;
		case 33028:
			result = "Not context";
			break;
		case 33029:
			result = "Address invalid. This may be due to a memory address that is not valid for the PLC";
			break;
		case 33030:
			result = "Data type not supported";
			break;
		case 33031:
			result = "Data type not consistent";
			break;
		case 33034:
			result = "Object doesn’t exist. This may be due to a data block that doesn’t exist in the PLC";
			break;
		case 32769:
			result = "Not permitted in this mode";
			break;
		case 33794:
			result = "Maybe CPU already in RUN or already in STOP";
			break;
		case 33537:
			result = "Not enough memory on CPU";
			break;
		case 34048:
			result = "Wrong PDU (response data) size";
			break;
		case 33796:
			result = "Serious error";
			break;
		case 53250:
			result = "Step7: variant of command is illegal.";
			break;
		case 34562:
			result = "Not address";
			break;
		case 53409:
			result = "Step7: function is not allowed in the current protection level.";
			break;
		case 53252:
			result = "Step7: status for this command is illegal.";
			break;
		case 53824:
			result = "Coordination rules were violated";
			break;
		case 53825:
			result = "Protection level too low";
			break;
		case 53826:
			result = "Protection violation while processing F-blocks; F-blocks can only be processed after password input";
			break;
		case 53761:
			result = "Syntax error: block name";
			break;
		case 53762:
			result = "Syntax error: function parameter";
			break;
		case 53763:
			result = "Syntax error: block type";
			break;
		case 53764:
			result = "No linked data block in CPU";
			break;
		case 53765:
			result = "Object already exists";
			break;
		case 53766:
			result = "Object already exists";
			break;
		case 53767:
			result = "Data block in EPROM";
			break;
		case 53769:
			result = "Block doesn’t exist";
			break;
		case 53774:
			result = "No block available";
			break;
		case 53776:
			result = "Block number too large";
			break;
		case 54274:
			result = "Invalid SSL index";
			break;
		case 54273:
			result = "Invalid SSL ID";
			break;
		case 54281:
			result = "Diagnosis: DP Error";
			break;
		case 54278:
			result = "Information doesn’t exist";
			break;
		case 56321:
			result = "Maybe invalid BCD code or Invalid time format";
			break;
		case 55298:
			result = "This job does not exist";
			break;
		case 65535:
			result = "Timeout, check RS232 interface";
			break;
		case 65487:
			result = "API function called with an invalid parameter";
			break;
		case 61185:
			result = "Wrong ID2, cyclic job handle";
			break;
		}
		return result;
	}

	public string GetErrorMessage(byte byteLow)
	{
		return GetErrorMessage(0, byteLow);
	}

	private void GetRK512Errors(byte byteHigh, byte byteLow)
	{
		int num = byteHigh * 256 + byteLow;
		string message = $"UNKNOWN ERROR: {num}";
		switch (num)
		{
		case 12:
			message = "Incorrect status word:group message for all errors which can be attributed to the status word";
			break;
		case 10:
			message = "Source/destination type (e.g. DE) illegal Area (initial address, length) illegal (negative value) ";
			break;
		case 50:
			message = "DB/DX disabled on partner by coordination flag";
			break;
		case 52:
			message = "Partner detects incorrect message length (total length)";
			break;
		case 54:
			message = "Partner detects synchronization error: Message sequence is faulty";
			break;
		case 42:
			message = "No cold restart has yet been performed on the partner.";
			break;
		case 16:
			message = "Error in message header:1st or 4th command byte in header is incorrect";
			break;
		case 18:
			message = "Partner signals Job type illegal";
			break;
		case 20:
			message = "DB/DX does not exist or is illegal or DB/DX too short: (initialaddress + length)> area or";
			break;
		case 22:
			message = "Error in message header, detected by partner: 3rd command byte in header is incorrect";
			break;
		}
		if (byteHigh != 0 || byteLow != 0)
		{
			throw new InvalidOperationException(message);
		}
	}
}
