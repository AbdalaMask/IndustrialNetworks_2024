using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using NetStudio.Common.AsrsLink;
using NetStudio.Database.SqlServer;
using Microsoft.Data.SqlClient;

namespace NetStudio.AsrsLink;

public class AsrsTableDA : SqlServerBase
{
	private const string ColumnName = "ColumnName";

	private const string DataType = "DataType";

	private const string MaxLength = "MaxLength";

	private const string Precision = "Precision";

	private const string Scale = "Scale";

	private const string IsNullable = "IsNullable";

	private const string PrimaryKey = "PrimaryKey";

	public AsrsTableDA(string connectionString)
	{
		base.ConnectionString = connectionString;
	}

	public List<AsrsTable> GetTableNames(string database)
	{
		List<AsrsTable> list = new List<AsrsTable>();
		using SqlConnection sqlConnection = new SqlConnection(base.ConnectionString);
		SqlCommand obj = new SqlCommand
		{
			Connection = sqlConnection,
			CommandType = CommandType.Text,
			CommandText = "SELECT TABLE_NAME AS [TableName] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG='" + database + "'"
		};
		sqlConnection.Open();
		SqlDataReader sqlDataReader = obj.ExecuteReader();
		int num = 0;
		while (sqlDataReader.Read())
		{
			num = list.Count + 1;
			list.Add(new AsrsTable
			{
				Id = num,
				Name = DataReaderExtensions.GetString(sqlDataReader, "TableName")
			});
		}
		return list;
	}

	public async Task<List<AsrsTable>> GetTableNamesAsync(string database)
	{
		List<AsrsTable> result = new List<AsrsTable>();
		using (SqlConnection connection = new SqlConnection(base.ConnectionString))
		{
			SqlCommand obj = new SqlCommand
			{
				Connection = connection,
				CommandType = CommandType.Text,
				CommandText = "SELECT TABLE_NAME AS [TableName] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG='" + database + "'"
			};
			connection.Open();
			SqlDataReader sqlDataReader = await obj.ExecuteReaderAsync();
			while (sqlDataReader.Read())
			{
				int id = result.Count + 1;
				result.Add(new AsrsTable
				{
					Id = id,
					Name = DataReaderExtensions.GetString(sqlDataReader, "TableName")
				});
			}
		}
		return result;
	}

	public List<SqlColumnInfo> GetColumnsInfo(string tableName)
	{
		List<SqlColumnInfo> list = new List<SqlColumnInfo>();
		using SqlConnection sqlConnection = new SqlConnection(base.ConnectionString);
		SqlCommand obj = new SqlCommand
		{
			Connection = sqlConnection,
			CommandType = CommandType.Text,
			CommandText = $"SELECT c.name 'ColumnName', t.Name 'DataType', c.max_length 'MaxLength', c.precision 'Precision' , c.scale  'Scale', c.is_nullable 'IsNullable', ISNULL(i.is_primary_key, 0) 'PrimaryKey'\r\n                            FROM sys.columns c INNER JOIN sys.types t ON c.user_type_id = t.user_type_id LEFT OUTER JOIN sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id\r\n                            LEFT OUTER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id\r\n                            WHERE c.object_id = OBJECT_ID('{tableName}')"
		};
		sqlConnection.Open();
		SqlDataReader sqlDataReader = obj.ExecuteReader();
		int num = 0;
		while (sqlDataReader.Read())
		{
			num = list.Count + 1;
			list.Add(new SqlColumnInfo
			{
				Id = num,
				ColumnName = DataReaderExtensions.GetString(sqlDataReader, "ColumnName"),
				DataType = DataReaderExtensions.GetString(sqlDataReader, "DataType")
			});
		}
		return list;
	}

	public dynamic GetValueByColumn(string xCommandText)
	{
		object result = 0;
		DataTable dataTable = new DataTable("tblData");
		using (SqlConnection sqlConnection = new SqlConnection(base.ConnectionString))
		{
			SqlCommand selectCommand = new SqlCommand
			{
				CommandType = CommandType.Text,
				CommandText = xCommandText,
				Connection = sqlConnection
			};
			sqlConnection.Open();
			new SqlDataAdapter(selectCommand).Fill(dataTable);
			if (dataTable.Rows.Count > 0)
			{
				result = dataTable.Rows[0][0];
			}
		}
		return result;
	}

	public List<string> ExecuteSQLQuery(string tableName, string columnName)
	{
		List<string> list = new List<string>();
		using SqlConnection sqlConnection = new SqlConnection(base.ConnectionString);
		SqlCommand obj = new SqlCommand
		{
			Connection = sqlConnection,
			CommandType = CommandType.Text,
			CommandText = $"SELECT {columnName} FROM {tableName}"
		};
		sqlConnection.Open();
		SqlDataReader sqlDataReader = obj.ExecuteReader();
		while (sqlDataReader.Read())
		{
			list.Add($"{sqlDataReader[columnName]}");
		}
		return list;
	}

	public int ExecuteNonQuery(string commandText)
	{
        using SqlConnection sqlConnection = new SqlConnection(base.ConnectionString);
		sqlConnection.Open();
		return new SqlCommand
		{
			CommandType = CommandType.Text,
			CommandText = commandText,
			Connection = sqlConnection
		}.ExecuteNonQuery();
	}
}
