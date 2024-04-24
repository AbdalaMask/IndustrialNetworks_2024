using System;
using System.Collections.Generic;
using System.Text;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.Panasonic.Mewtocol.Codes;

namespace NetStudio.Panasonic.Mewtocol;

public class MessageBuilder
{
	public const int Resolution = 10;

	public const ByteOrder Number_Style = ByteOrder.LittleEndian;

	public const char StandardHeader = '%';

	public const char ExpandedHeader = '<';

	public const char Command = '#';

	public const char Normal_Response = '$';

	public const char Error_Response = '!';

	public const char Terminator = '\r';

	public const char Delimiter = '&';

	public const char Contact_ON = '1';

	public const char Contact_OFF = '0';

	public static Dictionary<string, string> Errors = new Dictionary<string, string>
	{
		{ "22", "WACK error." },
		{ "23", "MEWTOCOL station No. overlap." },
		{ "24", "ET-LAN unit hardware error." },
		{ "26", "MEWTOCOL station No. setting error." },
		{ "27", "No support error." },
		{ "28", "No response error." },
		{ "30", "Time-out error." },
		{ "32", "Transmission impossible error." },
		{ "33", "Communication stop" },
		{ "36", "No destination error." },
		{ "38", "Other communication errors." },
		{ "40", "BCC error." },
		{ "41", "Format error." },
		{ "42", "Format error." },
		{ "43", "Procedure error." },
		{ "50", "Link setting error." },
		{ "51", "Simultaneous operation error." },
		{ "52", "Transmit disable error." },
		{ "53", "Busy rrror." },
		{ "60", "Parameter error." },
		{ "61", "Data error" },
		{ "62", "Registration error." },
		{ "63", "Mode error." },
		{ "65", "Protect error." },
		{ "66", "Address error." },
		{ "67", "No data error." },
		{ "72", "Timeout error occurred while waiting for a transmission answer." },
		{ "73", "Timeout error occurred while waiting for the transmission buffer to become empty." },
		{ "74", "Timeout error occurred while waiting for a response." }
	};

	public string BCC(string IP)
	{
		 
		byte[] bytes = Encoding.ASCII.GetBytes(IP);
		int num = 0;
		for (int i = 0; i < bytes.Length; i++)
		{
			num ^= bytes[i];
		}
		return num.ToString("X2");
	}

