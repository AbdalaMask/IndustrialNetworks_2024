using System;
using System.Collections.Generic;
using System.Linq;
using NetStudio.Common;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.Vigor.Enums;

namespace NetStudio.Vigor;

public class VSBuilder
{
	internal class CHAR
	{
		internal const string STX = "02";

		internal const string EXT = "03";

		internal const string ACK = "06";

		internal const string DLE = "10";
	}

	internal class HEX
	{
		internal const byte STX = 2;

		internal const byte EXT = 3;

		internal const byte ACK = 6;

		internal const byte DLE = 16;
	}

	internal string ReadByAddress(byte stationNo, int numOfBytes, FunctionCode function, DeviceCode deviceCode, string address, int numberOfDevices)
	{
		string text = stationNo.ToString("X2");
		text += VSUtility.ByteSwap(numOfBytes.ToString("X4"));
		text += $"{(byte)function:X2}";
		text += $"{(byte)deviceCode:X2}";
		text += address;
		text += VSUtility.ByteSwap(numberOfDevices.ToString("X4"));
		string value = SumCheck(text);
		string value2 = AddOnCode10H(text);
		return $"{"10"}{"02"}{value2}{"10"}{"03"}{value}";
	}

	internal string ReadByDeviceId(byte stationNo, int numOfBytes, FunctionCode function, DeviceCode deviceCode, string deviceId, int numberOfDevices)
	{
		string text = stationNo.ToString("X2");
		text += VSUtility.ByteSwap(numOfBytes.ToString("X4"));
		text += $"{(byte)function:X2}";
		text += $"{(byte)deviceCode:X2}";
		text += VSUtility.GetHexAddress(deviceId);
		text += VSUtility.ByteSwap(numberOfDevices.ToString("X4"));
		string value = SumCheck(text);
		string value2 = AddOnCode10H(text);
		return $"{"10"}{"02"}{value2}{"10"}{"03"}{value}";
	}

	internal string WriteByAddress(byte stationNo, int numOfBytes, FunctionCode function, DeviceCode deviceCode, string address, byte[] values)
	{
		int num = values.Length / 2 + ((values.Length % 2 != 0) ? 1 : 0);
		string text = stationNo.ToString("X2");
		text += VSUtility.ByteSwap(numOfBytes.ToString("X4"));
		text += $"{(byte)function:X2}";
		text += $"{(byte)deviceCode:X2}";
		text += address;
		text += VSUtility.ByteSwap(num.ToString("X4"));
		text += Conversion.BytesToHex(values);
		string value = SumCheck(text);
		string value2 = AddOnCode10H(text);
		return $"{"10"}{"02"}{value2}{"10"}{"03"}{value}";
	}

	internal string WriteByDeviceId(byte stationNo, int numOfBytes, FunctionCode function, DeviceCode deviceCode, string deviceId, byte[] values)
	{
		bool isOctal = deviceCode == DeviceCode.ExternalInputX || deviceCode == DeviceCode.ExternalOutputY;
		int num = values.Length / 2 + ((values.Length % 2 != 0) ? 1 : 0);
		string text = stationNo.ToString("X2");
		text += VSUtility.ByteSwap(numOfBytes.ToString("X4"));
		text += $"{(byte)function:X2}";
		text += $"{(byte)deviceCode:X2}";
		text += VSUtility.GetHexAddress(deviceId, isOctal);
		text += VSUtility.ByteSwap(num.ToString("X4"));
		text += Conversion.BytesToHex(values);
		string value = SumCheck(text);
		string value2 = AddOnCode10H(text);
		return $"{"10"}{"02"}{value2}{"10"}{"03"}{value}";
	}

	private string SumCheck(string data)
	{
		 
		string text = Conversion.HexToBytes(data).Sum((byte byte_0) => byte_0).ToString("X2");
		string source = text.Substring(text.Length - 2, 2);
		return string.Join("", source.Select((char char_0) => ((byte)char_0).ToString("X2")));
	}

	internal string AddOnCode10H(string frame)
	{
		string text = string.Empty;
		int num = frame.Length / 2;
		for (int i = 0; i < num; i++)
		{
			string text2 = frame.Substring(i * 2, 2);
			text += text2;
			if (text2 == "10")
			{
				text += "10";
			}
		}
		return text;
	}

