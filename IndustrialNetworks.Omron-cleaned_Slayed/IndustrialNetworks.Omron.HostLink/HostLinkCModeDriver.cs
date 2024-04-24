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
using NetStudio.Omron.Models;

namespace NetStudio.Omron.HostLink;

public class HostLinkCModeDriver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, HostLinkCModeProtocol> Masters = new Dictionary<string, HostLinkCModeProtocol>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private BindingList<IpsLog> Logs;

	private Channel channel;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public HostLinkCModeDriver(Channel channel, BindingList<IpsLog> eventLogs)
	{
		this.channel = channel;
		Logs = eventLogs;
		OnInitialize(this.channel);
	}

	private void OnInitialize(Channel channel)
	{
		
		string address_key = string.Empty;
		Dictionary<Device, Dictionary<string, List<Tag>>> devices = new Dictionary<Device, Dictionary<string, List<Tag>>>();
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			try
			{
				Masters.Add(channel.Name, new HostLinkCModeProtocol(channel.Adapter));
			}
			catch (Exception ex)
			{
				MessageLog(channel.Name, ex.Message);
			}
		}
		else
		{
			foreach (Device device in channel.Devices)
			{
				try
				{
					Masters.Add(channel.Name + "." + device.Name, new HostLinkCModeProtocol(device.Adapter));
				}
				catch (Exception ex2)
				{
					MessageLog(channel.Name + "." + device.Name, ex2.Message);
				}
			}
		}
		Parallel.ForEach((IEnumerable<Device>)channel.Devices, (Action<Device>)delegate(Device device)
		{
			device.ByteOrder = ByteOrder.LittleEndian;
			device.BlockSize = 30;
			Devices.Add(channel.Name + "." + device.Name, device);
			if (!devices.ContainsKey(device))
			{
				devices.Add(device, new Dictionary<string, List<Tag>>());
			}
			Dictionary<string, List<Tag>> dictionary = devices[device];
			foreach (Group group in device.Groups)
			{
				foreach (Tag tag3 in group.Tags)
				{
					address_key = OmronUtility.GetMemory(tag3.Address);
					tag3.WordAddress = OmronUtility.GetWordAddress(tag3.Address);
					tag3.BitAddress = OmronUtility.GetBitAddress(tag3.Address);
					if (!dictionary.ContainsKey(address_key))
					{
						dictionary.Add(address_key, new List<Tag>());
					}
					tag3.FullName = $"{channel.Name}.{device.Name}.{group.Name}.{tag3.Name}";
					dictionary[address_key].Add(tag3);
					Tags.Add(tag3.FullName, tag3);
				}
			}
		});
		if (devices.Any())
		{
			Parallel.ForEach((IEnumerable<KeyValuePair<Device, Dictionary<string, List<Tag>>>>)devices, (Action<KeyValuePair<Device, Dictionary<string, List<Tag>>>>)delegate(KeyValuePair<Device, Dictionary<string, List<Tag>>> item)
			{
				Device key = item.Key;
				Dictionary<string, List<Tag>> value = item.Value;
				if (!_packages.ContainsKey(key))
				{
					_packages.Add(key, new List<ReadPacket>());
				}
				foreach (KeyValuePair<string, List<Tag>> item1 in value)
				{
					ushort num = 0;
					int index = 0;
					int num2 = 0;
					int num3 = 0;
					int blockSize = key.BlockSize;
					List<Tag> value2 = item1.Value;
					int count = value2.Count;
					value2.Sort((Tag tg, Tag tag_1) => OmronUtility.GetTotalBits(tg.Address).CompareTo(OmronUtility.GetTotalBits(tag_1.Address)));
					for (int i = 0; i < count; i++)
					{
						num = OmronUtility.GetSizeOfDataType(value2[i]);
						int wordAddress3 = OmronUtility.GetWordAddress(value2[i].Address);
						num3 = wordAddress3 - num2 + num;
						if (i == 0 || num3 > blockSize)
						{
							num2 = wordAddress3;
							num3 = wordAddress3 - num2 + num;
							_packages[key].Add(new ReadPacket
							{
								StationNo = (byte)key.StationNo,
								Address = num2.ToString("D4"),
								ConnectRetries = key.ConnectRetries,
								ReceivingDelay = key.ReceivingDelay
							});
							index = _packages[key].Count - 1;
							_packages[key][index].Memory = item1.Key;
						}
						value2[i].WordAddress = OmronUtility.GetWordAddress(value2[i].Address);
						value2[i].BitAddress = OmronUtility.GetBitAddress(value2[i].Address);
						_packages[key][index].Tags.Add(value2[i]);
					}
				}
			});
		}
		HostLinkCModeBuilder gClass = new HostLinkCModeBuilder();
		string empty = string.Empty;
		foreach (List<ReadPacket> value3 in _packages.Values)
		{
			foreach (ReadPacket item2 in value3)
			{
				Tag tag = item2.Tags[0];
				Tag tag2 = item2.Tags[item2.Tags.Count - 1];
				if (tag.Address.StartsWith("SYS"))
				{
					item2.SendMsg = gClass.ReadMsg(item2.StationNo, "MS");
					continue;
				}
				int wordAddress = OmronUtility.GetWordAddress(tag.Address);
				int wordAddress2 = OmronUtility.GetWordAddress(tag2.Address);
				int sizeOfDataType = OmronUtility.GetSizeOfDataType(tag2);
				item2.NumOfWords = wordAddress2 - wordAddress + sizeOfDataType;
				OmronUtility.GetWordAddress(tag.Address);
				OmronUtility.GetBitAddress(tag.Address);
				item2.Address = wordAddress.ToString("D4");
				empty = item2.Address + item2.NumOfWords.ToString("D4");
				item2.SendMsg = gClass.ReadMsg(item2.StationNo, HostLinkCModeBuilder.HeaderCodesForRead[item2.Memory], empty);
			}
		}
	}

	public void Start()
	{
		
		
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
			MessageLog(channel.Name + ".", ex.Message);
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
			string source = channel.Name + "." + device.ToString() + ".";
			MessageLog(source, "Lost connection to device.");
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
		if (channel == null)
		{
			return;
		}
		string source = channel.Name + ".";
		try
		{
			if (!channel.Devices.Where((Device device_0) => device_0.Active).Any())
			{
				return;
			}
			foreach (Device device3 in channel.Devices)
			{
				device3.Status = DeviceStatus.Connecting;
			}
			HostLinkCModeProtocol master = Masters[channel.Name];
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
								IPSResult iPSResult = await ReadAsync(channel, device, master, item);
								switch (iPSResult.Status)
								{
								case CommStatus.Success:
								{
									if (device.Status == DeviceStatus.Connected)
									{
										break;
									}
									device.Status = DeviceStatus.Connected;
									device.ReconnectManual = false;
									device.ReconnectFlag = false;
									source = channel.Name + "." + device.ToString() + ".";
									IpsLog[] array = DriverDataSource.Logs.Where((IpsLog ipsLog_0) => ipsLog_0.Source == source).ToArray();
									if (array != null)
									{
										IpsLog[] array2 = array;
										for (int i = 0; i < array2.Length; i++)
										{
											DriverDataSource.RemoveLogAsync(array2[i]);
										}
									}
									break;
								}
								case CommStatus.Error:
									source = channel.Name + "." + device.ToString() + ".";
									MessageLog(source, iPSResult.Message);
									break;
								case CommStatus.Timeout:
									if (device.Status != DeviceStatus.Connecting && device.Status != DeviceStatus.Connected)
									{
										Thread.Sleep(Utility.ConnectRetries);
										if (device.AutoReconnect)
										{
											source = channel.Name + "." + device.ToString() + ".";
											MessageLog(source, "Reconnecting to the device.", EvenType.Information);
										}
									}
									else
									{
										if (!device.AutoReconnect)
										{
											device.Active = false;
										}
										ResetDevice(device);
									}
									break;
								}
							}
							catch (Exception ex)
							{
								source = channel.Name + "." + device.ToString() + ".";
								MessageLog(source, ex.Message);
							}
						}
					}
				}
				catch (Exception ex2)
				{
					MessageLog(source, ex2.Message);
					Thread.Sleep(100);
				}
			}
		 
		}
		catch (Exception ex3)
		{
			MessageLog(source, ex3.Message);
		}
		finally
		{
			ResetAll();
			Masters[channel.Name].Disconnect();
		}
	}

	private async Task OnEthernetPolling(CancellationToken cancellationToken, Device device)
	{
		string key = channel.Name + "." + device.Name;
		if (!Masters.ContainsKey(key))
		{
			return;
		}
		HostLinkCModeProtocol master = Masters[key];
		string source = channel.Name + "." + device.ToString() + ".";
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
						goto IL_0255;
					}
					if (channel.Devices.Count > 1)
					{
						continue;
					}
					Thread.Sleep(30000);
					goto IL_0255;
					IL_0255:
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
							IPSResult iPSResult = await ReadAsync(channel, device, master, item);
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
					source = channel.Name + "." + device.ToString() + ".";
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
			await master.DisconnectAsync();
		}
	}

	public async Task<IPSResult> ReadAsync(Channel channel, Device device, HostLinkCModeProtocol master, ReadPacket RP)
	{
		
		
		
		IPSResult result;
		if (RP.Memory == "SYS")
		{
			result = master.ReadCpuMode(device.StationNo);
		}
		else
		{
			result = await master.Read(RP);
		}
		await Task.Run(delegate
		{
			if (result.Status == CommStatus.Success)
			{
				int num = 0;
				int count = RP.Tags.Count;
				string values_Hex = result.Values_Hex;
				while (num < count)
				{
					try
					{
						int startIndex = 4 * (RP.Tags[num].WordAddress - RP.Tags[0].WordAddress);
						switch (RP.Tags[num].DataType)
						{
						default:
							RP.Tags[num].Status = TagStatus.Bad;
							break;
						case DataType.BOOL:
						{
							BOOL[] array = BOOL.ParseArray(values_Hex.Substring(startIndex, 4));
							RP.Tags[num].Value = array[RP.Tags[num].BitAddress];
							RP.Tags[num].Status = TagStatus.Good;
							break;
						}
						case DataType.BYTE:
						{
							int startIndex2 = 2 * OmronUtility.GetWordAddress(RP.Tags[num].Address);
							RP.Tags[num].Value = BYTE.GetByteFromHex(values_Hex.Substring(startIndex2, 2));
							RP.Tags[num].Status = TagStatus.Good;
							break;
						}
						case DataType.INT:
							RP.Tags[num].Value = INT.Parse(values_Hex.Substring(startIndex, 4), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.UINT:
							RP.Tags[num].Value = UINT.Parse(values_Hex.Substring(startIndex, 4), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.WORD:
							RP.Tags[num].Value = WORD.Parse(values_Hex.Substring(startIndex, 4), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.DINT:
							RP.Tags[num].Value = DINT.Parse(values_Hex.Substring(startIndex, 8), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.UDINT:
							RP.Tags[num].Value = UDINT.Parse(values_Hex.Substring(startIndex, 8), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.DWORD:
							RP.Tags[num].Value = DWORD.Parse(values_Hex.Substring(startIndex, 8), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.REAL:
							RP.Tags[num].Value = REAL.Parse(values_Hex.Substring(startIndex, 8), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.LINT:
							RP.Tags[num].Value = LINT.Parse(values_Hex.Substring(startIndex, 16), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.ULINT:
							RP.Tags[num].Value = ULINT.Parse(values_Hex.Substring(startIndex, 16), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.LWORD:
							RP.Tags[num].Value = LWORD.Parse(values_Hex.Substring(startIndex, 16), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.LREAL:
							RP.Tags[num].Value = LREAL.Parse(values_Hex.Substring(startIndex, 16), ByteOrder.LittleEndian);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.TIME16:
							RP.Tags[num].Value = TIME16.Parse(values_Hex.Substring(startIndex, 4), ByteOrder.LittleEndian, RP.Tags[num].Resolution, TypeStyles.Decimal);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.TIME32:
							RP.Tags[num].Value = TIME32.Parse(values_Hex.Substring(startIndex, 8), ByteOrder.LittleEndian, RP.Tags[num].Resolution);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						case DataType.STRING:
						{
							string value_hex = result.Values_Hex.Substring(num * 64, 64);
							RP.Tags[num].Value = STRING.HexToString(value_hex, device.ByteOrder);
							RP.Tags[num].Status = TagStatus.Good;
							break;
						}
						}
					}
					catch (IndexOutOfRangeException ex)
					{
						RP.Tags[num].SetDefaultValue();
						string source = channel.Name + "." + device.ToString() + ".";
						MessageLog(source, ex.Message);
					}
					catch (ArgumentOutOfRangeException)
					{
						RP.Tags[num].SetDefaultValue();
						string source2 = channel.Name + "." + device.ToString() + ".";
						string message = $"Tag(Name={RP.Tags[num].FullName}, Address={RP.Tags[num].Address}): {"Out-of-range address or the data read from the device is missing."}";
						MessageLog(source2, message);
					}
					catch (Exception ex3)
					{
						RP.Tags[num].SetDefaultValue();
						string source3 = channel.Name + "." + device.ToString() + ".";
						string message2 = $"Tag(Name={RP.Tags[num].FullName}, Address={RP.Tags[num].Address}): {ex3.Message}";
						MessageLog(source3, message2);
					}
					finally
					{
						num++;
					}
				}
				return;
			}
			foreach (Tag tag in RP.Tags)
			{
				tag.SetDefaultValue();
			}
		});
		return result;
	}

	public async Task<IPSResult> WriteAsync(Tag tg, dynamic value)
	{
		
		
		return await Task.Run(delegate
		{
			IPSResult iPSResult = new IPSResult();
			try
			{
				string[] array = tg.FullName.Split('.');
				if (array.Length < 3)
				{
					iPSResult.Message = "Write: Failure, invalid tag name: " + tg.FullName;
					iPSResult.Status = CommStatus.Error;
					return iPSResult;
				}
				string key = ((channel.ConnectionType == ConnectionType.Serial) ? array[0].Trim() : (array[0].Trim() + "." + array[1].Trim()));
				string key2 = array[0].Trim() + "." + array[1].Trim();
				if (!Masters.ContainsKey(key))
				{
					iPSResult.Message = "Write: Failure, invalid tag name: " + tg.FullName;
					iPSResult.Status = CommStatus.Error;
					return iPSResult;
				}
				Device device = Devices[key2];
				HostLinkCModeProtocol hostLinkCModeProtocol = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[key]);
				int stationNo = device.StationNo;
				int wordAddress = OmronUtility.GetWordAddress(tg.Address, isFins: true);
				string memory = OmronUtility.GetMemory(tg.Address);
				string header = HostLinkCModeBuilder.HeaderCodesForWrite[memory];
				string empty = string.Empty;
				switch (tg.DataType)
				{
				default:
					throw new NotSupportedException();
				case DataType.BOOL:
					empty = (((bool)value) ? "01" : "00");
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.BYTE:
				{
					Mode mode = Mode.PROGRAM;
					if (value == 0)
					{
						mode = Mode.PROGRAM;
					}
					else if (value == 2)
					{
						mode = Mode.MONITOR;
					}
					else
					{
						if (!((value == 4) ? true : false))
						{
							iPSResult.Status = CommStatus.Error;
							iPSResult.Message = "Tag can only take the value 0 or 2 or 4.";
							return iPSResult;
						}
						mode = Mode.RUN;
					}
					iPSResult = hostLinkCModeProtocol.WriteMode(stationNo, mode);
					break;
				}
				case DataType.INT:
					empty = INT.ToHex((INT)value);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.UINT:
					empty = UINT.ToHex((UINT)value);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.WORD:
					empty = WORD.ToHex((WORD)value, ByteOrder.LittleEndian);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.DINT:
					empty = DINT.ToHex((DINT)value);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.UDINT:
					empty = UDINT.ToHex((UDINT)value);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.DWORD:
					empty = DWORD.ToHex((DWORD)value, ByteOrder.LittleEndian);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.REAL:
					empty = REAL.ToHex((REAL)value);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.LINT:
					empty = LINT.ToHex((LINT)value);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.ULINT:
					empty = ULINT.ToHex((ULINT)value);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.LWORD:
					empty = LWORD.ToHex((LWORD)value, ByteOrder.LittleEndian);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.LREAL:
					empty = LREAL.ToHex((LREAL)value);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				case DataType.TIME16:
				{
					ushort value2 = (ushort)(((TIME16)value).Value.TotalMilliseconds / (double)(int)tg.Resolution);
					empty = $"0000{value2}".Right(4);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				}
				case DataType.TIME32:
					empty = TIME32.ToHex((TIME32)value, ByteOrder.BigEndian, tg.Resolution);
					iPSResult = hostLinkCModeProtocol.Write(stationNo, header, wordAddress, empty);
					break;
				}
			}
			catch (Exception ex)
			{
				iPSResult.Message = "Write: Failure. " + ex.Message;
				iPSResult.Status = CommStatus.Error;
				MessageLog(tg.FullName, ex.Message);
			}
			return iPSResult;
		});
	}

	private void MessageLog(string source, string message, EvenType evenType = EvenType.Error)
	{
		
	
		IpsLog ipsLog = DriverDataSource.Logs.FirstOrDefault((IpsLog ipsLog_0) => ipsLog_0 != null && ipsLog_0.Source == source && ipsLog_0.Message == message);
		if (ipsLog == null)
		{
			DriverDataSource.AddLogAsync(new IpsLog
			{
				EvenType = evenType,
				Source = source,
				Message = message,
				Time = DateTime.Now,
				Counter = 1u
			});
		}
		else
		{
			ipsLog.Time = DateTime.Now;
			ipsLog.Counter++;
		}
	}
}
