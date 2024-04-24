using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.AsrsLink;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.Database.SqlServer;
using NetStudio.DriverComm.Interfaces;

namespace NetStudio.AsrsLink;

public class AsrsLinkManager
{
	public const string TAG = "AS/RS Link";

	private string _ConnectionString = string.Empty;

	private CancellationTokenSource? cancellationTokenSource;

	public AsrsServer _AsrsServer;

	private Dictionary<string, Tag> _Tags;

	private IProtocolManager _Protocol;

	public AsrsLinkManager(IProtocolManager protocol, AsrsServer server, Dictionary<string, Tag> tags)
	{
		_Protocol = protocol;
		_AsrsServer = server;
		_Tags = tags;
		_ConnectionString = string.Format(SqlServerBase.FormatConnectionString, server.ServerName, server.DatabaseName, server.Login, server.Password);
		if (server == null || server.Tables.Count <= 0)
		{
			return;
		}
		foreach (AsrsTable table in server.Tables)
		{
			if (table.Rows.Count <= 0)
			{
				continue;
			}
			foreach (AsrsRow row in table.Rows)
			{
				row.LinkToSqlCommandText = $"UPDATE {table.Name} SET {row.ColumnName} = {{0}} WHERE {row.ColumnId} = '{row.ValueOfId}'";
				row.LinkToPlcCommandText = $"SELECT {row.ColumnName} FROM {table.Name} WHERE {row.ColumnId} = '{row.ValueOfId}'";
			}
		}
	}

	public void Start()
	{
		cancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
		Thread thread = new Thread(delegate(object? obj)
		{
			OnAsrsConnector((CancellationToken)obj);
		});
		thread.IsBackground = true;
		thread.Start(cancellationTokenSource.Token);
	}

	public async Task StopAsync()
	{
		if (cancellationTokenSource != null)
		{
			await cancellationTokenSource.CancelAsync();
		}
	}

