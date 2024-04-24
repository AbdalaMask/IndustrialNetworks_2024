namespace NetStudio.LS.Xgt.Cnet;

internal class InstructionType
{
	public const string SS = "SS";

	public const string SB = "SB";

	public static readonly byte[] SS_BYTES = new byte[2] { 83, 83 };

	public static readonly byte[] SB_BYTES = new byte[2] { 83, 66 };
}
