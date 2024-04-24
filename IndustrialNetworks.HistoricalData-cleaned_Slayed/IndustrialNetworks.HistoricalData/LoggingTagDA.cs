using System;
using System.Collections.Generic;
using System.Data;
using NetStudio.Common.Historiant;
using NetStudio.Database.SqlServer;
using Microsoft.Data.SqlClient;

namespace NetStudio.HistoricalData;

public class LoggingTagDA : SqlServerBase
{
	protected const string TableName = "HistoricalData";

	protected const string LogName = "LogName";

	protected const string ChannelId = "ChannelId";

	protected const string DeviceId = "DeviceId";

	protected const string GroupId = "GroupId";

	protected const string TagId = "TagId";

	protected const string DTime = "DTime";

	protected const string Value = "Value";

	protected const string Offset = "Offset";

	private string _connectionString = string.Empty;

	public LoggingTagDA(string connectionString)
	{
		_connectionString = connectionString;
	}

	public int Insert(LoggingTag loggingtg)
	{
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.Connection = new SqlConnection(_connectionString);
		string[] columnNames = new string[8] { "LogName", "Value", "DTime", "Offset", "ChannelId", "DeviceId", "GroupId", "TagId" };
		object[] values = new object[8] { loggingtg.LogName, loggingtg.Value, loggingtg.DTime, loggingtg.Offset, loggingtg.ChannelId, loggingtg.DeviceId, loggingtg.GroupId, loggingtg.TagId };
		int result = 0;
		if (loggingtg.LowLimit == 0m && loggingtg.HighLimit == loggingtg.LowLimit)
		{
			result = InsertTable("HistoricalData", columnNames, values, sqlCommand);
		}
		else
		{
			LoggingLimit loggingLimit = loggingtg.LoggingLimit;
			if (loggingLimit != 0 && loggingLimit == LoggingLimit.OutsideDeadband)
			{
				if (loggingtg.Value < loggingtg.LowLimit && loggingtg.Value > loggingtg.HighLimit)
				{
					result = InsertTable("HistoricalData", columnNames, values, sqlCommand);
				}
			}
			else if (loggingtg.Value >= loggingtg.LowLimit && loggingtg.Value <= loggingtg.HighLimit)
			{
				result = InsertTable("HistoricalData", columnNames, values, sqlCommand);
			}
		}
		return result;
	}

