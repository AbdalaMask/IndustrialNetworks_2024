using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CoreWCF;
using NetStudio.Common.Alarms;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Interfaces;
using NetStudio.DriverComm.Models;

namespace NetStudio.DriverManager;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
public class DriverServerManagerPS : IDriverServerManager, IDisposable
{
	private IDataChangeEvent? client;

	private ClientInfo? clientInfo;

	public async Task<bool> Connect(ClientInfo clientInfo)
	{
		return await Task.Run(delegate
		{
			try
			{
				client = OperationContext.Current.GetCallbackChannel<IDataChangeEvent>();
				if (client != null)
				{
					if (DriverHelper.Protocol != null && DriverDataSource.Devices != null)
					{
						Parallel.ForEach((IEnumerable<Device>)DriverDataSource.Devices, (Action<Device>)delegate(Device device)
						{
							client.DeviceChanged($"{device.ChannelId}.{device.Name}", device.Status, device.Active, device.AutoReconnect);
							foreach (Group group in device.Groups)
							{
								client.TagsChanged(group.Tags);
							}
						});
						if (DriverDataSource.AnalogAlarms.Any())
						{
							client.AnalogAlarmListChanged(DriverDataSource.AnalogAlarms.ToList());
						}
						if (DriverDataSource.Logs.Any())
						{
							client.LogListChanged(DriverDataSource.Logs.ToList());
						}
					}
					AddEvents();
				}
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Connect=>Exception: " + ex.Message);
			}
			return false;
		});
	}

	public async Task<bool> Disconnect(ClientInfo? clientInfo)
	{
		
		return await Task.Run(delegate
		{
			try
			{
				RemoveEvents();
				if (clientInfo != null && DriverDataSource.Clients.ContainsKey(clientInfo.Id))
				{
					DriverDataSource.Clients.Remove(clientInfo.Id);
				}
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Disconnect=>Exception: " + ex.Message);
			}
			finally
			{
				client = null;
			}
			return false;
		});
	}

	private void AddEvents()
	{
		if (DriverDataSource.Devices.Any())
		{
			lock (DriverDataSource.Devices)
			{
				foreach (Device device in DriverDataSource.Devices)
				{
					device.PropertyChanged += OnDeviceChanged;
				}
			}
		}
		if (DriverDataSource.Tags.Any())
		{
			lock (DriverDataSource.Tags)
			{
				foreach (Tag value in DriverDataSource.Tags.Values)
				{
					value.PropertyChanged += OnTagChanged;
				}
			}
		}
		DriverDataSource.OnAnalogAlarmChanged = (EventAnalogAlarmChanged)Delegate.Combine(DriverDataSource.OnAnalogAlarmChanged, new EventAnalogAlarmChanged(OnAnalogAlarmsChanged));
		DriverDataSource.OnIpsLogChanged = (EventIpsLogChanged)Delegate.Combine(DriverDataSource.OnIpsLogChanged, new EventIpsLogChanged(OnLogChanged));
	}

	private void RemoveEvents()
	{
		try
		{
			foreach (Device device in DriverDataSource.Devices)
			{
				lock (device)
				{
					device.PropertyChanged -= OnDeviceChanged;
				}
			}
			foreach (Tag value in DriverDataSource.Tags.Values)
			{
				lock (value)
				{
					value.PropertyChanged -= OnTagChanged;
				}
			}
			DriverDataSource.OnAnalogAlarmChanged = (EventAnalogAlarmChanged)Delegate.Remove(DriverDataSource.OnAnalogAlarmChanged, new EventAnalogAlarmChanged(OnAnalogAlarmsChanged));
			DriverDataSource.OnIpsLogChanged = (EventIpsLogChanged)Delegate.Remove(DriverDataSource.OnIpsLogChanged, new EventIpsLogChanged(OnLogChanged));
		}
		catch (Exception)
		{
		}
	}

	public void Dispose()
	{
		try
		{
			Disconnect(clientInfo);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Dispose=>Exception: " + ex.Message);
		}
	}

	public async Task<IPSResult> WriteTag(string tagName, dynamic value)
	{
		return await DriverHelper.Protocol.WriteTagAsync(tagName, (object)value);
	}

