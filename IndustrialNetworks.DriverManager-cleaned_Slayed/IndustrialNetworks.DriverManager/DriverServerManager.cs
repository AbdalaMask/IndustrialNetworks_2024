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

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
public class DriverServerManager : IDriverServerManager, IDisposable
{
	public static Dictionary<string, IDataChangeEvent> Clients { get; set; } = new Dictionary<string, IDataChangeEvent>();


	public async Task<bool> Connect(ClientInfo clientInfo)
	{
		
		return await Task.Run(delegate
		{
			try
			{
				IDataChangeEvent client = OperationContext.Current.GetCallbackChannel<IDataChangeEvent>();
				if (client != null)
				{
					if (!Clients.ContainsKey(clientInfo.Id))
					{
						if (DriverDataSource.Devices != null && DriverDataSource.Logs != null)
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
						lock (Clients)
						{
							Clients.Add(clientInfo.Id, client);
						}
					}
					if (Clients.Count == 1)
					{
						AddEvents();
					}
				}
			}
			catch (Exception)
			{
			}
			return true;
		});
	}

	public async Task<bool> Disconnect(ClientInfo clientInfo)
	{
		
		return await Task.Run(delegate
		{
			try
			{
				if (Clients.Any() && Clients.ContainsKey(clientInfo.Id))
				{
					lock (Clients)
					{
						Clients.Remove(clientInfo.Id);
						if (Clients.Count == 0)
						{
							RemoveEvents();
						}
					}
				}
			}
			catch (Exception)
			{
			}
			return true;
		});
	}

	private void DisconnectAll()
	{
		try
		{
			if (Clients.Any())
			{
				RemoveEvents();
				Clients.Clear();
			}
		}
		catch (Exception)
		{
		}
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
			DisconnectAll();
		}
		catch (Exception)
		{
		}
	}

	private void OnDeviceChanged(object? sender, PropertyChangedEventArgs e)
	{
		try
		{
			if (Clients.Any())
			{
				Device device = (Device)sender;
				if (device == null)
				{
					return;
				}
				lock (Clients)
				{
					Parallel.ForEach((IEnumerable<KeyValuePair<string, IDataChangeEvent>>)Clients, (Action<KeyValuePair<string, IDataChangeEvent>>)delegate(KeyValuePair<string, IDataChangeEvent> client)
					{
						try
						{
							client.Value.DeviceChanged($"{device.ChannelId}.{device.Name}", device.Status, device.Active, device.AutoReconnect);
						}
						catch (CommunicationObjectAbortedException)
						{
							Clients.Remove(client.Key);
						}
						catch (Exception)
						{
						}
					});
					return;
				}
			}
			DisconnectAll();
		}
		catch (Exception)
		{
		}
	}

	private void OnTagChanged(object? sender, PropertyChangedEventArgs e)
	{
		try
		{
			if (!Clients.Any() || sender == null || !(e.PropertyName != "Time"))
			{
				return;
			}
			Tag tg = (Tag)sender;
			lock (Clients)
			{
				Parallel.ForEach((IEnumerable<KeyValuePair<string, IDataChangeEvent>>)Clients, (Action<KeyValuePair<string, IDataChangeEvent>>)delegate(KeyValuePair<string, IDataChangeEvent> client)
				{
					try
					{
						client.Value.TagChanged(tg.FullName, tg.Status, (object)tg.Value);
					}
					catch (CommunicationObjectAbortedException)
					{
						Clients.Remove(client.Key);
					}
					catch (Exception)
					{
					}
				});
			}
		}
		catch (Exception)
		{
		}
	}

	private void OnLogChanged(IpsLog? ipsLog_0)
	{
		
		if (!Clients.Any())
		{
			return;
		}
		lock (Clients)
		{
			Parallel.ForEach((IEnumerable<KeyValuePair<string, IDataChangeEvent>>)Clients, (Action<KeyValuePair<string, IDataChangeEvent>>)delegate(KeyValuePair<string, IDataChangeEvent> client)
			{
				try
				{
					client.Value.LogChangedAsync(ipsLog_0);
				}
				catch (CommunicationObjectAbortedException)
				{
					Clients.Remove(client.Key);
				}
				catch (Exception)
				{
				}
			});
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

	private void OnAnalogAlarmsChanged(AnalogAlarm? alarm, ListChangedType type)
	{
		
		if (alarm == null)
		{
			return;
		}
		lock (Clients)
		{
			Parallel.ForEach((IEnumerable<KeyValuePair<string, IDataChangeEvent>>)Clients, (Action<KeyValuePair<string, IDataChangeEvent>>)delegate(KeyValuePair<string, IDataChangeEvent> client)
			{
				try
				{
					switch (type)
					{
					case ListChangedType.ItemDeleted:
						client.Value.AnalogAlarmChangedAsync(alarm, ListChangedType.ItemDeleted);
						break;
					case ListChangedType.ItemAdded:
						client.Value.AnalogAlarmChangedAsync(alarm, ListChangedType.ItemAdded);
						break;
					}
				}
				catch (CommunicationObjectAbortedException)
				{
					Clients.Remove(client.Key);
				}
				catch (Exception)
				{
				}
			});
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
