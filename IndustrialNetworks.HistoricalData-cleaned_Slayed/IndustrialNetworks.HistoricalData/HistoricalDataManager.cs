using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Historiant;
using NetStudio.Common.Manager;

namespace NetStudio.HistoricalData;

public class HistoricalDataManager
{
	public const string TAG = "Historical Data";

	private IHistoricalData _dataLogService;

	private List<DataLog> _dataLogs;

	private Dictionary<string, Tag> _Tags;

	public List<NetStudio.Common.Historiant.HistoricalData> _dhCollection = new List<NetStudio.Common.Historiant.HistoricalData>();

	private Dictionary<string, TimeKey> _TimeKeys = new Dictionary<string, TimeKey>();

	private CancellationTokenSource? cancellationTokenSource;

	public HistoricalDataManager(List<DataLog> dataLogs, Dictionary<string, Tag> tags)
	{
		_dataLogs = dataLogs;
		_dataLogs = _dataLogs ?? new List<DataLog>();
		_Tags = tags ?? new Dictionary<string, Tag>();
		if (_dataLogs.Count <= 0)
		{
			return;
		}

        foreach (DataLog dataLog in _dataLogs)
		{
			if (dataLog.DataLogName == "<Add new>")
			{
				continue;
			}
			NetStudio.Common.Historiant.HistoricalData historicalData = new NetStudio.Common.Historiant.HistoricalData();
			switch (dataLog.StorageType)
			{
			case StorageType.Database:
				historicalData.DataString = $"Data Source={dataLog.ServerName};Initial Catalog={dataLog.DataLogName};User ID={dataLog.Login};Password={dataLog.Password};Connect Timeout=60;Persist Security Info=True;User Instance=False;TrustServerCertificate=True;";
				_dataLogService = new SqlServerHistoricalData();
                    HDResult hDResult = _dataLogService.GetSingle(dataLog);
                    if (hDResult == null || (hDResult != null && !hDResult.Success))
				{
					hDResult = _dataLogService.Create(dataLog);
				}
				break;
			}
			Dictionary<string, List<LoggingTag>> dictionary = new Dictionary<string, List<LoggingTag>>();
			DateTime now = DateTime.Now;
			foreach (LoggingTag loggingTag in dataLog.LoggingTags)
			{
				TimeKey value = new TimeKey
				{
					CycleName = loggingTag.CycleName,
					StartDate = now,
					EndDate = now
				};
				if (!dictionary.ContainsKey(loggingTag.CycleName))
				{
					dictionary.Add(loggingTag.CycleName, new List<LoggingTag>());
					dictionary[loggingTag.CycleName].Add(loggingTag);
					_TimeKeys.Add(loggingTag.CycleName, value);
				}
				else
				{
					dictionary[loggingTag.CycleName].Add(loggingTag);
				}
			}
			historicalData.DataLog = dataLog;
			historicalData.LoggingTags = dictionary;
			_dhCollection.Add(historicalData);
		}
	}

	public void Start()
	{
		cancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
		Thread thread = new Thread(delegate(object? obj)
		{
			OnLoggingTags((CancellationToken)obj);
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

	private void OnLoggingTags(CancellationToken cancellationToken)
	{
		_ = DateTime.Now;
		while (!cancellationToken.IsCancellationRequested && _dhCollection.Count > 0)
		{
			try
			{
				foreach (NetStudio.Common.Historiant.HistoricalData item2 in _dhCollection)
				{
					if (item2.LoggingTags == null)
					{
						continue;
					}
					foreach (KeyValuePair<string, List<LoggingTag>> item in item2.LoggingTags)
					{
						if (!string.IsNullOrEmpty(item.Key) && !string.IsNullOrWhiteSpace(item.Key))
						{
							_TimeKeys[item.Key].EndDate = DateTime.Now;
							if (!((_TimeKeys[item.Key].EndDate - _TimeKeys[item.Key].StartDate).TotalSeconds >= (double)item.Value[0].Cycle.Seconds))
							{
								continue;
							}
							_TimeKeys[item.Key].StartDate = _TimeKeys[item.Key].EndDate;
							Parallel.ForEach((IEnumerable<LoggingTag>)item.Value, (Action<LoggingTag>)delegate(LoggingTag loggingtg)
							{
								if (_Tags[loggingtg.TagName].Value != null)
								{
									if (_Tags[loggingtg.TagName].DataType == DataType.BOOL)
									{
										loggingtg.Value = ((_Tags[loggingtg.TagName].Value ? true : false) ? 1 : 0);
									}
									else
									{
										loggingtg.Value = Convert.ToDecimal(_Tags[loggingtg.TagName].Value);
									}
									loggingtg.Offset = _Tags[loggingtg.TagName].Offset;
									loggingtg.DTime = _TimeKeys[item.Key].EndDate;
								}
							});
							switch (item2.DataLog.StorageType)
							{
							case StorageType.Database:
								new LoggingTagDA(item2.DataString).InsertMultipes(item.Value);
								break;
							}
							continue;
						}
						switch (item2.DataLog.StorageType)
						{
						case StorageType.Database:
						{
							LoggingTagDA loggingTagDA = new LoggingTagDA(item2.DataString);
							foreach (LoggingTag item3 in item.Value)
							{
								if (_Tags[item3.TagName].Value == null)
								{
									continue;
								}
								if (_Tags[item3.TagName].DataType == DataType.BOOL)
								{
									decimal num = ((_Tags[item3.TagName].Value ? true : false) ? 1 : 0);
									if (item3.Value != num)
									{
										item3.Value = num;
										item3.Offset = _Tags[item3.TagName].Offset;
										item3.DTime = DateTime.Now;
										loggingTagDA.Insert(item3);
									}
								}
								else if (item3.Value != _Tags[item3.TagName].Value)
								{
									item3.Value = Convert.ToDecimal(_Tags[item3.TagName].Value);
									item3.Offset = _Tags[item3.TagName].Offset;
									item3.DTime = DateTime.Now;
									loggingTagDA.Insert(item3);
								}
							}
							break;
						}
						}
					}
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				Thread.Sleep(5);
			}
		}
	}

	public HDResult AddDataLog(DataLog dataLog)
	{
		HDResult hDResult = new HDResult();
		try
		{
			hDResult = _dataLogService.Create(dataLog);
		}
		catch (Exception ex)
		{
			hDResult.Message = ex.Message;
		}
		return hDResult;
	}

	public HDResult EditDataLog(DataLog dataLog)
	{
		HDResult hDResult = new HDResult();
		try
		{
			hDResult = _dataLogService.Update(dataLog);
		}
		catch (Exception ex)
		{
			hDResult.Message = ex.Message;
		}
		return hDResult;
	}
}
