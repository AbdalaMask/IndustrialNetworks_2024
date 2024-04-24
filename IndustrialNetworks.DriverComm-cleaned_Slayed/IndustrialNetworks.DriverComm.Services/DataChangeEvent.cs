using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using NetStudio.Common.Alarms;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Interfaces;
using NetStudio.DriverComm.Models;

namespace NetStudio.DriverComm.Services;

[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
public class DataChangeEvent : IDataChangeEvent
{
	public void DeviceChanged(string deviceName, DeviceStatus status, bool active, bool autoReconnect)
	{
		if (ClientDataSource.Devices.ContainsKey(deviceName))
		{
			ClientDataSource.Devices[deviceName].Status = status;
			ClientDataSource.Devices[deviceName].Active = active;
			ClientDataSource.Devices[deviceName].AutoReconnect = autoReconnect;
		}
	}

	public async Task DeviceChangedAsync(string deviceName, DeviceStatus status, bool active, bool autoReconnect)
	{
	
		await Task.Run(delegate
		{
			if (ClientDataSource.Devices.ContainsKey(deviceName))
			{
				ClientDataSource.Devices[deviceName].Status = status;
				ClientDataSource.Devices[deviceName].Active = active;
				ClientDataSource.Devices[deviceName].AutoReconnect = autoReconnect;
			}
		});
	}

	public void TagsChanged(List<Tag> tags)
	{
		foreach (Tag tag in tags)
		{
			if (ClientDataSource.Tags.ContainsKey(tag.FullName))
			{
				ClientDataSource.Tags[tag.FullName].Value = (object)tag.Value;
				ClientDataSource.Tags[tag.FullName].Status = tag.Status;
			}
		}
	}

	public void TagChanged(string tagName, TagStatus status, dynamic value)
	{
		if (ClientDataSource.Tags.ContainsKey(tagName))
		{
			ClientDataSource.Tags[tagName].Value = (object)value;
			ClientDataSource.Tags[tagName].Status = status;
		}
	}

	public async Task TagChangedAsync(string tagName, TagStatus status, dynamic value)
	{
		
		
		await Task.Run(delegate
		{
			if (ClientDataSource.Tags.ContainsKey(tagName))
			{
				ClientDataSource.Tags[tagName].Value = (object)value;
				ClientDataSource.Tags[tagName].Status = status;
			}
		});
	}

	public void AnalogAlarmListChanged(List<AnalogAlarm> alarms)
	{
		if (alarms == null)
		{
			return;
		}
		ClientDataSource.AnalogAlarms.Clear();
		foreach (AnalogAlarm alarm in alarms)
		{
			ClientDataSource.AnalogAlarms.Add(alarm);
		}
	}

	public void AnalogAlarmChangedAsync(AnalogAlarm alarm, ListChangedType listChangedType)
	{
		
		if (alarm == null)
		{
			return;
		}
		switch (listChangedType)
		{
		case ListChangedType.ItemDeleted:
		{
			AnalogAlarm analogAlarm = ClientDataSource.AnalogAlarms.Where((AnalogAlarm analogAlarm_0) => analogAlarm_0.TagName == alarm.TagName && analogAlarm_0.AlarmText == alarm.AlarmText).FirstOrDefault();
			if (analogAlarm != null)
			{
				ClientDataSource.AnalogAlarms.Remove(analogAlarm);
			}
			break;
		}
		case ListChangedType.ItemAdded:
			ClientDataSource.AnalogAlarms.Add(alarm);
			break;
		}
	}

	public void LogChangedAsync(IpsLog ipsLog_0)
	{
		
		if (ipsLog_0 != null)
		{
			switch (ipsLog_0.LogType)
			{
			case IpsLogType.Add:
				ClientDataSource.AddLog(ipsLog_0);
				break;
			case IpsLogType.Update:
			{
				IpsLog ipsLog2 = ClientDataSource.Logs.Where((IpsLog ipsLog_1) => ipsLog_1.Message == ipsLog_0.Message).FirstOrDefault();
				if (ipsLog2 != null)
				{
					ipsLog2.Counter = ipsLog_0.Counter;
					ipsLog2.Message = ipsLog_0.Message;
					ipsLog2.Time = ipsLog_0.Time;
				}
				else
				{
					ClientDataSource.AddLog(ipsLog_0);
				}
				break;
			}
			case IpsLogType.Remove:
			{
				IpsLog ipsLog = ClientDataSource.Logs.Where((IpsLog ipsLog_1) => ipsLog_1.Message == ipsLog_0.Message).FirstOrDefault();
				if (ipsLog != null)
				{
					ClientDataSource.RemoveLog(ipsLog);
				}
				break;
			}
			}
		}
		else
		{
			ClientDataSource.RemoveLogAll();
		}
		ClientDataSource.Logs.ResetBindings();
	}

	public void LogListChanged(List<IpsLog> logs)
	{
		if (logs != null)
		{
			foreach (IpsLog ipsLog_0 in logs)
			{
				switch (ipsLog_0.LogType)
				{
				case IpsLogType.Add:
					ClientDataSource.AddLog(ipsLog_0);
					break;
				case IpsLogType.Update:
				{
					IpsLog ipsLog2 = ClientDataSource.Logs.Where((IpsLog ipsLog_1) => ipsLog_1.Message == ipsLog_0.Message).FirstOrDefault();
					if (ipsLog2 != null)
					{
						ipsLog2.Counter = ipsLog_0.Counter;
						ipsLog2.Message = ipsLog_0.Message;
						ipsLog2.Time = ipsLog_0.Time;
					}
					else
					{
						ClientDataSource.AddLog(ipsLog_0);
					}
					break;
				}
				case IpsLogType.Remove:
				{
					IpsLog ipsLog = ClientDataSource.Logs.Where((IpsLog ipsLog_1) => ipsLog_1.Message == ipsLog_0.Message).FirstOrDefault();
					if (ipsLog != null)
					{
						ClientDataSource.RemoveLog(ipsLog);
					}
					break;
				}
				}
			}
		}
		else
		{
			ClientDataSource.RemoveLogAll();
		}
		ClientDataSource.Logs.ResetBindings();
	}
}
