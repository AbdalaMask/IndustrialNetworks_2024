using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using NetStudio.Common.Alarms;
using NetStudio.Common.Manager;

namespace NetStudio.Common.IndusCom;

public class DriverDataSource
{
	public static AppSettings AppSettings = null;

	public static Dictionary<string, ClientInfo> Clients = new Dictionary<string, ClientInfo>();

	public static EventIpsLogChanged? OnIpsLogChanged = null;

	public static EventAnalogAlarmChanged? OnAnalogAlarmChanged = null;

	public static IndustrialProtocol IndusProtocol { get; set; } = null;


	public static BindingList<IpsLog> Logs { get; set; } = new BindingList<IpsLog>();


	public static List<Channel> Channels { get; set; } = new List<Channel>();


	public static BindingList<Device> Devices { get; set; } = new BindingList<Device>();


	public static Dictionary<string, Tag> Tags { get; set; } = new Dictionary<string, Tag>();


	public static BindingList<AnalogAlarm> AnalogAlarms { get; set; } = new BindingList<AnalogAlarm>();


	public static BindingList<DiscreteAlarm> DiscreteAlarms { get; set; } = new BindingList<DiscreteAlarm>();


	public static void SaveLog(string source, string message, EvenType evenType = EvenType.Error)
	{
		
	
		IpsLog ipsLog = Logs.FirstOrDefault((IpsLog ipsLog_0) => ipsLog_0 != null && ipsLog_0.Source == source && ipsLog_0.Message == message);
		if (ipsLog == null)
		{
			lock (Logs)
			{
				Logs.Insert(0, new IpsLog
				{
					EvenType = evenType,
					Source = source,
					Message = message,
					Time = DateTime.Now,
					Counter = 1u
				});
				return;
			}
		}
		lock (Logs)
		{
			ipsLog.Time = DateTime.Now;
			ipsLog.Counter++;
		}
	}

	public static void SaveLog(IpsLog ipsLog_0)
	{
		
		IpsLog ipsLog = Logs.FirstOrDefault((IpsLog ipsLog_1) => ipsLog_1 != null && ipsLog_1.Source == ipsLog_0.Source && ipsLog_1.Message == ipsLog_0.Message);
		if (ipsLog == null)
		{
			lock (Logs)
			{
				ipsLog_0.LogType = IpsLogType.Add;
				ipsLog_0.Time = DateTime.Now;
				Logs.Insert(0, ipsLog_0);
				if (OnIpsLogChanged != null)
				{
					OnIpsLogChanged(ipsLog_0);
				}
				return;
			}
		}
		lock (Logs)
		{
			ipsLog.Time = DateTime.Now;
			ipsLog.Counter++;
		}
	}

	public static void AddLogAsync(IpsLog ipsLog_0)
	{
		ipsLog_0.LogType = IpsLogType.Add;
		lock (Logs)
		{
			Logs.Insert(0, ipsLog_0);
			if (OnIpsLogChanged != null)
			{
				OnIpsLogChanged(ipsLog_0);
			}
		}
	}

	public static async Task<bool> UpdateLogAsync(IpsLog ipsLog_0)
	{
		
		return await Task.Run(delegate
		{
			lock (Logs)
			{
				ipsLog_0.LogType = IpsLogType.Update;
				ipsLog_0.Time = DateTime.Now;
				if (OnIpsLogChanged != null)
				{
					OnIpsLogChanged(ipsLog_0);
				}
			}
			return true;
		});
	}

	public static async Task<bool> RemoveLogAsync(IpsLog ipsLog_0)
	{
		
		return await Task.Run(delegate
		{
			ipsLog_0.LogType = IpsLogType.Remove;
			lock (Logs)
			{
				Logs.Remove(ipsLog_0);
				if (OnIpsLogChanged != null)
				{
					OnIpsLogChanged(ipsLog_0);
				}
			}
			return true;
		});
	}

	public static async Task<bool> RemoveLogAllAsync()
	{
		return await Task.Run(delegate
		{
			lock (Logs)
			{
				Logs.Clear();
				if (OnIpsLogChanged != null)
				{
					OnIpsLogChanged(null);
				}
			}
			return true;
		});
	}
}