	public int InsertMultipes(List<LoggingTag> tags)
	{
		DataTable dataTable = new DataTable();
		dataTable.TableName = "HistoricalData";
		dataTable.Columns.Add("LogName", typeof(string));
		dataTable.Columns.Add("Value", typeof(decimal));
		dataTable.Columns.Add("DTime", typeof(DateTime));
		dataTable.Columns.Add("Offset", typeof(float));
		dataTable.Columns.Add("ChannelId", typeof(int));
		dataTable.Columns.Add("DeviceId", typeof(int));
		dataTable.Columns.Add("GroupId", typeof(int));
		dataTable.Columns.Add("TagId", typeof(int));
		foreach (LoggingTag tag in tags)
		{
			if (tag.LowLimit == 0m && tag.HighLimit == tag.LowLimit)
			{
				dataTable.Rows.Add(tag.LogName, tag.Value, tag.DTime, tag.Offset, tag.ChannelId, tag.DeviceId, tag.GroupId, tag.TagId);
				continue;
			}
			LoggingLimit loggingLimit = tag.LoggingLimit;
			if (loggingLimit != 0 && loggingLimit == LoggingLimit.OutsideDeadband)
			{
				if (tag.Value < tag.LowLimit && tag.Value > tag.HighLimit)
				{
					dataTable.Rows.Add(tag.LogName, tag.Value, tag.DTime, tag.Offset, tag.ChannelId, tag.DeviceId, tag.GroupId, tag.TagId);
				}
			}
			else if (tag.Value >= tag.LowLimit && tag.Value <= tag.HighLimit)
			{
				dataTable.Rows.Add(tag.LogName, tag.Value, tag.DTime, tag.Offset, tag.ChannelId, tag.DeviceId, tag.GroupId, tag.TagId);
			}
		}
		using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(_connectionString))
		{
			sqlBulkCopy.BulkCopyTimeout = 60;
			sqlBulkCopy.DestinationTableName = "HistoricalData";
			sqlBulkCopy.WriteToServer(dataTable);
			sqlBulkCopy.Close();
		}
		return tags.Count;
	}

	public int Update(LoggingTag loggingtg)
	{
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.Connection = new SqlConnection(_connectionString);
		string[] columnNames = new string[8] { "LogName", "ChannelId", "DeviceId", "GroupId", "TagId", "DTime", "Value", "Offset" };
		object[] values = new object[6] { loggingtg.LogName, loggingtg.ChannelId, loggingtg.DeviceId, loggingtg.GroupId, loggingtg.TagId, loggingtg.Offset };
		string[] keyColumns = new string[1] { "ChannelId" };
		object[] keyValues = new object[1] { loggingtg.ChannelId };
		return UpdateTable("HistoricalData", columnNames, values, keyColumns, keyValues, sqlCommand);
	}

	public int Delete(LoggingTag loggingtg)
	{
		new SqlCommand().Connection = new SqlConnection(_connectionString);
		string[] keyColumns = new string[1] { "ChannelId" };
		object[] keyValues = new object[1] { loggingtg.ChannelId };
		return DeleteTable("HistoricalData", keyColumns, keyValues);
	}

	public string? GetTable(string tableName)
	{
		string result = null;
		using SqlConnection sqlConnection = new SqlConnection(_connectionString);
		SqlCommand obj = new SqlCommand
		{
			Connection = sqlConnection,
			CommandType = CommandType.Text,
			CommandText = $"SELECT * FROM sys.objects WHERE name = '{tableName}'"
		};
		sqlConnection.Open();
		SqlDataReader sqlDataReader = obj.ExecuteReader();
		while (sqlDataReader.Read())
		{
			result = sqlDataReader.GetString(0);
		}
		return result;
	}

	public List<LoggingTag> GetByPaging(int target, int pageIndex, int pageSize, string condition, out int pageTotal)
	{
		List<LoggingTag> list = new List<LoggingTag>();
		using SqlConnection sqlConnection = new SqlConnection(_connectionString);
		SqlCommand sqlCommand = new SqlCommand("ProcSelectLoggingTags", sqlConnection);
		sqlCommand.CommandType = CommandType.StoredProcedure;
		sqlCommand.Connection = sqlConnection;
		sqlCommand.Parameters.AddWithValue("@Target", target);
		sqlCommand.Parameters.AddWithValue("@PageIndex", pageIndex);
		sqlCommand.Parameters.AddWithValue("@PageSize", pageSize);
		sqlCommand.Parameters.Add("@Condition", SqlDbType.NVarChar);
		sqlCommand.Parameters["@Condition"].SqlValue = condition;
		sqlCommand.Parameters.Add("@PageTotal", SqlDbType.Int);
		sqlCommand.Parameters["@PageTotal"].Direction = ParameterDirection.Output;
		sqlConnection.Open();
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		foreach (DataRow row in dataTable.Rows)
		{
			LoggingTag loggingTag = new LoggingTag();
			loggingTag.LogName = string.Format("{0}", row["LogName"]);
			loggingTag.ChannelId = int.Parse(string.Format("{0}", row["ChannelId"]));
			loggingTag.DeviceId = int.Parse(string.Format("{0}", row["DeviceId"]));
			loggingTag.GroupId = int.Parse(string.Format("{0}", row["GroupId"]));
			loggingTag.TagId = int.Parse(string.Format("{0}", row["TagId"]));
			loggingTag.Value = decimal.Parse(string.Format("{0}", row["Value"]));
			if (row["DTime"] != DBNull.Value)
			{
				loggingTag.DTime = DateTime.Parse(string.Format("{0}", row["DTime"]));
			}
			loggingTag.Offset = float.Parse(string.Format("{0}", row["Offset"]));
			list.Add(loggingTag);
		}
		string s = string.Format("{0}", sqlCommand.Parameters["@PageTotal"].Value);
		pageTotal = int.Parse(s);
		return list;
	}
}