	public string RemoteControlMessage(ushort stationNo, OperationCode operationCode)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{21}";
		text += $"{operationCode}";
		text += BCC(text);
		return text + "\r";
	}

	public string ReadPLCStatusMessage(ushort stationNo)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{20}";
		text += BCC(text);
		return text + "\r";
	}

	public string ReadContactMessage(ushort stationNo, string memory, string startAddress, int quantity)
	{
		string text = (int.Parse(startAddress) + quantity - 1).ToString("D4");
		string text2 = $"{60}";
		text2 += stationNo.ToString("X2");
		text2 += "#";
		text2 += $"{2}";
		text2 += memory;
		text2 += startAddress.PadLeft(4, '0');
		text2 += text;
		text2 += BCC(text2);
		return text2 + "\r";
	}

	public string WriteContactMessage(ushort stationNo, string memory, string address, bool value)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{4}";
		text += memory;
		text += address.PadLeft(4, '0');
		ReadOnlySpan<char> readOnlySpan = text;
		char reference = (value ? '1' : '0');
		text = string.Concat(readOnlySpan, new ReadOnlySpan<char>(ref reference));
		text += BCC(text);
		return text + "\r";
	}

	public string WriteContactMessage(ushort stationNo, string memory, string startAddres, string endAddress, string values_hex)
	{
		string text = $"{60}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{6}";
		text += memory;
		text += startAddres;
		text += endAddress;
		text += values_hex;
		text += BCC(text);
		return text + "\r";
	}

	public string ReadDataAreaMessage(ushort stationNo, string memory, string startAddress, int quantity)
	{
		string text = (int.Parse(startAddress) + quantity - 1).ToString("D5");
		string text2 = $"{60}";
		text2 += stationNo.ToString("X2");
		text2 += "#";
		text2 += $"{7}";
		text2 += memory;
		text2 += startAddress.PadLeft(5, '0');
		text2 += text;
		text2 += BCC(text2);
		return text2 + "\r";
	}

	public string WriteDataAreaMessage(ushort stationNo, string memory, string startAddress, string endAddress, string values_hex)
	{
		string text = $"{60}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{8}";
		text += memory;
		text += startAddress;
		text += endAddress;
		text += values_hex;
		text += BCC(text);
		return text + "\r";
	}

	public string RemoteControlMessage(ushort stationNo, string mode)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{21}";
		text += mode;
		text += BCC(text);
		return text + "\r";
	}

	public string ReadSingleContactMessage(ushort stationNo, ContactCode contactCode, string address)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{0}";
		text += $"{contactCode}";
		text += address;
		text += BCC(text);
		return text + "\r";
	}

	public string WriteSingleContactMessage(ushort stationNo, ContactCode contactCode, string address, bool value)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{4}";
		text += $"{contactCode}";
		text += address;
		ReadOnlySpan<char> readOnlySpan = text;
		char reference = (value ? '1' : '0');
		text = string.Concat(readOnlySpan, new ReadOnlySpan<char>(ref reference));
		text += BCC(text);
		return text + "\r";
	}

	public string ReadPluralContactMessage(ushort stationNo, ContactCode contactCode, string address_hex, int numberOfContacts)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{1}";
		text += $"{numberOfContacts}";
		int num = Conversion.HexToInt(address_hex);
		for (int i = 0; i < numberOfContacts; i++)
		{
			text += $"{contactCode}";
			text += (num + i).ToString("X4");
		}
		text += BCC(text);
		return text + "\r";
	}

	public string WritePluralContactMessage(ushort stationNo, ContactCode contactCode, string address_hex, BOOL[] values)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{5}";
		text += $"{values.Length}";
		int num = Conversion.HexToInt(address_hex);
		for (int i = 0; i < values.Length; i++)
		{
			text += $"{contactCode}";
			text += (num + i).ToString("X4");
			ReadOnlySpan<char> readOnlySpan = text;
			char reference = (values[i] ? '1' : '0');
			text = string.Concat(readOnlySpan, new ReadOnlySpan<char>(ref reference));
		}
		text += BCC(text);
		return text + "\r";
	}

	public string ReadContactMessage(ushort stationNo, ContactCode contactCode, string address_dec, int quantity)
	{
		string text = (int.Parse(address_dec) + quantity - 1).ToString("D4");
		string text2 = $"{60}";
		text2 += stationNo.ToString("X2");
		text2 += "#";
		text2 += $"{2}";
		text2 += $"{contactCode}";
		text2 += address_dec;
		text2 += text;
		text2 += BCC(text2);
		return text2 + "\r";
	}

	public string WriteContactMessage(ushort stationNo, ContactCode contactCode, string startAddres, string endAddress, string values_hex)
	{
		string text = $"{60}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{6}";
		text += $"{contactCode}";
		text += startAddres;
		text += endAddress;
		text += values_hex;
		text += BCC(text);
		return text + "\r";
	}

	public string ReadDataAreaMessage(ushort stationNo, DataCode dataCode, string address_hex, int quantity)
	{
		string text = (int.Parse(address_hex) + quantity - 1).ToString("D5");
		string text2 = $"{60}";
		text2 += stationNo.ToString("X2");
		text2 += "#";
		text2 += $"{7}";
		text2 += $"{dataCode}";
		text2 += address_hex;
		text2 += text;
		text2 += BCC(text2);
		return text2 + "\r";
	}

	public string WriteDataAreaMessage(ushort stationNo, DataCode dataCode, string startAddress, string endAddress, string values_hex)
	{
		string text = $"{60}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{8}";
		text += $"{dataCode}";
		text += startAddress;
		text += endAddress;
		text += values_hex;
		text += BCC(text);
		return text + "\r";
	}

	public string ReadSetValueAreaMessage(ushort stationNo, string startAddress, int quantity)
	{
		string text = (int.Parse(startAddress) + quantity - 1).ToString("D4");
		string text2 = $"{60}";
		text2 += stationNo.ToString("X2");
		text2 += "#";
		text2 += $"{9}";
		text2 += startAddress.PadLeft(4, '0');
		text2 += text;
		text2 += BCC(text2);
		return text2 + "\r";
	}

	public string WriteSetValueAreaMessage(ushort stationNo, string startAddress, string endAddress, string values_hex)
	{
		string text = $"{60}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{10}";
		text += startAddress;
		text += endAddress;
		text += values_hex;
		text += BCC(text);
		return text + "\r";
	}

	public string ReadElapsedValueMessage(ushort stationNo, string startAddress, int quantity)
	{
		string text = (int.Parse(startAddress) + quantity - 1).ToString("D4");
		string text2 = $"{60}";
		text2 += stationNo.ToString("X2");
		text2 += "#";
		text2 += $"{11}";
		text2 += startAddress.PadLeft(4, '0');
		text2 += text;
		text2 += BCC(text2);
		return text2 + "\r";
	}

	public string WriteElapsedValueMessage(ushort stationNo, string startAddress, string endAddress, string values_hex)
	{
		string text = $"{60}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{12}";
		text += startAddress;
		text += endAddress;
		text += values_hex;
		text += BCC(text);
		return text + "\r";
	}

	public string PresetContactAreaMessage(ushort stationNo, ContactCode contactCode, string startAddress, string endAddress, WORD value)
	{
		string text = $"{60}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{16}";
		text += contactCode;
		text += startAddress;
		text += endAddress;
		text += value.Value;
		text += BCC(text);
		return text + "\r";
	}

	public string PresetDataAreaMessage(ushort stationNo, DataCode dataCode, string startAddress, string endAddress, WORD value)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{17}";
		text += dataCode;
		text += startAddress;
		text += endAddress;
		string text2 = text;
		WORD wORD = value;
		text = text2 + wORD.ToString();
		text += BCC(text);
		return text + "\r";
	}

	public string RegisterContactsMonitoredMessage(ushort stationNo, string listContactNo)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{13}";
		text += listContactNo;
		text += BCC(text);
		return text + "\r";
	}

	public string ResetContactsMonitoredMessage(ushort stationNo)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{13}";
		text += "FFFFF";
		text += BCC(text);
		return text + "\r";
	}

	public string RegisterDataMonitoredMessage(ushort stationNo, string listWordNo)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{14}";
		text += listWordNo;
		text += BCC(text);
		return text + "\r";
	}

	public string ResetDataMonitoredMessage(ushort stationNo)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{14}";
		text += "FFFFF";
		text += BCC(text);
		return text + "\r";
	}

	public string MonitoringStartMessage(ushort stationNo)
	{
		string text = $"{37}";
		text += stationNo.ToString("X2");
		text += "#";
		text += $"{15}";
		text += BCC(text);
		return text + "\r";
	}
}