	public void OnAsrsConnector(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			if (_AsrsServer.Active)
			{
				if (_AsrsServer.Synchronized)
				{
					Synchronized();
				}
				else
				{
					Asynchronous();
				}
				Thread.Sleep(100);
			}
			else
			{
				Thread.Sleep(5000);
			}
		}
	}

	private async void Synchronized()
	{
		AsrsTableDA asrsTableDA = new AsrsTableDA(_ConnectionString);
		foreach (AsrsTable table in _AsrsServer.Tables)
		{
			if (table.Rows == null || table.Rows.Count <= 0)
			{
				Thread.Sleep(100);
				continue;
			}
			foreach (AsrsRow row in table.Rows)
			{
				try
				{
					if (!((_Tags[row.TagName].Value != null) ? true : false))
					{
						continue;
					}
					dynamic valueByColumn = asrsTableDA.GetValueByColumn(row.LinkToPlcCommandText);
					switch (row.Mode)
					{
					case OperatingMode.All:
						switch (row.Trigger)
						{
						}
						break;
					case OperatingMode.WriteToController:
						switch (row.Trigger)
						{
						case AsrsTrigger.Positive:
						{
							bool positive = ((!(valueByColumn is bool)) ? (!((valueByColumn == 0) ? true : false)) : ((bool)valueByColumn));
							if (_Tags[row.TagName].Value == false && positive && (row.Value == false || (object)row.Value == null))
							{
								await _Protocol.WriteTagAsync(row.TagName, true);
							}
							row.Value = positive;
							break;
						}
						case AsrsTrigger.Negative:
						{
							bool negative = ((!(valueByColumn is bool)) ? (!((valueByColumn == 0) ? true : false)) : ((bool)valueByColumn));
							if (_Tags[row.TagName].Value == true && !negative && (row.Value == true || (object)row.Value == null))
							{
								await _Protocol.WriteTagAsync(row.TagName, false);
							}
							row.Value = negative;
							break;
						}
						case AsrsTrigger.Directive:
							if (_Tags[row.TagName].Value != valueByColumn)
							{
								await _Protocol.WriteTagAsync(row.TagName, (object)valueByColumn);
							}
							break;
						case AsrsTrigger.UponChange:
							if (_Tags[row.TagName].Value != valueByColumn)
							{
								await _Protocol.WriteTagAsync(row.TagName, (object)valueByColumn);
							}
							break;
						}
						break;
					case OperatingMode.WriteToSQL:
						switch (row.Trigger)
						{
						case AsrsTrigger.Positive:
							if (_Tags[row.TagName].Value == true)
							{
								if (valueByColumn)
								{
									await _Protocol.WriteTagAsync(row.TagName, false);
								}
								else
								{
									asrsTableDA.ExecuteNonQuery(string.Format(row.LinkToSqlCommandText, 1));
								}
							}
							break;
						case AsrsTrigger.Negative:
							if (_Tags[row.TagName].Value == false)
							{
								if (valueByColumn)
								{
									asrsTableDA.ExecuteNonQuery(string.Format(row.LinkToSqlCommandText, 0));
								}
								else
								{
									await _Protocol.WriteTagAsync(row.TagName, true);
								}
							}
							break;
						case AsrsTrigger.Directive:
							if (_Tags[row.TagName].Value != valueByColumn)
							{
								asrsTableDA.ExecuteNonQuery(string.Format(row.LinkToSqlCommandText, ((_Tags[row.TagName].Value == true) ? true : false) ? 1 : 0));
							}
							break;
						case AsrsTrigger.UponChange:
							if (_Tags[row.TagName].Value != valueByColumn)
							{
								if (_Tags[row.TagName].DataType == DataType.STRING)
								{
									asrsTableDA.ExecuteNonQuery(string.Format(row.LinkToSqlCommandText, "'" + _Tags[row.TagName].Value + "'"));
								}
								else
								{
									asrsTableDA.ExecuteNonQuery(string.Format(row.LinkToSqlCommandText, _Tags[row.TagName].Value));
								}
							}
							break;
						}
						break;
					}
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private async void Asynchronous()
	{
		AsrsTableDA asrsTableDA = new AsrsTableDA(_ConnectionString);
		foreach (AsrsTable table in _AsrsServer.Tables)
		{
			if (table.Rows == null || table.Rows.Count <= 0)
			{
				Thread.Sleep(100);
				continue;
			}
			foreach (AsrsRow row in table.Rows)
			{
				try
				{
					dynamic val = asrsTableDA.GetValueByColumn(row.LinkToPlcCommandText);
					if (_Tags[row.TagName].Value != null && row.Value != _Tags[row.TagName].Value && (row.Mode == OperatingMode.WriteToSQL || row.Mode == OperatingMode.All))
					{
						row.Value = (object)_Tags[row.TagName].Value;
						switch (row.Trigger)
						{
						case AsrsTrigger.Positive:
							if (row.Value)
							{
								asrsTableDA.ExecuteNonQuery(string.Format(row.LinkToSqlCommandText, (_Tags[row.TagName].Value ? true : false) ? 1 : 0));
							}
							break;
						case AsrsTrigger.Negative:
							if ((!row.Value))
							{
								asrsTableDA.ExecuteNonQuery(string.Format(row.LinkToSqlCommandText, (_Tags[row.TagName].Value ? true : false) ? 1 : 0));
							}
							break;
						case AsrsTrigger.Directive:
							asrsTableDA.ExecuteNonQuery(string.Format(row.LinkToSqlCommandText, (_Tags[row.TagName].Value ? true : false) ? 1 : 0));
							break;
						case AsrsTrigger.UponChange:
							if (_Tags[row.TagName].Value != null)
							{
								if (_Tags[row.TagName].DataType == DataType.STRING)
								{
									asrsTableDA.ExecuteNonQuery(string.Format(row.LinkToSqlCommandText, "'" + _Tags[row.TagName].Value + "'"));
								}
								else
								{
									asrsTableDA.ExecuteNonQuery(string.Format(row.LinkToSqlCommandText, _Tags[row.TagName].Value));
								}
							}
							break;
						}
					}
					else
					{
						if (!((_Tags[row.TagName].Value != null && row.Value != val && (row.Mode == OperatingMode.WriteToController || row.Mode == OperatingMode.All)) ? true : false))
						{
							continue;
						}
						row.Value = (object)val;
						if (_Tags[row.TagName].DataType == DataType.BOOL)
						{
							val = (val ? true : false);
						}
						switch (row.Trigger)
						{
						case AsrsTrigger.Positive:
							if (row.Value)
							{
								await _Protocol.WriteTagAsync(row.TagName, (object)val);
							}
							break;
						case AsrsTrigger.Negative:
							if ((!row.Value))
							{
								await _Protocol.WriteTagAsync(row.TagName, (object)val);
							}
							break;
						case AsrsTrigger.UponChange:
							if (val != null)
							{
								await _Protocol.WriteTagAsync(row.TagName, (object)val);
							}
							break;
						case AsrsTrigger.Directive:
							await _Protocol.WriteTagAsync(row.TagName, (object)val);
							break;
						}
						continue;
					}
				}
				catch (Exception)
				{
				}
			}
		}
	}
}
