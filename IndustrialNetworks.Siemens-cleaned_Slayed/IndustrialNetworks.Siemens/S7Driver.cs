using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Siemens.Models;
using NetStudio.Siemens.NetTcp;

namespace NetStudio.Siemens;

public class S7Driver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, IS7Protocol> Masters = new Dictionary<string, IS7Protocol>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private BindingList<IpsLog> Logs;

	private Channel channel;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public S7Driver(Channel channel, BindingList<IpsLog> eventLogs)
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
		case IpsProtocolType.S7_TCP:
			foreach (Device device in channel.Devices)
			{
				try
				{
					TcpProtocol value = new TcpProtocol(device.Adapter, (CPUType)device.DeviceType);
					Masters.Add(channel.Name + "." + device.Name, value);
				}
				catch (Exception ex)
				{
					DriverDataSource.SaveLog(channel.Name + "." + device.Name, ex.Message);
				}
			}
			break;
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
				foreach (Tag tag in group.Tags)
				{
					empty = S7Utility.GetAddressPrefix(tag.Address);
					if (!dictionary2.ContainsKey(empty))
					{
						dictionary2.Add(empty, new List<Tag>());
					}
					try
					{
						tag.FullName = $"{channel.Name}.{device2.Name}.{group.Name}.{tag.Name}";
						if (tag.Mode != TagMode.WriteOnly)
						{
							dictionary2[empty].Add(tag);
						}
						Tags.Add(tag.FullName, tag);
					}
					catch (Exception)
					{
					}
				}
			}
		}
		if (dictionary.Any())
		{
			foreach (KeyValuePair<Device, Dictionary<string, List<Tag>>> item in dictionary)
			{
				Device key = item.Key;
				Dictionary<string, List<Tag>> value2 = item.Value;
				if (!_packages.ContainsKey(key))
				{
					_packages.Add(key, new List<ReadPacket>());
				}
				foreach (KeyValuePair<string, List<Tag>> item2 in value2)
				{
					ushort num = 0;
					int index = 0;
					int num2 = 0;
					int num3 = 0;
					int num4 = 1024;
					List<Tag> value3 = item2.Value;
					int count = value3.Count;
					value3.Sort((Tag tg, Tag tag_1) => S7Utility.GetSortAddress(tg).CompareTo(S7Utility.GetSortAddress(tag_1)));
					for (int i = 0; i < count; i++)
					{
						num = S7Utility.GetSizeOfDataType(value3[i]);
						if (value3[i].DataType == DataType.STRING)
						{
							num += 2;
						}
						int byteAddress = S7Utility.GetByteAddress(value3[i].Address);
						num3 = byteAddress - num2 + num;
						if (i == 0 || num3 > num4 || value3[i].DataType == DataType.STRING)
						{
							num2 = byteAddress;
							num3 = byteAddress - num2 + num;
							ReadPacket readPacket = new ReadPacket
							{
								Memory = S7Utility.GetMemoryByAddress(value3[i].Address),
								Address = num2,
								ConnectRetries = key.ConnectRetries,
								ReceivingDelay = key.ReceivingDelay
							};
							if (readPacket.Memory == Memory.Datablock)
							{
								if (value3[i].Address.StartsWith("DB"))
								{
									string text = value3[i].Address.Substring(0, value3[i].Address.IndexOf('.'));
									readPacket.DBNumber = int.Parse(text.Substring(2));
								}
								else
								{
									if (!value3[i].Address.StartsWith("V"))
									{
										throw new NotSupportedException(value3[i].Address + ": Unsupported memory type.");
									}
									readPacket.DBNumber = 1;
								}
							}
							_packages[key].Add(readPacket);
							index = _packages[key].Count - 1;
						}
						value3[i].WordAddress = byteAddress - num2;
						if (value3[i].DataType == DataType.BOOL)
						{
							value3[i].BitAddress = S7Utility.GetIndexOfBit(value3[i].Address);
						}
						_packages[key][index].Tags.Add(value3[i]);
						_packages[key][index].Quantity = num3;
					}
				}
			}
		}
		TcpBuilder builder = new TcpBuilder();
		foreach (List<ReadPacket> value4 in _packages.Values)
		{
			Parallel.ForEach((IEnumerable<ReadPacket>)value4, (Action<ReadPacket>)delegate(ReadPacket RP)
			{
				if (RP.Tags.Count > 0)
				{
					RP.SendBytes = builder.ReadDataMessage(RP);
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
		IS7Protocol master = Masters[channel.Name];
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
							if (channel.Devices.Count <= 1)
							{
								Thread.Sleep(1000);
							}
							continue;
						}
						if (device.ReconnectFlag && device.AutoReconnect)
						{
							DateTime now = DateTime.Now;
							if ((now - device.startDisconnected).Seconds < device.ConnectRetries)
							{
								continue;
							}
							device.startDisconnected = now;
							source = channel.Name + "." + device.ToString() + ".";
							DriverDataSource.SaveLog(source, "Reconnecting to the device.", EvenType.Information);
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
									DriverDataSource.SaveLog(source, iPSResult.Message);
									break;
								case CommStatus.Timeout:
									if (device.Status != DeviceStatus.Connecting && device.Status != DeviceStatus.Connected)
									{
										Thread.Sleep(Utility.ConnectRetries);
										if (device.AutoReconnect)
										{
											source = channel.Name + "." + device.ToString() + ".";
											DriverDataSource.SaveLog(source, "Reconnecting to the device.", EvenType.Information);
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
								Exception ex2 = ex;
								Exception exception_0 = ex2;
								source = channel.Name + "." + device.ToString() + ".";
								if (DriverDataSource.Logs.FirstOrDefault((IpsLog ipsLog_0) => ipsLog_0.Source == source && ipsLog_0.Message == exception_0.Message) == null)
								{
									DriverDataSource.AddLogAsync(new IpsLog
									{
										Source = source,
										Message = exception_0.Message
									});
								}
							}
						}
					}
				}
				catch (Exception ex3)
				{
					Exception ex2 = ex3;
					Exception exception_0 = ex2;
					if (DriverDataSource.Logs.FirstOrDefault((IpsLog ipsLog_0) => ipsLog_0.Message == exception_0.Message) == null)
					{
						DriverDataSource.AddLogAsync(new IpsLog
						{
							Source = ((channel == null) ? "Unknown" : ("Channel: " + channel.ToString())),
							Message = exception_0.Message
						});
					}
					Thread.Sleep(100);
				}
			}
		 
		}
		catch (Exception ex4)
		{
			Exception ex2 = ex4;
			Exception exception_0 = ex2;
			if (DriverDataSource.Logs.FirstOrDefault((IpsLog ipsLog_0) => ipsLog_0.Message == exception_0.Message) == null)
			{
				DriverDataSource.AddLogAsync(new IpsLog
				{
					Source = ((channel == null) ? "Unknown" : ("Channel: " + channel.ToString())),
					Message = exception_0.Message
				});
			}
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
		IS7Protocol master = Masters[key];
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
							device.ReconnectFlag = !(await master.ReconnectAsync());
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

	public async Task<IPSResult> ReadAsync(Channel channel, Device device, IS7Protocol master, ReadPacket RP)
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
					int wordAddress = RP.Tags[y].WordAddress;
					switch (RP.Tags[y].DataType)
					{
					default:
						RP.Tags[y].Status = TagStatus.Bad;
						break;
					case DataType.BOOL:
					{
						BOOL[] @bool = BOOL.GetBool(values[wordAddress]);
						RP.Tags[y].Value = @bool[RP.Tags[y].BitAddress];
						RP.Tags[y].Status = TagStatus.Good;
						break;
					}
					case DataType.BYTE:
						RP.Tags[y].Value = values[wordAddress];
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.INT:
						RP.Tags[y].Value = new INT(S7Utility.Sort(new byte[2]
						{
							values[wordAddress],
							values[wordAddress + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UINT:
						RP.Tags[y].Value = new UINT(S7Utility.Sort(new byte[2]
						{
							values[wordAddress],
							values[wordAddress + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.WORD:
						RP.Tags[y].Value = new WORD(S7Utility.Sort(new byte[2]
						{
							values[wordAddress],
							values[wordAddress + 1]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DINT:
						RP.Tags[y].Value = new DINT(S7Utility.Sort(new byte[4]
						{
							values[wordAddress],
							values[wordAddress + 1],
							values[wordAddress + 2],
							values[wordAddress + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UDINT:
						RP.Tags[y].Value = new UDINT(S7Utility.Sort(new byte[4]
						{
							values[wordAddress],
							values[wordAddress + 1],
							values[wordAddress + 2],
							values[wordAddress + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DWORD:
						RP.Tags[y].Value = new DWORD(S7Utility.Sort(new byte[4]
						{
							values[wordAddress],
							values[wordAddress + 1],
							values[wordAddress + 2],
							values[wordAddress + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.REAL:
						RP.Tags[y].Value = new REAL(S7Utility.Sort(new byte[4]
						{
							values[wordAddress],
							values[wordAddress + 1],
							values[wordAddress + 2],
							values[wordAddress + 3]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LINT:
						RP.Tags[y].Value = new LINT(S7Utility.Sort(new byte[8]
						{
							values[wordAddress],
							values[wordAddress + 1],
							values[wordAddress + 2],
							values[wordAddress + 3],
							values[wordAddress + 4],
							values[wordAddress + 5],
							values[wordAddress + 6],
							values[wordAddress + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.ULINT:
						RP.Tags[y].Value = new ULINT(S7Utility.Sort(new byte[8]
						{
							values[wordAddress],
							values[wordAddress + 1],
							values[wordAddress + 2],
							values[wordAddress + 3],
							values[wordAddress + 4],
							values[wordAddress + 5],
							values[wordAddress + 6],
							values[wordAddress + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LWORD:
						RP.Tags[y].Value = new LWORD(S7Utility.Sort(new byte[8]
						{
							values[wordAddress],
							values[wordAddress + 1],
							values[wordAddress + 2],
							values[wordAddress + 3],
							values[wordAddress + 4],
							values[wordAddress + 5],
							values[wordAddress + 6],
							values[wordAddress + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LREAL:
						RP.Tags[y].Value = new LREAL(S7Utility.Sort(new byte[8]
						{
							values[wordAddress],
							values[wordAddress + 1],
							values[wordAddress + 2],
							values[wordAddress + 3],
							values[wordAddress + 4],
							values[wordAddress + 5],
							values[wordAddress + 6],
							values[wordAddress + 7]
						}));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME16:
						RP.Tags[y].Value = new TIME16(S7Utility.Sort(new byte[2]
						{
							values[wordAddress],
							values[wordAddress + 1]
						}), RP.Tags[y].Resolution);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME32:
						RP.Tags[y].Value = new TIME32(S7Utility.Sort(new byte[4]
						{
							values[wordAddress],
							values[wordAddress + 1],
							values[wordAddress + 2],
							values[wordAddress + 3]
						}), RP.Tags[y].Resolution);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.STRING:
					{
						int num = y * (RP.Tags[y].Resolution + 2);
						byte[] array = new byte[values[num + 1]];
						Array.Copy(values, num + 2, array, 0, array.Length);
						RP.Tags[y].Value = Encoding.ASCII.GetString(array);
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
			string key = array[0] + "." + array[1];
			if (!Masters.ContainsKey(key))
			{
				result.Message = "Write: Failure, invalid tag name: " + tg.FullName;
				result.Status = CommStatus.Error;
				return result;
			}
			IS7Protocol iS7Protocol = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[key]);
			Device device = Devices[key];
			WritePacket writePacket = new WritePacket
			{
				Memory = S7Utility.GetMemoryByAddress(tg.Address),
				Address = S7Utility.GetOffsetAddress(tg.Address),
				Quantity = 1,
				ConnectRetries = device.ConnectRetries,
				ReceivingDelay = device.ReceivingDelay
			};
			if (writePacket.Memory == Memory.Datablock)
			{
				string addressPrefix = S7Utility.GetAddressPrefix(tg.Address);
				writePacket.DBNumber = int.Parse(addressPrefix.Substring(2));
			}
			switch (tg.DataType)
			{
			default:
				throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
			case DataType.BOOL:
				writePacket.IsBit = true;
				writePacket.Data = BOOL.ToBytes((BOOL)value);
				break;
			case DataType.BYTE:
				writePacket.Data = new byte[1] { (byte)value };
				break;
			case DataType.INT:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes((INT)value));
				break;
			case DataType.UINT:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes((UINT)value));
				break;
			case DataType.WORD:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes(Convert.ToUInt16(((WORD)value).Value, 16)));
				break;
			case DataType.DINT:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes((DINT)value));
				break;
			case DataType.UDINT:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes((UDINT)value));
				break;
			case DataType.DWORD:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes(Convert.ToUInt32(((DWORD)value).Value, 16)));
				break;
			case DataType.REAL:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes((REAL)value));
				break;
			case DataType.LINT:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes((LINT)value));
				break;
			case DataType.ULINT:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes((ULINT)value));
				break;
			case DataType.LWORD:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes(Convert.ToUInt64(((LWORD)value).Value, 16)));
				break;
			case DataType.LREAL:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes((LREAL)value));
				break;
			case DataType.TIME16:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes((ushort)(value.Value.TotalMilliseconds / tg.Resolution)));
				break;
			case DataType.TIME32:
				writePacket.Data = S7Utility.Sort(BitConverter.GetBytes((int)(value.Value.TotalMilliseconds / tg.Resolution)));
				break;
			case DataType.STRING:
			{
				byte[] bytes = Encoding.ASCII.GetBytes($"{(string)value}");
				if (bytes.Length > 254)
				{
					throw new InvalidOperationException($"String of length  {bytes.Length} characters is not valid. String with maximum length 254 characters");
				}
				writePacket.Data = new byte[bytes.Length + 2];
				writePacket.Data[0] = (byte)tg.Resolution;
				writePacket.Data[1] = (byte)bytes.Length;
				Array.Copy(bytes, 0, writePacket.Data, 2, bytes.Length);
				break;
			}
			}
			result = await iS7Protocol.WriteAsync(writePacket);
			if (result.Status == CommStatus.Success)
			{
				result.Message = "Write: Success.";
			}
			else
			{
				result.Message = "Write: Failure.";
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
