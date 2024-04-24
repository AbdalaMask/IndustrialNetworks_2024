using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Modbus.ASCII;
using NetStudio.Modbus.RTU;
using NetStudio.Modbus.TCP;

namespace NetStudio.Modbus;

public class ModbusDriver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, IModbusProtocol> Masters = new Dictionary<string, IModbusProtocol>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private readonly Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private readonly BindingList<IpsLog> Logs;

	private Channel channel;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public ModbusDriver(Channel channel, BindingList<IpsLog> eventLogs)
	{
		this.channel = channel;
		Logs = eventLogs;
		OnInitialize(this.channel);
	}

	private void OnInitialize(Channel channel)
	{
		decimal num = default(decimal);
		int num2 = 0;
		IpsAddress ipsAddress = null;
		Dictionary<Device, List<Tag>> dictionary = new Dictionary<Device, List<Tag>>();
		Dictionary<Device, List<Tag>> dictionary2 = new Dictionary<Device, List<Tag>>();
		Dictionary<Device, List<Tag>> dictionary3 = new Dictionary<Device, List<Tag>>();
		Dictionary<Device, List<Tag>> dictionary4 = new Dictionary<Device, List<Tag>>();
		switch (channel.Protocol)
		{
		case IpsProtocolType.MODBUS_TCP:
			foreach (Device device in channel.Devices)
			{
				try
				{
					ModbusTcpProtocol value3 = new ModbusTcpProtocol(device.Adapter);
					Masters.Add(channel.Name + "." + device.Name, value3);
				}
				catch (Exception ex5)
				{
					DriverDataSource.SaveLog(channel.Name + "." + device.Name, ex5.Message);
				}
			}
			break;
		default:
			if (channel.ConnectionType == ConnectionType.Serial)
			{
				try
				{
					ModbusRtuProtocol value2 = new ModbusRtuProtocol(channel.Adapter);
					Masters.Add(channel.Name, value2);
				}
				catch (Exception ex3)
				{
					DriverDataSource.SaveLog(channel.Name, ex3.Message);
				}
				break;
			}
			foreach (Device device2 in channel.Devices)
			{
				try
				{
					Masters.Add(channel.Name + "." + device2.Name, new ModbusRtuProtocol(device2.Adapter));
				}
				catch (Exception ex4)
				{
					DriverDataSource.SaveLog(channel.Name + "." + device2.Name, ex4.Message);
				}
			}
			break;
		case IpsProtocolType.MODBUS_ASCII:
			if (channel.ConnectionType == ConnectionType.Serial)
			{
				try
				{
					ModbusAsciiProtocol value = new ModbusAsciiProtocol(channel.Adapter);
					Masters.Add(channel.Name, value);
				}
				catch (Exception ex)
				{
					DriverDataSource.SaveLog(channel.Name, ex.Message);
				}
				break;
			}
			foreach (Device device3 in channel.Devices)
			{
				try
				{
					Masters.Add(channel.Name + "." + device3.Name, new ModbusAsciiProtocol(device3.Adapter));
				}
				catch (Exception ex2)
				{
					DriverDataSource.SaveLog(channel.Name + "." + device3.Name, ex2.Message);
				}
			}
			break;
		}
		foreach (Device device4 in channel.Devices)
		{
			Devices.Add($"{channel.Id}.{device4.Id}", device4);
			foreach (Group group in device4.Groups)
			{
				foreach (Tag tag in group.Tags)
				{
					tag.FullName = $"{channel.Name}.{device4.Name}.{group.Name}.{tag.Name}";
					Tags.Add(tag.FullName, tag);
					num = decimal.Parse(tag.Address);
					if (num >= 1m && num <= 9999m)
					{
						if (!dictionary.ContainsKey(device4))
						{
							dictionary.Add(device4, new List<Tag>());
						}
						ipsAddress = ModbusUtility.GetAddress(decimal.Parse(tag.Address), device4.BaseAddress, isBit: true);
						tag.WordAddress = ipsAddress.WordAddress;
						tag.BitAddress = ipsAddress.BitAddress;
						dictionary[device4].Add(tag);
						continue;
					}
					if ((num >= 10001m && num <= 19999m) || (num >= 100001m && num <= 199999m))
					{
						if (!dictionary2.ContainsKey(device4))
						{
							dictionary2.Add(device4, new List<Tag>());
						}
						ipsAddress = ModbusUtility.GetAddress(decimal.Parse(tag.Address), device4.BaseAddress);
						tag.WordAddress = ipsAddress.WordAddress;
						tag.BitAddress = ipsAddress.BitAddress;
						dictionary2[device4].Add(tag);
						continue;
					}
					if ((num >= 30001m && num <= 39999m) || (num >= 300001m && num <= 399999m))
					{
						if (!dictionary4.ContainsKey(device4))
						{
							dictionary4.Add(device4, new List<Tag>());
						}
						ipsAddress = ModbusUtility.GetAddress(decimal.Parse(tag.Address), device4.BaseAddress);
						tag.WordAddress = ipsAddress.WordAddress;
						tag.BitAddress = ipsAddress.BitAddress;
						dictionary4[device4].Add(tag);
						continue;
					}
					if (tag.DataType == DataType.BOOL && !tag.Address.Contains("."))
					{
						if (!dictionary.ContainsKey(device4))
						{
							dictionary.Add(device4, new List<Tag>());
						}
						ipsAddress = ModbusUtility.GetAddress(decimal.Parse(tag.Address), device4.BaseAddress, isBit: true);
						tag.WordAddress = ipsAddress.WordAddress;
						tag.BitAddress = ipsAddress.BitAddress;
						dictionary[device4].Add(tag);
						continue;
					}
					if ((num >= 40001m && num <= 49999m) || (num >= 400001m && num <= 465535m))
					{
						if (tag.DataType == DataType.BOOL && !tag.Address.Contains("."))
						{
							if (!dictionary.ContainsKey(device4))
							{
								dictionary.Add(device4, new List<Tag>());
							}
							ipsAddress = ModbusUtility.GetAddress(decimal.Parse(tag.Address), device4.BaseAddress);
							tag.WordAddress = ipsAddress.WordAddress;
							tag.BitAddress = ipsAddress.BitAddress;
							dictionary[device4].Add(tag);
						}
						else
						{
							if (!dictionary3.ContainsKey(device4))
							{
								dictionary3.Add(device4, new List<Tag>());
							}
							ipsAddress = ModbusUtility.GetAddress(decimal.Parse(tag.Address), device4.BaseAddress);
							tag.WordAddress = ipsAddress.WordAddress;
							tag.BitAddress = ipsAddress.BitAddress;
							dictionary3[device4].Add(tag);
						}
						continue;
					}
					throw new Exception($"Invalid modbus address: {num}");
				}
			}
		}
		if (dictionary.Any())
		{
			foreach (KeyValuePair<Device, List<Tag>> item in dictionary)
			{
				Device key = item.Key;
				if (!_packages.ContainsKey(key))
				{
					_packages.Add(key, new List<ReadPacket>());
				}
				int index = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = key.BlockSize * 16;
				List<Tag> list = (from tg in item.Value
					orderby tg.WordAddress, tg.BitAddress
					select tg).ToList();
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					num2 = list[i].WordAddress;
					num4 = num2 - num3 + 1;
					if (i == 0 || num4 > num5)
					{
						num3 = num2;
						num4 = 1;
						_packages[key].Add(new ReadPacket
						{
							Function = 1,
							StationNo = (byte)key.StationNo,
							Address = list[i].WordAddress,
							ConnectRetries = key.ConnectRetries,
							ReceivingDelay = key.ReceivingDelay
						});
						index = _packages[key].Count - 1;
					}
					_packages[key][index].Tags.Add(list[i]);
					_packages[key][index].Quantity = num4;
				}
			}
		}
		if (dictionary2.Any())
		{
			foreach (KeyValuePair<Device, List<Tag>> item2 in dictionary2)
			{
				Device key2 = item2.Key;
				if (!_packages.ContainsKey(key2))
				{
					_packages.Add(key2, new List<ReadPacket>());
				}
				int index2 = 0;
				int num6 = 0;
				int num7 = 0;
				int num8 = key2.BlockSize * 16;
				List<Tag> list2 = (from tg in item2.Value
					orderby tg.WordAddress, tg.BitAddress
					select tg).ToList();
				int count2 = list2.Count;
				for (int j = 0; j < count2; j++)
				{
					num2 = list2[j].WordAddress;
					num7 = num2 - num6 + 1;
					if (j == 0 || num7 > num8)
					{
						num6 = num2;
						num7 = 1;
						_packages[key2].Add(new ReadPacket
						{
							Function = 2,
							StationNo = (byte)key2.StationNo,
							Address = list2[j].WordAddress,
							ConnectRetries = key2.ConnectRetries,
							ReceivingDelay = key2.ReceivingDelay
						});
						index2 = _packages[key2].Count - 1;
					}
					_packages[key2][index2].Tags.Add(list2[j]);
					_packages[key2][index2].Quantity = num7;
				}
			}
		}
		if (dictionary3.Any())
		{
			foreach (KeyValuePair<Device, List<Tag>> item3 in dictionary3)
			{
				Device key3 = item3.Key;
				if (!_packages.ContainsKey(key3))
				{
					_packages.Add(key3, new List<ReadPacket>());
				}
				ushort num9 = 0;
				int index3 = 0;
				int num10 = 0;
				int num11 = 0;
				List<Tag> list3 = (from tg in item3.Value
					orderby tg.WordAddress, tg.BitAddress
					select tg).ToList();
				int count3 = list3.Count;
				for (int k = 0; k < count3; k++)
				{
					switch (list3[k].DataType)
					{
					default:
						num9 = 1;
						break;
					case DataType.LINT:
					case DataType.ULINT:
					case DataType.LWORD:
					case DataType.LREAL:
						num9 = 4;
						break;
					case DataType.INT:
					case DataType.UINT:
					case DataType.WORD:
					case DataType.TIME16:
						num9 = 1;
						break;
					case DataType.DINT:
					case DataType.UDINT:
					case DataType.DWORD:
					case DataType.REAL:
					case DataType.TIME32:
						num9 = 2;
						break;
					case DataType.STRING:
						break;
					}
					num2 = list3[k].WordAddress;
					num11 = num2 - num10 + num9;
					if (k == 0 || num11 > key3.BlockSize)
					{
						num10 = num2;
						num11 = num2 - num10 + num9;
						_packages[key3].Add(new ReadPacket
						{
							Function = 3,
							StationNo = (byte)key3.StationNo,
							Address = list3[k].WordAddress,
							ConnectRetries = key3.ConnectRetries,
							ReceivingDelay = key3.ReceivingDelay
						});
						index3 = _packages[key3].Count - 1;
					}
					_packages[key3][index3].Tags.Add(list3[k]);
					_packages[key3][index3].Quantity = num11;
				}
			}
		}
		if (dictionary4.Any())
		{
			foreach (KeyValuePair<Device, List<Tag>> item4 in dictionary4)
			{
				Device key4 = item4.Key;
				if (!_packages.ContainsKey(key4))
				{
					_packages.Add(key4, new List<ReadPacket>());
				}
				ushort num12 = 0;
				int index4 = 0;
				int num13 = 0;
				int num14 = 0;
				int blockSize = key4.BlockSize;
				List<Tag> list4 = (from tg in item4.Value
					orderby tg.WordAddress, tg.BitAddress
					select tg).ToList();
				int count4 = list4.Count;
				for (int l = 0; l < count4; l++)
				{
					switch (list4[l].DataType)
					{
					case DataType.LINT:
					case DataType.ULINT:
					case DataType.LWORD:
					case DataType.LREAL:
						num12 = 4;
						break;
					case DataType.INT:
					case DataType.UINT:
					case DataType.WORD:
					case DataType.TIME16:
						num12 = 1;
						break;
					case DataType.DINT:
					case DataType.UDINT:
					case DataType.DWORD:
					case DataType.REAL:
					case DataType.TIME32:
						num12 = 2;
						break;
					}
					num2 = list4[l].WordAddress;
					num14 = num2 - num13 + num12;
					if (l == 0 || num14 > blockSize)
					{
						num13 = num2;
						num14 = num2 - num13 + num12;
						_packages[key4].Add(new ReadPacket
						{
							Function = 4,
							StationNo = (byte)key4.StationNo,
							Address = list4[l].WordAddress,
							ConnectRetries = key4.ConnectRetries,
							ReceivingDelay = key4.ReceivingDelay
						});
						index4 = _packages[key4].Count - 1;
					}
					_packages[key4][index4].Tags.Add(list4[l]);
					_packages[key4][index4].Quantity = num14;
				}
			}
		}
		switch (channel.Protocol)
		{
		case IpsProtocolType.MODBUS_TCP:
		{
			ModbusTcpBuilder modbusTcpBuilder_0 = new ModbusTcpBuilder();
			if (!_packages.Any())
			{
				break;
			}
			{
				foreach (KeyValuePair<Device, List<ReadPacket>> package in _packages)
				{
					Parallel.ForEach((IEnumerable<ReadPacket>)package.Value, (Action<ReadPacket>)delegate(ReadPacket RP)
					{
						RP.SendBytes = modbusTcpBuilder_0.ReadMessage(0, RP.StationNo, RP.Function, RP.Address, RP.Quantity);
					});
				}
				break;
			}
		}
		case IpsProtocolType.MODBUS_RTU:
		{
			ModbusRtuBuilder modbusRtuBuilder_0 = new ModbusRtuBuilder();
			if (!_packages.Any())
			{
				break;
			}
			{
				foreach (KeyValuePair<Device, List<ReadPacket>> package2 in _packages)
				{
					Parallel.ForEach((IEnumerable<ReadPacket>)package2.Value, (Action<ReadPacket>)delegate(ReadPacket RP)
					{
						RP.SendBytes = modbusRtuBuilder_0.ReadMessage(RP.StationNo, RP.Function, RP.Address, RP.Quantity);
					});
				}
				break;
			}
		}
		case IpsProtocolType.MODBUS_ASCII:
		{
			ModbusAsciiBuilder ascii = new ModbusAsciiBuilder();
			if (!_packages.Any())
			{
				break;
			}
			{
				foreach (KeyValuePair<Device, List<ReadPacket>> package3 in _packages)
				{
					Parallel.ForEach((IEnumerable<ReadPacket>)package3.Value, (Action<ReadPacket>)delegate(ReadPacket RP)
					{
						RP.SendMsg = ascii.ReadMessage(RP.StationNo, RP.Function, RP.Address, RP.Quantity);
					});
				}
				break;
			}
		}
		}
	}

	public void Start()
	{
		
		
		if (channel == null)
		{
			return;
		}
		cancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			Thread thread = new Thread(async delegate(object? obj)
			{
				await OnSerialPolling((CancellationToken)obj);
			});
			thread.IsBackground = true;
			thread.Start(cancellationTokenSource.Token);
		}
		else
		{
			if (!channel.Devices.Any())
			{
				return;
			}
			foreach (Device device in channel.Devices)
			{
				Thread thread2 = new Thread(async delegate(object? obj)
				{
					KeyValuePair<CancellationToken, Device> keyValuePair = (KeyValuePair<CancellationToken, Device>)obj;
					await OnEthernetPolling(keyValuePair.Key, keyValuePair.Value);
				});
				thread2.IsBackground = true;
				thread2.Start(new KeyValuePair<CancellationToken, Device>(cancellationTokenSource.Token, device));
			}
		}
	}

	public async Task StopAsync()
	{
		if (cancellationTokenSource != null)
		{
			await cancellationTokenSource.CancelAsync();
		}
	}

	private async Task ResetAllAsync()
	{
		await Task.Run(delegate
		{
			try
			{
				foreach (Device value in Devices.Values)
				{
					Parallel.ForEach((IEnumerable<ReadPacket>)_packages[value], (Action<ReadPacket>)delegate(ReadPacket RP)
					{
						Parallel.ForEach((IEnumerable<Tag>)RP.Tags, (Action<Tag>)delegate(Tag tg)
						{
							tg.SetDefaultValue();
						});
					});
				}
			}
			catch (Exception ex)
			{
				DriverDataSource.SaveLog(channel.Name + ".", ex.Message);
			}
		});
	}

	private void ResetDevice(Device device)
	{
		try
		{
			if (!_packages.ContainsKey(device))
			{
				return;
			}
			device.ReconnectFlag = true;
			device.startDisconnected = DateTime.Now;
			device.Status = DeviceStatus.Disconnected;
			DriverDataSource.SaveLog(channel.Name + "." + device.ToString() + ".", "Lost connection to device.");
			Parallel.ForEach((IEnumerable<Group>)device.Groups, (Action<Group>)delegate(Group group)
			{
				foreach (Tag tag in group.Tags)
				{
					tag.SetDefaultValue();
				}
			});
		}
		catch (Exception)
		{
		}
	}

	private async Task OnSerialPolling(CancellationToken cancellationToken)
	{
		if (channel == null || !Masters.ContainsKey(channel.Name))
		{
			return;
		}
		IModbusProtocol master = Masters[channel.Name];
		string source = channel.Name + ".";
		try
		{
			try
			{
				if (!channel.Devices.Where((Device device_0) => device_0.Active).Any())
				{
					goto end_IL_00c5;
				}
				master.Connect();
				while (!cancellationToken.IsCancellationRequested )
				{
					try
					{
						foreach (Device device in channel.Devices)
						{
							if (!device.Active)
							{
								if (channel.Devices.Count > 1)
								{
									continue;
								}
								Thread.Sleep(30000);
							}
							if (device.ReconnectFlag)
							{
								if (!device.AutoReconnect)
								{
									break;
								}
								await Task.Delay(5000);
								if (device.Status == DeviceStatus.Disconnected)
								{
									source = channel.Name + "." + device.ToString() + ".";
									DriverDataSource.SaveLog(source, "Reconnecting to the device.", EvenType.Information);
									Device device2 = device;
									device2.ReconnectFlag = !(await master.ConnectAsync());
								}
								continue;
							}
							foreach (ReadPacket item in _packages[device])
							{
								try
								{
									IPSResult iPSResult;
									switch (item.Function)
									{
									default:
										throw new NotSupportedException();
									case 1:
									case 2:
										iPSResult = await ReadStatus(channel, device, master, item);
										break;
									case 3:
									case 4:
										iPSResult = await ReadRegisters(channel, device, master, item);
										break;
									}
									if (iPSResult == null)
									{
										continue;
									}
									if (iPSResult.Status == CommStatus.Success)
									{
										if (device.Status == DeviceStatus.Connected)
										{
											continue;
										}
										device.Status = DeviceStatus.Connected;
										device.ReconnectManual = false;
										device.ReconnectFlag = false;
										IpsLog[] array = DriverDataSource.Logs.Where((IpsLog ipsLog_0) => ipsLog_0.Source == source).ToArray();
										if (array != null)
										{
											IpsLog[] array2 = array;
											for (int i = 0; i < array2.Length; i++)
											{
												await DriverDataSource.RemoveLogAsync(array2[i]);
											}
										}
										continue;
									}
									if (iPSResult.Status == CommStatus.Timeout)
									{
										device.startDisconnected = DateTime.Now;
										if (device.AutoReconnect)
										{
											device.ReconnectFlag = true;
										}
										else
										{
											device.Active = false;
										}
										ResetDevice(device);
										break;
									}
									DriverDataSource.SaveLog(source, iPSResult.Message);
								}
								catch (Exception ex)
								{
									source = channel.Name + "." + device.ToString() + ".";
									DriverDataSource.SaveLog(source, ex.Message);
								}
							}
						}
					}
					catch (Exception ex2)
					{
						DriverDataSource.SaveLog(channel.Name, ex2.Message);
						Thread.Sleep(100);
					}
				}
			 
				goto end_IL_00c0;
				end_IL_00c5:;
			}
			catch (Exception ex3)
			{
				DriverDataSource.SaveLog(channel.Name, ex3.Message);
				goto end_IL_00c0;
			}
			end_IL_00c0:;
		}
		finally
		{
			await ResetAllAsync();
			master.Disconnect();
		}
	}

	private async Task OnEthernetPolling(CancellationToken cancellationToken, Device device)
	{
		string key = channel.Name + "." + device.Name;
		if (!Masters.ContainsKey(key))
		{
			return;
		}
		string source = channel.Name + "." + device.ToString() + ".";
		IModbusProtocol master = Masters[key];
		try
		{
			if (!channel.Devices.Where((Device device_0) => device_0.Active).Any())
			{
				return;
			}
			device.Status = DeviceStatus.Connecting;
			if (await master.ConnectAsync())
			{
				device.Status = DeviceStatus.Connected;
				device.ReconnectManual = false;
			}
			else
			{
				device.ReconnectFlag = true;
				DriverDataSource.SaveLog(source, "Failed to connect device.");
			}
		}
		catch (Exception ex)
		{
			device.Status = DeviceStatus.Disconnected;
			device.ReconnectFlag = true;
			DriverDataSource.SaveLog(source, "Failed to connect device. " + ex.Message);
		}
		try
		{
			while (!cancellationToken.IsCancellationRequested )
			{
				try
				{
					if (device.Active)
					{
						goto IL_025d;
					}
					if (channel.Devices.Count > 1)
					{
						continue;
					}
					Thread.Sleep(30000);
					goto IL_025d;
					IL_025d:
					if (device.ReconnectFlag)
					{
						if (!device.AutoReconnect)
						{
							break;
						}
						await Task.Delay(5000);
						if (device.Status == DeviceStatus.Disconnected)
						{
							source = channel.Name + "." + device.ToString() + ".";
							DriverDataSource.SaveLog(source, "Reconnecting to the device.", EvenType.Information);
							device.ReconnectFlag = !(await master.ConnectAsync());
						}
						continue;
					}
					foreach (ReadPacket item in _packages[device])
					{
						try
						{
							IPSResult iPSResult;
							switch (item.Function)
							{
							default:
								throw new NotSupportedException();
							case 1:
							case 2:
								iPSResult = await ReadStatus(channel, device, master, item);
								break;
							case 3:
							case 4:
								iPSResult = await ReadRegisters(channel, device, master, item);
								break;
							}
							if (iPSResult == null)
							{
								continue;
							}
							if (iPSResult.Status == CommStatus.Success)
							{
								if (device.Status == DeviceStatus.Connected)
								{
									continue;
								}
								device.Status = DeviceStatus.Connected;
								device.ReconnectManual = false;
								device.ReconnectFlag = false;
								IpsLog[] array = DriverDataSource.Logs.Where((IpsLog ipsLog_0) => ipsLog_0.Source == source).ToArray();
								if (array != null)
								{
									IpsLog[] array2 = array;
									for (int i = 0; i < array2.Length; i++)
									{
										await DriverDataSource.RemoveLogAsync(array2[i]);
									}
								}
								continue;
							}
							if (iPSResult.Status == CommStatus.Timeout)
							{
								device.startDisconnected = DateTime.Now;
								if (device.AutoReconnect)
								{
									device.ReconnectFlag = true;
								}
								else
								{
									device.Active = false;
								}
								ResetDevice(device);
								break;
							}
							DriverDataSource.SaveLog(source, iPSResult.Message);
						}
						catch (Exception ex2)
						{
							DriverDataSource.SaveLog(source, ex2.Message);
						}
					}
				}
				catch (Exception ex3)
				{
					DriverDataSource.SaveLog(source, ex3.Message);
					Thread.Sleep(100);
				}
			}
			 
		}
		catch (Exception ex4)
		{
			DriverDataSource.SaveLog(source, ex4.Message);
		}
		finally
		{
			await ResetAllAsync();
			master.Disconnect();
		}
	}

	public async Task<IPSResult> ReadStatus(Channel channel, Device device, IModbusProtocol Master, ReadPacket RP)
	{
		
		IPSResult iPSResult = await Master.ReadStatusAsync(RP);
		try
		{
			if (iPSResult.Status == CommStatus.Success)
			{
				BOOL[] coils = BOOL.GetBools(iPSResult.Values);
				if (coils != null && coils.Any())
				{
					int count = RP.Tags.Count;
					int index;
					Parallel.For(0, count, delegate(int y, ParallelLoopState state)
					{
						index = RP.Tags[y].WordAddress - RP.Address;
						RP.Tags[y].Value = coils[index];
						RP.Tags[y].Status = TagStatus.Good;
					});
				}
				else
				{
					Parallel.ForEach((IEnumerable<Tag>)RP.Tags, (Action<Tag>)delegate(Tag tg)
					{
						tg.SetDefaultValue();
					});
				}
			}
			else
			{
				Parallel.ForEach((IEnumerable<Tag>)RP.Tags, (Action<Tag>)delegate(Tag tg)
				{
					tg.SetDefaultValue();
				});
			}
		}
		catch (Exception ex)
		{
			Parallel.ForEach((IEnumerable<Tag>)RP.Tags, (Action<Tag>)delegate(Tag tg)
			{
				tg.SetDefaultValue();
			});
			string source = channel.Name + "." + device.ToString() + ".";
			string message = RP.ToString() + ": " + ex.Message;
			DriverDataSource.SaveLog(source, message);
		}
		return iPSResult;
	}

	public async Task<IPSResult> ReadRegisters(Channel channel, Device device, IModbusProtocol master, ReadPacket RP)
	{
		
		
		
		IPSResult iPSResult = await master.ReadRegisterAsync(RP);
		if (iPSResult.Status == CommStatus.Success)
		{
			int count = RP.Tags.Count;
			byte[] values = iPSResult.Values;
			Parallel.For(0, count, delegate(int y, ParallelLoopState state)
			{
				try
				{
					int num = 2 * (RP.Tags[y].WordAddress - RP.Address);
					switch (RP.Tags[y].DataType)
					{
					case DataType.BOOL:
					{
						BOOL[] bools = BOOL.GetBools(BYTE.SortBytes(new byte[2]
						{
							values[num],
							values[num + 1]
						}, device.ByteOrder));
						RP.Tags[y].Value = bools[RP.Tags[y].BitAddress];
						RP.Tags[y].Value2 = bools;
						RP.Tags[y].Status = TagStatus.Good;
						break;
					}
					default:
						RP.Tags[y].Status = TagStatus.Bad;
						break;
					case DataType.INT:
						if (RP.Tags[y].IsScaling)
						{
							if (RP.Tags[y].IsOperator)
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], INT.Parse(new byte[2]
								{
									values[num],
									values[num + 1]
								}, device.ByteOrder).Value) + RP.Tags[y].Offset;
							}
							else
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], INT.Parse(new byte[2]
								{
									values[num],
									values[num + 1]
								}, device.ByteOrder).Value) - RP.Tags[y].Offset;
							}
						}
						else if (RP.Tags[y].Offset > 1f)
						{
							RP.Tags[y].Value = (float)(short)INT.Parse(new byte[2]
							{
								values[num],
								values[num + 1]
							}, device.ByteOrder) / RP.Tags[y].Offset;
						}
						else
						{
							RP.Tags[y].Value = INT.Parse(new byte[2]
							{
								values[num],
								values[num + 1]
							}, device.ByteOrder);
						}
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UINT:
						if (RP.Tags[y].IsScaling)
						{
							if (RP.Tags[y].IsOperator)
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], UINT.Parse(new byte[2]
								{
									values[num],
									values[num + 1]
								}, device.ByteOrder).Value) + RP.Tags[y].Offset;
							}
							else
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], UINT.Parse(new byte[2]
								{
									values[num],
									values[num + 1]
								}, device.ByteOrder).Value) - RP.Tags[y].Offset;
							}
						}
						else if (RP.Tags[y].Offset > 1f)
						{
							RP.Tags[y].Value = (float)(int)(ushort)UINT.Parse(new byte[2]
							{
								values[num],
								values[num + 1]
							}, device.ByteOrder) / RP.Tags[y].Offset;
						}
						else
						{
							RP.Tags[y].Value = UINT.Parse(new byte[2]
							{
								values[num],
								values[num + 1]
							}, device.ByteOrder);
						}
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.WORD:
						if (RP.Tags[y].IsScaling)
						{
							if (RP.Tags[y].IsOperator)
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], UINT.Parse(new byte[2]
								{
									values[num],
									values[num + 1]
								}, device.ByteOrder).Value) + RP.Tags[y].Offset;
							}
							else
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], UINT.Parse(new byte[2]
								{
									values[num],
									values[num + 1]
								}, device.ByteOrder).Value) - RP.Tags[y].Offset;
							}
						}
						else
						{
							RP.Tags[y].Value = WORD.Parse(new byte[2]
							{
								values[num],
								values[num + 1]
							}, device.ByteOrder);
						}
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DINT:
						if (RP.Tags[y].IsScaling)
						{
							if (RP.Tags[y].IsOperator)
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], DINT.Parse(new byte[4]
								{
									values[num],
									values[num + 1],
									values[num + 2],
									values[num + 3]
								}, device.ByteOrder).Value) + RP.Tags[y].Offset;
							}
							else
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], DINT.Parse(new byte[4]
								{
									values[num],
									values[num + 1],
									values[num + 2],
									values[num + 3]
								}, device.ByteOrder).Value) - RP.Tags[y].Offset;
							}
						}
						else if (RP.Tags[y].Offset > 1f)
						{
							RP.Tags[y].Value = (float)(int)DINT.Parse(new byte[4]
							{
								values[num],
								values[num + 1],
								values[num + 2],
								values[num + 3]
							}, device.ByteOrder) / RP.Tags[y].Offset;
						}
						else
						{
							RP.Tags[y].Value = DINT.Parse(new byte[4]
							{
								values[num],
								values[num + 1],
								values[num + 2],
								values[num + 3]
							}, device.ByteOrder);
						}
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UDINT:
						if (RP.Tags[y].IsScaling)
						{
							if (RP.Tags[y].IsOperator)
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], UDINT.Parse(new byte[4]
								{
									values[num],
									values[num + 1],
									values[num + 2],
									values[num + 3]
								}, device.ByteOrder).Value) + RP.Tags[y].Offset;
							}
							else
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], UDINT.Parse(new byte[4]
								{
									values[num],
									values[num + 1],
									values[num + 2],
									values[num + 3]
								}, device.ByteOrder).Value) - RP.Tags[y].Offset;
							}
						}
						else if (RP.Tags[y].Offset > 1f)
						{
							RP.Tags[y].Value = (float)(uint)UDINT.Parse(new byte[4]
							{
								values[num],
								values[num + 1],
								values[num + 2],
								values[num + 3]
							}, device.ByteOrder) / RP.Tags[y].Offset;
						}
						else
						{
							RP.Tags[y].Value = UDINT.Parse(new byte[4]
							{
								values[num],
								values[num + 1],
								values[num + 2],
								values[num + 3]
							}, device.ByteOrder);
						}
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DWORD:
						if (RP.Tags[y].IsScaling)
						{
							if (RP.Tags[y].IsOperator)
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], UDINT.Parse(new byte[4]
								{
									values[num],
									values[num + 1],
									values[num + 2],
									values[num + 3]
								}, device.ByteOrder).Value) + RP.Tags[y].Offset;
							}
							else
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], UDINT.Parse(new byte[4]
								{
									values[num],
									values[num + 1],
									values[num + 2],
									values[num + 3]
								}, device.ByteOrder).Value) - RP.Tags[y].Offset;
							}
						}
						else
						{
							RP.Tags[y].Value = DWORD.Parse(new byte[4]
							{
								values[num],
								values[num + 1],
								values[num + 2],
								values[num + 3]
							}, device.ByteOrder);
						}
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.REAL:
						RP.Tags[y].Value = REAL.Parse(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}, device.ByteOrder);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LINT:
						if (RP.Tags[y].IsScaling)
						{
							if (RP.Tags[y].IsOperator)
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], LINT.Parse(new byte[8]
								{
									values[num],
									values[num + 1],
									values[num + 2],
									values[num + 3],
									values[num + 4],
									values[num + 5],
									values[num + 6],
									values[num + 7]
								}, device.ByteOrder).Value) + RP.Tags[y].Offset;
							}
							else
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], LINT.Parse(new byte[8]
								{
									values[num],
									values[num + 1],
									values[num + 2],
									values[num + 3],
									values[num + 4],
									values[num + 5],
									values[num + 6],
									values[num + 7]
								}, device.ByteOrder).Value) - RP.Tags[y].Offset;
							}
						}
						else if (RP.Tags[y].Offset > 1f)
						{
							RP.Tags[y].Value = (float)(long)LINT.Parse(new byte[8]
							{
								values[num],
								values[num + 1],
								values[num + 2],
								values[num + 3],
								values[num + 4],
								values[num + 5],
								values[num + 6],
								values[num + 7]
							}, device.ByteOrder) / RP.Tags[y].Offset;
						}
						else
						{
							RP.Tags[y].Value = LINT.Parse(new byte[8]
							{
								values[num],
								values[num + 1],
								values[num + 2],
								values[num + 3],
								values[num + 4],
								values[num + 5],
								values[num + 6],
								values[num + 7]
							}, device.ByteOrder);
						}
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.ULINT:
						if (RP.Tags[y].IsScaling)
						{
							if (RP.Tags[y].IsOperator)
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], ULINT.Parse(new byte[8]
								{
									values[num],
									values[num + 1],
									values[num + 2],
									values[num + 3],
									values[num + 4],
									values[num + 5],
									values[num + 6],
									values[num + 7]
								}, device.ByteOrder).Value) + RP.Tags[y].Offset;
							}
							else
							{
								RP.Tags[y].Value = Utility.Interpolation(RP.Tags[y], ULINT.Parse(new byte[8]
								{
									values[num],
									values[num + 1],
									values[num + 2],
									values[num + 3],
									values[num + 4],
									values[num + 5],
									values[num + 6],
									values[num + 7]
								}, device.ByteOrder).Value) - RP.Tags[y].Offset;
							}
						}
						else if (RP.Tags[y].Offset > 1f)
						{
							RP.Tags[y].Value = (float)(ulong)ULINT.Parse(new byte[8]
							{
								values[num],
								values[num + 1],
								values[num + 2],
								values[num + 3],
								values[num + 4],
								values[num + 5],
								values[num + 6],
								values[num + 7]
							}, device.ByteOrder) / RP.Tags[y].Offset;
						}
						else
						{
							RP.Tags[y].Value = ULINT.Parse(new byte[8]
							{
								values[num],
								values[num + 1],
								values[num + 2],
								values[num + 3],
								values[num + 4],
								values[num + 5],
								values[num + 6],
								values[num + 7]
							}, device.ByteOrder);
						}
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LWORD:
						RP.Tags[y].Value = LWORD.Parse(new byte[8]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3],
							values[num + 4],
							values[num + 5],
							values[num + 6],
							values[num + 7]
						}, device.ByteOrder);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LREAL:
						RP.Tags[y].Value = LREAL.Parse(new byte[8]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3],
							values[num + 4],
							values[num + 5],
							values[num + 6],
							values[num + 7]
						}, device.ByteOrder);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME16:
						RP.Tags[y].Value = TIME16.Parse(new byte[2]
						{
							values[num],
							values[num + 1]
						}, device.ByteOrder);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME32:
						RP.Tags[y].Value = TIME32.Parse(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}, device.ByteOrder);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.STRING:
						break;
					}
				}
				catch (IndexOutOfRangeException ex)
				{
					RP.Tags[y].SetDefaultValue();
					DriverDataSource.SaveLog(channel.Name + "." + device.ToString() + ".", ex.Message);
				}
				catch (ArgumentOutOfRangeException)
				{
					RP.Tags[y].SetDefaultValue();
					string source = channel.Name + "." + device.ToString() + ".";
					string message = $"Tag(Name={RP.Tags[y].FullName}, Address={RP.Tags[y].Address}): {"Out-of-range address or the data read from the device is missing."}";
					DriverDataSource.SaveLog(source, message);
				}
				catch (Exception ex3)
				{
					RP.Tags[y].SetDefaultValue();
					string source2 = channel.Name + "." + device.ToString() + ".";
					string message2 = $"Tag(Name={RP.Tags[y].FullName}, Address={RP.Tags[y].Address}): {ex3.Message}";
					DriverDataSource.SaveLog(source2, message2);
				}
			});
		}
		else
		{
			Parallel.ForEach((IEnumerable<Tag>)RP.Tags, (Action<Tag>)delegate(Tag tg)
			{
				tg.SetDefaultValue();
			});
		}
		return iPSResult;
	}

	public async Task<IPSResult> WriteAsync(Tag tg, dynamic value)
	{
		IPSResult result = new IPSResult();
		try
		{
			string text = $"{tg.ChannelId}.{tg.DeviceId}";
			if (!Devices.ContainsKey(text))
			{
				result.Status = CommStatus.Error;
				result.Message = "Device Id=" + text + " does not exist.";
				return result;
			}
			Device device = Devices[text];
			_ = device.StationNo;
			IModbusProtocol modbusProtocol = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[channel.Name + "." + device.Name]);
			WritePacket writePacket = new WritePacket
			{
				StationNo = (byte)device.StationNo,
				Quantity = 1,
				Address = tg.WordAddress,
				ConnectRetries = device.ConnectRetries,
				ReceivingDelay = device.ReceivingDelay
			};
			if (tg.DataType == DataType.BOOL)
			{
				if (tg.Address.Contains("."))
				{
					tg.Value2[tg.BitAddress] = (bool)value;
					writePacket.DataDec = BYTE.SortBytes(BOOL.ToBytes(tg.Value2), device.ByteOrder);
					result = await modbusProtocol.WriteRegisterAsync(writePacket);
				}
				else
				{
					writePacket.DataDec = ((!(BOOL)value) ? new byte[2] : new byte[2] { 255, 0 });
					result = await modbusProtocol.WriteCoilAsync(writePacket);
				}
			}
			else
			{
				switch (tg.DataType)
				{
				default:
					throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
				case DataType.INT:
					writePacket.DataDec = INT.ToBytes((INT)value, device.ByteOrder);
					break;
				case DataType.UINT:
					writePacket.DataDec = UINT.ToBytes((UINT)value, device.ByteOrder);
					break;
				case DataType.WORD:
					writePacket.DataDec = WORD.ToBytes((WORD)value, device.ByteOrder);
					break;
				case DataType.DINT:
					writePacket.Quantity = 2;
					writePacket.DataDec = DINT.ToBytes((DINT)value, device.ByteOrder);
					break;
				case DataType.UDINT:
					writePacket.Quantity = 2;
					writePacket.DataDec = UDINT.ToBytes((UDINT)value, device.ByteOrder);
					break;
				case DataType.DWORD:
					writePacket.Quantity = 2;
					writePacket.DataDec = DWORD.ToBytes((DWORD)value, device.ByteOrder);
					break;
				case DataType.REAL:
					writePacket.Quantity = 2;
					writePacket.DataDec = REAL.ToBytes((REAL)value, device.ByteOrder);
					break;
				case DataType.LINT:
					writePacket.Quantity = 4;
					writePacket.DataDec = LINT.ToBytes((LINT)value, device.ByteOrder);
					break;
				case DataType.ULINT:
					writePacket.Quantity = 4;
					writePacket.DataDec = ULINT.ToBytes((ULINT)value, device.ByteOrder);
					break;
				case DataType.LWORD:
					writePacket.Quantity = 4;
					writePacket.DataDec = LWORD.ToBytes((LWORD)value, device.ByteOrder);
					break;
				case DataType.LREAL:
					writePacket.Quantity = 4;
					writePacket.DataDec = LREAL.ToBytes((LREAL)value, device.ByteOrder);
					break;
				case DataType.TIME16:
					writePacket.DataDec = TIME16.ToBytes((TIME16)value, device.ByteOrder);
					break;
				case DataType.TIME32:
					writePacket.Quantity = 2;
					writePacket.DataDec = TIME32.ToBytes((TIME32)value, device.ByteOrder);
					break;
				}
				result = await modbusProtocol.WriteRegisterAsync(writePacket);
			}
		}
		catch (Exception ex)
		{
			result.Message = "Write: Failure. " + ex.Message;
			result.Status = CommStatus.Error;
			DriverDataSource.SaveLog(tg.FullName, ex.Message);
		}
		return result;
	}
}
