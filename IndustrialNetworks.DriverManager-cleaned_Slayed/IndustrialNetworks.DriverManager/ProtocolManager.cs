using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Alarms;
using NetStudio.AsrsLink;
using NetStudio.Common.Alarms;
using NetStudio.Common.AsrsLink;
using NetStudio.Common.Historiant;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Delta;
using NetStudio.DriverComm.Interfaces;
using NetStudio.DriverComm.Models;
using NetStudio.Fatek;
using NetStudio.HistoricalData;
using NetStudio.Keyence.MC.Ethernet;
using NetStudio.LS.Xgt.Cnet;
using NetStudio.LS.Xgt.FEnet;
using NetStudio.Mitsubishi.Dedicated;
using NetStudio.Mitsubishi.FXSerial;
using NetStudio.Mitsubishi.MC.Ethernet;
using NetStudio.Mitsubishi.SLMP;
using NetStudio.Modbus;
using NetStudio.Omron.Fins;
using NetStudio.Omron.HostLink;
using NetStudio.Panasonic.Mewtocol;
using NetStudio.Siemens;
using NetStudio.Vigor;

namespace NetStudio.DriverManager;

public class ProtocolManager : IProtocolManager, IAsyncDisposable
{
	public EditManger _EditManger;

	private readonly Dictionary<int, S7Driver> S7Drivers;

	private readonly Dictionary<int, NetStudio.Keyence.MC.Ethernet.MCDriver> KeyenceMCDrivers;

	private readonly Dictionary<int, SLMPDriver> SLMPDrivers;

	private readonly Dictionary<int, NetStudio.Mitsubishi.MC.Ethernet.MCDriver> MCDrivers;

	private readonly Dictionary<int, DedicatedDriver> DedicatedDrivers;

	private readonly Dictionary<int, FXSerialDriver> FxSerialDrivers;

	private readonly Dictionary<int, FinsDriver> FinsDrivers;

	private readonly Dictionary<int, HostLinkFinsDriver> HostLinkFinsDrivers;

	private readonly Dictionary<int, HostLinkCModeDriver> HostLinkCModeDrivers;

	private readonly Dictionary<int, MewtocolDriver> MewtocolDrivers;

	private readonly Dictionary<int, ModbusDriver> ModbusDrivers;

	private readonly Dictionary<int, NetStudio.LS.Xgt.Cnet.XgtDriver> CnetDrivers;

	private readonly Dictionary<int, NetStudio.LS.Xgt.FEnet.XgtDriver> FEnetDrivers;

	private readonly Dictionary<int, DeltaDriver> DeltaDrivers;

	private readonly Dictionary<int, VSDriver> VSDrivers;

	private readonly Dictionary<int, VBDriver> VBDrivers;

	private readonly Dictionary<int, FatekDriver> FatekDrivers;

	private HistoricalDataManager historicalDataManager;

	private AsrsLinkManager asrsLinkManager;

	private AlarmManager alarmManager;

	public ProtocolManager(AppSettings appSettings)
	{
		DriverDataSource.Logs.ListChanged += Logs_ListChanged;
		S7Drivers = new Dictionary<int, S7Driver>();
		KeyenceMCDrivers = new Dictionary<int, NetStudio.Keyence.MC.Ethernet.MCDriver>();
		SLMPDrivers = new Dictionary<int, SLMPDriver>();
		MCDrivers = new Dictionary<int, NetStudio.Mitsubishi.MC.Ethernet.MCDriver>();
		DedicatedDrivers = new Dictionary<int, DedicatedDriver>();
		FxSerialDrivers = new Dictionary<int, FXSerialDriver>();
		FinsDrivers = new Dictionary<int, FinsDriver>();
		HostLinkFinsDrivers = new Dictionary<int, HostLinkFinsDriver>();
		HostLinkCModeDrivers = new Dictionary<int, HostLinkCModeDriver>();
		MewtocolDrivers = new Dictionary<int, MewtocolDriver>();
		ModbusDrivers = new Dictionary<int, ModbusDriver>();
		CnetDrivers = new Dictionary<int, NetStudio.LS.Xgt.Cnet.XgtDriver>();
		FEnetDrivers = new Dictionary<int, NetStudio.LS.Xgt.FEnet.XgtDriver>();
		DeltaDrivers = new Dictionary<int, DeltaDriver>();
		VSDrivers = new Dictionary<int, VSDriver>();
		VBDrivers = new Dictionary<int, VBDriver>();
		FatekDrivers = new Dictionary<int, FatekDriver>();
		if (appSettings != null && appSettings.Settings != null)
		{
			_EditManger = _EditManger ?? new EditManger();
			DriverDataSource.AppSettings = appSettings;
			if (!File.Exists(appSettings.Settings.FileName) || (File.Exists(appSettings.Settings.FileName) && !appSettings.Settings.FileName.ToLower().Contains(".json")))
			{
				CreateFileProjectDefault();
			}
		}
	}

