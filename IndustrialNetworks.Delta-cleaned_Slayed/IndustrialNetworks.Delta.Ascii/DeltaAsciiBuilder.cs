using System;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.Delta.Models;

namespace NetStudio.Delta.Ascii;

public class DeltaAsciiBuilder : BaseBuilder
{
	protected const char Header = ':';

	protected const char CR = '\r';

	protected const char LF = '\n';

	protected string Trailer = $"{13}{10}";

	public string ReadMessage(byte stationNo, byte func, int address, int quantity)
	{
		string text = stationNo.ToString("X2");
		text += func.ToString("X2");
		text += address.ToString("X4");
		text += quantity.ToString("X4");
		return $"{58}{text}{LRC(text)}{Trailer}";
	}

	protected string WriteMessage(byte stationNo, int address, byte func, string hex_value)
	{
		string text = stationNo.ToString("X2");
		text += func.ToString("X2");
		text += address.ToString("X4");
		text += hex_value;
		return $"{58}{text}{LRC(text)}{Trailer}";
	}

	protected string WriteMultipleMessage(byte stationNo, int address, byte func, int quantity, string hex_value)
	{
		string text = stationNo.ToString("X2");
		text += func.ToString("X2");
		text += address.ToString("X4");
		text += quantity.ToString("X4");
		text += (hex_value.Length / 2).ToString("X2");
		for (int i = 0; i < hex_value.Length; i += 2)
		{
			text += hex_value.Substring(i, 2);
		}
		return $"{58}{text}{LRC(text)}{Trailer}";
	}

	private string LRC(string data)
	{
		 
		byte[] array = Conversion.HexToBytes(data);
		byte b = 0;
		for (int i = 0; i < array.Length; i++)
		{
			b += array[i];
		}
		return ((byte)((b ^ 0xFF) + 1)).ToString("X2");
	}

	private byte LRC(byte[] data)
	{
	 
		byte b = 0;
		foreach (byte b2 in data)
		{
			b += b2;
		}
		return (byte)((b ^ 0xFF) + 1);
	}
}
