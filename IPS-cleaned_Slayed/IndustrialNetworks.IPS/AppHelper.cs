using System;
using System.Threading.Tasks;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.DriverComm;
using NetStudio.DriverComm.Interfaces;
using NetStudio.DriverComm.Models;
using NetStudio.DriverManager;
using NetStudio.IPS.Entity;
using NetStudio.IPS.Local;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS;

internal class AppHelper
{
	public static LocalAppSettings Settings = new LocalAppSettings();

	private static bool _DataChanged = false;

	public static IndustrialProtocol IndusProtocol { get; set; } = null;


	public static bool DataChanged
	{
		get
		{
			return _DataChanged;
		}
		set
		{
			_DataChanged = value;
			if (value)
			{
				ReloadLoggingTag = value;
			}
		}
	}

	public static bool ReloadLoggingTag { get; set; }

	public static bool CloseApp { get; set; }

	public static IDriverServerManager Driver
	{
		get
		{
			if (Settings.Mode)
			{
				return ClientHelper.Driver;
			}
			return (IDriverServerManager)(object)new DriverServerManager();
		}
	}

	public static void ReadSettings()
	{
		Settings.IP = NetStudio.IPS.Properties.Settings.Default.IP;
		Settings.Port = NetStudio.IPS.Properties.Settings.Default.Port;
		Settings.Username = NetStudio.IPS.Properties.Settings.Default.UserName;
		string password = NetStudio.IPS.Properties.Settings.Default.Password;
		Settings.Password = RSACryptoUtility.DecryptString("u14ca0598a4e4133bbat2ea2165a1812", password);
		Settings.Mode = NetStudio.IPS.Properties.Settings.Default.Mode;
		Settings.Directory = NetStudio.IPS.Properties.Settings.Default.Directory;
		Settings.FileName = NetStudio.IPS.Properties.Settings.Default.FileName;
		Settings.Scaling = NetStudio.IPS.Properties.Settings.Default.ScalingColumn;
		Settings.Offset = NetStudio.IPS.Properties.Settings.Default.OffsetColumn;
		Settings.Resolution = NetStudio.IPS.Properties.Settings.Default.OffsetColumn;
		UpdateClientInfo();
	}

	public static void WriteSettings(LocalAppSettings settings)
	{
		if (!Settings.Mode)
		{
			NetStudio.IPS.Properties.Settings.Default.Mode = settings.Mode;
			NetStudio.IPS.Properties.Settings.Default.IP = settings.IP;
			NetStudio.IPS.Properties.Settings.Default.Port = settings.Port;
			NetStudio.IPS.Properties.Settings.Default.UserName = settings.Username;
			NetStudio.IPS.Properties.Settings.Default.Password = RSACryptoUtility.EncryptString("u14ca0598a4e4133bbat2ea2165a1812", settings.Password);
			NetStudio.IPS.Properties.Settings.Default.Directory = settings.Directory;
			NetStudio.IPS.Properties.Settings.Default.FileName = settings.FileName;
			NetStudio.IPS.Properties.Settings.Default.OffsetColumn = settings.Offset;
			NetStudio.IPS.Properties.Settings.Default.ScalingColumn = settings.Scaling;
			NetStudio.IPS.Properties.Settings.Default.ResolutionColumn = settings.Resolution;
			NetStudio.IPS.Properties.Settings.Default.Save();
			UpdateClientInfo();
		}
	}

	private static void UpdateClientInfo()
	{
		ClientHelper.IP = Settings.IP;
		ClientHelper.Port = Settings.Port;
		ClientHelper.UserName = Settings.Username;
		ClientHelper.Password = Settings.Password;
	}

	public static bool ReadProject()
	{
        ApiResponse apiResponse = !Settings.Mode ? EditHelper.Editor.Read(Settings.FileName) : ClientHelper.Editor.Read();
        if (apiResponse != null)
		{
			if (apiResponse.Success)
			{
				EditHelper.IndusProtocol = (IndustrialProtocol)apiResponse.Data;
				return apiResponse.Success;
			}
			throw new Exception(apiResponse.Message);
		}
		return false;
	}

	public static bool WriteProject()
	{
		ApiResponse apiResponse = null;
		if (EditHelper.IndusProtocol != null)
		{
			apiResponse = ((!Settings.Mode) ? EditHelper.Editor.Write(Settings.FileName, EditHelper.IndusProtocol) : ClientHelper.Editor.Write(EditHelper.IndusProtocol));
			if (apiResponse != null)
			{
				if (!apiResponse.Success)
				{
					throw new Exception(apiResponse.Message);
				}
				DataChanged = false;
			}
		}
		return apiResponse?.Success ?? false;
	}

	public static async Task<bool> ReadProjectAsync()
	{
		ApiResponse apiResponse = (Settings.Mode ? (await ClientHelper.Editor.ReadAsync()) : (await EditHelper.Editor.ReadAsync(Settings.FileName)));
		if (apiResponse != null)
		{
			if (apiResponse.Success)
			{
				EditHelper.IndusProtocol = (IndustrialProtocol)apiResponse.Data;
				return apiResponse.Success;
			}
			throw new Exception(apiResponse.Message);
		}
		return false;
	}

	public static async Task<bool> WriteProjectAsync()
	{
		ApiResponse apiResponse = null;
		if (EditHelper.IndusProtocol != null)
		{
			apiResponse = (Settings.Mode ? (await ClientHelper.Editor.WriteAsync(EditHelper.IndusProtocol)) : (await EditHelper.Editor.WriteAsync(Settings.FileName, EditHelper.IndusProtocol)));
			if (apiResponse != null)
			{
				if (!apiResponse.Success)
				{
					throw new Exception(apiResponse.Message);
				}
				DataChanged = false;
			}
		}
		return apiResponse?.Success ?? false;
	}
}
