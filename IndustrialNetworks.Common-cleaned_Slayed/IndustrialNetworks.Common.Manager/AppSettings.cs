using NetStudio.Common.Security;

namespace NetStudio.Common.Manager;

public class AppSettings
{
	public Settings Settings { get; set; }

	public License License { get; set; }

	public AppSettings()
	{
		Settings = new Settings();
		License = new License();
	}
}
