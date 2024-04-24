using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Vigor.Enums;

namespace NetStudio.Vigor;

public class VSDriver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, VSProtocol> Masters = new Dictionary<string, VSProtocol>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private BindingList<IpsLog> Logs;

	private Channel channel;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public VSDriver(Channel channel, BindingList<IpsLog> eventLogs)
	{
		this.channel = channel;
		Logs = eventLogs;
		OnInitialize(this.channel);
	}

	private void OnInitialize(Channel channel)
	{
		string empty = string.Empty;
		Dictionary<Device, Dictionary<string, List<Tag>>> dictionary = new Dictionary<Device, Dictionary<string, List<Tag>>>();
		if (channel.ConnectionType == ConnectionType.Serial)
		{
			try
			{
				Masters.Add(channel.Name, new VSProtocol(channel.Adapter));
			}
			catch (Exception ex)
			{
				DriverDataSource.SaveLog(channel.Name, ex.Message);
			}
		}
		else
		{
			foreach (Device device in channel.Devices)
			{
				try
				{
					Masters.Add(channel.Name + "." + device.Name, new VSProtocol(device.Adapter));
				}
				catch (Exception ex2)
				{
					DriverDataSource.SaveLog(channel.Name + "." + device.Name, ex2.Message);
				}
			}
		}
		foreach (Device device2 in channel.Devices)
		{
			Devices.Add(channel.Name + "." + device2.Name, device2);
			if (!dictionary.ContainsKey(device2))
			{
				dictionary.Add(device2, new Dictionary<string, List<Tag>>());
			}
			Dictionary<string, List<Tag>> dictionary2 = dictionary[device2];
			foreach (Group group in device2.Groups)
			{
				foreach (Tag tag3 in group.Tags)
				{
					empty = VSUtility.GetSpecialMemory(tag3);
					if (!dictionary2.ContainsKey(empty))
					{
						dictionary2.Add(empty, new List<Tag>());
					}
					tag3.FullName = $"{channel.Name}.{device2.Name}.{group.Name}.{tag3.Name}";
					dictionary2[empty].Add(tag3);
					Tags.Add(tag3.FullName, tag3);
				}
			}
		}
		if (dictionary.Any())
		{
			foreach (KeyValuePair<Device, Dictionary<string, List<Tag>>> item in dictionary)
			{
				Device key = item.Key;
				Dictionary<string, List<Tag>> value = item.Value;
				if (!_packages.ContainsKey(key))
				{
					_packages.Add(key, new List<ReadPacket>());
				}
				foreach (KeyValuePair<string, List<Tag>> item2 in value)
				{
					ushort num = 0;
					int index = 0;
					int num2 = 0;
					int num3 = 0;
					int num4 = key.BlockSize;
					List<Tag> value2 = item2.Value;
					int count = value2.Count;
					value2.Sort((Tag tg, Tag tag_1) => VSUtility.GetTotalBits(tg.Address).CompareTo(VSUtility.GetTotalBits(tag_1.Address)));
					if (value2[0].DataType == DataType.BOOL)
					{
						num4 = 16 * key.BlockSize;
					}
					for (int i = 0; i < count; i++)
					{
						num = VSUtility.GetSizeOfDataType(value2[i].DataType);
						int wordAddress = VSUtility.GetWordAddress(value2[i].Address);
						num3 = wordAddress - num2 + num;
						if (i == 0 || num3 > num4)
						{
							num2 = wordAddress;
							num3 = wordAddress - num2 + num;
							_packages[key].Add(new ReadPacket
							{
								StationNo = (byte)key.StationNo,
								Address = num2.ToString()
							});
							index = _packages[key].Count - 1;
							_packages[key][index].Memory = item2.Key;
						}
						value2[i].WordAddress = VSUtility.GetWordAddress(value2[i].Address);
						value2[i].BitAddress = VSUtility.GetBitAddress(value2[i].Address);
						_packages[key][index].Tags.Add(value2[i]);
					}
				}
			}
		}
		VSBuilder builder = new VSBuilder();
		foreach (List<ReadPacket> value3 in _packages.Values)
		{
			Parallel.ForEach((IEnumerable<ReadPacket>)value3, (Action<ReadPacket>)delegate(ReadPacket RP)
			{
				if (RP.Tags.Count > 0)
				{
					Tag tag = RP.Tags[0];
					Tag tag2 = RP.Tags[RP.Tags.Count - 1];
					int sizeOfDataType = VSUtility.GetSizeOfDataType(tag2.DataType);
					RP.DeviceCode = VSUtility.DeviceCodes[RP.Memory];
					RP.Quantity = tag2.WordAddress - tag.WordAddress + sizeOfDataType;
					RP.WordAddress = int.Parse(tag.WordAddress.ToString(), NumberStyles.HexNumber);
					if (tag.DataType == DataType.BOOL)
					{
						RP.Function = FunctionCode.BitDeviceRead;
						RP.SendBytes = builder.ReadMsg(RP);
						RP.NumOfBytes = RP.Quantity / 8 + ((RP.Quantity % 8 != 0) ? 1 : 0);
					}
					else
					{
						if (RP.Memory.StartsWith("C"))
						{
							RP.Quantity = tag2.WordAddress - tag.WordAddress + 1;
						}
						RP.Function = FunctionCode.WordDeviceRead;
						RP.SendBytes = builder.ReadMsg(RP);
						if (RP.Memory.StartsWith("C"))
						{
							if (RP.DeviceCode == DeviceCode.Counter16Bit)
							{
								RP.NumOfBytes = 2 * RP.Quantity;
							}
							else
							{
								RP.NumOfBytes = 4 * RP.Quantity;
							}
						}
						else
						{
							RP.NumOfBytes = 2 * RP.Quantity;
						}
					}
				}
			});
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
			VSProtocol master = Masters[channel.Name];
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
					DriverDataSource.SaveLog(source, ex2.Message);
					Thread.Sleep(100);
				}
			}
			 
		}
		catch (Exception ex3)
		{
			DriverDataSource.SaveLog(source, ex3.Message);
		}
		finally
		{
			ResetAll();
			if (Masters.ContainsKey(channel.Name))
			{
				Masters[channel.Name].Disconnect();
			}
		}
	}

	private async Task OnEthernetPolling(CancellationToken cancellationToken, Device device)
	{
		string key = channel.Name + "." + device.Name;
		if (!Masters.ContainsKey(key))
		{
			return;
		}
		VSProtocol master = Masters[key];
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

	public async Task<IPSResult> ReadAsync(Channel channel, Device device, VSProtocol master, ReadPacket RP)
	{
		
		
		
		IPSResult result = await master.ReadAsync(RP);
		if (result.Status == CommStatus.Success)
		{
			int tagCount = RP.Tags.Count;
			byte[] values = result.Values;
			Parallel.For(0, tagCount, delegate(int y, ParallelLoopState state)
			{
				try
				{
					int indexOfWordAddress = VSUtility.GetIndexOfWordAddress(RP.Tags[0], RP.Tags[y]);
					switch (RP.Tags[y].DataType)
					{
					default:
						RP.Tags[y].Status = TagStatus.Bad;
						break;
					case DataType.BOOL:
					{
						BOOL[] bools = BOOL.GetBools(values, ByteOrder.LittleEndian);
						if (tagCount <= bools.Length)
						{
							while (y < tagCount)
							{
								try
								{
									if (RP.Tags[y].Address.Contains("."))
									{
										RP.Tags[y].Value = bools[RP.Tags[y].BitAddress];
									}
									else
									{
										int num2 = RP.Tags[y].WordAddress - RP.Tags[0].WordAddress;
										RP.Tags[y].Value = bools[num2];
									}
									RP.Tags[y].Status = TagStatus.Good;
								}
								catch (Exception)
								{
									RP.Tags[y].Status = TagStatus.Bad;
								}
								finally
								{
									y++;
								}
							}
							y--;
						}
						break;
					}
					case DataType.BYTE:
						RP.Tags[y].Value = values[indexOfWordAddress];
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.INT:
						RP.Tags[y].Value = new INT(VSUtility.Sort(new byte[2]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UINT:
						RP.Tags[y].Value = new UINT(VSUtility.Sort(new byte[2]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.WORD:
						RP.Tags[y].Value = new WORD(VBUtility.Sort(new byte[2]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DINT:
						if (RP.Tags[y].Address.StartsWith("C"))
						{
							int num = 2 * indexOfWordAddress;
							RP.Tags[y].Value = new DINT(VSUtility.Sort(new byte[4]
							{
								values[num],
								values[num + 1],
								values[num + 2],
								values[num + 3]
							}));
						}
						else
						{
							RP.Tags[y].Value = new DINT(VSUtility.Sort(new byte[4]
							{
								values[indexOfWordAddress],
								values[indexOfWordAddress + 1],
								values[indexOfWordAddress + 2],
								values[indexOfWordAddress + 3]
							}));
						}
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UDINT:
						RP.Tags[y].Value = new UDINT(VSUtility.Sort(new byte[4]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1],
							values[indexOfWordAddress + 2],
							values[indexOfWordAddress + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DWORD:
						RP.Tags[y].Value = new DWORD(VBUtility.Sort(new byte[4]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1],
							values[indexOfWordAddress + 2],
							values[indexOfWordAddress + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.REAL:
						RP.Tags[y].Value = new REAL(VSUtility.Sort(new byte[4]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1],
							values[indexOfWordAddress + 2],
							values[indexOfWordAddress + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LINT:
						RP.Tags[y].Value = new LINT(VSUtility.Sort(new byte[8]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1],
							values[indexOfWordAddress + 2],
							values[indexOfWordAddress + 3],
							values[indexOfWordAddress + 4],
							values[indexOfWordAddress + 5],
							values[indexOfWordAddress + 6],
							values[indexOfWordAddress + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.ULINT:
						RP.Tags[y].Value = new ULINT(VSUtility.Sort(new byte[8]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1],
							values[indexOfWordAddress + 2],
							values[indexOfWordAddress + 3],
							values[indexOfWordAddress + 4],
							values[indexOfWordAddress + 5],
							values[indexOfWordAddress + 6],
							values[indexOfWordAddress + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LWORD:
						RP.Tags[y].Value = new LWORD(VBUtility.Sort(new byte[8]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1],
							values[indexOfWordAddress + 2],
							values[indexOfWordAddress + 3],
							values[indexOfWordAddress + 4],
							values[indexOfWordAddress + 5],
							values[indexOfWordAddress + 6],
							values[indexOfWordAddress + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LREAL:
						RP.Tags[y].Value = new LREAL(VSUtility.Sort(new byte[8]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1],
							values[indexOfWordAddress + 2],
							values[indexOfWordAddress + 3],
							values[indexOfWordAddress + 4],
							values[indexOfWordAddress + 5],
							values[indexOfWordAddress + 6],
							values[indexOfWordAddress + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME16:
						RP.Tags[y].Value = new TIME16(VBUtility.Sort(new byte[2]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1]
						}), RP.Tags[y].Resolution);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME32:
						RP.Tags[y].Value = new TIME32(VBUtility.Sort(new byte[4]
						{
							values[indexOfWordAddress],
							values[indexOfWordAddress + 1],
							values[indexOfWordAddress + 2],
							values[indexOfWordAddress + 3]
						}), RP.Tags[y].Resolution);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.STRING:
					{
						string value_hex = result.Values_Hex.Substring(8).Substring(indexOfWordAddress, 64);
						RP.Tags[y].Value = STRING.HexToString(value_hex, ByteOrder.LittleEndian);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					}
					}
				}
				catch (IndexOutOfRangeException ex2)
				{
					RP.Tags[y].SetDefaultValue();
					DriverDataSource.SaveLog(channel.Name + "." + device.ToString() + ".", ex2.Message);
				}
				catch (ArgumentOutOfRangeException)
				{
					RP.Tags[y].SetDefaultValue();
					string source = channel.Name + "." + device.ToString() + ".";
					string message = $"Tag(Name={RP.Tags[y].FullName}, Address={RP.Tags[y].Address}): {"Out-of-range address or the data read from the device is missing."}";
					DriverDataSource.SaveLog(source, message);
				}
				catch (Exception ex4)
				{
					RP.Tags[y].SetDefaultValue();
					string source2 = channel.Name + "." + device.ToString() + ".";
					string message2 = $"Tag(Name={RP.Tags[y].FullName}, Address={RP.Tags[y].Address}): {ex4.Message}";
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
		return result;
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
			string key = ((channel.ConnectionType == ConnectionType.Serial) ? array[0].Trim() : (array[0].Trim() + "." + array[1].Trim()));
			string key2 = array[0].Trim() + "." + array[1].Trim();
			if (!Masters.ContainsKey(key))
			{
				result.Message = "Write: Failure, invalid tag name: " + tg.FullName;
				result.Status = CommStatus.Error;
				return result;
			}
			_ = Devices[key2];
			VSProtocol vSProtocol = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[key]);
			_ = tg.DataType;
			string memory = VSUtility.GetMemory(tg.Address);
			DeviceCode deviceCode = VSUtility.DeviceCodes[memory];
			if (memory == "D" && tg.WordAddress >= 9000)
			{
				deviceCode = VSUtility.DeviceCodes["SD"];
			}
			WritePacket writePacket = new WritePacket
			{
				Memory = VSUtility.GetMemory(tg.Address),
				Address = tg.Address,
				WordAddress = tg.WordAddress,
				BitAddress = tg.BitAddress,
				Function = FunctionCode.WordDeviceWrite,
				DeviceCode = deviceCode
			};
			Array.Empty<byte>();
			switch (tg.DataType)
			{
			default:
				throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
			case DataType.BOOL:
				if (memory == "M" && tg.WordAddress >= 9000)
				{
					_ = VSUtility.DeviceCodes["SM"];
				}
				writePacket.IsBit = true;
				writePacket.Function = FunctionCode.BitDeviceWrite;
				writePacket.ValueDec = Conversion.BoolToBytes((bool)value);
				break;
			case DataType.BYTE:
				writePacket.ValueDec = BYTE.ToBytes(new BYTE[1] { (BYTE)value });
				break;
			case DataType.INT:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((INT)value));
				break;
			case DataType.UINT:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((UINT)value));
				break;
			case DataType.WORD:
				writePacket.ValueDec = BitConverter.GetBytes(Convert.ToUInt16(((WORD)value).Value, 16));
				break;
			case DataType.DINT:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((DINT)value));
				break;
			case DataType.UDINT:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((UDINT)value));
				break;
			case DataType.DWORD:
				writePacket.ValueDec = BitConverter.GetBytes(Convert.ToUInt32(((DWORD)value).Value, 16));
				break;
			case DataType.REAL:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((REAL)value));
				break;
			case DataType.LINT:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((LINT)value));
				break;
			case DataType.ULINT:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((ULINT)value));
				break;
			case DataType.LWORD:
				writePacket.ValueDec = BitConverter.GetBytes(Convert.ToUInt64(((LWORD)value).Value, 16));
				break;
			case DataType.LREAL:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((LREAL)value));
				break;
			case DataType.TIME16:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((ushort)(value.Value.TotalMilliseconds / tg.Resolution)));
				break;
			case DataType.TIME32:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((int)(value.Value.TotalMilliseconds / tg.Resolution)));
				break;
			}
			result = await vSProtocol.WriteAsync(writePacket);
			if (tg.Mode == TagMode.WriteOnly)
			{
				tg.Value = result.Status == CommStatus.Success;
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