	private void CreateFileProjectDefault()
	{
		string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		DriverDataSource.AppSettings.Settings.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "IPS.json");
		if (!File.Exists(DriverDataSource.AppSettings.Settings.FileName))
		{
			_EditManger = _EditManger ?? new EditManger();
			_EditManger.Write(new IndustrialProtocol());
		}
	}

	private void Logs_ListChanged(object? sender, ListChangedEventArgs e)
	{
		try
		{
			if (DriverDataSource.Logs.Count > 512)
			{
				DriverDataSource.Logs.RemoveAt(DriverDataSource.Logs.Count - 1);
				DriverDataSource.Logs.ResetBindings();
			}
		}
		catch (Exception)
		{
		}
	}

	public async Task OnLoadProject()
	{
		if (DriverDataSource.AppSettings.Settings != null)
		{
			DriverServerManager.Clients.Clear();
			ApiResponse apiResponse = _EditManger.Read();
			if (apiResponse.Success && apiResponse.Data != null && apiResponse.Data != null)
			{
				DriverDataSource.IndusProtocol = (IndustrialProtocol)apiResponse.Data;
				await ParseChannel(DriverDataSource.IndusProtocol.Channels);
				await OnInitializeHistoricalDataAsync(DriverDataSource.IndusProtocol.DataLogs);
				await OnInitializeAsrsLinkAsync(DriverDataSource.IndusProtocol.AsrsServer);
				await OnInitializeAlarmAsync(DriverDataSource.IndusProtocol.Alarms.AnalogAlarms);
			}
		}
	}

	private async Task CLearAllAsync()
	{
		await Task.Run(delegate
		{
			DriverDataSource.Channels.Clear();
			DriverDataSource.Devices.Clear();
			DriverDataSource.Tags.Clear();
			DriverDataSource.Logs.Clear();
			S7Drivers.Clear();
			KeyenceMCDrivers.Clear();
			SLMPDrivers.Clear();
			MCDrivers.Clear();
			DedicatedDrivers.Clear();
			FxSerialDrivers.Clear();
			FinsDrivers.Clear();
			HostLinkFinsDrivers.Clear();
			HostLinkCModeDrivers.Clear();
			MewtocolDrivers.Clear();
			ModbusDrivers.Clear();
			CnetDrivers.Clear();
			FEnetDrivers.Clear();
			DeltaDrivers.Clear();
			VSDrivers.Clear();
			VBDrivers.Clear();
			FatekDrivers.Clear();
		});
	}

	private async Task ParseChannel(List<Channel> channelList)
	{
		if (channelList == null)
		{
			return;
		}
		await CLearAllAsync();
		DriverDataSource.Channels = channelList;
		foreach (Channel channel in DriverDataSource.Channels)
		{
			switch (channel.Protocol)
			{
			case IpsProtocolType.S7_TCP:
			{
				S7Driver s7Driver = new S7Driver(channel, DriverDataSource.Logs);
				S7Drivers.Add(channel.Id, s7Driver);
				foreach (Device device in channel.Devices)
				{
					DriverDataSource.Devices.Add(device);
				}
				foreach (KeyValuePair<string, Tag> tag in s7Driver.Tags)
				{
					DriverDataSource.Tags.Add(tag.Key, tag.Value);
				}
				break;
			}
			case IpsProtocolType.CNET_XGT_PROTOCOL:
			{
				NetStudio.LS.Xgt.Cnet.XgtDriver xgtDriver = new NetStudio.LS.Xgt.Cnet.XgtDriver(channel, DriverDataSource.Logs);
				CnetDrivers.Add(channel.Id, xgtDriver);
				foreach (Device value in xgtDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value);
				}
				foreach (KeyValuePair<string, Tag> tag2 in xgtDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag2.Key, tag2.Value);
				}
				break;
			}
			case IpsProtocolType.FENET_XGT_PROTOCOL:
			{
				NetStudio.LS.Xgt.FEnet.XgtDriver xgtDriver2 = new NetStudio.LS.Xgt.FEnet.XgtDriver(channel, DriverDataSource.Logs);
				FEnetDrivers.Add(channel.Id, xgtDriver2);
				foreach (Device value2 in xgtDriver2.Devices.Values)
				{
					DriverDataSource.Devices.Add(value2);
				}
				foreach (KeyValuePair<string, Tag> tag3 in xgtDriver2.Tags)
				{
					DriverDataSource.Tags.Add(tag3.Key, tag3.Value);
				}
				break;
			}
			case IpsProtocolType.MEWTOCOL_PROTOCOL:
			{
				MewtocolDriver mewtocolDriver = new MewtocolDriver(channel, DriverDataSource.Logs);
				MewtocolDrivers.Add(channel.Id, mewtocolDriver);
				foreach (Device value3 in mewtocolDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value3);
				}
				foreach (KeyValuePair<string, Tag> tag4 in mewtocolDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag4.Key, tag4.Value);
				}
				break;
			}
			case IpsProtocolType.MISUBISHI_MC_PROTOCOL:
			{
				NetStudio.Mitsubishi.MC.Ethernet.MCDriver mCDriver2 = new NetStudio.Mitsubishi.MC.Ethernet.MCDriver(channel, DriverDataSource.Logs);
				MCDrivers.Add(channel.Id, mCDriver2);
				foreach (Device value4 in mCDriver2.Devices.Values)
				{
					DriverDataSource.Devices.Add(value4);
				}
				foreach (KeyValuePair<string, Tag> tag5 in mCDriver2.Tags)
				{
					DriverDataSource.Tags.Add(tag5.Key, tag5.Value);
				}
				break;
			}
			case IpsProtocolType.MISUBISHI_SLMP_PROTOCOL:
			{
				SLMPDriver sLMPDriver = new SLMPDriver(channel, DriverDataSource.Logs);
				SLMPDrivers.Add(channel.Id, sLMPDriver);
				foreach (Device value5 in sLMPDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value5);
				}
				foreach (KeyValuePair<string, Tag> tag6 in sLMPDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag6.Key, tag6.Value);
				}
				break;
			}
			case IpsProtocolType.DEDICATED1_PROTOCOL:
			{
				DedicatedDriver dedicatedDriver2 = new DedicatedDriver(channel, DriverDataSource.Logs, ControlProcedure.Format1);
				DedicatedDrivers.Add(channel.Id, dedicatedDriver2);
				foreach (Device value6 in dedicatedDriver2.Devices.Values)
				{
					DriverDataSource.Devices.Add(value6);
				}
				foreach (KeyValuePair<string, Tag> tag7 in dedicatedDriver2.Tags)
				{
					DriverDataSource.Tags.Add(tag7.Key, tag7.Value);
				}
				break;
			}
			case IpsProtocolType.DEDICATED4_PROTOCOL:
			{
				DedicatedDriver dedicatedDriver = new DedicatedDriver(channel, DriverDataSource.Logs, ControlProcedure.Format4);
				DedicatedDrivers.Add(channel.Id, dedicatedDriver);
				foreach (Device value7 in dedicatedDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value7);
				}
				foreach (KeyValuePair<string, Tag> tag8 in dedicatedDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag8.Key, tag8.Value);
				}
				break;
			}
			case IpsProtocolType.FX_SERIAL_PROTOCOL:
			{
				FXSerialDriver fXSerialDriver = new FXSerialDriver(channel, DriverDataSource.Logs);
				FxSerialDrivers.Add(channel.Id, fXSerialDriver);
				foreach (Device value8 in fXSerialDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value8);
				}
				foreach (KeyValuePair<string, Tag> tag9 in fXSerialDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag9.Key, tag9.Value);
				}
				break;
			}
			case IpsProtocolType.FINS_TCP_PROTOCOL:
			case IpsProtocolType.FINS_UDP_PROTOCOL:
			{
				FinsDriver finsDriver = new FinsDriver(channel, DriverDataSource.Logs);
				FinsDrivers.Add(channel.Id, finsDriver);
				foreach (Device value9 in finsDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value9);
				}
				foreach (KeyValuePair<string, Tag> tag10 in finsDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag10.Key, tag10.Value);
				}
				break;
			}
			case IpsProtocolType.HOSTLINK_FINS_PROTOCOL:
			{
				HostLinkFinsDriver hostLinkFinsDriver = new HostLinkFinsDriver(channel, DriverDataSource.Logs);
				HostLinkFinsDrivers.Add(channel.Id, hostLinkFinsDriver);
				foreach (Device value10 in hostLinkFinsDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value10);
				}
				foreach (KeyValuePair<string, Tag> tag11 in hostLinkFinsDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag11.Key, tag11.Value);
				}
				break;
			}
			case IpsProtocolType.HOSTLINK_CMODE_PROTOCOL:
			{
				HostLinkCModeDriver val = new HostLinkCModeDriver(channel, DriverDataSource.Logs);
				HostLinkCModeDrivers.Add(channel.Id, val);
				foreach (Device value11 in val.Devices.Values)
				{
					DriverDataSource.Devices.Add(value11);
				}
				foreach (KeyValuePair<string, Tag> tag12 in val.Tags)
				{
					DriverDataSource.Tags.Add(tag12.Key, tag12.Value);
				}
				break;
			}
			case IpsProtocolType.MODBUS_TCP:
			case IpsProtocolType.MODBUS_RTU:
			case IpsProtocolType.MODBUS_ASCII:
			{
				ModbusDriver modbusDriver = new ModbusDriver(channel, DriverDataSource.Logs);
				ModbusDrivers.Add(channel.Id, modbusDriver);
				foreach (Device value12 in modbusDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value12);
				}
				foreach (KeyValuePair<string, Tag> tag13 in modbusDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag13.Key, tag13.Value);
				}
				break;
			}
			case IpsProtocolType.VS_PROTOCOL:
			{
				VSDriver vSDriver = new VSDriver(channel, DriverDataSource.Logs);
				VSDrivers.Add(channel.Id, vSDriver);
				foreach (Device value13 in vSDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value13);
				}
				foreach (KeyValuePair<string, Tag> tag14 in vSDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag14.Key, tag14.Value);
				}
				break;
			}
			case IpsProtocolType.VB_PROTOCOL:
			{
				VBDriver vBDriver = new VBDriver(channel, DriverDataSource.Logs);
				VBDrivers.Add(channel.Id, vBDriver);
				foreach (Device value14 in vBDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value14);
				}
				foreach (KeyValuePair<string, Tag> tag15 in vBDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag15.Key, tag15.Value);
				}
				break;
			}
			case IpsProtocolType.FATEK_PROTOCOL:
			{
				FatekDriver fatekDriver = new FatekDriver(channel, DriverDataSource.Logs);
				FatekDrivers.Add(channel.Id, fatekDriver);
				foreach (Device value15 in fatekDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value15);
				}
				foreach (KeyValuePair<string, Tag> tag16 in fatekDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag16.Key, tag16.Value);
				}
				break;
			}
			case IpsProtocolType.DELTA_ASCII:
			case IpsProtocolType.DELTA_RTU:
			case IpsProtocolType.DELTA_TCP:
			{
				DeltaDriver deltaDriver = new DeltaDriver(channel, DriverDataSource.Logs);
				DeltaDrivers.Add(channel.Id, deltaDriver);
				foreach (Device value16 in deltaDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value16);
				}
				foreach (KeyValuePair<string, Tag> tag17 in deltaDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag17.Key, tag17.Value);
				}
				break;
			}
			case IpsProtocolType.KEYENCE_MC_PROTOCOL:
			{
				NetStudio.Keyence.MC.Ethernet.MCDriver mCDriver = new NetStudio.Keyence.MC.Ethernet.MCDriver(channel, DriverDataSource.Logs);
				KeyenceMCDrivers.Add(channel.Id, mCDriver);
				foreach (Device value17 in mCDriver.Devices.Values)
				{
					DriverDataSource.Devices.Add(value17);
				}
				foreach (KeyValuePair<string, Tag> tag18 in mCDriver.Tags)
				{
					DriverDataSource.Tags.Add(tag18.Key, tag18.Value);
				}
				break;
			}
			}
		}
	}

	private async Task OnInitializeHistoricalDataAsync(List<DataLog> dataLogs)
	{
	
		await Task.Run(delegate
		{
			try
			{
				dataLogs = dataLogs ?? new List<DataLog>();
				Dictionary<string, Tag> dictionary = new Dictionary<string, Tag>();
				if (dataLogs.Count > 0)
				{
					foreach (DataLog item in dataLogs)
					{
						foreach (LoggingTag loggingTag in item.LoggingTags)
						{
							if (DriverDataSource.Tags.ContainsKey(loggingTag.TagName))
							{
								Tag value = DriverDataSource.Tags[loggingTag.TagName];
								dictionary.Add(loggingTag.TagName, value);
							}
						}
					}
				}
				historicalDataManager = new HistoricalDataManager(dataLogs, dictionary);
			}
			catch (Exception ex)
			{
				DriverDataSource.Logs.Insert(0, new IpsLog
				{
					Counter = 1u,
					EvenType = EvenType.Error,
					Source = "Historical Data",
					Message = ex.Message,
					Time = DateTime.Now
				});
			}
		});
	}

	private async Task OnInitializeAsrsLinkAsync(AsrsServer server)
	{
		 
		await Task.Run(delegate
		{
			try
			{
				server = server ?? new AsrsServer();
				Dictionary<string, Tag> dictionary = new Dictionary<string, Tag>();
				if (server.Tables.Count > 0)
				{
					foreach (AsrsTable table in server.Tables)
					{
						foreach (AsrsRow row in table.Rows)
						{
							if (DriverDataSource.Tags.ContainsKey(row.TagName))
							{
								Tag value = DriverDataSource.Tags[row.TagName];
								dictionary.Add(row.TagName, value);
							}
						}
					}
				}
				asrsLinkManager = new AsrsLinkManager(DriverHelper.Protocol, server, dictionary);
			}
			catch (Exception ex)
			{
				DriverDataSource.Logs.Insert(0, new IpsLog
				{
					Counter = 1u,
					EvenType = EvenType.Error,
					Source = "AS/RS Link",
					Message = ex.Message,
					Time = DateTime.Now
				});
			}
		});
	}

	private async Task OnInitializeAlarmAsync(List<AnalogAlarm> analogAlarms)
	{
		 
		await Task.Run(delegate
		{
			try
			{
				analogAlarms = analogAlarms ?? new List<AnalogAlarm>();
				Dictionary<string, Tag> dictionary = new Dictionary<string, Tag>();
				if (analogAlarms.Count > 0)
				{
					foreach (AnalogAlarm item in analogAlarms)
					{
						if (DriverDataSource.Tags.ContainsKey(item.TagName))
						{
							Tag value = DriverDataSource.Tags[item.TagName];
							if (!dictionary.ContainsKey(item.TagName))
							{
								dictionary.Add(item.TagName, value);
							}
						}
					}
				}
				alarmManager = new AlarmManager(analogAlarms, dictionary);
			}
			catch (Exception ex)
			{
				DriverDataSource.Logs.Insert(0, new IpsLog
				{
					Counter = 1u,
					EvenType = EvenType.Error,
					Source = "Alarms",
					Message = ex.Message,
					Time = DateTime.Now
				});
			}
		});
	}

	public async Task StartAsync()
	{
	 
		MachineInfo machine = NetStudio.Common.Security.LicenseManager.GetMachineInfo();
		NetStudio.Common.Security.License license = NetStudio.Common.Security.LicenseManager.GetLicense();
		DriverHelper.Polling = true;
		if (DriverDataSource.Channels != null && DriverDataSource.Channels.Any())
		{
			await Parallel.ForEachAsync(DriverDataSource.Channels, new ParallelOptions
			{
				MaxDegreeOfParallelism = 3
			}, async delegate(Channel channel, CancellationToken _)
			{
				await Task.Run(delegate
				{
					switch (channel.Protocol)
					{
					case IpsProtocolType.S7_TCP:
						S7Drivers[channel.Id].Start();
						break;
					case IpsProtocolType.CNET_XGT_PROTOCOL:
						CnetDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.FENET_XGT_PROTOCOL:
						FEnetDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.MEWTOCOL_PROTOCOL:
						MewtocolDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.MISUBISHI_MC_PROTOCOL:
						MCDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.MISUBISHI_SLMP_PROTOCOL:
						SLMPDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.DEDICATED1_PROTOCOL:
						DedicatedDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.DEDICATED4_PROTOCOL:
						DedicatedDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.FX_SERIAL_PROTOCOL:
						FxSerialDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.FINS_TCP_PROTOCOL:
					case IpsProtocolType.FINS_UDP_PROTOCOL:
						FinsDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.HOSTLINK_FINS_PROTOCOL:
						HostLinkFinsDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.HOSTLINK_CMODE_PROTOCOL:
						HostLinkCModeDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.MODBUS_TCP:
					case IpsProtocolType.MODBUS_RTU:
					case IpsProtocolType.MODBUS_ASCII:
						ModbusDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.VS_PROTOCOL:
						VSDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.VB_PROTOCOL:
						VBDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.FATEK_PROTOCOL:
						FatekDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.S7_MPI:
					case IpsProtocolType.S7_PPI:
					case IpsProtocolType.ASCII_PROTOCOL:
						break;
					case IpsProtocolType.DELTA_ASCII:
					case IpsProtocolType.DELTA_RTU:
					case IpsProtocolType.DELTA_TCP:
						DeltaDrivers[channel.Id].Start();
						break;
					case IpsProtocolType.KEYENCE_MC_PROTOCOL:
						KeyenceMCDrivers[channel.Id].Start();
						break;
					}
				});
			});
		}
		if (historicalDataManager != null)
		{
			historicalDataManager.Start();
		}
		if (alarmManager != null)
		{
			alarmManager.Start();
		}
		if (asrsLinkManager != null)
		{
			asrsLinkManager.Start();
		}
	}

	public async Task StopAsync()
	{
		DriverHelper.Polling = false;
		await Parallel.ForEachAsync(S7Drivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(S7Driver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(KeyenceMCDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(NetStudio.Keyence.MC.Ethernet.MCDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(MCDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(NetStudio.Mitsubishi.MC.Ethernet.MCDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(SLMPDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(SLMPDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(DedicatedDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(DedicatedDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(FxSerialDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(FXSerialDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(FinsDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(FinsDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(HostLinkFinsDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(HostLinkFinsDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(HostLinkCModeDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(HostLinkCModeDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(MewtocolDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(MewtocolDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(ModbusDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(ModbusDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(CnetDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(NetStudio.LS.Xgt.Cnet.XgtDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(FEnetDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(NetStudio.LS.Xgt.FEnet.XgtDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(DeltaDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(DeltaDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(VSDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(VSDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(VBDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(VBDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		await Parallel.ForEachAsync(FatekDrivers.Values, new ParallelOptions
		{
			MaxDegreeOfParallelism = 3
		}, async delegate(FatekDriver item, CancellationToken _)
		{
			await item.StopAsync();
		});
		if (historicalDataManager != null)
		{
			await historicalDataManager.StopAsync();
		}
		if (alarmManager != null)
		{
			await alarmManager.StopAsync();
		}
		if (asrsLinkManager != null)
		{
			await asrsLinkManager.StopAsync();
		}
		Task.WaitAll();
	}

	public async Task<ApiResponse> RestartAsync()
	{
		ApiResponse result = new ApiResponse();
		StopAsync().Wait();
		Thread.Sleep(2000);
		OnLoadProject().Wait();
		await StartAsync();
		result.Success = true;
		result.Message = "Restarted the driver server successfully.";
		return result;
	}

	public async Task<IPSResult> WriteTagAsync(string tagName, dynamic value)
	{
		IPSResult iPSResult = new IPSResult();
		if (DriverDataSource.Tags.ContainsKey(tagName))
		{
			Tag tag = DriverDataSource.Tags[tagName];
			if (tag.Mode == TagMode.ReadOnly)
			{
				return new IPSResult
				{
					Status = CommStatus.Error,
					Message = "You cannot write a value to this tag(" + tagName + "), because this tag is read-only."
				};
			}
			if (!S7Drivers.ContainsKey(tag.ChannelId) || S7Drivers[tag.ChannelId] == null)
			{
				if (!KeyenceMCDrivers.ContainsKey(tag.ChannelId) || KeyenceMCDrivers[tag.ChannelId] == null)
				{
					if (!MCDrivers.ContainsKey(tag.ChannelId) || MCDrivers[tag.ChannelId] == null)
					{
						if (!SLMPDrivers.ContainsKey(tag.ChannelId) || SLMPDrivers[tag.ChannelId] == null)
						{
							if (!DedicatedDrivers.ContainsKey(tag.ChannelId) || DedicatedDrivers[tag.ChannelId] == null)
							{
								if (!FxSerialDrivers.ContainsKey(tag.ChannelId) || FxSerialDrivers[tag.ChannelId] == null)
								{
									if (!FinsDrivers.ContainsKey(tag.ChannelId) || FinsDrivers[tag.ChannelId] == null)
									{
										if (!HostLinkFinsDrivers.ContainsKey(tag.ChannelId) || HostLinkFinsDrivers[tag.ChannelId] == null)
										{
											if (!HostLinkCModeDrivers.ContainsKey(tag.ChannelId) || HostLinkCModeDrivers[tag.ChannelId] == null)
											{
												if (!MewtocolDrivers.ContainsKey(tag.ChannelId) || MewtocolDrivers[tag.ChannelId] == null)
												{
													if (!ModbusDrivers.ContainsKey(tag.ChannelId) || ModbusDrivers[tag.ChannelId] == null)
													{
														if (!CnetDrivers.ContainsKey(tag.ChannelId) || CnetDrivers[tag.ChannelId] == null)
														{
															if (!FEnetDrivers.ContainsKey(tag.ChannelId) || FEnetDrivers[tag.ChannelId] == null)
															{
																if (!DeltaDrivers.ContainsKey(tag.ChannelId) || DeltaDrivers[tag.ChannelId] == null)
																{
																	if (!VSDrivers.ContainsKey(tag.ChannelId) || VSDrivers[tag.ChannelId] == null)
																	{
																		if (!VBDrivers.ContainsKey(tag.ChannelId) || VBDrivers[tag.ChannelId] == null)
																		{
																			if (!FatekDrivers.ContainsKey(tag.ChannelId) || FatekDrivers[tag.ChannelId] == null)
																			{
																				iPSResult.Status = CommStatus.Error;
																				iPSResult.Message = new NotSupportedException().Message;
																			}
																			else
																			{
																				iPSResult = await FatekDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
																			}
																		}
																		else
																		{
																			iPSResult = await VBDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
																		}
																	}
																	else
																	{
																		iPSResult = await VSDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
																	}
																}
																else
																{
																	iPSResult = await DeltaDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
																}
															}
															else
															{
																iPSResult = await FEnetDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
															}
														}
														else
														{
															iPSResult = await CnetDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
														}
													}
													else
													{
														iPSResult = await ModbusDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
													}
												}
												else
												{
													iPSResult = await MewtocolDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
												}
											}
											else
											{
												iPSResult = await HostLinkCModeDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
											}
										}
										else
										{
											iPSResult = await HostLinkFinsDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
										}
									}
									else
									{
										iPSResult = await FinsDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
									}
								}
								else
								{
									iPSResult = await FxSerialDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
								}
							}
							else
							{
								iPSResult = await DedicatedDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
							}
						}
						else
						{
							iPSResult = await SLMPDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
						}
					}
					else
					{
						iPSResult = await MCDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
					}
				}
				else
				{
					iPSResult = await KeyenceMCDrivers[tag.ChannelId].WriteAsync(tag, (object)value);
				}
			}
			else
			{
				iPSResult = await S7Drivers[tag.ChannelId].WriteAsync(tag, (object)value);
			}
		}
		else
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = "Tag(" + tagName + ") does not exist.";
		}
		return iPSResult;
	}

	public async Task<IPSResult> WriteBlockAsync(Block block)
	{
		IPSResult iPSResult = new IPSResult();
		foreach (BlockItem item in block.Items)
		{
			if (DriverDataSource.Tags.ContainsKey(item.TagName))
			{
				Tag tag = DriverDataSource.Tags[item.TagName];
				if (tag.Mode != TagMode.ReadOnly)
				{
					if (S7Drivers.ContainsKey(tag.ChannelId) && S7Drivers[tag.ChannelId] != null)
					{
						iPSResult = await S7Drivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (KeyenceMCDrivers.ContainsKey(tag.ChannelId) && KeyenceMCDrivers[tag.ChannelId] != null)
					{
						iPSResult = await KeyenceMCDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (MCDrivers.ContainsKey(tag.ChannelId) && MCDrivers[tag.ChannelId] != null)
					{
						iPSResult = await MCDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (SLMPDrivers.ContainsKey(tag.ChannelId) && SLMPDrivers[tag.ChannelId] != null)
					{
						iPSResult = await SLMPDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (DedicatedDrivers.ContainsKey(tag.ChannelId) && DedicatedDrivers[tag.ChannelId] != null)
					{
						iPSResult = await DedicatedDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (FxSerialDrivers.ContainsKey(tag.ChannelId) && FxSerialDrivers[tag.ChannelId] != null)
					{
						iPSResult = await FxSerialDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (FinsDrivers.ContainsKey(tag.ChannelId) && FinsDrivers[tag.ChannelId] != null)
					{
						iPSResult = await HostLinkFinsDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (HostLinkFinsDrivers.ContainsKey(tag.ChannelId) && HostLinkFinsDrivers[tag.ChannelId] != null)
					{
						iPSResult = await HostLinkFinsDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (HostLinkCModeDrivers.ContainsKey(tag.ChannelId) && HostLinkCModeDrivers[tag.ChannelId] != null)
					{
						iPSResult = await HostLinkFinsDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (MewtocolDrivers.ContainsKey(tag.ChannelId) && MewtocolDrivers[tag.ChannelId] != null)
					{
						iPSResult = await MewtocolDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (ModbusDrivers.ContainsKey(tag.ChannelId) && ModbusDrivers[tag.ChannelId] != null)
					{
						iPSResult = await ModbusDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (CnetDrivers.ContainsKey(tag.ChannelId) && CnetDrivers[tag.ChannelId] != null)
					{
						iPSResult = await CnetDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (FEnetDrivers.ContainsKey(tag.ChannelId) && FEnetDrivers[tag.ChannelId] != null)
					{
						iPSResult = await FEnetDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (DeltaDrivers.ContainsKey(tag.ChannelId) && DeltaDrivers[tag.ChannelId] != null)
					{
						iPSResult = await DeltaDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (VSDrivers.ContainsKey(tag.ChannelId) && VSDrivers[tag.ChannelId] != null)
					{
						iPSResult = await VSDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					if (FatekDrivers.ContainsKey(tag.ChannelId) && FatekDrivers[tag.ChannelId] != null)
					{
						iPSResult = await FatekDrivers[tag.ChannelId].WriteAsync(tag, (object)item.Value);
						continue;
					}
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = new NotSupportedException().Message;
					return iPSResult;
				}
				return new IPSResult
				{
					Status = CommStatus.Error,
					Message = "You cannot write a value to this tag(" + item.TagName + "), because this tag is read-only."
				};
			}
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = "Tag(" + item.TagName + ") does not exist.";
			return iPSResult;
		}
		return iPSResult;
	}

	public async Task<ApiResponse> ClearLogAsync()
	{
		return await Task.Run(async delegate
		{
			ApiResponse result = new ApiResponse();
			try
			{
				await DriverDataSource.RemoveLogAllAsync();
				result.Success = true;
				result.Message = "Clear log successfully!";
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
			}
			return result;
		});
	}

	public async ValueTask DisposeAsync()
	{
		await StopAsync();
	}

	public List<Channel> GetChannels()
	{
		return DriverDataSource.Channels;
	}
}