	internal string SubOnCode10H(string IP)
	{
		string text = string.Empty;
		int num = IP.Length / 2;
		string text2 = string.Empty;
		for (int i = 0; i < num; i++)
		{
			string text3 = IP.Substring(i * 2, 2);
			if (text3 == "10" && text2 == "10")
			{
				text2 = string.Empty;
				continue;
			}
			text += text3;
			text2 = text3;
		}
		return text;
	}

	internal byte[] ReadMsg(ReadPacket RP)
	{
		byte[] array = new byte[16]
		{
			16,
			2,
			(byte)RP.StationNo,
			7,
			0,
			(byte)RP.Function,
			(byte)RP.DeviceCode,
			(byte)RP.WordAddress,
			(byte)(RP.WordAddress >> 8),
			(byte)(RP.WordAddress >> 16),
			(byte)RP.Quantity,
			(byte)(RP.Quantity >> 8),
			16,
			3,
			0,
			0
		};
		byte[] array2 = SumCheck(array);
		array[14] = array2[0];
		array[15] = array2[1];
		return AddOnCode10H(array);
	}

	internal byte[] WriteMsg(WritePacket WP)
	{
		byte[] array = new byte[16 + WP.ValueDec.Length];
		array[0] = 16;
		array[1] = 2;
		array[2] = (byte)WP.StationNo;
		int num = WP.ValueDec.Length + 7;
		array[3] = (byte)num;
		array[4] = (byte)(num >> 8);
		array[5] = (byte)WP.Function;
		array[6] = (byte)WP.DeviceCode;
		if (WP.IsBit)
		{
			if (VSUtility.IsOctal(WP.Memory))
			{
				WP.BitAddress = Conversion.IntToOctal(WP.BitAddress);
			}
			array[7] = (byte)WP.BitAddress;
			array[8] = (byte)(WP.BitAddress >> 8);
			array[9] = (byte)(WP.BitAddress >> 16);
		}
		else
		{
			array[7] = (byte)WP.WordAddress;
			array[8] = (byte)(WP.WordAddress >> 8);
			array[9] = (byte)(WP.WordAddress >> 16);
		}
		int num2 = WP.ValueDec.Length / 2 + ((WP.ValueDec.Length % 2 != 0) ? 1 : 0);
		array[10] = (byte)num2;
		array[11] = (byte)(num2 >> 8);
		for (int i = 0; i < WP.ValueDec.Length; i++)
		{
			array[12 + i] = WP.ValueDec[i];
		}
		array[12 + WP.ValueDec.Length] = 16;
		array[13 + WP.ValueDec.Length] = 3;
		byte[] array2 = SumCheck(array);
		array[14 + WP.ValueDec.Length] = array2[0];
		array[15 + WP.ValueDec.Length] = array2[1];
		return AddOnCode10H(array);
	}

	private byte[] SumCheck(byte[] frame)
	{
		int num = 0;
		int num2 = frame.Length - 4;
		 
		for (int i = 2; i < num2; i++)
		{
			num = (num + frame[i]) % 256;
		}
		string text = num.ToString("X2").Right(2);
		return new byte[2]
		{
			(byte)text[0],
			(byte)text[1]
		};
	}

	internal byte[] AddOnCode10H(byte[] frame)
	{
		List<byte> list = new List<byte>();
		int num = frame.Length - 4;
		list.Add(frame[0]);
		list.Add(frame[1]);
		for (int i = 2; i < num; i++)
		{
			list.Add(frame[i]);
			if (frame[i] == 16)
			{
				list.Add(frame[i]);
			}
		}
		list.Add(frame[^4]);
		list.Add(frame[^3]);
		list.Add(frame[^2]);
		list.Add(frame[^1]);
		return list.ToArray();
	}

	internal byte[] SubOnCode10H(byte[] bytes)
	{
		List<byte> list = new List<byte>();
		int num = bytes.Length;
		byte b = 0;
		for (int i = 0; i < num; i++)
		{
			if (bytes[i] == 16 && b == 16)
			{
				b = 0;
				continue;
			}
			list.Add(bytes[i]);
			b = bytes[i];
		}
		return list.ToArray();
	}

	public static string DecimalToOctal(int decimalNumber)
	{
		return Convert.ToString(decimalNumber, 8);
	}
}
