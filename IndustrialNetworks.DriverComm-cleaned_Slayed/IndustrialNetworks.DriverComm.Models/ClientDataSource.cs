using System.Collections.Generic;
using System.ComponentModel;
using NetStudio.Common.Alarms;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;

namespace NetStudio.DriverComm.Models;

public static class ClientDataSource
{
	public static EventCommunicationStateChanged? OnCommunicationStateChanged = null;

	public static EventIpsLogChanged? OnIpsLogChanged = null;

	public static List<Channel> Channels { get; set; } = null;


	public static Dictionary<string, Device> Devices { get; set; } = new Dictionary<string, Device>();


	public static Dictionary<string, Tag> Tags { get; set; } = new Dictionary<string, Tag>();


	public static BindingList<IpsLog> Logs { get; set; } = new BindingList<IpsLog>();


	public static Dictionary<int, AlarmClass> AlarmClasses { get; set; } = new Dictionary<int, AlarmClass>();


	public static BindingList<AnalogAlarm> AnalogAlarms { get; set; } = new BindingList<AnalogAlarm>();


	public static BindingList<DiscreteAlarm> DiscreteAlarms { get; set; } = new BindingList<DiscreteAlarm>();


	public static void AddLog(IpsLog ipsLog_0)
	{
		lock (Logs)
		{
			Logs.Insert(0, ipsLog_0);
		}
		if (OnIpsLogChanged != null)
		{
			OnIpsLogChanged(ipsLog_0);
		}
	}

	public static void RemoveLog(IpsLog ipsLog_0)
	{
		lock (Logs)
		{
			Logs.Remove(ipsLog_0);
		}
		if (OnIpsLogChanged != null)
		{
			OnIpsLogChanged(ipsLog_0);
		}
	}

	public static void RemoveLogAll()
	{
		lock (Logs)
		{
			Logs.Clear();
		}
		if (OnIpsLogChanged != null)
		{
			OnIpsLogChanged(null);
		}
	}

	public static void ResetAll()
	{
		if (Channels != null)
		{
			Channels.Clear();
		}
		Devices.Clear();
		Tags.Clear();
		Logs.Clear();
		AnalogAlarms.Clear();
		DiscreteAlarms.Clear();
	}
}
