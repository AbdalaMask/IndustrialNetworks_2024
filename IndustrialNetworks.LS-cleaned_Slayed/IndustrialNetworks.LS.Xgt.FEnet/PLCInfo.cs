namespace NetStudio.LS.Xgt.FEnet;

public class PLCInfo
{
	public static byte[] Server { get; private set; } = new byte[2];


	public static byte[] Client { get; private set; } = new byte[2] { 0, 1 };

}
