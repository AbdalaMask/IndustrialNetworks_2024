namespace NetStudio.Mitsubishi.MC.Serial;

public class RequestData
{
	public Command Command { get; set; }

	public byte MessageWait { get; set; }

	public string HeadDevice { get; set; }

	public int NumOfWords { get; set; }
}
