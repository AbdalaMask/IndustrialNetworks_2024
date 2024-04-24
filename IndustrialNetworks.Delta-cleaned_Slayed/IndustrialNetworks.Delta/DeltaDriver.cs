using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Delta.Ascii;
using NetStudio.Delta.Models;
using NetStudio.Delta.Rtu;
using NetStudio.Delta.Tcp;

namespace NetStudio.Delta;

public class DeltaDriver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, IDeltaMaster> Masters = new Dictionary<string, IDeltaMaster>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private readonly Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private readonly BindingList<IpsLog> Logs;

	private Channel channel;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public DeltaDriver(Channel channel, BindingList<IpsLog> eventLogs)
	{
		this.channel = channel;
		Logs = eventLogs;
		OnInitialize(this.channel);
	}

	private void OnInitialize(Channel channel)
	{
		string empty = string.Empty;
		Dictionary<Device, Dictionary<string, List<Tag>>> dictionary = new Dictionary<Device, Dictionary<string, List<Tag>>>();
		switch (channel.Protocol)
		{
		case IpsProtocolType.DELTA_ASCII:
			if (channel.ConnectionType == ConnectionType.Serial)
			{
				try
				{
					DeltaAsciiProtocol value3 = new DeltaAsciiProtocol(channel.Adapter);
					Masters.Add(channel.Name, value3);
				}
				catch (Exception ex4)
				{
					DriverDataSource.SaveLog(channel.Name, ex4.Message);
				}
				break;
			}
			foreach (Device device in channel.Devices)
			{
				try
				{
					if (device.DeviceType == 2)
					{
						device.BlockSize = 60;
					}
					Masters.Add(channel.Name + "." + device.Name, new DeltaAsciiProtocol(device.Adapter));
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
					DeltaRtuProtocol value2 = new DeltaRtuProtocol(channel.Adapter);
					Masters.Add(channel.Name, value2);
				}
				catch (Exception ex2)
				{
					DriverDataSource.SaveLog(channel.Name, ex2.Message);
				}
				break;
			}
			foreach (Device device2 in channel.Devices)
			{
				try
				{
					if (device2.DeviceType == 2)
					{
						device2.BlockSize = 60;
					}
					Masters.Add(channel.Name + "." + device2.Name, new DeltaRtuProtocol(device2.Adapter));
				}
				catch (Exception ex3)
				{
					DriverDataSource.SaveLog(channel.Name + "." + device2.Name, ex3.Message);
				}
			}
			break;
		case IpsProtocolType.DELTA_TCP:
			foreach (Device device3 in channel.Devices)
			{
				try
				{
					if (device3.DeviceType == 2)
					{
						device3.BlockSize = 60;
					}
					else
					{
						device3.BlockSize = 100;
					}
					DeltaTcpProtocol value = new DeltaTcpProtocol(device3.Adapter);
					Masters.Add(channel.Name + "." + device3.Name, value);
				}
				catch (Exception ex)
				{
					DriverDataSource.SaveLog(channel.Name + "." + device3.Name, ex.Message);
				}
			}
			break;
		}
		foreach (Device device4 in channel.Devices)
		{
			Devices.Add(channel.Name + "." + device4.Name, device4);
			if (!dictionary.ContainsKey(device4))
			{
				dictionary.Add(device4, new Dictionary<string, List<Tag>>());
			}
			bool flag = device4.DeviceType == 2;
			Dictionary<string, List<Tag>> dictionary2 = dictionary[device4];
			foreach (Group group in device4.Groups)
			{
				foreach (Tag tag in group.Tags)
				{
					empty = DeltaUtility.GetMemory(tag);
					switch (empty)
					{
					case "C":
						if (tag.DataType != 0)
						{
							empty = "CW";
						}
						break;
					case "T":
						if (tag.DataType != 0)
						{
							empty = "TW";
						}
						break;
					case "HC":
						if (tag.DataType != 0)
						{
							empty = "HCD";
						}
						break;
					}
					if (!dictionary2.ContainsKey(empty))
					{
						dictionary2.Add(empty, new List<Tag>());
					}
					IpsAddress ipsAddress = (flag ? DeltaUtility.DvpSeries.GetIpsAddress(tag) : DeltaUtility.AhAsSeries.GetIpsAddress(tag));
					tag.WordAddress = ipsAddress.WordAddress;
					tag.BitAddress = ipsAddress.BitAddress;
					tag.FullName = $"{channel.Name}.{device4.Name}.{group.Name}.{tag.Name}";
					dictionary2[empty].Add(tag);
					Tags.Add(tag.FullName, tag);
				}
			}
		}
		if (dictionary.Any())
		{
			foreach (KeyValuePair<Device, Dictionary<string, List<Tag>>> item2 in dictionary)
			{
				Device key = item2.Key;
				Dictionary<string, List<Tag>> value4 = item2.Value;
				if (!_packages.ContainsKey(key))
				{
					_packages.Add(key, new List<ReadPacket>());
				}
				foreach (KeyValuePair<string, List<Tag>> item3 in value4)
				{
					string empty2 = string.Empty;
					int num = 0;
					int index = 0;
					int num2 = 0;
					int num3 = 0;
					int blockSize = key.BlockSize;
					List<Tag> list = (from tg in item3.Value
						orderby tg.WordAddress, tg.BitAddress
						select tg).ToList();
					int count = list.Count;
					for (int i = 0; i < count; i++)
					{
						empty2 = DeltaUtility.GetMemory(list[i]);
						bool flag2 = key.DeviceType == 2;
						if (!DeltaUtility.AhAsSeries.IsBitOnlyMemory(empty2) && (flag2 || !(empty2 == "HC") || list[i].DataType != 0) && (flag2 || !(empty2 == "T") || list[i].DataType != 0) && (flag2 || !(empty2 == "C") || list[i].DataType != 0) && (!flag2 || list[i].DataType != 0))
						{
							num = DeltaUtility.GetSizeOfDataType(list[i]);
							int wordAddress = list[i].WordAddress;
							num3 = wordAddress - num2 + num;
							if (i == 0 || num3 > blockSize)
							{
								num2 = wordAddress;
								num3 = wordAddress - num2 + num;
								_packages[key].Add(new ReadPacket
								{
									ConnectRetries = key.ConnectRetries,
									ReceivingDelay = key.ReceivingDelay,
									StationNo = (byte)key.StationNo,
									Memory = empty2,
									Address = new IpsAddress(list[i].WordAddress, list[i].BitAddress)
								});
								index = _packages[key].Count - 1;
							}
							_packages[key][index].Tags.Add(list[i]);
							if (_packages[key][index].Quantity < num3)
							{
								_packages[key][index].Quantity = num3;
							}
						}
						else
						{
							int bitAddress = list[i].BitAddress;
							num3 = bitAddress - num2 + 1;
							if (i == 0 || num3 > 1600)
							{
								num2 = bitAddress;
								num3 = bitAddress - num2 + num;
								_packages[key].Add(new ReadPacket
								{
									StationNo = (byte)key.StationNo,
									ConnectRetries = key.ConnectRetries,
									ReceivingDelay = key.ReceivingDelay,
									Memory = empty2,
									Address = new IpsAddress(list[i].WordAddress, list[i].BitAddress)
								});
								index = _packages[key].Count - 1;
							}
							_packages[key][index].Tags.Add(list[i]);
							if (_packages[key][index].Quantity < num3)
							{
								_packages[key][index].Quantity = num3;
							}
						}
					}
				}
			}
		}
		switch (channel.Protocol)
		{
		case IpsProtocolType.DELTA_ASCII:
		{
			DeltaAsciiBuilder ascii = new DeltaAsciiBuilder();
			if (!_packages.Any())
			{
				break;
			}
			{
				foreach (KeyValuePair<Device, List<ReadPacket>> item in _packages)
				{
					Parallel.ForEach((IEnumerable<ReadPacket>)item.Value, (Action<ReadPacket>)delegate(ReadPacket RP)
					{
						bool flag4 = item.Key.DeviceType == 2;
						if (!DeltaUtility.AhAsSeries.IsBitOnlyMemory(RP.Tags[0]) && (flag4 || !RP.Tags[0].Address.StartsWith("HC") || RP.Tags[0].DataType != 0) && (flag4 || !RP.Tags[0].Address.StartsWith("T") || RP.Tags[0].DataType != 0) && (flag4 || !RP.Tags[0].Address.StartsWith("C") || RP.Tags[0].DataType != 0) && (!flag4 || RP.Tags[0].DataType != 0))
						{
							RP.Function = 3;
							int wordAddress3 = RP.Address.WordAddress;
							RP.SendMsg = ascii.ReadMessage(RP.StationNo, RP.Function, wordAddress3, RP.Quantity);
						}
						else
						{
							RP.Function = 1;
							int bitAddress3 = RP.Address.BitAddress;
							RP.SendMsg = ascii.ReadMessage(RP.StationNo, RP.Function, bitAddress3, RP.Quantity);
						}
					});
				}
				break;
			}
		}
		case IpsProtocolType.DELTA_RTU:
		{
			DeltaRtuBuilder deltaRtuBuilder_0 = new DeltaRtuBuilder();
			if (!_packages.Any())
			{
				break;
			}
			{
				foreach (KeyValuePair<Device, List<ReadPacket>> item in _packages)
				{
					Parallel.ForEach((IEnumerable<ReadPacket>)item.Value, (Action<ReadPacket>)delegate(ReadPacket RP)
					{
						bool flag3 = item.Key.DeviceType == 2;
						if (!DeltaUtility.AhAsSeries.IsBitOnlyMemory(RP.Tags[0]) && (flag3 || !RP.Tags[0].Address.StartsWith("HC") || RP.Tags[0].DataType != 0) && (flag3 || !RP.Tags[0].Address.StartsWith("T") || RP.Tags[0].DataType != 0) && (flag3 || !RP.Tags[0].Address.StartsWith("C") || RP.Tags[0].DataType != 0) && (!flag3 || RP.Tags[0].DataType != 0))
						{
							RP.Function = 3;
							int wordAddress2 = RP.Address.WordAddress;
							RP.SendBytes = deltaRtuBuilder_0.ReadMessage(RP.StationNo, wordAddress2, RP.Function, RP.Quantity);
						}
						else
						{
							RP.Function = 1;
							int bitAddress2 = RP.Address.BitAddress;
							RP.SendBytes = deltaRtuBuilder_0.ReadMessage(RP.StationNo, bitAddress2, RP.Function, RP.Quantity);
						}
					});
				}
				break;
			}
		}
		case IpsProtocolType.DELTA_TCP:
		{
			DeltaTcpBuilder deltaTcpBuilder_0 = new DeltaTcpBuilder();
			if (!_packages.Any())
			{
				break;
			}
			{
				foreach (KeyValuePair<Device, List<ReadPacket>> item in _packages)
				{
					Parallel.ForEach((IEnumerable<ReadPacket>)item.Value, (Action<ReadPacket>)delegate(ReadPacket RP)
					{
						bool flag5 = item.Key.DeviceType == 2;
						if (!DeltaUtility.AhAsSeries.IsBitOnlyMemory(RP.Tags[0]) && (flag5 || !RP.Tags[0].Address.StartsWith("HC") || RP.Tags[0].DataType != 0) && (flag5 || !RP.Tags[0].Address.StartsWith("T") || RP.Tags[0].DataType != 0) && (flag5 || !RP.Tags[0].Address.StartsWith("C") || RP.Tags[0].DataType != 0) && (!flag5 || RP.Tags[0].DataType != 0))
						{
							RP.Function = 3;
							int wordAddress4 = RP.Address.WordAddress;
							RP.SendBytes = deltaTcpBuilder_0.ReadMessage(0, RP.StationNo, wordAddress4, RP.Function, RP.Quantity);
						}
						else
						{
							RP.Function = 1;
							int bitAddress4 = RP.Address.BitAddress;
							RP.SendBytes = deltaTcpBuilder_0.ReadMessage(0, RP.StationNo, bitAddress4, RP.Function, RP.Quantity);
						}
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

	private void ResetAll()
	{
		try
		{
			foreach (Device value in Devices.Values)
			{
				Parallel.ForEach((IEnumerable<ReadPacket>)_packages[value], (Action<ReadPacket>)delegate(ReadPacket RP)
				{
					foreach (Tag tag in RP.Tags)
					{
						tag.SetDefaultValue();
					}
				});
			}
		}
		catch (Exception ex)
		{
			DriverDataSource.SaveLog(channel.Name + ".", ex.Message);
		}
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
		IDeltaMaster master = Masters[channel.Name];
		string source = channel.Name + ".";
		try
		{
			if (!channel.Devices.Where((Device device_0) => device_0.Active).Any())
			{
				return;
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
			 
		}
		catch (Exception ex3)
		{
			DriverDataSource.SaveLog(channel.Name, ex3.Message);
		}
		finally
		{
			master.Disconnect();
			ResetAll();
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
		IDeltaMaster master = Masters[key];
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
						goto IL_023b;
					}
					if (channel.Devices.Count > 1)
					{
						continue;
					}
					Thread.Sleep(30000);
					goto IL_023b;
					IL_023b:
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
			ResetAll();
			master.Disconnect();
		}
	}

	public async Task<IPSResult> ReadStatus(Channel channel, Device device, IDeltaMaster Master, ReadPacket RP)
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
						index = RP.Tags[y].BitAddress - RP.Tags[0].BitAddress;
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
		}
		catch (Exception ex)
		{
			foreach (Tag tag in RP.Tags)
			{
				tag.Status = TagStatus.Bad;
			}
			string source = channel.Name + "." + device.ToString() + ".";
			string message = RP.ToString() + ": " + ex.Message;
			DriverDataSource.SaveLog(source, message);
		}
		return iPSResult;
	}

	public async Task<IPSResult> ReadRegisters(Channel channel, Device device, IDeltaMaster master, ReadPacket RP)
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
					int num = 2 * (RP.Tags[y].WordAddress - RP.Address.WordAddress);
					switch (RP.Tags[y].DataType)
					{
					case DataType.BOOL:
					{
						BOOL[] bools = BOOL.GetBools(new byte[2]
						{
							values[num],
							values[num + 1]
						}, ByteOrder.BigEndian);
						int num2 = RP.Tags[y].BitAddress % 16;
						RP.Tags[y].Value = bools[num2];
						RP.Tags[y].Status = TagStatus.Good;
						break;
					}
					default:
						RP.Tags[y].Status = TagStatus.Bad;
						break;
					case DataType.INT:
						RP.Tags[y].Value = new INT(DeltaUtility.Sort(new byte[2]
						{
							values[num],
							values[num + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UINT:
						RP.Tags[y].Value = new UINT(DeltaUtility.Sort(new byte[2]
						{
							values[num],
							values[num + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.WORD:
						RP.Tags[y].Value = new WORD(DeltaUtility.Sort(new byte[2]
						{
							values[num],
							values[num + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DINT:
						if (RP.Tags[y].Address.StartsWith("HC"))
						{
							num = 2 * num;
						}
						RP.Tags[y].Value = new DINT(DeltaUtility.Sort(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UDINT:
						if (RP.Tags[y].Address.StartsWith("HC"))
						{
							num = 2 * num;
						}
						RP.Tags[y].Value = new UDINT(DeltaUtility.Sort(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DWORD:
						if (RP.Tags[y].Address.StartsWith("HC"))
						{
							num = 2 * num;
						}
						RP.Tags[y].Value = new DWORD(DeltaUtility.Sort(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.REAL:
						if (RP.Tags[y].Address.StartsWith("HC"))
						{
							num = 2 * num;
						}
						RP.Tags[y].Value = new REAL(DeltaUtility.Sort(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LINT:
						if (RP.Tags[y].Address.StartsWith("HC"))
						{
							num = 2 * num;
						}
						RP.Tags[y].Value = new LINT(DeltaUtility.Sort(new byte[8]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3],
							values[num + 4],
							values[num + 5],
							values[num + 6],
							values[num + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.ULINT:
						if (RP.Tags[y].Address.StartsWith("HC"))
						{
							num = 2 * num;
						}
						RP.Tags[y].Value = new ULINT(DeltaUtility.Sort(new byte[8]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3],
							values[num + 4],
							values[num + 5],
							values[num + 6],
							values[num + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LWORD:
						if (RP.Tags[y].Address.StartsWith("HC"))
						{
							num = 2 * num;
						}
						RP.Tags[y].Value = new LWORD(DeltaUtility.Sort(new byte[8]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3],
							values[num + 4],
							values[num + 5],
							values[num + 6],
							values[num + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LREAL:
						if (RP.Tags[y].Address.StartsWith("HC"))
						{
							num = 2 * num;
						}
						RP.Tags[y].Value = new LREAL(DeltaUtility.Sort(new byte[8]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3],
							values[num + 4],
							values[num + 5],
							values[num + 6],
							values[num + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME16:
						RP.Tags[y].Value = TIME16.Parse(DeltaUtility.Sort(new byte[2]
						{
							values[num],
							values[num + 1]
						}), RP.Tags[y].Resolution);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME32:
						if (RP.Tags[y].Address.StartsWith("HC"))
						{
							num = 2 * num;
						}
						RP.Tags[y].Value = TIME32.Parse(DeltaUtility.Sort(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}), RP.Tags[y].Resolution);
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
		return iPSResult;
	}

	public async Task<IPSResult> WriteAsync(Tag tg, dynamic value)
	{
		IPSResult result = new IPSResult();
		try
		{
			string[] array = tg.FullName.Split('.');
			if (array.Length < 3)
			{
				result.Message = "Write: Failure, invalid tag name: " + tg.FullName;
				result.Status = CommStatus.Error;
				return result;
			}
			string text = array[0] + "." + array[1];
			if (!Devices.ContainsKey(text))
			{
				result.Status = CommStatus.Error;
				result.Message = "Device Id=" + text + " does not exist.";
				return result;
			}
			Device device = Devices[text];
			IDeltaMaster deltaMaster = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[channel.Name + "." + device.Name]);
			WritePacket writePacket = new WritePacket
			{
				StationNo = (byte)device.StationNo,
				Memory = DeltaUtility.GetMemory(tg),
				Address = new IpsAddress(tg.WordAddress, tg.BitAddress),
				ConnectRetries = device.ConnectRetries,
				ReceivingDelay = device.ReceivingDelay
			};
			if (tg.DataType == DataType.BOOL)
			{
				writePacket.DataDec = ((!(BOOL)value) ? new byte[2] : new byte[2] { 255, 0 });
				result = await deltaMaster.WriteCoilAsync(writePacket);
			}
			else
			{
				switch (tg.DataType)
				{
				default:
					throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
				case DataType.INT:
					writePacket.DataDec = DeltaUtility.Sort(BitConverter.GetBytes((INT)value));
					break;
				case DataType.UINT:
					writePacket.DataDec = DeltaUtility.Sort(BitConverter.GetBytes((UINT)value));
					break;
				case DataType.WORD:
				{
					byte[] bytesFromHex2 = BYTE.GetBytesFromHex(((WORD)value).Value);
					writePacket.DataDec = new byte[2]
					{
						bytesFromHex2[0],
						bytesFromHex2[1]
					};
					break;
				}
				case DataType.DINT:
					writePacket.DataDec = DeltaUtility.Sort(BitConverter.GetBytes((DINT)value));
					break;
				case DataType.UDINT:
					writePacket.DataDec = DeltaUtility.Sort(BitConverter.GetBytes((UDINT)value));
					break;
				case DataType.DWORD:
				{
					byte[] bytesFromHex = BYTE.GetBytesFromHex(((DWORD)value).Value);
					writePacket.DataDec = new byte[4]
					{
						bytesFromHex[2],
						bytesFromHex[3],
						bytesFromHex[0],
						bytesFromHex[1]
					};
					break;
				}
				case DataType.REAL:
					writePacket.DataDec = DeltaUtility.Sort(BitConverter.GetBytes((REAL)value));
					break;
				case DataType.LINT:
					writePacket.DataDec = DeltaUtility.Sort(BitConverter.GetBytes((LINT)value));
					break;
				case DataType.ULINT:
					writePacket.DataDec = DeltaUtility.Sort(BitConverter.GetBytes((ULINT)value));
					break;
				case DataType.LWORD:
					writePacket.DataDec = DeltaUtility.Sort(BYTE.GetBytesFromHex(((LWORD)value).Value));
					break;
				case DataType.LREAL:
					writePacket.DataDec = DeltaUtility.Sort(BitConverter.GetBytes((LREAL)value));
					break;
				case DataType.TIME16:
					writePacket.DataDec = DeltaUtility.Sort(BitConverter.GetBytes((ushort)((TIME16)value).Value.TotalMilliseconds / tg.Resolution));
					break;
				case DataType.TIME32:
					writePacket.DataDec = DeltaUtility.Sort(BitConverter.GetBytes((uint)((TIME32)value).Value.TotalMilliseconds / tg.Resolution));
					break;
				case DataType.STRING:
					break;
				}
				result = await deltaMaster.WriteRegisterAsync(writePacket);
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
