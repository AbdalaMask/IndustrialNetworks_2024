using System.Data;
using NetStudio.Common.Historiant;
using NetStudio.Database.SqlServer;
using Microsoft.Data.SqlClient;

namespace NetStudio.HistoricalData;

public class DataLogDA : SqlServerBase
{
	protected const string DataLogName = "DataLogName";

	protected const string StorageType = "StorageType";

	protected const string DataRecordsPerLog = "DataRecordsPerLog";

	protected const string Path = "Path";

	protected const string ServerName = "ServerName";

	protected const string Login = "Login";

	protected const string Password = "Password";

	protected const string Active = "Active";

	protected const string Description = "Description";

	public DataLogDA(string connectionString)
	{
		base.ConnectionString = connectionString;
	}

	public bool Create(DataLog dataLog)
	{
        using (SqlConnection sqlConnection = new SqlConnection(string.Format(SqlServerBase.FormatConnectionString, dataLog.ServerName, "master", dataLog.Login, dataLog.Password)))
		{
			SqlCommand obj = new SqlCommand
			{
				Connection = sqlConnection,
				CommandType = CommandType.Text
			};
			sqlConnection.Open();
			obj.CommandText = "USE master";
			obj.ExecuteNonQuery();
			obj.CommandText = string.Format("IF NOT EXISTS(SELECT * FROM sys.sysdatabases WHERE name = '{0}')\r\n                                                BEGIN\r\n\t                                                CREATE DATABASE {0}\r\n                                                END", dataLog.DataLogName);
			obj.ExecuteNonQuery();
		}
		using SqlConnection sqlConnection2 = new SqlConnection(base.ConnectionString);
		SqlCommand obj2 = new SqlCommand
		{
			Connection = sqlConnection2,
			CommandType = CommandType.Text
		};
		sqlConnection2.Open();
		obj2.CommandText = "USE " + dataLog.DataLogName;
		obj2.ExecuteNonQuery();
		obj2.CommandText = "CREATE TABLE Information\r\n                                    (\r\n\t                                    [Id] INT IDENTITY NOT NULL,\r\n\t                                    [Author] NVARCHAR(512) NOT NULL,\r\n\t                                    [Phone] VARCHAR(25) NOT NULL,\r\n\t                                    [Email] VARCHAR(512)  NOT NULL,\r\n\t                                    [Description] NVARCHAR(MAX),\r\n\t                                    -- Thêm rằng buộc cho bảng.\r\n\t                                    CONSTRAINT [PK_InformationId] PRIMARY KEY ([Id]),\r\n\t                                    CONSTRAINT [UQ_Author]  UNIQUE ([Author])\r\n                                    )";
		obj2.ExecuteNonQuery();
		obj2.CommandText = "INSERT INTO Information([Author], [Phone], [Email], [Description]) VALUES(N'Hoàng Văn Lưu', '+84-909-886-483', N'hoangluu.automation@gmail.com', N'Designed By Industrial Networks: https://www.youtube.com/NetStudio')";
		obj2.ExecuteNonQuery();
		obj2.CommandText = "IF NOT EXISTS(SELECT * FROM sys.objects WHERE name = 'HistoricalData')\r\n                                    BEGIN                                                            \r\n                                        CREATE TABLE HistoricalData\r\n                                        (\r\n                                            [LogName] NVARCHAR(160) NOT NULL,\r\n                                            [Value] DECIMAL(24, 4) NOT NULL DEFAULT 0,\r\n                                            [DTime] DATETIME NOT NULL DEFAULT GETDATE(), \r\n                                            [Offset] FLOAT NOT NULL DEFAULT 0,\r\n                                            [ChannelId] INT NOT NULL,\r\n                                            [DeviceId] INT NOT NULL,\r\n                                            [GroupId] INT NOT NULL,\r\n                                            [TagId] INT NOT NULL,\r\n                                        );\r\n                                    END";
		obj2.ExecuteNonQuery();
		return true;
	}

	public bool Update(DataLog dataLog)
	{
        using SqlConnection sqlConnection = new SqlConnection(string.Format(SqlServerBase.FormatConnectionString, dataLog.ServerName, "master", dataLog.Login, dataLog.Password));
		SqlCommand obj = new SqlCommand
		{
			Connection = sqlConnection,
			CommandType = CommandType.Text
		};
		sqlConnection.Open();
		obj.CommandText = "USE master;";
		obj.ExecuteNonQuery();
		obj.CommandText = "ALTER DATABASE " + dataLog.OldDataLogName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
		obj.ExecuteNonQuery();
		obj.CommandText = $"ALTER DATABASE {dataLog.OldDataLogName} MODIFY NAME = {dataLog.DataLogName};";
		obj.ExecuteNonQuery();
		obj.CommandText = "ALTER DATABASE " + dataLog.DataLogName + " SET MULTI_USER;";
		obj.ExecuteNonQuery();
		return true;
	}

	public bool Delete(DataLog dataLog)
	{
		string connectionString = string.Format(SqlServerBase.FormatConnectionString, dataLog.ServerName, "master", dataLog.Login, dataLog.Password);
		string commandText = string.Format("IF EXISTS(SELECT * FROM sys.sysdatabases WHERE name = '{0}')\r\n                                            BEGIN   \r\n                                                DROP DATABASE {0}\r\n                                            END", dataLog.ServerName);
		using SqlConnection sqlConnection = new SqlConnection(connectionString);
		SqlCommand obj = new SqlCommand
		{
			Connection = sqlConnection,
			CommandType = CommandType.Text,
			CommandText = commandText
		};
		sqlConnection.Open();
		return obj.ExecuteNonQuery() > 0;
	}

	public string? GetDatabase(DataLog dataLog)
	{
		string result = null;
		using SqlConnection sqlConnection = new SqlConnection(string.Format(SqlServerBase.FormatConnectionString, dataLog.ServerName, "master", dataLog.Login, dataLog.Password));
		SqlCommand obj = new SqlCommand
		{
			Connection = sqlConnection,
			CommandType = CommandType.Text,
			CommandText = $"SELECT * FROM sys.sysdatabases WHERE name = '{dataLog.DataLogName}'"
		};
		sqlConnection.Open();
		SqlDataReader sqlDataReader = obj.ExecuteReader();
		while (sqlDataReader.Read())
		{
			result = sqlDataReader.GetString(0);
		}
		return result;
	}
}
