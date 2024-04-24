using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using NetStudio.Common.Hardware;
using NetStudio.Common.IndusCom;

namespace NetStudio.Common.Security;

public class LicenseManager
{
	public static MachineInfo MachineInfo => GetMachineInfo();

	public static License GetLicense()
	{
		License license = null;
		string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
		if (File.Exists(path))
		{
			string text = File.ReadAllText(path);
			if (!string.IsNullOrEmpty(text))
			{
				JsonObject jsonObject = JsonSerializer.Deserialize<JsonObject>(text);
				if (jsonObject != null)
				{
					License license2 = jsonObject["License"].Deserialize<License>();
					if (license2 != null)
					{
						license = license2;
					}
				}
			}
		}
		return license ?? new License();
	}

	public static MachineInfo GetMachineInfo()
	{
		string pathRoot = Path.GetPathRoot(Assembly.GetEntryAssembly().Location);
		return new MachineInfo
		{
			Name = pathRoot,
			SerialNumber = HardwareInfo.GetDriveInfo(pathRoot)
		};
	}
}
