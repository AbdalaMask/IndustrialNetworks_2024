using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace NetStudio.Database.SqlServer;

public class SqlServerBase
{
	public static readonly string FormatConnectionString = "Data Source={0};Initial Catalog={1};User ID={2};Password={3};Connect Timeout=8;Persist Security Info=True;User Instance=False;TrustServerCertificate=True;";

	public string ConnectionString { get; set; } = string.Empty;


	public bool TestConnection(string connectionString)
	{
		using SqlConnection sqlConnection = new SqlConnection(connectionString);
		SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(name) from sys.databases", sqlConnection);
		sqlConnection.Open();
		return sqlCommand.ExecuteScalar() != null;
	}

	public async Task<bool> TestConnectionAsync(string connectionString)
	{
		using SqlConnection connection = new SqlConnection(connectionString);
		SqlCommand sqlCommand = new SqlCommand("SELECT COUNT(name) from sys.databases", connection);
		connection.Open();
		return await sqlCommand.ExecuteScalarAsync() != null;
	}

	protected int InsertTableWithReturnID(string tableName, string[] columnNames, object[] values, out int autoID)
	{
		int result = 0;
		autoID = 0;
		using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
		{
			SqlCommand sqlCommand = new SqlCommand();
			sqlCommand.Connection = sqlConnection;
			sqlCommand.CommandType = CommandType.Text;
			string text = "Insert into " + tableName + "(";
			string text2 = " Values(";
			for (int i = 0; i < columnNames.Length; i++)
			{
				if (values[i] != null)
				{
					text = text + columnNames[i] + ",";
					text2 = text2 + "@" + columnNames[i] + ",";
				}
			}
			text = text.Remove(text.Length - 1);
			text2 = text2.Remove(text2.Length - 1);
			text = text + ")" + text2 + ")";
			text += "; SET @AutoID = SCOPE_IDENTITY();";
			sqlCommand.CommandText = text;
			for (int j = 0; j < columnNames.Length; j++)
			{
				if (values[j] != null)
				{
					sqlCommand.Parameters.AddWithValue(columnNames[j], values[j]);
				}
			}
			SqlParameter sqlParameter = new SqlParameter("@AutoID", SqlDbType.Int);
			sqlParameter.Direction = ParameterDirection.Output;
			sqlCommand.Parameters.Add(sqlParameter);
			sqlConnection.Open();
			result = sqlCommand.ExecuteNonQuery();
			try
			{
				if (sqlParameter.Value != null)
				{
					autoID = (int)sqlParameter.Value;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}");
			}
		}
		return result;
	}

	protected int InsertTable(string tableName, string[] columnNames, object[] values)
	{
        using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.Connection = sqlConnection;
		sqlCommand.CommandType = CommandType.Text;
		string text = "Insert into " + tableName + "(";
		string text2 = " Values(";
		for (int i = 0; i < columnNames.Length; i++)
		{
			if (values[i] != null)
			{
				text = text + columnNames[i] + ",";
				text2 = text2 + "@" + columnNames[i] + ",";
			}
		}
		text = text.Remove(text.Length - 1);
		text2 = text2.Remove(text2.Length - 1);
		text = text + ")" + text2 + ")";
		sqlCommand.CommandText = text;
		for (int j = 0; j < columnNames.Length; j++)
		{
			if (values[j] != null)
			{
				sqlCommand.Parameters.AddWithValue(columnNames[j], values[j]);
			}
		}
		sqlConnection.Open();
		return sqlCommand.ExecuteNonQuery();
	}

	protected int InsertTable(string tableName, string[] columnNames, object[] values, SqlCommand sqlCommand_0)
	{
		sqlCommand_0.CommandType = CommandType.Text;
		sqlCommand_0.Connection.Open();
		string text = "Insert into " + tableName + "(";
		string text2 = " Values(";
		for (int i = 0; i < columnNames.Length; i++)
		{
			if (values[i] != null)
			{
				text = text + columnNames[i] + ",";
				text2 = text2 + "@" + columnNames[i] + ",";
			}
		}
		text = text.Remove(text.Length - 1);
		text2 = text2.Remove(text2.Length - 1);
		text = text + ")" + text2 + ")";
		sqlCommand_0.CommandText = text;
		for (int j = 0; j < columnNames.Length; j++)
		{
			if (values[j] != null)
			{
				sqlCommand_0.Parameters.AddWithValue(columnNames[j], values[j]);
			}
		}
		int result = sqlCommand_0.ExecuteNonQuery();
		sqlCommand_0.Connection.Close();
		return result;
	}

