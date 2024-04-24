namespace NetStudio.Common.IndusCom;

public class IpsAddress
{
	public int WordAddress { get; set; }

	public int BitAddress { get; set; }

	public IpsAddress()
	{
	}

	public IpsAddress(int wordAddress, int bitAddress)
	{
		WordAddress = wordAddress;
		BitAddress = bitAddress;
	}
}
