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

namespace NetStudio.Mitsubishi.FXSerial;

public class FXSerialDriver
{
	private CancellationTokenSource? cancellationTokenSource;

	public Dictionary<string, FXSerialProtocol> Masters = new Dictionary<string, FXSerialProtocol>();

	public readonly Dictionary<string, Device> Devices = new Dictionary<string, Device>();

	public readonly Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

	private Dictionary<Device, List<ReadPacket>> _packages = new Dictionary<Device, List<ReadPacket>>();

	private BindingList<IpsLog> Logs;

	private Channel channel;

	private MachineInfo _machine;

	private NetStudio.Common.Security.License _license;

	public FXSerialDriver(Channel channel, BindingList<IpsLog> eventLogs)
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
				FXSerialProtocol value = new FXSerialProtocol(channel.Adapter);
				Masters.Add(channel.Name, value);
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
					Masters.Add(channel.Name + "." + device.Name, new FXSerialProtocol(device.Adapter));
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
					empty = FXSerialUtility.GetSpecialMemory(tag3);
					tag3.ByteAddress = FXSerialUtility.GetByteAddress(tag3);
					tag3.BitAddress = FXSerialUtility.GetBitAddress(tag3);
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
					int blockSize = key.BlockSize;
					List<Tag> value3 = item2.Value;
					int count = value3.Count;
					value3.Sort((Tag tg, Tag tag_1) => FXSerialUtility.GetTotalBits(tg.Address).CompareTo(FXSerialUtility.GetTotalBits(tag_1.Address)));
					for (int i = 0; i < count; i++)
					{
						num = FXSerialUtility.GetSizeOfDataType(value3[i]);
						int byteAddress = FXSerialUtility.GetByteAddress(value3[i]);
						num3 = byteAddress - num2 + num;
						if (i == 0 || num3 > blockSize)
						{
							num2 = byteAddress;
							num3 = byteAddress - num2 + num;
							_packages[key].Add(new ReadPacket
							{
								Address = num2.ToString("D4"),
								ConnectRetries = key.ConnectRetries,
								ReceivingDelay = key.ReceivingDelay
							});
							index = _packages[key].Count - 1;
							_packages[key][index].Memory = item2.Key;
						}
						value3[i].ByteAddress = FXSerialUtility.GetByteAddress(value3[i]);
						value3[i].BitAddress = FXSerialUtility.GetBitAddress(value3[i]);
						_packages[key][index].Tags.Add(value3[i]);
					}
				}
			}
		}
		FXSerialBuilder builder = new FXSerialBuilder();
		foreach (List<ReadPacket> value4 in _packages.Values)
		{
			Parallel.ForEach((IEnumerable<ReadPacket>)value4, (Action<ReadPacket>)delegate(ReadPacket RP)
			{
				if (RP.Tags.Count > 0)
				{
					Tag tag = RP.Tags[0];
					Tag tag2 = RP.Tags[RP.Tags.Count - 1];
					int sizeOfDataType = FXSerialUtility.GetSizeOfDataType(tag2);
					RP.NumOfBytes = tag2.ByteAddress - tag.ByteAddress + sizeOfDataType;
					RP.SendMsg = builder.ReadBytesMsg((ushort)tag.ByteAddress, (byte)RP.NumOfBytes);
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
		string source = channel.Name + ".";
		FXSerialProtocol master = Masters[channel.Name];
		try
		{
			if (!channel.Devices.Where((Device device_0) => device_0.Active).Any() || !Masters.ContainsKey(channel.Name))
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

	public async Task<IPSResult> ReadAsync(Channel channel, Device device, FXSerialProtocol master, ReadPacket RP)
	{
		
		
		
		IPSResult result = new IPSResult
		{
			Message = "Read request failed.",
			Status = CommStatus.Error
		};
		if (device.Status == DeviceStatus.Disconnected)
		{
			result = master.OnInitializeCommunication();
			if (result.Status != CommStatus.Success)
			{
				return result;
			}
		}
		result = await master.ReadAsync(RP);
		await Task.Run(delegate
		{
			if (result.Status == CommStatus.Success)
			{
				int count = RP.Tags.Count;
				string values_hex = result.Values_Hex;
				Parallel.For(0, count, delegate(int y, ParallelLoopState state)
				{
					try
					{
						int num = RP.Tags[y].ByteAddress - RP.Tags[0].ByteAddress;
						switch (RP.Tags[y].DataType)
						{
						default:
							RP.Tags[y].Status = TagStatus.Bad;
							break;
						case DataType.BOOL:
						{
							BOOL[] @bool = BOOL.GetBool(Convert.ToByte(values_hex.Substring(2 * num, 2), 16));
							RP.Tags[y].Value = @bool[RP.Tags[y].BitAddress % 8];
							RP.Tags[y].Status = TagStatus.Good;
							break;
						}
						case DataType.BYTE:
						{
							int startIndex = 2 * FXSerialUtility.GetByteAddress(RP.Tags[y]);
							RP.Tags[y].Value = BYTE.GetByteFromHex(values_hex.Substring(startIndex, 2));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						}
						case DataType.INT:
							num = 2 * num;
							RP.Tags[y].Value = new INT(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 4)), isHexa: true);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.UINT:
							num = 2 * num;
							RP.Tags[y].Value = new UINT(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 4)), isHexa: true);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.WORD:
							num = 2 * num;
							RP.Tags[y].Value = new WORD(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 4)));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.DINT:
							num = 2 * num;
							RP.Tags[y].Value = new DINT(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 8)), isHexa: true);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.UDINT:
							num = 2 * num;
							RP.Tags[y].Value = new UDINT(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 8)), isHexa: true);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.DWORD:
							num = 2 * num;
							RP.Tags[y].Value = new DWORD(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 8)));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.REAL:
							num = 2 * num;
							RP.Tags[y].Value = new REAL(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 8)), isHexa: true);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.LINT:
							num = 4 * num;
							RP.Tags[y].Value = new LINT(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 16)), isHexa: true);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.ULINT:
							num = 4 * num;
							RP.Tags[y].Value = new ULINT(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 16)), isHexa: true);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.LWORD:
							num = 4 * num;
							RP.Tags[y].Value = new LWORD(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 16)));
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.LREAL:
							num = 4 * num;
							RP.Tags[y].Value = new LREAL(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 16)), isHexa: true);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.TIME16:
							num = 2 * num;
							RP.Tags[y].Value = new TIME16(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 4)), RP.Tags[y].Resolution, isHexa: true);
							RP.Tags[y].Status = TagStatus.Good;
							break;
						case DataType.TIME32:
							num = 2 * num;
							RP.Tags[y].Value = new TIME32(FXSerialUtility.ToByteOrder(values_hex.Substring(num, 8)), RP.Tags[y].Resolution, isHexa: true);
							RP.Tags[y].Status = TagStatus.Good;
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
		});
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
			FXSerialProtocol fXSerialProtocol = ((channel.ConnectionType == ConnectionType.Serial) ? Masters[channel.Name] : Masters[key]);
			WritePacket writePacket = new WritePacket
			{
				ConnectRetries = device.ConnectRetries,
				ReceivingDelay = device.ReceivingDelay
			};
			writePacket.StartAddress = ((tg.DataType == DataType.BOOL) ? ((ushort)tg.BitAddress) : ((ushort)tg.ByteAddress));
			switch (tg.DataType)
			{
			default:
				throw new NotSupportedException($"{tg.DataType}: {"This data type is not supported."}");
			case DataType.BOOL:
				writePacket.IsBit = true;
				writePacket.ValueHex = (((bool)value) ? "7" : "8");
				break;
			case DataType.BYTE:
				writePacket.ValueHex = BYTE.ToHex(new BYTE[1] { (BYTE)value });
				break;
			case DataType.INT:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(((short)value).ToString("X4"));
				break;
			case DataType.UINT:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(((ushort)value).ToString("X4"));
				break;
			case DataType.WORD:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(((WORD)value).ToString());
				break;
			case DataType.DINT:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(((int)value).ToString("X8"));
				break;
			case DataType.UDINT:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(((uint)value).ToString("X8"));
				break;
			case DataType.DWORD:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(((DWORD)value).ToString());
				break;
			case DataType.REAL:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(BitConverter.ToInt32(BitConverter.GetBytes((float)value), 0).ToString("X8"));
				break;
			case DataType.LINT:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(((long)value).ToString("X16"));
				break;
			case DataType.ULINT:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(((ulong)value).ToString("X16"));
				break;
			case DataType.LWORD:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(((LWORD)value).ToString());
				break;
			case DataType.LREAL:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder(BitConverter.ToInt64(BitConverter.GetBytes((double)value), 0).ToString("X16"));
				break;
			case DataType.TIME16:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder((((TIME16)value).Value.TotalMilliseconds / (double)(int)tg.Resolution).ToString("X4"));
				break;
			case DataType.TIME32:
				writePacket.ValueHex = FXSerialUtility.ToByteOrder((((TIME32)value).Value.TotalMilliseconds / (double)(int)tg.Resolution).ToString("X8"));
				break;
			}
			result = await fXSerialProtocol.WriteAsync(writePacket);
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