	protected int UpdateTable(string tableName, string[] columnNames, object[] values, string[] keyColumns, object[] keyValues)
	{
        using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.Connection = sqlConnection;
		sqlCommand.CommandType = CommandType.Text;
		string text = "Update " + tableName + " set ";
		for (int i = 0; i < columnNames.Length; i++)
		{
			text = text + columnNames[i] + "=@" + columnNames[i] + ",";
		}
		text = text.Remove(text.Length - 1);
		string text2 = " Where ";
		for (int j = 0; j < keyColumns.Length; j++)
		{
			text2 = text2 + keyColumns[j] + "=@Original_" + keyColumns[j] + " AND ";
		}
		text2 = text2.Remove(text2.Length - 4);
		sqlCommand.CommandText = text + text2;
		for (int k = 0; k < columnNames.Length; k++)
		{
			if (values[k] != null)
			{
				sqlCommand.Parameters.AddWithValue(columnNames[k], values[k]);
			}
			else
			{
				sqlCommand.Parameters.AddWithValue(columnNames[k], DBNull.Value);
			}
		}
		for (int l = 0; l < keyColumns.Length; l++)
		{
			sqlCommand.Parameters.AddWithValue("@Original_" + keyColumns[l], keyValues[l]);
		}
		sqlConnection.Open();
		return sqlCommand.ExecuteNonQuery();
	}

	protected int UpdateTable(string tableName, string[] columnNames, object[] values, string[] keyColumns, object[] keyValues, SqlCommand sqlCommand_0)
	{
		sqlCommand_0.CommandType = CommandType.Text;
		sqlCommand_0.Connection.Open();
		string text = "Update " + tableName + " set ";
		for (int i = 0; i < columnNames.Length; i++)
		{
			text = text + columnNames[i] + "=@" + columnNames[i] + ",";
		}
		text = text.Remove(text.Length - 1);
		string text2 = " Where ";
		for (int j = 0; j < keyColumns.Length; j++)
		{
			text2 = text2 + keyColumns[j] + "=@Original_" + keyColumns[j] + " AND ";
		}
		text2 = text2.Remove(text2.Length - 4);
		sqlCommand_0.CommandText = text + text2;
		for (int k = 0; k < columnNames.Length; k++)
		{
			if (values[k] != null)
			{
				sqlCommand_0.Parameters.AddWithValue(columnNames[k], values[k]);
			}
			else
			{
				sqlCommand_0.Parameters.AddWithValue(columnNames[k], DBNull.Value);
			}
		}
		for (int l = 0; l < keyColumns.Length; l++)
		{
			sqlCommand_0.Parameters.AddWithValue("@Original_" + keyColumns[l], keyValues[l]);
		}
		int result = sqlCommand_0.ExecuteNonQuery();
		sqlCommand_0.Connection.Close();
		return result;
	}

	protected int DeleteTable(string tableName, string[] keyColumns, object[] keyValues)
	{
        using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.Connection = sqlConnection;
		sqlCommand.CommandType = CommandType.Text;
		string text = "Delete " + tableName;
		string text2 = " Where ";
		for (int i = 0; i < keyColumns.Length; i++)
		{
			text2 = text2 + keyColumns[i] + "=@" + keyColumns[i] + " AND ";
		}
		text2 = text2.Remove(text2.Length - 4);
		sqlCommand.CommandText = text + text2;
		for (int j = 0; j < keyColumns.Length; j++)
		{
			sqlCommand.Parameters.AddWithValue(keyColumns[j], keyValues[j]);
		}
		sqlConnection.Open();
		return sqlCommand.ExecuteNonQuery();
	}

	protected int DeleteTable(string tableName, string[] keyColumns, object[] keyValues, SqlCommand sqlCommand_0)
	{
		sqlCommand_0.CommandType = CommandType.Text;
		sqlCommand_0.Connection.Open();
		string text = "Delete " + tableName;
		string text2 = " Where ";
		for (int i = 0; i < keyColumns.Length; i++)
		{
			text2 = text2 + keyColumns[i] + "=@" + keyColumns[i] + " AND ";
		}
		text2 = text2.Remove(text2.Length - 4);
		sqlCommand_0.CommandText = text + text2;
		for (int j = 0; j < keyColumns.Length; j++)
		{
			sqlCommand_0.Parameters.AddWithValue(keyColumns[j], keyValues[j]);
		}
		int result = sqlCommand_0.ExecuteNonQuery();
		sqlCommand_0.Connection.Close();
		return result;
	}

