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
using NetStudio.Panasonic.Mewtocol.Codes;

namespace NetStudio.Panasonic.Mewtocol;

public class MewtocolDriver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, MewtocolProtocol> Masters = new Dictionary<string, MewtocolProtocol>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private BindingList<IpsLog> Logs;

	private Channel channel;

	private MessageBuilder builder;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public MewtocolDriver(Channel channel, BindingList<IpsLog> eventLogs)
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
				Masters.Add(channel.Name, new MewtocolProtocol(channel.Adapter));
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
					Masters.Add(channel.Name + "." + device.Name, new MewtocolProtocol(device.Adapter));
				}
				catch (Exception ex2)
				{
					DriverDataSource.SaveLog(channel.Name + "." + device.Name, ex2.Message);
				}
			}
		}
		foreach (Device device2 in channel.Devices)
		{
			device2.BlockSize = 450;
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
					empty = MewtocolUtility.GetMemory(tag);
					if (!dictionary2.ContainsKey(empty))
					{
						dictionary2.Add(empty, new List<Tag>());
					}
					tag.WordAddress = MewtocolUtility.GetWordAddress(tag);
					tag.FullName = $"{channel.Name}.{device2.Name}.{group.Name}.{tag.Name}";
					if (tag.Mode != TagMode.WriteOnly)
					{
						dictionary2[empty].Add(tag);
					}
					Tags.Add(tag.FullName, tag);
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
					List<Tag> value2 = item2.Value;
					int count = value2.Count;
					value2.Sort((Tag tg, Tag tag_1) => MewtocolUtility.GetFullAddress(tg).CompareTo(MewtocolUtility.GetFullAddress(tag_1)));
					for (int i = 0; i < count; i++)
					{
						num = MewtocolUtility.GetSizeOfDataType(value2[i]);
						num3 = value2[i].WordAddress - num2 + num;
						if (i == 0 || num3 > key.BlockSize)
						{
							num2 = value2[i].WordAddress;
							num3 = value2[i].WordAddress - num2 + num;
							_packages[key].Add(new ReadPacket
							{
								StationNo = (byte)key.StationNo,
								ConnectRetries = key.ConnectRetries,
								ReceivingDelay = key.ReceivingDelay
							});
							index = _packages[key].Count - 1;
						}
						_packages[key][index].Tags.Add(value2[i]);
						if (_packages[key][index].Quantity < num3)
						{
							_packages[key][index].Quantity = num3;
						}
						else
						{
							_packages[key][index].Quantity += num;
						}
					}
				}
			}
		}
		builder = new MessageBuilder();
		foreach (List<ReadPacket> value3 in _packages.Values)
		{
			Parallel.ForEach((IEnumerable<ReadPacket>)value3, (Action<ReadPacket>)delegate(ReadPacket RP)
			{
				if (RP.Tags.Count > 0)
				{
					KeyValuePair<string, string> keyValuePair = MewtocolUtility.ConvertToWordAddress(RP.Tags[0]);
					AreaCode areaCode = MewtocolUtility.Areas[keyValuePair.Key];
					string memory = MewtocolUtility.DataCodes[keyValuePair.Key];
					switch (areaCode)
					{
					case AreaCode.Contact:
						RP.SendMsg = builder.ReadContactMessage(RP.StationNo, memory, keyValuePair.Value, RP.Quantity);
						break;
					case AreaCode.Data:
						RP.SendMsg = builder.ReadDataAreaMessage(RP.StationNo, memory, keyValuePair.Value, RP.Quantity);
						break;
					case AreaCode.SetValue:
						RP.SendMsg = builder.ReadSetValueAreaMessage(RP.StationNo, keyValuePair.Value, RP.Quantity);
						break;
					case AreaCode.ElapsedValue:
						RP.SendMsg = builder.ReadElapsedValueMessage(RP.StationNo, keyValuePair.Value, RP.Quantity);
						break;
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
		MewtocolProtocol master = Masters[channel.Name];
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
		MewtocolProtocol master = Masters[key];
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

	public async Task<IPSResult> ReadAsync(Channel channel, Device device, MewtocolProtocol master, ReadPacket RP)
	{
		
		
		
		IPSResult result = await master.ReadAsync(RP);
		if (result.Status == CommStatus.Success)
		{
			int count = RP.Tags.Count;
			string values_hexa = result.Values_Hex;
			Parallel.For(0, count, delegate(int y, ParallelLoopState state)
			{
				try
				{
					int num = 4 * (RP.Tags[y].WordAddress - RP.Tags[0].WordAddress);
					switch (RP.Tags[y].DataType)
					{
					case DataType.BOOL:
					{
						BOOL[] array = BYTE.GetBytesFromHex(values_hexa.Substring(num, 2) + values_hexa.Substring(num + 2, 2)).SelectMany((byte byte_0) => BOOL.GetBool(byte_0)).ToArray();
						byte byteFromHex = BYTE.GetByteFromHex(RP.Tags[y].Address.Last().ToString());
						RP.Tags[y].Value = array[byteFromHex];
						RP.Tags[y].Status = TagStatus.Good;
						break;
					}
					default:
						RP.Tags[y].Status = TagStatus.Bad;
						break;
					case DataType.INT:
						RP.Tags[y].Value = new INT(MewtocolUtility.Sort(values_hexa.Substring(num, 4)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UINT:
						RP.Tags[y].Value = new UINT(MewtocolUtility.Sort(values_hexa.Substring(num, 4)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.WORD:
						RP.Tags[y].Value = new WORD(MewtocolUtility.Sort(values_hexa.Substring(num, 4)));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DINT:
						RP.Tags[y].Value = new DINT(MewtocolUtility.Sort(values_hexa.Substring(num, 8)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UDINT:
						RP.Tags[y].Value = new UDINT(MewtocolUtility.Sort(values_hexa.Substring(num, 8)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DWORD:
						RP.Tags[y].Value = new DWORD(MewtocolUtility.Sort(values_hexa.Substring(num, 8)));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.REAL:
						RP.Tags[y].Value = new REAL(MewtocolUtility.Sort(values_hexa.Substring(num, 8)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LINT:
						RP.Tags[y].Value = new LINT(MewtocolUtility.Sort(values_hexa.Substring(num, 16)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.ULINT:
						RP.Tags[y].Value = new ULINT(MewtocolUtility.Sort(values_hexa.Substring(num, 16)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LWORD:
						RP.Tags[y].Value = new LWORD(MewtocolUtility.Sort(values_hexa.Substring(num, 16)));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LREAL:
						RP.Tags[y].Value = new LREAL(MewtocolUtility.Sort(values_hexa.Substring(num, 16)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME16:
						RP.Tags[y].Value = new TIME16(MewtocolUtility.Sort(values_hexa.Substring(num, 4)), 10, isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME32:
						RP.Tags[y].Value = new TIME32(MewtocolUtility.Sort(values_hexa.Substring(num, 8)), 10, isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.STRING:
					{
						string value_hex = result.Values_Hex.Substring(8).Substring(num, 2 * RP.Tags[y].Resolution);
						RP.Tags[y].Value = STRING.HexToString(value_hex, ByteOrder.LittleEndian);
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
				MewtocolProtocol mewtocolProtocol = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[key]);
				string memory = MewtocolUtility.GetMemory(tg);
				WritePacket writePacket = new WritePacket
				{
					StationNo = (ushort)device.StationNo,
					Address = tg.Address,
					Memory = memory,
					AreaCode = MewtocolUtility.Areas[memory],
					IsBit = (tg.DataType == DataType.BOOL),
					ConnectRetries = device.ConnectRetries,
					ReceivingDelay = device.ReceivingDelay
				};
				if (writePacket.IsBit)
				{
					if (writePacket.AreaCode == AreaCode.OperationMode)
					{
						string mode = ((value) ? "R" : "P");
						writePacket.SendMsg = builder.RemoteControlMessage(writePacket.StationNo, mode);
					}
					else
					{
						string address = tg.Address.Substring(memory.Length);
						writePacket.SendMsg = builder.WriteContactMessage(writePacket.StationNo, writePacket.Memory, address, (bool)value);
					}
					iPSResult = mewtocolProtocol.WriteBit(writePacket);
					if (tg.Mode == TagMode.WriteOnly && iPSResult.Status == CommStatus.Success)
					{
						tg.Value = (object)value;
					}
				}
				else
				{
					string values_hex;
					switch (tg.DataType)
					{
					default:
						throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
					case DataType.INT:
						writePacket.Quantity = 1;
						values_hex = MewtocolUtility.Sort(((short)value).ToString("X4"));
						break;
					case DataType.UINT:
						writePacket.Quantity = 1;
						values_hex = MewtocolUtility.Sort(((ushort)value).ToString("X4"));
						break;
					case DataType.WORD:
						writePacket.Quantity = 1;
						values_hex = MewtocolUtility.Sort(((WORD)value).ToString());
						break;
					case DataType.DINT:
						writePacket.Quantity = 2;
						values_hex = MewtocolUtility.Sort(((int)value).ToString("X8"));
						break;
					case DataType.UDINT:
						writePacket.Quantity = 2;
						values_hex = MewtocolUtility.Sort(((uint)value).ToString("X8"));
						break;
					case DataType.DWORD:
						writePacket.Quantity = 2;
						values_hex = MewtocolUtility.Sort(((DWORD)value).ToString());
						break;
					case DataType.REAL:
						writePacket.Quantity = 2;
						values_hex = MewtocolUtility.Sort(BitConverter.ToInt32(BitConverter.GetBytes((float)value), 0).ToString("X8"));
						break;
					case DataType.LINT:
						writePacket.Quantity = 4;
						values_hex = MewtocolUtility.Sort(((long)value).ToString("X16"));
						break;
					case DataType.ULINT:
						writePacket.Quantity = 4;
						values_hex = MewtocolUtility.Sort(((ulong)value).ToString("X16"));
						break;
					case DataType.LWORD:
						writePacket.Quantity = 4;
						values_hex = MewtocolUtility.Sort(((DWORD)value).ToString());
						break;
					case DataType.LREAL:
						writePacket.Quantity = 4;
						values_hex = MewtocolUtility.Sort(BitConverter.ToInt64(BitConverter.GetBytes((double)value), 0).ToString("X16"));
						break;
					case DataType.TIME16:
						writePacket.Quantity = 1;
						values_hex = MewtocolUtility.Sort(((ushort)(((TIME16)value).Value.TotalMilliseconds / 10.0)).ToString("X4"));
						break;
					case DataType.TIME32:
						writePacket.Quantity = 2;
						values_hex = MewtocolUtility.Sort(((int)(((TIME32)value).Value.TotalMilliseconds / 10.0)).ToString("X8"));
						break;
					case DataType.STRING:
					{
						string text = (STRING)value;
						int length = text.Length;
						if (length > tg.Resolution)
						{
							throw new InvalidDataException($"Type: STRING\n\r, range: maximum character of the acceptable range is {tg.Resolution}.");
						}
						if (tg.Resolution % 2 != 0)
						{
							tg.Resolution++;
						}
						text = text.PadRight(tg.Resolution, '\0');
						byte[] bytes = Encoding.ASCII.GetBytes(text);
						string text2 = string.Empty;
						byte[] array2 = bytes;
						foreach (byte b in array2)
						{
							text2 += b.ToString("X2");
						}
						writePacket.Quantity = 2 + tg.Resolution / 2;
						string text3 = MewtocolUtility.Sort(tg.Resolution.ToString("X4"));
						string text4 = MewtocolUtility.Sort(length.ToString("X4"));
						values_hex = text3 + text4 + text2;
						break;
					}
					}
					string memory2 = MewtocolUtility.DataCodes[memory];
					switch (writePacket.AreaCode)
					{
					default:
						throw new NotSupportedException("This data type is not supported.");
					case AreaCode.Contact:
					{
						string startAddress = $"{tg.WordAddress}".PadLeft(4, '0');
						string endAddress = (tg.WordAddress + writePacket.Quantity - 1).ToString("D4");
						writePacket.SendMsg = builder.WriteContactMessage(writePacket.StationNo, memory2, startAddress, endAddress, values_hex);
						break;
					}
					case AreaCode.Data:
					{
						string startAddress = $"{tg.WordAddress}".PadLeft(5, '0');
						string endAddress = (tg.WordAddress + writePacket.Quantity - 1).ToString("D5");
						writePacket.SendMsg = builder.WriteDataAreaMessage(writePacket.StationNo, memory2, startAddress, endAddress, values_hex);
						break;
					}
					case AreaCode.SetValue:
					{
						string startAddress = $"{tg.WordAddress}".PadLeft(4, '0');
						string endAddress = (tg.WordAddress + writePacket.Quantity - 1).ToString("D4");
						writePacket.SendMsg = builder.WriteSetValueAreaMessage(writePacket.StationNo, startAddress, endAddress, values_hex);
						break;
					}
					case AreaCode.ElapsedValue:
					{
						string startAddress = $"{tg.WordAddress}".PadLeft(4, '0');
						string endAddress = (tg.WordAddress + writePacket.Quantity - 1).ToString("D4");
						writePacket.SendMsg = builder.WriteElapsedValueMessage(writePacket.StationNo, startAddress, endAddress, values_hex);
						break;
					}
					}
					iPSResult = mewtocolProtocol.Write(writePacket);
				}
			}
			catch (Exception ex)
			{
				iPSResult.Message = "Write: Failure. " + ex.Message;
				iPSResult.Status = CommStatus.Error;
				DriverDataSource.SaveLog(tg.FullName, ex.Message);
			}
			return iPSResult;
		});
	}
}
