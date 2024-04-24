using System;
using System.IO;
using System.Threading.Tasks;
using NetStudio.AsrsLink;
using NetStudio.Common.AsrsLink;
using NetStudio.Common.Files;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Database.SqlServer;
using NetStudio.DriverComm.Interfaces;
using NetStudio.DriverComm.Models;

namespace NetStudio.DriverManager;

public class EditManger : IEditManger
{
	private JsonObjectManager _JsonObjectManager;

	public EditManger()
	{
		_JsonObjectManager = new JsonObjectManager();
	}

	public ApiResponse Read()
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			if (File.Exists(DriverDataSource.AppSettings.Settings.FileName))
			{
				apiResponse.Data = _JsonObjectManager.Read<IndustrialProtocol>(DriverDataSource.AppSettings.Settings.FileName);
				apiResponse.Success = true;
				apiResponse.Message = "Read request successfully.";
			}
			else
			{
				apiResponse.Message = "The file(" + DriverDataSource.AppSettings.Settings.FileName + ") does not exist.";
			}
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public ApiResponse Write(IndustrialProtocol data)
	{
		ApiResponse apiResponse = new ApiResponse
		{
			Message = "Write data: failure."
		};
		try
		{
			data = data ?? new IndustrialProtocol();
			_JsonObjectManager.Write(DriverDataSource.AppSettings.Settings.FileName, data);
			apiResponse.Success = true;
			apiResponse.Message = "Write data: successfully.";
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public async Task<ApiResponse> ReadAsync()
	{
		ApiResponse response = new ApiResponse();
		try
		{
			if (File.Exists(DriverDataSource.AppSettings.Settings.FileName))
			{
				ApiResponse apiResponse = response;
				apiResponse.Data = await _JsonObjectManager.ReadAsync<IndustrialProtocol>(DriverDataSource.AppSettings.Settings.FileName);
				response.Success = true;
				response.Message = "Read request successfully.";
			}
			else
			{
				response.Message = "The file(" + DriverDataSource.AppSettings.Settings.FileName + ") does not exist.";
			}
		}
		catch (Exception ex)
		{
			response.Message = ex.Message;
		}
		return response;
	}

	public async Task<ApiResponse> WriteAsync(IndustrialProtocol data)
	{
		ApiResponse response = new ApiResponse
		{
			Message = "Write data: failure."
		};
		try
		{
			data = data ?? new IndustrialProtocol();
			await _JsonObjectManager.WriteAsync(DriverDataSource.AppSettings.Settings.FileName, data);
			response.Success = true;
			response.Message = "Write data: successfully.";
		}
		catch (Exception ex)
		{
			response.Message = ex.Message;
		}
		return response;
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

	public ApiResponse GetColumnsInfo(SqlRequestInfo rstInfo)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			AsrsTableDA asrsTableDA = new AsrsTableDA(string.Format(SqlServerBase.FormatConnectionString, rstInfo.ServerName, rstInfo.DatabaseName, rstInfo.Login, rstInfo.Password));
			apiResponse.Data = asrsTableDA.GetColumnsInfo(rstInfo.TableName);
			apiResponse.Success = true;
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public async Task<ApiResponse> GetColumnsInfoAsync(SqlRequestInfo rstInfo)
	{
		
		ApiResponse response = new ApiResponse();
		try
		{
			return await Task.Run(delegate
			{
				AsrsTableDA asrsTableDA = new AsrsTableDA(string.Format(SqlServerBase.FormatConnectionString, rstInfo.ServerName, rstInfo.DatabaseName, rstInfo.Login, rstInfo.Password));
				response.Data = asrsTableDA.GetColumnsInfo(rstInfo.TableName);
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
