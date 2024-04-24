using System;
using System.IO;
using System.Threading.Tasks;
using NetStudio.AsrsLink;
using NetStudio.Common.AsrsLink;
using NetStudio.Common.Files;
using NetStudio.Common.Manager;
using NetStudio.Database.SqlServer;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Entity;

namespace NetStudio.IPS.Local;

public class LocalEditManger
{
	private JsonObjectManager _JsonObjectManager;

	private LocalAppSettings _Settings;

	public LocalEditManger(LocalAppSettings setting)
	{
		_Settings = setting;
		_JsonObjectManager = new JsonObjectManager();
	}

	public ApiResponse New(string projectName)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", projectName + ".json");
			if (File.Exists(text))
			{
				apiResponse.Message = "The project(" + projectName + ") already exist.";
			}
			else
			{
				_JsonObjectManager.Write(text, new IndustrialProtocol());
				apiResponse.Data = true;
				apiResponse.Success = true;
				apiResponse.Message = "Read request successfully.";
			}
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public ApiResponse Open(string fileName)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			if (File.Exists(fileName))
			{
				apiResponse.Data = _JsonObjectManager.Read<IndustrialProtocol>(fileName);
				apiResponse.Success = true;
				apiResponse.Message = "Read request successfully.";
			}
			else
			{
				apiResponse.Message = "The file(" + fileName + ") does not exist.";
			}
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public ApiResponse Read(string fileName)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			if (File.Exists(fileName))
			{
				apiResponse.Data = _JsonObjectManager.Read<IndustrialProtocol>(fileName);
				apiResponse.Success = true;
				apiResponse.Message = "Read request successfully.";
			}
			else
			{
				_JsonObjectManager.Write(fileName, new IndustrialProtocol());
			}
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public ApiResponse Write(string fileName, IndustrialProtocol data)
	{
		ApiResponse apiResponse = new ApiResponse
		{
			Message = "Write data: failure."
		};
		try
		{
			_JsonObjectManager.Write(fileName, data);
			apiResponse.Success = true;
			apiResponse.Message = "Write data: successfully.";
			AppHelper.DataChanged = false;
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public ApiResponse GetSettings()
	{
		return new ApiResponse
		{
			Data = _Settings,
			Success = true,
			Message = "Read request successfully."
		};
	}

	public ApiResponse GetAll(string path = "")
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			if (string.IsNullOrEmpty(path))
			{
				Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
			}
			apiResponse.Data = Directory.GetFiles(path);
			apiResponse.Success = true;
			apiResponse.Message = "Read request successfully.";
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public async Task<ApiResponse> NewAsync(string fileName)
	{
		ApiResponse response = new ApiResponse();
		try
		{
			if (File.Exists(fileName))
			{
				response.Message = "The project(" + fileName + ") already exist.";
			}
			else
			{
				IndustrialProtocol ip = new IndustrialProtocol();
				await _JsonObjectManager.WriteAsync(fileName, ip);
				response.Data = ip;
				response.Success = true;
				response.Message = "Read request successfully.";
			}
		}
		catch (Exception ex)
		{
			response.Message = ex.Message;
		}
		return response;
	}

	public async Task<ApiResponse> OpenAsync(string fileName)
	{
		ApiResponse response = new ApiResponse();
		try
		{
			if (File.Exists(fileName))
			{
				ApiResponse apiResponse = response;
				apiResponse.Data = await _JsonObjectManager.ReadAsync<IndustrialProtocol>(fileName);
				response.Success = true;
				response.Message = "Read request successfully.";
			}
			else
			{
				response.Message = "The file(" + fileName + ") does not exist.";
			}
		}
		catch (Exception ex)
		{
			response.Message = ex.Message;
		}
		return response;
	}

	public async Task<ApiResponse> ReadAsync(string fileName)
	{
		ApiResponse response = new ApiResponse();
		try
		{
			if (!File.Exists(fileName))
			{
				await _JsonObjectManager.WriteAsync(fileName, new IndustrialProtocol());
			}
			else
			{
				ApiResponse apiResponse = response;
				apiResponse.Data = await _JsonObjectManager.ReadAsync<IndustrialProtocol>(fileName);
				response.Success = true;
				response.Message = "Read request successfully.";
			}
		}
		catch (Exception ex)
		{
			response.Message = ex.Message;
		}
		return response;
	}

	public async Task<ApiResponse> WriteAsync(string fileName, IndustrialProtocol data)
	{
		ApiResponse response = new ApiResponse
		{
			Message = "Write data: failure."
		};
		try
		{
			await _JsonObjectManager.WriteAsync(fileName, data);
			response.Success = true;
			response.Message = "Write data: successfully.";
		}
		catch (Exception ex)
		{
			response.Message = ex.Message;
		}
		return response;
	}

	public async Task<ApiResponse> GetSettingsAsync()
	{
		return await Task.FromResult(GetSettings());
	}

	public async Task<ApiResponse> GetAllAsync(string path = "")
	{
		return await Task.FromResult(GetAll(path));
	}

	public ApiResponse TestConnection(string connectionString)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrWhiteSpace(connectionString))
			{
				SqlServerBase sqlServerBase = new SqlServerBase();
				apiResponse.Data = connectionString;
				apiResponse.Success = sqlServerBase.TestConnection(connectionString);
				if (apiResponse.Success)
				{
					apiResponse.Message = "Connection successful!";
				}
				else
				{
					apiResponse.Message = "Connection failed!";
				}
			}
			else
			{
				apiResponse.Message = "The connection string(" + connectionString + ") does not exist.";
			}
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public async Task<ApiResponse> TestConnectionAsync(string connectionString)
	{
		ApiResponse response = new ApiResponse();
		try
		{
			if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrWhiteSpace(connectionString))
			{
				SqlServerBase sqlServerBase = new SqlServerBase();
				response.Data = connectionString;
				ApiResponse apiResponse = response;
				apiResponse.Success = await sqlServerBase.TestConnectionAsync(connectionString);
				if (response.Success)
				{
					response.Message = "Connection successful!";
				}
				else
				{
					response.Message = "Connection failed!";
				}
			}
			else
			{
				response.Message = "The connection string(" + connectionString + ") does not exist.";
			}
		}
		catch (Exception ex)
		{
			response.Message = ex.Message;
		}
		return response;
	}

