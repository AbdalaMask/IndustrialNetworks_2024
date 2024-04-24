using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;

namespace NetStudio.Mitsubishi.MC.Ethernet;

public class MCDriver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, MCProtocol> Masters = new Dictionary<string, MCProtocol>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private BindingList<IpsLog> Logs;

	private Channel channel;

	private Frame frameMC;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public MCDriver(Channel channel, BindingList<IpsLog> eventLogs)
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
				Masters.Add(channel.Name, new MCProtocol(channel.Adapter, frameMC));
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
					Masters.Add(channel.Name + "." + device.Name, new MCProtocol(device.Adapter, frameMC));
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
					empty = MCUtility.GetMemory(tag3);
					if (!dictionary2.ContainsKey(empty))
					{
						dictionary2.Add(empty, new List<Tag>());
					}
					tag3.FullName = $"{channel.Name}.{device2.Name}.{group.Name}.{tag3.Name}";
					tag3.WordAddress = MCUtility.GetWordAddress(tag3);
					tag3.BitAddress = MCUtility.GetHeaderDeviceNumber(tag3) % 16;
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
					int num = 0;
					int index = 0;
					int num2 = 0;
					int num3 = 0;
					List<Tag> list = (from tg in item2.Value
						orderby tg.WordAddress, tg.BitAddress
						select tg).ToList();
					int count = list.Count;
					for (int i = 0; i < count; i++)
					{
						num = MCUtility.GetSizeOfDataType(list[i]);
						int wordAddress = list[i].WordAddress;
						num3 = wordAddress - num2 + num;
						if (i == 0 || num3 > key.BlockSize)
						{
							num2 = wordAddress;
							num3 = wordAddress - num2 + num;
							_packages[key].Add(new ReadPacket
							{
								StationNo = (byte)key.StationNo,
								ConnectRetries = key.ConnectRetries,
								ReceivingDelay = key.ReceivingDelay
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
		MCBuilder builder = new MCBuilder();
		foreach (List<ReadPacket> value2 in _packages.Values)
		{
			Parallel.ForEach((IEnumerable<ReadPacket>)value2, (Action<ReadPacket>)delegate(ReadPacket RP)
			{
				if (RP.Tags.Count > 0)
				{
					Tag tag = RP.Tags[0];
					Tag tag2 = RP.Tags[RP.Tags.Count - 1];
					int sizeOfDataType = MCUtility.GetSizeOfDataType(tag2);
					RP.WordAddress = tag.WordAddress;
					RP.Address = tag.Address;
					RP.Quantity = MCUtility.GetIndexOfWordAddress(tag, tag2) + sizeOfDataType;
					RP.DeviceCode = MCUtility.GetDeviceCode(tag);
					if (frameMC == Frame.Frame3E)
					{
						RP.SendBytes = builder.ReadMC3EMsg(RP);
					}
					else
					{
						RP.SendBytes = builder.ReadMC4EMsg(RP);
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
		MCProtocol master = Masters[channel.Name];
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
		MCProtocol master = Masters[key];
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

	public async Task<IPSResult> ReadAsync(Channel channel, Device device, MCProtocol master, ReadPacket RP)
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
					int num = 2 * MCUtility.GetIndexOfWordAddress(RP.Tags[0], RP.Tags[y]);
					switch (RP.Tags[y].DataType)
					{
					default:
						RP.Tags[y].Status = TagStatus.Bad;
						break;
					case DataType.BOOL:
					{
						BOOL[] bools = BOOL.GetBools(MCUtility.Sort(new byte[2]
						{
							values[num],
							values[num + 1]
						}));
						RP.Tags[y].Value = bools[RP.Tags[y].BitAddress];
						RP.Tags[y].Status = TagStatus.Good;
						break;
					}
					case DataType.BYTE:
						RP.Tags[y].Value = values[num];
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.INT:
						RP.Tags[y].Value = new INT(MCUtility.Sort(new byte[2]
						{
							values[num],
							values[num + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UINT:
						RP.Tags[y].Value = new UINT(MCUtility.Sort(new byte[2]
						{
							values[num],
							values[num + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.WORD:
						RP.Tags[y].Value = new WORD(MCUtility.Sort(new byte[2]
						{
							values[num],
							values[num + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DINT:
						RP.Tags[y].Value = new DINT(MCUtility.Sort(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UDINT:
						RP.Tags[y].Value = new UDINT(MCUtility.Sort(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DWORD:
						RP.Tags[y].Value = new DWORD(MCUtility.Sort(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.REAL:
						RP.Tags[y].Value = new REAL(MCUtility.Sort(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LINT:
						RP.Tags[y].Value = new LINT(MCUtility.Sort(new byte[8]
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
						RP.Tags[y].Value = new ULINT(MCUtility.Sort(new byte[8]
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
						RP.Tags[y].Value = new LWORD(MCUtility.Sort(new byte[8]
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
						RP.Tags[y].Value = new LREAL(MCUtility.Sort(new byte[8]
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
						RP.Tags[y].Value = new TIME16(MCUtility.Sort(new byte[2]
						{
							values[num],
							values[num + 1]
						}), RP.Tags[y].Resolution);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME32:
						RP.Tags[y].Value = new TIME32(MCUtility.Sort(new byte[4]
						{
							values[num],
							values[num + 1],
							values[num + 2],
							values[num + 3]
						}), RP.Tags[y].Resolution);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.STRING:
					{
						byte[] array = new byte[RP.Tags[y].Resolution];
						Array.Copy(values, num, array, 0, array.Length);
						string text = Encoding.ASCII.GetString(array);
						if (text.Length > RP.Tags[y].Resolution)
						{
							text = text.Substring(0, RP.Tags[y].Resolution);
						}
						RP.Tags[y].Value = text;
						RP.Tags[y].Status = TagStatus.Good;
						break;
					}
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
			Device device = Devices[key2];
			MCProtocol mCProtocol = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[key]);
			WritePacket writePacket = new WritePacket
			{
				DeviceCode = MCUtility.GetDeviceCode(tg),
				WordAddress = tg.WordAddress,
				Quantity = 1,
				ConnectRetries = device.ConnectRetries,
				ReceivingDelay = device.ReceivingDelay
			};
			if (MCUtility.IsBitMemory(MCUtility.GetMemory(tg)))
			{
				writePacket.WordAddress = MCUtility.GetHeaderDeviceNumber(tg);
			}
			MCBuilder mCBuilder;
			switch (tg.DataType)
			{
			case DataType.BOOL:
				writePacket.IsBit = true;
				writePacket.WordAddress = MCUtility.GetHeaderDeviceNumber(tg);
				writePacket.Data = MCUtility.BoolsToBytes(new BOOL[1] { (bool)value });
				goto IL_0a6e;
			default:
				throw new NotSupportedException();
			case DataType.INT:
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes((INT)value));
				goto IL_0a6e;
			case DataType.UINT:
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes((UINT)value));
				goto IL_0a6e;
			case DataType.WORD:
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes(Convert.ToUInt16(((WORD)value).Value, 16)));
				goto IL_0a6e;
			case DataType.DINT:
				writePacket.Quantity = 2;
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes((DINT)value));
				goto IL_0a6e;
			case DataType.UDINT:
				writePacket.Quantity = 2;
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes((UDINT)value));
				goto IL_0a6e;
			case DataType.DWORD:
				writePacket.Quantity = 2;
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes(Convert.ToUInt32(((DWORD)value).Value, 16)));
				goto IL_0a6e;
			case DataType.REAL:
				writePacket.Quantity = 2;
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes((REAL)value));
				goto IL_0a6e;
			case DataType.LINT:
				writePacket.Quantity = 4;
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes((LINT)value));
				goto IL_0a6e;
			case DataType.ULINT:
				writePacket.Quantity = 4;
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes((ULINT)value));
				goto IL_0a6e;
			case DataType.LWORD:
				writePacket.Quantity = 4;
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes(Convert.ToUInt64(((LWORD)value).Value, 16)));
				goto IL_0a6e;
			case DataType.LREAL:
				writePacket.Quantity = 4;
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes((LREAL)value));
				goto IL_0a6e;
			case DataType.TIME16:
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes((ushort)(value.Value.TotalMilliseconds / tg.Resolution)));
				goto IL_0a6e;
			case DataType.TIME32:
				writePacket.Quantity = 2;
				writePacket.Data = MCUtility.Sort(BitConverter.GetBytes((int)(value.Value.TotalMilliseconds / tg.Resolution)));
				goto IL_0a6e;
			case DataType.STRING:
				{
					string text = (string)value;
					if (text.Length <= tg.Resolution)
					{
						text = text.PadRight(tg.Resolution, '\0');
						writePacket.Data = Encoding.ASCII.GetBytes(text);
						writePacket.Quantity = tg.Resolution / 2;
						goto IL_0a6e;
					}
					throw new InvalidDataException($"The string length exceeds the allowed string length(Max={tg.Resolution} characters)");
				}
				IL_0a6e:
				mCBuilder = new MCBuilder();
				if (frameMC == Frame.Frame3E)
				{
					writePacket.Data = mCBuilder.WriteMC3EMsg(writePacket);
				}
				else
				{
					writePacket.Data = mCBuilder.WriteMC4EMsg(writePacket);
				}
				result = await mCProtocol.WriteAsync(writePacket);
				break;
			}
		}
		catch (Exception ex)
		{
			result.Message = ex.Message;
			result.Status = CommStatus.Error;
			DriverDataSource.SaveLog(tg.FullName, ex.Message);
		}
		return result;
	}
}
