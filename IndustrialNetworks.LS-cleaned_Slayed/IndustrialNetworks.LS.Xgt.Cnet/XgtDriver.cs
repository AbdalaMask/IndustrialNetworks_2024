using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;

namespace NetStudio.LS.Xgt.Cnet;

public class XgtDriver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, XgtProtocol> Masters = new Dictionary<string, XgtProtocol>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private BindingList<IpsLog> Logs;

	private Channel channel;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public XgtDriver(Channel channel, BindingList<IpsLog> eventLogs)
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
				Masters.Add(channel.Name, new XgtProtocol(channel.Adapter));
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
					Masters.Add(channel.Name + "." + device.Name, new XgtProtocol(device.Adapter));
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
				foreach (Tag tag in group.Tags)
				{
					empty = tag.Address.Substring(0, 1);
					if (!dictionary2.ContainsKey(empty))
					{
						dictionary2.Add(empty, new List<Tag>());
					}
					tag.FullName = $"{channel.Name}.{device2.Name}.{group.Name}.{tag.Name}";
					dictionary2[empty].Add(tag);
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
					int blockSize = key.BlockSize;
					List<Tag> value2 = item2.Value;
					int count = value2.Count;
					value2.Sort((Tag tg, Tag tag_1) => XgtUtility.GetWordAddress(tg).CompareTo(XgtUtility.GetWordAddress(tag_1)));
					for (int i = 0; i < count; i++)
					{
						num = XgtUtility.GetSizeOfDataType(value2[i]);
						value2[i].WordAddress = XgtUtility.GetWordAddress(value2[i]);
						num3 = value2[i].WordAddress - num2 + num;
						if (i == 0 || num3 > blockSize)
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
					}
				}
			}
		}
		XgtBuilder builder = new XgtBuilder();
		foreach (List<ReadPacket> value5 in _packages.Values)
		{
			Parallel.ForEach((IEnumerable<ReadPacket>)value5, (Action<ReadPacket>)delegate(ReadPacket RP)
			{
				if (RP.Tags.Count > 0)
				{
					RP.Address = RP.Tags[0].Address;
					if (RP.Tags[0].DataType == DataType.BOOL)
					{
						if (RP.Address[0] != 'I' && RP.Address[0] != 'Q')
						{
							if (RP.Address[0] == 'P' || RP.Address[0] == 'M' || RP.Address[0] == 'L' || RP.Address[0] == 'K' || RP.Address[0] == 'F')
							{
								if (RP.Address.Length == 2)
								{
									RP.Address.Insert(1, "0");
								}
								RP.Address = RP.Address.Substring(0, RP.Address.Length - 1);
							}
						}
						else
						{
							string value3 = RP.Address.Substring(0, 1);
							int num4 = int.Parse(RP.Address.Substring(1, RP.Address.Length - 1));
							int num5 = num4 / 16;
							int value4 = num4 % 16;
							RP.Address = $"{value3}{num5 / 8}.{num5 % 8}.{value4}";
						}
					}
					RP.Address = "%" + RP.Address.Insert(1, "W");
					RP.SendMsg = builder.ReadingDirecVariableContinuously(RP);
				}
			});
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
		if (channel == null)
		{
			return;
		}
		XgtProtocol master = Masters[channel.Name];
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
		XgtProtocol master = Masters[key];
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

	public async Task<IPSResult> ReadAsync(Channel channel, Device device, XgtProtocol Master, ReadPacket RP)
	{
		
		
		
		IPSResult result = await Master.ReadAsync(RP);
		if (result.Status == CommStatus.Success)
		{
			int count = RP.Tags.Count;
			string values_hex = result.Values_Hex;
			Parallel.For(0, count, delegate(int y, ParallelLoopState state)
			{
				try
				{
					int startIndex = 4 * (RP.Tags[y].WordAddress - RP.Tags[0].WordAddress);
					switch (RP.Tags[y].DataType)
					{
					case DataType.BOOL:
					{
						BOOL[] array = BOOL.ToArray(XgtUtility.Sort(values_hex.Substring(startIndex, 4)));
						if (RP.Tags[y].Address[0] != 'I' && RP.Tags[y].Address[0] != 'Q')
						{
							byte byteFromHex = BYTE.GetByteFromHex($"{RP.Tags[y].Address.Last()}");
							RP.Tags[y].Value = array[byteFromHex];
						}
						else
						{
							int num = int.Parse(RP.Tags[y].Address.Substring(1, RP.Tags[y].Address.Length - 1)) % 16;
							RP.Tags[y].Value = array[num];
						}
						RP.Tags[y].Status = TagStatus.Good;
						break;
					}
					default:
						RP.Tags[y].Status = TagStatus.Bad;
						break;
					case DataType.INT:
						RP.Tags[y].Value = new INT(XgtUtility.Sort(values_hex.Substring(startIndex, 4)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UINT:
						RP.Tags[y].Value = new UINT(XgtUtility.Sort(values_hex.Substring(startIndex, 4)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.WORD:
						RP.Tags[y].Value = new WORD(XgtUtility.Sort(values_hex.Substring(startIndex, 4)));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DINT:
						RP.Tags[y].Value = new DINT(XgtUtility.Sort(values_hex.Substring(startIndex, 8)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UDINT:
						RP.Tags[y].Value = new UDINT(XgtUtility.Sort(values_hex.Substring(startIndex, 8)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DWORD:
						RP.Tags[y].Value = new DWORD(XgtUtility.Sort(values_hex.Substring(startIndex, 8)));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.REAL:
						RP.Tags[y].Value = new REAL(XgtUtility.Sort(values_hex.Substring(startIndex, 8)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LINT:
						RP.Tags[y].Value = new LINT(XgtUtility.Sort(values_hex.Substring(startIndex, 16)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.ULINT:
						RP.Tags[y].Value = new ULINT(XgtUtility.Sort(values_hex.Substring(startIndex, 16)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LWORD:
						RP.Tags[y].Value = new LWORD(XgtUtility.Sort(values_hex.Substring(startIndex, 16)));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LREAL:
						RP.Tags[y].Value = new LREAL(XgtUtility.Sort(values_hex.Substring(startIndex, 16)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME16:
						RP.Tags[y].Value = new TIME16(XgtUtility.Sort(values_hex.Substring(startIndex, 4)), RP.Tags[y].Resolution, isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME32:
						RP.Tags[y].Value = new TIME32(XgtUtility.Sort(values_hex.Substring(startIndex, 8)), RP.Tags[y].Resolution, isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.STRING:
					{
						string value_hex = result.Values_Hex.Substring(y * 64, 64);
						RP.Tags[y].Value = STRING.HexToString(value_hex);
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
		IPSResult result = new IPSResult();
		try
		{
			string[] array = tg.FullName.Split('.');
			if (array.Length < 3)
			{
				result.Message = "Write: Invalid tag name: " + tg.FullName;
				result.Status = CommStatus.Error;
				return result;
			}
			string key = ((channel.ConnectionType == ConnectionType.Serial) ? array[0].Trim() : (array[0].Trim() + "." + array[1].Trim()));
			string key2 = array[0].Trim() + "." + array[1].Trim();
			if (!Masters.ContainsKey(key))
			{
				result.Message = "Write: Invalid tag name: " + tg.FullName;
				result.Status = CommStatus.Error;
				return result;
			}
			Device device = Devices[key2];
			XgtProtocol xgtProtocol = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[key]);
			WritePacket writePacket = new WritePacket
			{
				StationNo = (ushort)device.StationNo,
				Address = "%" + tg.Address,
				ConnectRetries = device.ConnectRetries,
				ReceivingDelay = device.ReceivingDelay
			};
			if (tg.Address[0] == 'I' || tg.Address[0] == 'Q')
			{
				writePacket.Address = "%" + XgtUtility.GetAddressOfQorI(tg);
			}
			switch (tg.DataType)
			{
			default:
				throw new NotSupportedException();
			case DataType.BOOL:
				writePacket.IsBit = true;
				writePacket.Address = writePacket.Address.Insert(2, "X");
				writePacket.ValueHex = Conversion.BoolToHex((bool)value);
				break;
			case DataType.BYTE:
				writePacket.Address = writePacket.Address.Insert(2, "B");
				writePacket.ValueHex = BYTE.ToHex((byte)value);
				break;
			case DataType.INT:
				writePacket.Quantity = 1;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(((short)value).ToString("X4"));
				break;
			case DataType.UINT:
				writePacket.Quantity = 1;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(((ushort)value).ToString("X4"));
				break;
			case DataType.WORD:
				writePacket.Quantity = 1;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(((WORD)value).ToString());
				break;
			case DataType.DINT:
				writePacket.Quantity = 2;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(((int)value).ToString("X8"));
				break;
			case DataType.UDINT:
				writePacket.Quantity = 2;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(((uint)value).ToString("X8"));
				break;
			case DataType.DWORD:
				writePacket.Quantity = 2;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(((WORD)value).ToString());
				break;
			case DataType.REAL:
				writePacket.Quantity = 2;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(BitConverter.ToInt32(BitConverter.GetBytes((float)value), 0).ToString("X8"));
				break;
			case DataType.LINT:
				writePacket.Quantity = 4;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(((long)value).ToString("X16"));
				break;
			case DataType.ULINT:
				writePacket.Quantity = 4;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(((ulong)value).ToString("X16"));
				break;
			case DataType.LWORD:
				writePacket.Quantity = 4;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(((LWORD)value).ToString());
				break;
			case DataType.LREAL:
				writePacket.Quantity = 4;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort(BitConverter.ToInt64(BitConverter.GetBytes((double)value), 0).ToString("X16"));
				break;
			case DataType.TIME16:
				writePacket.Quantity = 1;
				writePacket.Address = writePacket.Address.Insert(2, "W");
				writePacket.ValueHex = XgtUtility.Sort((((TIME16)value).Value.TotalMilliseconds / (double)(int)tg.Resolution).ToString("X4"));
				break;
			case DataType.TIME32:
				writePacket.Quantity = 2;
				writePacket.ValueHex = XgtUtility.Sort((((TIME32)value).Value.TotalMilliseconds / (double)(int)tg.Resolution).ToString("X8"));
				break;
			case DataType.STRING:
			{
				string text = writePacket.Address.Substring(0, 2);
				int value2 = 2 * int.Parse(writePacket.Address.Substring(text.Length, writePacket.Address.Length - text.Length));
				writePacket.Address = $"{text}B{value2}";
				STRING sTRING = (STRING)value;
				if (string.IsNullOrEmpty(sTRING.Value))
				{
					sTRING = string.Empty;
				}
				sTRING = sTRING.Value.PadRight(31, '\0');
				writePacket.Quantity = sTRING.Value.Length;
				if (writePacket.Quantity > 31)
				{
					throw new InvalidDataException("Type: STRING\n\r, range: maximum character of the acceptable range is 31.");
				}
				writePacket.ValueHex = STRING.ToHex(sTRING, ByteOrder.LittleEndian);
				break;
			}
			}
			result = await xgtProtocol.WriteAsync(writePacket);
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