	public ApiResponse GetTableNames(SqlRequestInfo rstInfo)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			AsrsTableDA asrsTableDA = new AsrsTableDA(string.Format(SqlServerBase.FormatConnectionString, rstInfo.ServerName, rstInfo.DatabaseName, rstInfo.Login, rstInfo.Password));
			apiResponse.Data = asrsTableDA.GetTableNames(rstInfo.DatabaseName);
			apiResponse.Success = true;
			apiResponse.Message = "Read request successfully.";
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public async Task<ApiResponse> GetTableNamesAsync(SqlRequestInfo rstInfo)
	{
		ApiResponse response = new ApiResponse();
		try
		{
			AsrsTableDA asrsTableDA = new AsrsTableDA(string.Format(SqlServerBase.FormatConnectionString, rstInfo.ServerName, rstInfo.DatabaseName, rstInfo.Login, rstInfo.Password));
			ApiResponse apiResponse = response;
			apiResponse.Data = await asrsTableDA.GetTableNamesAsync(rstInfo.DatabaseName);
			response.Success = true;
			response.Message = "Read request successfully.";
		}
		catch (Exception ex)
		{
			response.Message = ex.Message;
		}
		return response;
	}

	public ApiResponse GetColumnsInfo(string tableName)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			AsrsServer asrsServer = AppHelper.IndusProtocol.AsrsServer;
			AsrsTableDA asrsTableDA = new AsrsTableDA(string.Format(SqlServerBase.FormatConnectionString, asrsServer.ServerName, asrsServer.DatabaseName, asrsServer.Login, asrsServer.Password));
			apiResponse.Data = asrsTableDA.GetColumnsInfo(tableName);
			apiResponse.Success = true;
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public async Task<ApiResponse> GetColumnsInfoAsync(string tableName)
	{
		 
		ApiResponse response = new ApiResponse();
		try
		{
			return await Task.Run(delegate
			{
				AsrsServer asrsServer = AppHelper.IndusProtocol.AsrsServer;
				AsrsTableDA asrsTableDA = new AsrsTableDA(string.Format(SqlServerBase.FormatConnectionString, asrsServer.ServerName, asrsServer.DatabaseName, asrsServer.Login, asrsServer.Password));
				response.Data = asrsTableDA.GetColumnsInfo(tableName);
				response.Success = true;
				return response;
			});
		}
		catch (Exception ex)
		{
			response.Message = ex.Message;
		}
		return response;
	}
}