	protected int RecordExisted(string tableName, string primaryColumnName, object value)
	{
		using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
		if (value.GetType().ToString().Equals("System.String"))
		{
			value = "'" + value?.ToString() + "'";
		}
		SqlCommand obj = new SqlCommand
		{
			Connection = sqlConnection,
			CommandType = CommandType.Text,
			CommandText = $"Select count(*) from {tableName} where {primaryColumnName}={value}"
		};
		sqlConnection.Open();
		return (int)obj.ExecuteScalar();
	}

	protected int GetNewId(string xTableName, string xColumnName)
	{
		int num = 0;
		using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
		SqlCommand obj = new SqlCommand
		{
			Connection = sqlConnection,
			CommandType = CommandType.Text,
			CommandText = $"SELECT MAX({xColumnName}) FROM {xTableName}"
		};
		sqlConnection.Open();
		SqlDataReader sqlDataReader = obj.ExecuteReader();
		while (sqlDataReader.Read())
		{
			try
			{
				num = sqlDataReader.GetInt16(0);
			}
			catch (Exception)
			{
				num = 0;
			}
		}
		return num + 1;
	}

	protected int GetNewId(string xTableName, string xColumnName, bool bool_0)
	{
		int num = 0;
		using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
		SqlCommand obj = new SqlCommand
		{
			Connection = sqlConnection,
			CommandType = CommandType.Text,
			CommandText = $"SELECT MAX({xColumnName}) FROM {xTableName} WHERE IsALSLink = {(bool_0 ? 1 : 0)}"
		};
		sqlConnection.Open();
		SqlDataReader sqlDataReader = obj.ExecuteReader();
		while (sqlDataReader.Read())
		{
			try
			{
				num = sqlDataReader.GetInt16(0);
			}
			catch (Exception)
			{
				num = 0;
			}
		}
		return num + 1;
	}

	protected List<T> SelectCollection<T>(string[] columnNames, SqlCommand sqlCommand_0) where T : new()
	{
		List<T> list = new List<T>();
		DataTable dataTable = new DataTable();
		new SqlDataAdapter(sqlCommand_0).Fill(dataTable);
		foreach (DataRow row in dataTable.Rows)
		{
			T val = new T();
			Type type = val.GetType();
			for (int i = 0; i < columnNames.Length; i++)
			{
				if (row[columnNames[i]] != DBNull.Value)
				{
					type.GetProperty(columnNames[i]).SetValue(val, row[columnNames[i]], null);
				}
			}
			list.Add(val);
		}
		return list;
	}

	protected DataTable SelectCollection(SqlCommand sqlCommand_0)
	{
		DataTable dataTable = new DataTable("tblData");
		using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
		sqlCommand_0.Connection = sqlConnection;
		sqlConnection.Open();
		new SqlDataAdapter(sqlCommand_0).Fill(dataTable);
		return dataTable;
	}

	protected DataTable SelectCollection(string tableName, SqlCommand sqlCommand_0)
	{
		DataTable dataTable = new DataTable(tableName);
		using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
		sqlCommand_0.Connection = sqlConnection;
		sqlConnection.Open();
		new SqlDataAdapter(sqlCommand_0).Fill(dataTable);
		return dataTable;
	}

	protected int ExecuteSQLQuery(SqlCommand sqlCommand_0)
	{
        using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
		sqlCommand_0.Connection = sqlConnection;
		sqlConnection.Open();
		return sqlCommand_0.ExecuteNonQuery();
	}

	protected short GetIdByParentId(string tableName, string colId, string parentId, int value)
	{
		short num = 0;
		using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.Connection = sqlConnection;
		sqlCommand.CommandType = CommandType.Text;
		sqlCommand.CommandText = $"SELECT MAX({colId}) FROM {tableName} WHERE {parentId} = {value}";
		sqlConnection.Open();
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		while (sqlDataReader.Read())
		{
			try
			{
				num = sqlDataReader.GetInt16(0);
			}
			catch (Exception)
			{
				num = 0;
			}
		}
		return (short)(num + 1);
	}
}
