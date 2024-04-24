using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.DriverComm.Models;

namespace NetStudio.IPS.Local;

public class LocalAppSettingsManager
{
	private AppSettings _AppSettings;

	public ApiResponse GetAppSettings()
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
			if (File.Exists(path))
			{
				string text = File.ReadAllText(path);
				if (!string.IsNullOrEmpty(text))
				{
					JsonObject jsonObject = JsonSerializer.Deserialize<JsonObject>(text);
					if (jsonObject != null)
					{
						_AppSettings = new AppSettings();
						_AppSettings.License = jsonObject["License"].Deserialize<License>() ?? new License();
						_AppSettings.Settings = jsonObject["Settings"].Deserialize<Settings>() ?? new Settings();
						MachineInfo machineInfo = LicenseManager.MachineInfo;
						_AppSettings.License.SerialNumber = machineInfo.SerialNumber;
						apiResponse.Data = _AppSettings;
						apiResponse.Success = true;
						apiResponse.Message = "Read request successfully.";
					}
				}
			}
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public ApiResponse SetAppSettings(AppSettings appSettings)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			_AppSettings = appSettings;
			string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
			if (File.Exists(text))
			{
				JsonObject jsonObject = GetJsonObject(text);
				if (jsonObject != null)
				{
					JsonNode jsonNode = jsonObject["Settings"];
					if (jsonNode != null && appSettings.Settings != null)
					{
						jsonNode["IPAddress"] = appSettings.Settings.IP;
						jsonNode["Port"] = appSettings.Settings.Port;
						jsonNode["Directory"] = appSettings.Settings.Directory;
						jsonNode["FileName"] = appSettings.Settings.FileName;
					}
					JsonNode jsonNode2 = jsonObject["License"];
					if (jsonNode2 != null && appSettings.License != null)
					{
						jsonNode2["SerialNumber"] = appSettings.License.SerialNumber;
						jsonNode2["Code"] = appSettings.License.Code;
					}
					string contents = JsonSerializer.Serialize(jsonObject);
					File.WriteAllText(text, contents);
				}
				apiResponse.Success = true;
				apiResponse.Message = "Write data: successfully.";
			}
			else
			{
				apiResponse.Message = "Not found: " + text;
			}
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public ApiResponse GetLicense()
	{
		ApiResponse appSettings = GetAppSettings();
		try
		{
			if (appSettings.Success && _AppSettings != null)
			{
				appSettings.Data = _AppSettings.License;
			}
		}
		catch (Exception ex)
		{
			appSettings.Message = ex.Message;
		}
		return appSettings;
	}

	public ApiResponse SetLicense(License license)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
			if (File.Exists(text))
			{
				JsonObject jsonObject = GetJsonObject(text);
				if (jsonObject != null)
				{
					JsonNode jsonNode = jsonObject["License"];
					if (jsonNode != null && license != null)
					{
						jsonNode["SerialNumber"] = license.SerialNumber;
						jsonNode["Code"] = license.Code;
					}
					string contents = JsonSerializer.Serialize(jsonObject);
					File.WriteAllText(text, contents);
				}
				apiResponse.Success = true;
				apiResponse.Message = "Write data: successfully.";
			}
			else
			{
				apiResponse.Message = "Not found: " + text;
			}
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	private JsonObject? GetJsonObject(string appsettings)
	{
		JsonObject result = null;
		if (File.Exists(appsettings))
		{
			string text = File.ReadAllText(appsettings);
			if (!string.IsNullOrEmpty(text))
			{
				result = JsonSerializer.Deserialize<JsonObject>(text);
			}
		}
		return result;
	}
}
