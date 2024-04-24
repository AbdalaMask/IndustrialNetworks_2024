namespace NetStudio.Common.Security;

public class RSAObject
{
	public RSAKey RSAKey { get; set; } = new RSAKey();


	public License LicenseKey { get; set; } = new License();


	public byte[] Data { get; set; }
}
