namespace NetStudio.Common.Manager;

public class IPSResult
{
	public int ErrorCode { get; set; }

	public CommStatus Status { get; set; } = CommStatus.Error;


	public byte[] Values { get; set; }

	public string Values_Hex { get; set; }

	public string Message { get; set; } = "Error";

}
