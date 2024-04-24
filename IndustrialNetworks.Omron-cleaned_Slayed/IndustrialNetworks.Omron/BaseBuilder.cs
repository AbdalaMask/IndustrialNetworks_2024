using System.Linq;
using System.Text;

namespace NetStudio.Omron;

public class BaseBuilder
{
	public const int MaxSize = 30;

	public string FCS(string data)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(data);
		byte b = 0;
		for (int i = 0; i < bytes.Length; i++)
		{
			b ^= bytes[i];
		}
		return b.ToString("X2");
	}

	public string FCSCheck(string data)
	{
		return Encoding.ASCII.GetBytes(data).Aggregate((byte byte_0, byte byte_1) => (byte)(byte_0 ^ byte_1)).ToString("X2");
	}
}
