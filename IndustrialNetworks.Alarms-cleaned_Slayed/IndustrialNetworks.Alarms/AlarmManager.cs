using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.Alarms;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;

namespace NetStudio.Alarms;

public class AlarmManager
{
	public const string TAG = "Alarms";

	private CancellationTokenSource? cancellationTokenSource;

	private Dictionary<string, Tag> _Tags;

	private List<AnalogAlarm> _AnalogAlarms;

	public AlarmManager(List<AnalogAlarm> analogAlarms, Dictionary<string, Tag> tags)
	{
		_AnalogAlarms = analogAlarms;
		_Tags = tags;
	}

	public void Start()
	{
		if (_AnalogAlarms != null && _AnalogAlarms.Count > 0)
		{
			cancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
			Thread thread = new Thread(delegate(object? obj)
			{
				OnAlarmRuntime((CancellationToken)obj);
			});
			thread.IsBackground = true;
			thread.Start(cancellationTokenSource.Token);
		}
	}

	public async Task StopAsync()
	{
		if (cancellationTokenSource != null)
		{
			await cancellationTokenSource.CancelAsync();
		}
	}

	public void OnAlarmRuntime(CancellationToken cancellation)
	{
		Thread.Sleep(2000);
		DriverDataSource.AnalogAlarms.Clear();
		DriverDataSource.DiscreteAlarms.Clear();
		bool flag = false;
		while (!cancellation.IsCancellationRequested)
		{
			try
			{
				if (_AnalogAlarms == null || _AnalogAlarms.Count <= 0)
				{
					continue;
				}
				foreach (AnalogAlarm item in _AnalogAlarms)
				{
					if (!_Tags.ContainsKey(item.TagName))
					{
						continue;
					}
					AnalogAlarm analogAlarm = DriverDataSource.AnalogAlarms.Where((AnalogAlarm analogAlarm_0) => analogAlarm_0.TagName == item.TagName && analogAlarm_0.AlarmText == item.AlarmText).FirstOrDefault();
					switch (item.LimitMode)
					{
					case LimitMode.Lower:
						if (item.LimitValue > _Tags[item.TagName].Value)
						{
							if (analogAlarm == null)
							{
								flag = true;
								item.DTime = DateTime.Now;
								DriverDataSource.AnalogAlarms.Add(item);
								if (DriverDataSource.OnAnalogAlarmChanged != null)
								{
									DriverDataSource.OnAnalogAlarmChanged(item, ListChangedType.ItemAdded);
								}
							}
						}
						else if (analogAlarm != null && analogAlarm.Status == AlarmStatus.Acknowledge && DriverDataSource.AnalogAlarms.Remove(analogAlarm))
						{
							analogAlarm.Status = AlarmStatus.None;
							if (DriverDataSource.OnAnalogAlarmChanged != null)
							{
								DriverDataSource.OnAnalogAlarmChanged(analogAlarm, ListChangedType.ItemDeleted);
							}
						}
						break;
					case LimitMode.Equal:
					{
                                if ((_Tags[item.TagName].DataType != 0) ? ((bool)(item.LimitValue == _Tags[item.TagName].Value)) : ((bool)(item.LimitValue == 1m == _Tags[item.TagName].Value)))
						{
							if (analogAlarm == null)
							{
								flag = true;
								item.DTime = DateTime.Now;
								DriverDataSource.AnalogAlarms.Add(item);
								if (DriverDataSource.OnAnalogAlarmChanged != null)
								{
									DriverDataSource.OnAnalogAlarmChanged(item, ListChangedType.ItemAdded);
								}
							}
						}
						else if (analogAlarm != null && analogAlarm.Status == AlarmStatus.Acknowledge && DriverDataSource.AnalogAlarms.Remove(analogAlarm))
						{
							analogAlarm.Status = AlarmStatus.None;
							if (DriverDataSource.OnAnalogAlarmChanged != null)
							{
								DriverDataSource.OnAnalogAlarmChanged(analogAlarm, ListChangedType.ItemDeleted);
							}
						}
						break;
					}
					case LimitMode.Higher:
						if (item.LimitValue < _Tags[item.TagName].Value)
						{
							if (analogAlarm == null)
							{
								flag = true;
								item.DTime = DateTime.Now;
								DriverDataSource.AnalogAlarms.Add(item);
								if (DriverDataSource.OnAnalogAlarmChanged != null)
								{
									DriverDataSource.OnAnalogAlarmChanged(item, ListChangedType.ItemAdded);
								}
							}
						}
						else if (analogAlarm != null && analogAlarm.Status == AlarmStatus.Acknowledge && DriverDataSource.AnalogAlarms.Remove(analogAlarm))
						{
							analogAlarm.Status = AlarmStatus.None;
							if (DriverDataSource.OnAnalogAlarmChanged != null)
							{
								DriverDataSource.OnAnalogAlarmChanged(analogAlarm, ListChangedType.ItemDeleted);
							}
						}
						break;
					}
					if (flag && item.Logging)
					{
						flag = false;
					}
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				Thread.Sleep(100);
			}
		}
	}
}