	public async Task<IPSResult> WriteBlock(Block block)
	{
		return await DriverHelper.Protocol.WriteBlockAsync(block);
	}

	public async Task<ApiResponse> ClearLog()
	{
		return await Task.Run(delegate
		{
			ApiResponse apiResponse = new ApiResponse();
			try
			{
				lock (DriverDataSource.Logs)
				{
					DriverDataSource.Logs.Clear();
					apiResponse.Success = true;
					apiResponse.Message = "Clear log successful!";
				}
			}
			catch (Exception ex)
			{
				apiResponse.Message = ex.Message;
			}
			return apiResponse;
		});
	}

	public async Task<ApiResponse> StartAsync()
	{
		ApiResponse result = new ApiResponse();
		try
		{
			await DriverHelper.Protocol.StartAsync();
			result.Success = true;
			result.Message = "Driver server started successfully!";
			return result;
		}
		catch (Exception ex)
		{
			result.Message = ex.Message;
		}
		return result;
	}

	public async Task<ApiResponse> StopAsync()
	{
		ApiResponse result = new ApiResponse();
		try
		{
			await DriverHelper.Protocol.StopAsync();
			result.Success = true;
			result.Message = "Driver server stopped successfully!";
			return result;
		}
		catch (Exception ex)
		{
			result.Message = ex.Message;
		}
		return result;
	}

	public async Task<ApiResponse> Restart()
	{
		ApiResponse result = new ApiResponse();
		try
		{
			await DriverHelper.Protocol.RestartAsync();
			result.Success = true;
			result.Message = "Driver server restarted successfully!";
		}
		catch (Exception ex)
		{
			result.Message = ex.Message;
		}
		return result;
	}

	public async Task<ApiResponse> CheckConnectAsync()
	{
		return await Task.FromResult(new ApiResponse
		{
			Success = true,
			Message = "Connected"
		});
	}

	private async void OnDeviceChanged(object? sender, PropertyChangedEventArgs e)
	{
		try
		{
			Device device = (Device)sender;
			if (device != null && client != null)
			{
				client.DeviceChangedAsync($"{device.ChannelId}.{device.Name}", device.Status, device.Active, device.AutoReconnect);
			}
		}
		catch (CommunicationObjectAbortedException ex)
		{
			await Disconnect(clientInfo);
			Console.WriteLine("CommunicationObjectAbortedException: " + ex.Message);
		}
		catch (Exception ex2)
		{
			Console.WriteLine("OnTagChanged=>Exception: " + ex2.Message);
		}
	}

	private async void OnTagChanged(object? sender, PropertyChangedEventArgs e)
	{
		try
		{
			if (sender != null && e.PropertyName != "Time" && client != null)
			{
				Tag tag = (Tag)sender;
				client.TagChanged(tag.FullName, tag.Status, (object)tag.Value);
			}
		}
		catch (CommunicationObjectAbortedException ex)
		{
			await Disconnect(clientInfo);
			Console.WriteLine("CommunicationObjectAbortedException: " + ex.Message);
		}
		catch (Exception ex2)
		{
			Console.WriteLine("OnTagChanged=>Exception: " + ex2.Message);
		}
	}

	private async void OnLogChanged(IpsLog? ipsLog_0)
	{
		try
		{
			if (ipsLog_0 != null && client != null)
			{
				client.LogChangedAsync(ipsLog_0);
			}
		}
		catch (CommunicationObjectAbortedException)
		{
			await Disconnect(clientInfo);
		}
		catch (Exception)
		{
		}
	}

	private async void OnAnalogAlarmsChanged(AnalogAlarm? alarm, ListChangedType type)
	{
		if (alarm == null || client == null)
		{
			return;
		}
		try
		{
			switch (type)
			{
			case ListChangedType.ItemDeleted:
				client.AnalogAlarmChangedAsync(alarm, ListChangedType.ItemDeleted);
				break;
			case ListChangedType.ItemAdded:
				client.AnalogAlarmChangedAsync(alarm, ListChangedType.ItemAdded);
				break;
			}
		}
		catch (CommunicationObjectAbortedException)
		{
			await Disconnect(clientInfo);
		}
		catch (Exception)
		{
		}
	}

