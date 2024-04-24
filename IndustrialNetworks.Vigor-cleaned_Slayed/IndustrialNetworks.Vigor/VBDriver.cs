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

namespace NetStudio.Vigor;

public class VBDriver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, VBProtocol> Masters = new Dictionary<string, VBProtocol>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private BindingList<IpsLog> Logs;

	private Channel channel;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public VBDriver(Channel channel, BindingList<IpsLog> eventLogs)
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
				Masters.Add(channel.Name, new VBProtocol(channel.Adapter));
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
					Masters.Add(channel.Name + "." + device.Name, new VBProtocol(device.Adapter));
				}
				catch (Exception ex2)
				{
					DriverDataSource.SaveLog(channel.Name + "." + device.Name, ex2.Message);
				}
			}
		}
		foreach (Device device2 in channel.Devices)
		{
			device2.BlockSize = 127;
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
					empty = VBUtility.GetSpecialMemory(tag3);
					if (empty == "T")
					{
						tag3.Resolution = VBUtility.GetResolution(tag3.Address);
					}
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
					value2.Sort((Tag tg, Tag tag_1) => VBUtility.GetBitAddress(tg).CompareTo(VBUtility.GetBitAddress(tag_1)));
					if (value2[0].DataType == DataType.BOOL)
					{
						num4 = 16 * key.BlockSize;
					}
					for (int i = 0; i < count; i++)
					{
						num = VBUtility.GetSizeOfDataType(value2[i].DataType);
						int byteAddress = VBUtility.GetByteAddress(value2[i]);
						num3 = byteAddress - num2 + num;
						if (i == 0 || num3 > num4)
						{
							num2 = byteAddress;
							num3 = byteAddress - num2 + num;
							_packages[key].Add(new ReadPacket
							{
								StationNo = (byte)key.StationNo,
								Address = num2.ToString()
							});
							index = _packages[key].Count - 1;
							_packages[key][index].Memory = item2.Key;
						}
						value2[i].ByteAddress = VBUtility.GetByteAddress(value2[i]);
						value2[i].BitAddress = VBUtility.GetBitAddress(value2[i]);
						value2[i].WordAddress = value2[i].ByteAddress / 2;
						_packages[key][index].Tags.Add(value2[i]);
					}
				}
			}
		}
		VBBuilder builder = new VBBuilder();
		foreach (List<ReadPacket> value3 in _packages.Values)
		{
			Parallel.ForEach((IEnumerable<ReadPacket>)value3, (Action<ReadPacket>)delegate(ReadPacket RP)
			{
				if (RP.Tags.Count > 0)
				{
					Tag tag = RP.Tags[0];
					Tag tag2 = RP.Tags[RP.Tags.Count - 1];
					int sizeOfDataType = VBUtility.GetSizeOfDataType(tag2.DataType);
					RP.NumOfBytes = tag2.ByteAddress - tag.ByteAddress + sizeOfDataType;
					RP.SendMsg = builder.ReadMsg((byte)RP.StationNo, tag.ByteAddress, RP.NumOfBytes);
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
			VBProtocol master = Masters[channel.Name];
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
		VBProtocol master = Masters[key];
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

	public async Task<IPSResult> ReadAsync(Channel channel, Device device, VBProtocol master, ReadPacket RP)
	{
		
		
		
		IPSResult iPSResult = await master.ReadAsync(RP);
		if (iPSResult.Status == CommStatus.Success)
		{
			int count = RP.Tags.Count;
			byte[] values = iPSResult.Values;
			Parallel.For(0, count, delegate(int y, ParallelLoopState state)
			{
				try
				{
					if (RP.Tags[y].DataType != 0 && RP.Tags[y].DataType != DataType.BYTE)
					{
						int indexOfByteAddress = VBUtility.GetIndexOfByteAddress(RP.Tags[0], RP.Tags[y]);
						switch (RP.Tags[y].DataType)
						{
						default:
							RP.Tags[y].Status = TagStatus.Bad;
							break;
						case DataType.INT:
							RP.Tags[y].Value = new INT(VBUtility.Sort(new byte[2]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.UINT:
							RP.Tags[y].Value = new UINT(VBUtility.Sort(new byte[2]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.WORD:
							RP.Tags[y].Value = new WORD(VBUtility.Sort(new byte[2]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.DINT:
							RP.Tags[y].Value = new DINT(VBUtility.Sort(new byte[4]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1],
								values[indexOfByteAddress + 2],
								values[indexOfByteAddress + 3]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.UDINT:
							RP.Tags[y].Value = new UDINT(VBUtility.Sort(new byte[4]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1],
								values[indexOfByteAddress + 2],
								values[indexOfByteAddress + 3]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.DWORD:
							RP.Tags[y].Value = new DWORD(VBUtility.Sort(new byte[4]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1],
								values[indexOfByteAddress + 2],
								values[indexOfByteAddress + 3]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.REAL:
							RP.Tags[y].Value = new REAL(VBUtility.Sort(new byte[4]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1],
								values[indexOfByteAddress + 2],
								values[indexOfByteAddress + 3]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.LINT:
							RP.Tags[y].Value = new LINT(VBUtility.Sort(new byte[8]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1],
								values[indexOfByteAddress + 2],
								values[indexOfByteAddress + 3],
								values[indexOfByteAddress + 4],
								values[indexOfByteAddress + 5],
								values[indexOfByteAddress + 6],
								values[indexOfByteAddress + 7]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.ULINT:
							RP.Tags[y].Value = new ULINT(VBUtility.Sort(new byte[8]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1],
								values[indexOfByteAddress + 2],
								values[indexOfByteAddress + 3],
								values[indexOfByteAddress + 4],
								values[indexOfByteAddress + 5],
								values[indexOfByteAddress + 6],
								values[indexOfByteAddress + 7]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.LWORD:
							RP.Tags[y].Value = new LWORD(VBUtility.Sort(new byte[8]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1],
								values[indexOfByteAddress + 2],
								values[indexOfByteAddress + 3],
								values[indexOfByteAddress + 4],
								values[indexOfByteAddress + 5],
								values[indexOfByteAddress + 6],
								values[indexOfByteAddress + 7]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.LREAL:
							RP.Tags[y].Value = new LREAL(VBUtility.Sort(new byte[8]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1],
								values[indexOfByteAddress + 2],
								values[indexOfByteAddress + 3],
								values[indexOfByteAddress + 4],
								values[indexOfByteAddress + 5],
								values[indexOfByteAddress + 6],
								values[indexOfByteAddress + 7]
							}));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.TIME16:
							RP.Tags[y].Value = new TIME16(VBUtility.Sort(new byte[2]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1]
							}), RP.Tags[y].Resolution);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.TIME32:
							RP.Tags[y].Value = new TIME32(VBUtility.Sort(new byte[4]
							{
								values[indexOfByteAddress],
								values[indexOfByteAddress + 1],
								values[indexOfByteAddress + 2],
								values[indexOfByteAddress + 3]
							}), RP.Tags[y].Resolution);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						}
					}
					else
					{
						int indexOfByteAddress2 = VBUtility.GetIndexOfByteAddress(RP.Tags[0], RP.Tags[y]);
						switch (RP.Tags[y].DataType)
						{
						case DataType.BYTE:
							RP.Tags[y].Value = values[indexOfByteAddress2];
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.BOOL:
						{
							BOOL[] @bool = BOOL.GetBool(values[indexOfByteAddress2]);
							int num = RP.Tags[y].BitAddress % 8;
							RP.Tags[y].Value = @bool[num];
							RP.Tags[y].Status = TagStatus.Good;
							break;
						}
						}
					}
				}
				catch (IndexOutOfRangeException ex)
				{
					RP.Tags[y].Status = TagStatus.Bad;
					DriverDataSource.SaveLog(channel.Name + "." + device.ToString() + ".", ex.Message);
				}
				catch (ArgumentOutOfRangeException)
				{
					RP.Tags[y].Status = TagStatus.Bad;
					string source = channel.Name + "." + device.ToString() + ".";
					string message = $"Tag(Name={RP.Tags[y].FullName}, Address={RP.Tags[y].Address}): {"Out-of-range address or the data read from the device is missing."}";
					DriverDataSource.SaveLog(source, message);
				}
				catch (Exception ex3)
				{
					Parallel.ForEach((IEnumerable<Tag>)RP.Tags, (Action<Tag>)delegate(Tag tg)
					{
						tg.SetDefaultValue();
					});
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
			string key = ((channel.ConnectionType == ConnectionType.Serial) ? array[0].Trim() : (array[0].Trim() + "." + array[1].Trim()));
			string key2 = array[0].Trim() + "." + array[1].Trim();
			if (!Masters.ContainsKey(key))
			{
				result.Message = "Write: Failure, invalid tag name: " + tg.FullName;
				result.Status = CommStatus.Error;
				return result;
			}
			_ = Devices[key2];
			VBProtocol vBProtocol = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[key]);
			WritePacket writePacket = new WritePacket
			{
				ByteAddress = tg.ByteAddress
			};
			switch (tg.DataType)
			{
			default:
				throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
			case DataType.BOOL:
				writePacket.BitAddress = tg.BitAddress;
				writePacket.IsBit = true;
				writePacket.ValueHex = (((bool)value) ? "70" : "71");
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
				writePacket.Quantity = 2;
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((DINT)value));
				break;
			case DataType.UDINT:
				writePacket.Quantity = 2;
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((UDINT)value));
				break;
			case DataType.DWORD:
				writePacket.Quantity = 2;
				writePacket.ValueDec = BitConverter.GetBytes(Convert.ToUInt32(((DWORD)value).Value, 16));
				break;
			case DataType.REAL:
				writePacket.Quantity = 2;
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((REAL)value));
				break;
			case DataType.LINT:
				writePacket.Quantity = 4;
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((LINT)value));
				break;
			case DataType.ULINT:
				writePacket.Quantity = 4;
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((ULINT)value));
				break;
			case DataType.LWORD:
				writePacket.Quantity = 4;
				writePacket.ValueDec = BitConverter.GetBytes(Convert.ToUInt64(((LWORD)value).Value, 16));
				break;
			case DataType.LREAL:
				writePacket.Quantity = 4;
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((LREAL)value));
				break;
			case DataType.TIME16:
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((ushort)(value.Value.TotalMilliseconds / tg.Resolution)));
				break;
			case DataType.TIME32:
				writePacket.Quantity = 2;
				writePacket.ValueDec = VBUtility.Sort(BitConverter.GetBytes((int)(value.Value.TotalMilliseconds / tg.Resolution)));
				break;
			}
			result = await vBProtocol.WriteAsync(writePacket);
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
