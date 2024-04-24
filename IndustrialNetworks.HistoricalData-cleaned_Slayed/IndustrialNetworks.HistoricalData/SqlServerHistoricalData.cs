using System;
using NetStudio.Common.Historiant;
using NetStudio.Database.SqlServer;

namespace NetStudio.HistoricalData;

public class SqlServerHistoricalData : IHistoricalData
{
	public HDResult Create(DataLog dataLog)
	{
		HDResult hDResult = Validate(dataLog);
		if (!hDResult.Success)
		{
			return hDResult;
		}
		DataLogDA dataLogDA = new DataLogDA(string.Format(SqlServerBase.FormatConnectionString, dataLog.ServerName, dataLog.DataLogName, dataLog.Login, dataLog.Password));
		if (!string.IsNullOrEmpty(dataLogDA.GetDatabase(dataLog)))
		{
			hDResult.Message = "The data log(" + dataLog.DataLogName + ") already exists.";
			return hDResult;
		}
		hDResult.Success = dataLogDA.Create(dataLog);
		if (hDResult.Success)
		{
			hDResult.Message = "Add new: The data log(" + dataLog.DataLogName + ") successfully!";
		}
		else
		{
			hDResult.Message = "Adding data log(" + dataLog.DataLogName + ") failed.";
		}
		return hDResult;
	}

	public HDResult Update(DataLog dataLog)
	{
		HDResult hDResult = Validate(dataLog);
		if (!hDResult.Success)
		{
			return hDResult;
		}
		DataLogDA dataLogDA = new DataLogDA(string.Format(SqlServerBase.FormatConnectionString, dataLog.ServerName, dataLog.DataLogName, dataLog.Login, dataLog.Password));
		string database = dataLogDA.GetDatabase(dataLog);
		if (!string.IsNullOrEmpty(database) && dataLog.OldDataLogName != database)
		{
			hDResult.Message = "The data log(" + dataLog.DataLogName + ") already exists.";
			return hDResult;
		}
		hDResult.Success = dataLogDA.Update(dataLog);
		if (hDResult.Success)
		{
			hDResult.Message = "Update: The data log(" + dataLog.DataLogName + ") successfully!";
		}
		else
		{
			hDResult.Message = "Update: The data log(" + dataLog.DataLogName + ") failed.";
		}
		return hDResult;
	}

	public HDResult Delete(DataLog dataLog)
	{
		HDResult hDResult = new HDResult();
		DataLogDA dataLogDA = new DataLogDA(string.Format(SqlServerBase.FormatConnectionString, dataLog.ServerName, dataLog.DataLogName, dataLog.Login, dataLog.Password));
		hDResult.Success = dataLogDA.Delete(dataLog);
		if (hDResult.Success)
		{
			hDResult.Message = "Add new: The data log(" + dataLog.DataLogName + ") successfully!";
		}
		else
		{
			hDResult.Message = "Adding data log(" + dataLog.DataLogName + ") failed.";
		}
		return hDResult;
	}

	public HDResult Validate(DataLog dataLog)
	{
		HDResult hDResult = new HDResult
		{
			Message = "Error: Unknow."
		};
		if (string.IsNullOrEmpty(dataLog.ServerName))
		{
			hDResult.Message = "Invalid: The server name is empty.";
			return hDResult;
		}
		if (string.IsNullOrEmpty(dataLog.Login))
		{
			hDResult.Message = "Invalid: The login is empty.";
			return hDResult;
		}
		if (string.IsNullOrEmpty(dataLog.Password))
		{
			hDResult.Message = "Invalid: The password is empty.";
			return hDResult;
		}
		hDResult.Success = true;
		hDResult.Message = "Success";
		return hDResult;
	}

	public HDResult GetSingle(DataLog dataLog)
	{
		HDResult hDResult = new HDResult();
		string database = new DataLogDA(string.Format(SqlServerBase.FormatConnectionString, dataLog.ServerName, dataLog.DataLogName, dataLog.Login, dataLog.Password)).GetDatabase(dataLog);
		if (!string.IsNullOrEmpty(database) && !string.IsNullOrWhiteSpace(database))
		{
			hDResult.Data = database;
			hDResult.Success = true;
		}
		return hDResult;
	}

	public HDResult GetAll()
	{
		throw new NotImplementedException();
	}
}
