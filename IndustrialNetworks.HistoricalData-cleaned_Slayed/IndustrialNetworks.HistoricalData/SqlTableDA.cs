using System.Data;
using NetStudio.Common.Historiant;
using NetStudio.Common.Manager;
using NetStudio.Database.SqlServer;
using Microsoft.Data.SqlClient;

namespace NetStudio.HistoricalData;

public class SqlTableDA : SqlServerBase
{
	public bool Create(DataLog dataLog, Channel channel)
	{
        using (SqlConnection sqlConnection = new SqlConnection($"Data Source={dataLog.ServerName};Initial Catalog={dataLog.DataLogName};User ID={dataLog.Login};Password={dataLog.Password};Connect Timeout=60;Persist Security Info=True;User Instance=False;TrustServerCertificate=True;"))
		{
			SqlCommand obj = new SqlCommand
			{
				Connection = sqlConnection,
				CommandType = CommandType.Text
			};
			sqlConnection.Open();
			obj.CommandText = "USE " + dataLog.DataLogName;
			obj.ExecuteNonQuery();
			obj.CommandText = "CREATE TABLE Channels\r\n                                    (\r\n\t                                    Id INT NOT NULL,\r\n\t                                    [Name] VARCHAR(160) NOT NULL,\r\n\t                                    [Description] NVARCHAR(256),\r\n\t                                    -- Thêm rằng buộc cho bảng.\r\n\t                                    CONSTRAINT [PK_Id] PRIMARY KEY (Id),\r\n\t                                    CONSTRAINT [UQ_Name]  UNIQUE ([Name])\r\n                                    )";
			obj.ExecuteNonQuery();
			obj.CommandText = "USE " + dataLog.DataLogName;
			obj.ExecuteNonQuery();
		}
		using SqlConnection sqlConnection2 = new SqlConnection(base.ConnectionString);
		SqlCommand obj2 = new SqlCommand
		{
			Connection = sqlConnection2,
			CommandType = CommandType.Text
		};
		sqlConnection2.Open();
		obj2.CommandText = "IF NOT EXISTS(SELECT * FROM sys.objects WHERE name = 'HistoricalData')\r\n                                    BEGIN                                                            \r\n                                        CREATE TABLE HistoricalData\r\n                                        (\r\n                                            [ChannelId] INT NOT NULL,\r\n                                            [DeviceId] INT NOT NULL,\r\n                                            [GroupId] INT NOT NULL,\r\n                                            [TagId] INT NOT NULL,\r\n                                            [DTime] DATETIME NOT NULL DEFAULT GETDATE(), \r\n                                            [Value] decimal(10, 3) NOT NULL DEFAULT 0\r\n                                        );\r\n                                    END";
		obj2.ExecuteNonQuery();
		return true;
	}
}
