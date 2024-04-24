namespace NetStudio.LS.Xgt.FEnet;

public class FEDataType
{
	public static readonly byte[] Bit = new byte[2];

	public static readonly byte[] Byte = new byte[2] { 1, 0 };

	public static readonly byte[] Word = new byte[2] { 2, 0 };

	public static readonly byte[] DWord = new byte[2] { 3, 0 };

	public static readonly byte[] LWord = new byte[2] { 4, 0 };

	public static readonly byte[] Continuous = new byte[2] { 20, 0 };
}
