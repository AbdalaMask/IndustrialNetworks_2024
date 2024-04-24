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

public class HostLinkFinsDriver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, HostLinkFinsProtocol> Masters = new Dictionary<string, HostLinkFinsProtocol>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private BindingList<IpsLog> Logs;

	private Channel channel;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public HostLinkFinsDriver(Channel channel, BindingList<IpsLog> eventLogs)
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
				Masters.Add(channel.Name, new HostLinkFinsProtocol(channel.Adapter));
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
					Masters.Add(channel.Name + "." + device.Name, new HostLinkFinsProtocol(device.Adapter));
				}
				catch (Exception ex2)
				{
					DriverDataSource.SaveLog(channel.Name + "." + device.Name, ex2.Message);
				}
			}
		}
		foreach (Device device2 in channel.Devices)
		{
			device2.ByteOrder = ByteOrder.LittleEndian;
			device2.BlockSize = 57;
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
					empty = OmronUtility.GetMemory(tag3.Address);
					tag3.WordAddress = OmronUtility.GetWordAddress(tag3.Address);
					tag3.BitAddress = OmronUtility.GetBitAddress(tag3.Address);
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
					int blockSize = key.BlockSize;
					List<Tag> value2 = item2.Value;
					int count = value2.Count;
					value2.Sort((Tag tg, Tag tag_1) => OmronUtility.GetTotalBits(tg.Address).CompareTo(OmronUtility.GetTotalBits(tag_1.Address)));
					for (int i = 0; i < count; i++)
					{
						num = OmronUtility.GetSizeOfDataType(value2[i]);
						int wordAddress = OmronUtility.GetWordAddress(value2[i].Address);
						num3 = wordAddress - num2 + num;
						if (i == 0 || num3 > blockSize)
						{
							num2 = wordAddress;
							num3 = wordAddress - num2 + num;
							_packages[key].Add(new ReadPacket
							{
								StationNo = (byte)key.StationNo,
								Address = num2.ToString("D4"),
								ConnectRetries = key.ConnectRetries,
								ReceivingDelay = key.ReceivingDelay
							});
							index = _packages[key].Count - 1;
							_packages[key][index].Memory = item2.Key;
						}
						value2[i].WordAddress = OmronUtility.GetWordAddress(value2[i].Address);
						value2[i].BitAddress = OmronUtility.GetBitAddress(value2[i].Address);
						_packages[key][index].Tags.Add(value2[i]);
					}
				}
			}
		}
		HostLinkFinsBuilder finsBuilder = new HostLinkFinsBuilder();
		foreach (List<ReadPacket> value3 in _packages.Values)
		{
			Parallel.ForEach((IEnumerable<ReadPacket>)value3, (Action<ReadPacket>)delegate(ReadPacket RP)
			{
				if (RP.Tags.Count > 0)
				{
					Tag tag = RP.Tags[0];
					Tag tag2 = RP.Tags[RP.Tags.Count - 1];
					int wordAddress2 = OmronUtility.GetWordAddress(tag.Address);
					int wordAddress3 = OmronUtility.GetWordAddress(tag2.Address);
					int sizeOfDataType = OmronUtility.GetSizeOfDataType(tag2);
					RP.NumOfWords = wordAddress3 - wordAddress2 + sizeOfDataType;
					string memoryAreaCode = HostLinkFinsBuilder.WordMemoryAreaCode[RP.Memory];
					OmronUtility.GetWordAddress(tag.Address);
					OmronUtility.GetBitAddress(tag.Address);
					RP.NumOfRecvBytes = 4 * RP.NumOfWords + 27;
					RP.SendMsg = finsBuilder.ReadMsg(RP.StationNo, memoryAreaCode, tag.WordAddress, tag.BitAddress, RP.NumOfWords);
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
			HostLinkFinsProtocol master = Masters[channel.Name];
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
		HostLinkFinsProtocol master = Masters[key];
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

	public async Task<IPSResult> ReadAsync(Channel channel, Device device, HostLinkFinsProtocol master, ReadPacket RP)
	{
		
		
		
		IPSResult result = await master.ReadAsync(RP);
		if (result.Status == CommStatus.Success)
		{
			string values_hexa = result.Values_Hex;
			int count = RP.Tags.Count;
			Parallel.For(0, count, delegate(int y, ParallelLoopState state)
			{
				try
				{
					int startIndex = 4 * (RP.Tags[y].WordAddress - RP.Tags[0].WordAddress);
					switch (RP.Tags[y].DataType)
					{
					default:
						RP.Tags[y].Status = TagStatus.Bad;
						break;
					case DataType.BOOL:
					{
						BOOL[] array = BOOL.ParseArray(values_hexa.Substring(startIndex, 4));
						RP.Tags[y].Value = array[RP.Tags[y].BitAddress];
						RP.Tags[y].Status = TagStatus.Good;
						break;
					}
					case DataType.BYTE:
					{
						int startIndex2 = 2 * OmronUtility.GetWordAddress(RP.Tags[y].Address);
						RP.Tags[y].Value = BYTE.GetByteFromHex(values_hexa.Substring(startIndex2, 2));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					}
					case DataType.INT:
						RP.Tags[y].Value = new INT(OmronUtility.Sort(values_hexa.Substring(startIndex, 4)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UINT:
						RP.Tags[y].Value = new UINT(OmronUtility.Sort(values_hexa.Substring(startIndex, 4)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.WORD:
						RP.Tags[y].Value = new WORD(OmronUtility.Sort(values_hexa.Substring(startIndex, 4)));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DINT:
						RP.Tags[y].Value = new DINT(OmronUtility.Sort(values_hexa.Substring(startIndex, 8)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.UDINT:
						RP.Tags[y].Value = new UDINT(OmronUtility.Sort(values_hexa.Substring(startIndex, 8)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.DWORD:
						RP.Tags[y].Value = new DWORD(OmronUtility.Sort(values_hexa.Substring(startIndex, 8)));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.REAL:
						RP.Tags[y].Value = new REAL(OmronUtility.Sort(values_hexa.Substring(startIndex, 8)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LINT:
						RP.Tags[y].Value = new LINT(OmronUtility.Sort(values_hexa.Substring(startIndex, 16)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.ULINT:
						RP.Tags[y].Value = new ULINT(OmronUtility.Sort(values_hexa.Substring(startIndex, 16)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LWORD:
						RP.Tags[y].Value = new LWORD(OmronUtility.Sort(values_hexa.Substring(startIndex, 16)));
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.LREAL:
						RP.Tags[y].Value = new LREAL(OmronUtility.Sort(values_hexa.Substring(startIndex, 16)), isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME16:
						RP.Tags[y].Value = new TIME16(OmronUtility.Sort(values_hexa.Substring(startIndex, 4)), RP.Tags[y].Resolution, isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.TIME32:
						RP.Tags[y].Value = new TIME32(OmronUtility.Sort(values_hexa.Substring(startIndex, 8)), RP.Tags[y].Resolution, isHexa: true);
						RP.Tags[y].Status = TagStatus.Good;
						break;
					case DataType.STRING:
					{
						string value_hex = result.Values_Hex.Substring(y * 64, 64);
						RP.Tags[y].Value = STRING.HexToString(value_hex, device.ByteOrder);
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
					RP.Tags[y].Status = TagStatus.Bad;
					string source = channel.Name + "." + device.ToString() + ".";
					string message = $"Tag(Name={RP.Tags[y].FullName}, Address={RP.Tags[y].Address}): {"Out-of-range address or the data read from the device is missing."}";
					DriverDataSource.SaveLog(source, message);
				}
				catch (Exception ex3)
				{
					RP.Tags[y].Status = TagStatus.Bad;
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
			HostLinkFinsProtocol hostLinkFinsProtocol = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[key]);
			int stationNo = device.StationNo;
			int wordAddress = OmronUtility.GetWordAddress(tg.Address, isFins: true);
			string memory = OmronUtility.GetMemory(tg.Address);
			string memoryAreaCode = HostLinkFinsBuilder.WordMemoryAreaCode[memory];
			_ = string.Empty;
			int numOfElements = 1;
			string dataHex;
			switch (tg.DataType)
			{
			case DataType.BOOL:
				memoryAreaCode = HostLinkFinsBuilder.BitMemoryAreaCode[memory];
				dataHex = (((bool)value) ? "01" : "00");
				break;
			default:
				throw new NotSupportedException();
			case DataType.INT:
				numOfElements = 1;
				dataHex = OmronUtility.Sort(((short)value).ToString("X4"));
				break;
			case DataType.UINT:
				numOfElements = 1;
				dataHex = OmronUtility.Sort(((ushort)value).ToString("X4"));
				break;
			case DataType.WORD:
				numOfElements = 1;
				dataHex = OmronUtility.Sort(((WORD)value).ToString());
				break;
			case DataType.DINT:
				numOfElements = 2;
				dataHex = OmronUtility.Sort(((int)value).ToString("X4"));
				break;
			case DataType.UDINT:
				numOfElements = 2;
				dataHex = OmronUtility.Sort(((uint)value).ToString("X4"));
				break;
			case DataType.DWORD:
				numOfElements = 2;
				dataHex = OmronUtility.Sort(((DWORD)value).ToString());
				break;
			case DataType.REAL:
				numOfElements = 2;
				dataHex = OmronUtility.Sort(BitConverter.ToInt32(BitConverter.GetBytes((float)value), 0).ToString("X8"));
				break;
			case DataType.LINT:
				dataHex = OmronUtility.Sort(((long)value).ToString("X16"));
				break;
			case DataType.ULINT:
				dataHex = OmronUtility.Sort(((ulong)value).ToString("X16"));
				break;
			case DataType.LWORD:
				dataHex = OmronUtility.Sort(((LWORD)value).ToString());
				break;
			case DataType.LREAL:
				dataHex = OmronUtility.Sort(BitConverter.ToInt64(BitConverter.GetBytes((double)value), 0).ToString("X16"));
				break;
			case DataType.TIME16:
				numOfElements = 1;
				dataHex = $"0000{((TIME16)value).Value.TotalMilliseconds / (double)(int)tg.Resolution}".Right(4);
				break;
			case DataType.TIME32:
				numOfElements = 2;
				dataHex = $"00000000{((TIME32)value).Value.TotalMilliseconds / (double)(int)tg.Resolution}".Right(8);
				break;
			}
			result = await hostLinkFinsProtocol.Write(stationNo, memoryAreaCode, wordAddress, tg.BitAddress, numOfElements, dataHex);
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