	public List<AnalogAlarm> GetAlarms()
	{
		return DriverDataSource.AnalogAlarms.ToList();
	}

	public List<AlarmClass> GetAlarmClasses()
	{
		return DriverDataSource.IndusProtocol.Alarms.AlarmClasses;
	}

	public Task<List<AnalogAlarm>> GetAlarmsAsync()
	{
		return Task.FromResult(DriverDataSource.AnalogAlarms.ToList());
	}

	public Task<List<AlarmClass>> GetAlarmClassesAsync()
	{
		return Task.FromResult(DriverDataSource.IndusProtocol.Alarms.AlarmClasses);
	}

	public ApiResponse OnAcknowledge(AnalogAlarm alarm)
	{
		
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			AnalogAlarm analogAlarm = DriverDataSource.AnalogAlarms.Where((AnalogAlarm analogAlarm_0) => analogAlarm_0.TagName == alarm.TagName && analogAlarm_0.AlarmText == alarm.AlarmText).FirstOrDefault();
			if (analogAlarm != null)
			{
				analogAlarm.Status = alarm.Status;
			}
			apiResponse.Success = true;
			apiResponse.Message = "Write data: successfully.";
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public Task<ApiResponse> OnAcknowledgeAsync(AnalogAlarm alarm)
	{
		
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			AnalogAlarm analogAlarm = DriverDataSource.AnalogAlarms.Where((AnalogAlarm analogAlarm_0) => analogAlarm_0.TagName == alarm.TagName && analogAlarm_0.AlarmText == alarm.AlarmText).FirstOrDefault();
			if (analogAlarm != null)
			{
				analogAlarm.Status = alarm.Status;
			}
			apiResponse.Success = true;
			apiResponse.Message = "Write data: successfully.";
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return Task.FromResult(apiResponse);
	}

	public ApiResponse OnAcknowledgeAll(List<AnalogAlarm> alarms)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			foreach (AnalogAlarm alarm in alarms)
			{
				AnalogAlarm analogAlarm = DriverDataSource.AnalogAlarms.Where((AnalogAlarm analogAlarm_0) => analogAlarm_0.TagName == alarm.TagName && analogAlarm_0.AlarmText == alarm.AlarmText).FirstOrDefault();
				if (analogAlarm != null)
				{
					analogAlarm.Status = alarm.Status;
				}
			}
			apiResponse.Success = true;
			apiResponse.Message = "Write data: successfully.";
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return apiResponse;
	}

	public Task<ApiResponse> OnAcknowledgeAllAsync(List<AnalogAlarm> alarms)
	{
		ApiResponse apiResponse = new ApiResponse();
		try
		{
			foreach (AnalogAlarm alarm in alarms)
			{
				AnalogAlarm analogAlarm = DriverDataSource.AnalogAlarms.Where((AnalogAlarm analogAlarm_0) => analogAlarm_0.TagName == alarm.TagName && analogAlarm_0.AlarmText == alarm.AlarmText).FirstOrDefault();
				if (analogAlarm != null)
				{
					analogAlarm.Status = alarm.Status;
				}
			}
			apiResponse.Success = true;
			apiResponse.Message = "Write data: successfully.";
		}
		catch (Exception ex)
		{
			apiResponse.Message = ex.Message;
		}
		return Task.FromResult(apiResponse);
	}

	public List<Channel>? GetChannels()
	{
		List<Channel> result = null;
		ApiResponse apiResponse = new EditManger().Read();
		if (apiResponse.Success && apiResponse.Data != null && apiResponse.Data != null)
		{
			result = ((IndustrialProtocol)apiResponse.Data).Channels;
		}
		return result;
	}

	public async Task<List<Channel>?> GetChannelsAsync()
	{
		List<Channel> result = null;
		ApiResponse apiResponse = new EditManger().Read();
		if (apiResponse.Success && apiResponse.Data != null && apiResponse.Data != null)
		{
			result = ((IndustrialProtocol)apiResponse.Data).Channels;
		}
		return await Task.FromResult(result);
	}
}
