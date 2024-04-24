using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetStudio.Common.AsrsLink;
using NetStudio.Common.Historiant;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Delta.Models;
using NetStudio.DriverComm;
using NetStudio.DriverComm.Models;
using NetStudio.Fatek;
using NetStudio.Keyence.MC;
using NetStudio.LS;
using NetStudio.Mitsubishi.Dedicated;
using NetStudio.Mitsubishi.FXSerial;
using NetStudio.Mitsubishi.MC;
using NetStudio.Mitsubishi.SLMP;
using NetStudio.Modbus;
using NetStudio.Omron;
using NetStudio.Panasonic.Mewtocol;
using NetStudio.Siemens;
using NetStudio.Vigor;

namespace NetStudio.IPS.Local;

public class EditHelper
{
	private static LocalEditManger _LocalEditManger = null;

	public static string Host { get; set; } = "net.tcp://127.0.0.1:5012/NetStudio";


	public static string UserName { get; set; } = string.Empty;


	public static string Password { get; set; } = string.Empty;


	public static ClientInfo ClientInfo { get; set; } = new ClientInfo();


	public static IndustrialProtocol? IndusProtocol { get; set; }

	public static LocalEditManger Editor
	{
		get
		{
			_LocalEditManger = _LocalEditManger ?? new LocalEditManger(AppHelper.Settings);
			return _LocalEditManger;
		}
	}

	public static async Task<ApiResponse> OnLoadProject()
	{
		ApiResponse apiResponse = (AppHelper.Settings.Mode ? (await ClientHelper.Editor.ReadAsync()) : (await Editor.ReadAsync(AppHelper.Settings.FileName)));
		if (apiResponse.Success)
		{
			IndusProtocol = (IndustrialProtocol)apiResponse.Data;
		}
		return apiResponse;
	}

	public static async void OnInitialize()
	{
		_LocalEditManger = _LocalEditManger ?? new LocalEditManger(AppHelper.Settings);
		if (IndusProtocol == null)
		{
			ApiResponse apiResponse = await _LocalEditManger.ReadAsync(AppHelper.Settings.FileName);
			if (apiResponse.Success && apiResponse.Data != null)
			{
				IndusProtocol = (IndustrialProtocol)apiResponse.Data;
			}
		}
	}

	public static void RemoveAll()
	{
		IndusProtocol?.Channels.Clear();
		ClientDataSource.Logs.Clear();
	}

	public static void AddChannel(Channel channel)
	{
		
		IndusProtocol = IndusProtocol ?? new IndustrialProtocol();
		if (IndusProtocol?.Channels.FirstOrDefault((Channel channel_0) => channel_0.Name.ToLower() == channel.Name.ToLower()) != null)
		{
			throw new InvalidOperationException("Channel name already exists.");
		}
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		if (industrialProtocol != null && industrialProtocol.Channels.Count == 0)
		{
			channel.Id = 1;
		}
		else
		{
			channel.Id = IndusProtocol.Channels.Max((Channel channel_0) => channel_0.Id) + 1;
		}
		IndusProtocol?.Channels.Add(channel);
	}

	public static void EditChannel(Channel channel)
	{
		
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == channel.Id);
			if (obj != null)
			{
				Channel channel2 = IndusProtocol?.Channels.FirstOrDefault((Channel channel_0) => channel_0.Name.ToLower() == channel.Name.ToLower());
				if (channel2 != null && channel2.Id != channel.Id)
				{
					throw new InvalidOperationException("Channel name already exists.");
				}
				((Channel)obj).Name = channel.Name;
				((Channel)obj).Manufacturer = channel.Manufacturer;
				((Channel)obj).ConnectionType = channel.ConnectionType;
				((Channel)obj).Protocol = channel.Protocol;
				((Channel)obj).Description = channel.Description;
				((Channel)obj).Adapter = channel.Adapter;
				((Channel)obj).Devices = channel.Devices;
				return;
			}
		}
		throw new InvalidOperationException("Channel does not exist.");
	}

	public static Channel CopyChannel(Channel channel)
	{
		
		Channel channel2 = IndusProtocol?.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == channel.Id);
		if (channel2 == null)
		{
			throw new InvalidOperationException("Channel does not exist.");
		}
		Channel channel3 = new Channel
		{
			Id = IndusProtocol.Channels.Max((Channel channel_0) => channel_0.Id) + 1,
			Name = channel2.Name,
			ConnectionType = channel2.ConnectionType,
			Manufacturer = channel2.Manufacturer,
			Adapter = channel2.Adapter,
			Protocol = channel2.Protocol,
			Description = channel2.Description
		};
		channel3.Name = $"{channel3.Name}-copy{channel3.Id}";
		IndusProtocol?.Channels.Add(channel3);
		foreach (Device device2 in channel2.Devices)
		{
			Device device = new Device
			{
				ChannelId = channel3.Id,
				Id = device2.Id,
				Name = device2.Name,
				CompanyId = device2.CompanyId,
				DeviceType = device2.DeviceType,
				BaseNo = device2.BaseNo,
				SlotNo = device2.SlotNo,
				Active = device2.Active,
				AutoReconnect = device2.AutoReconnect,
				ConnectRetries = device2.ConnectRetries,
				Adapter = device2.Adapter,
				BaseAddress = device2.BaseAddress,
				BlockSize = device2.BlockSize,
				ByteOrder = device2.ByteOrder,
				Description = device2.Description
			};
			channel3.Devices.Add(device);
			foreach (Group group2 in device2.Groups)
			{
				Group group = new Group
				{
					ChannelId = device.ChannelId,
					DeviceId = device2.Id,
					Id = group2.Id,
					Name = group2.Name,
					Description = group2.Description
				};
				device.Groups.Add(group);
				foreach (Tag tag in group2.Tags)
				{
					Tag item = new Tag
					{
						ChannelId = group.ChannelId,
						DeviceId = group.DeviceId,
						GroupId = group.Id,
						Id = tag.Id,
						Name = tag.Name,
						Address = tag.Address,
						DataType = tag.DataType,
						IsScaling = tag.IsScaling,
						AImax = tag.AImax,
						AImin = tag.AImin,
						RLmax = tag.RLmax,
						RLmin = tag.RLmin,
						Mode = tag.Mode,
						Description = tag.Description,
						Offset = tag.Offset
					};
					group.Tags.Add(item);
				}
			}
		}
		return channel3;
	}

	public static void RemoveChannel(Channel channel)
	{
		
		Channel channel2 = IndusProtocol?.Channels.FirstOrDefault((Channel channel_0) => channel_0.Name.ToLower() == channel.Name.ToLower());
		if (channel2 != null)
		{
			IndusProtocol?.Channels.Remove(channel2);
		}
	}

	public static List<Channel> GetChannels()
	{
		IndusProtocol = IndusProtocol ?? new IndustrialProtocol();
		return IndusProtocol?.Channels;
	}

	public static Channel? GetChannelByName(string channelName)
	{
		
		Channel result = null;
		if (IndusProtocol != null && IndusProtocol?.Channels != null)
		{
			IndustrialProtocol? industrialProtocol = IndusProtocol;
			if (industrialProtocol != null && industrialProtocol.Channels.Count > 0)
			{
				result = IndusProtocol?.Channels.FirstOrDefault((Channel channel_0) => channel_0.Name.ToLower() == channelName.ToLower());
			}
		}
		return result;
	}

	public static void AddDevice(Device device)
	{
		
		Channel channel = IndusProtocol?.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == device.ChannelId);
		if (channel == null)
		{
			throw new InvalidOperationException($"Can't find channel with Id={device.ChannelId}.");
		}
		if (channel.Devices.FirstOrDefault((Device device_0) => device_0.Name.ToLower() == device.Name.ToLower()) != null)
		{
			throw new InvalidOperationException("Device name already exists.");
		}
		if (channel.Devices.Count == 0)
		{
			device.Id = 1;
		}
		else
		{
			device.Id = channel.Devices.Max((Device device_0) => device_0.Id) + 1;
		}
		channel.Devices.Add(device);
		if (!device.Groups.Any())
		{
			return;
		}
		foreach (Group group in device.Groups)
		{
			group.ChannelId = device.Id;
			group.DeviceId = device.Id;
			if (!group.Tags.Any())
			{
				continue;
			}
			foreach (Tag tag in group.Tags)
			{
				tag.ChannelId = group.Id;
				tag.DeviceId = group.Id;
				tag.GroupId = group.Id;
			}
		}
	}

	public static void EditDevice(Device device)
	{
		
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == device.ChannelId);
			if (obj != null)
			{
				Device device2 = ((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Id == device.Id);
				if (device2 == null)
				{
					throw new InvalidOperationException("Device does not exist.");
				}
				Device device3 = ((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Name.ToLower() == device.Name.ToLower());
				if (device3 != null && device3.Id != device.Id)
				{
					throw new InvalidOperationException("Device name already exists.");
				}
				device2.ChannelId = device.ChannelId;
				device2.Name = device.Name;
				device2.CompanyId = device.CompanyId;
				device2.DeviceType = device.DeviceType;
				device2.BaseNo = device.BaseNo;
				device2.SlotNo = device.SlotNo;
				device2.Active = device.Active;
				device2.AutoReconnect = device.AutoReconnect;
				device2.ConnectRetries = device.ConnectRetries;
				device2.ReceivingDelay = device.ReceivingDelay;
				device2.StationNo = device.StationNo;
				device2.BaseAddress = device.BaseAddress;
				device2.BlockSize = device.BlockSize;
				device2.ByteOrder = device.ByteOrder;
				device2.Description = device.Description;
				if (device.Adapter != null)
				{
					device2.Adapter = device.Adapter;
				}
				return;
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={device.ChannelId}.");
	}

	public static Device CopyDevice(Device device)
	{
		
		Channel channel = IndusProtocol?.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == device.ChannelId);
		if (channel == null)
		{
			throw new InvalidOperationException($"Can't find channel with Id={device.ChannelId}.");
		}
		Device device2 = channel.Devices.FirstOrDefault((Device device_0) => device_0.Id == device.Id);
		if (device2 == null)
		{
			throw new InvalidOperationException("Device does not exist.");
		}
		Device device3 = new Device
		{
			ChannelId = channel.Id,
			Id = channel.Devices.Max((Device device_0) => device_0.Id) + 1,
			Name = device2.Name,
			CompanyId = device.CompanyId,
			DeviceType = device.DeviceType,
			BaseNo = device.BaseNo,
			SlotNo = device.SlotNo,
			StationNo = channel.Devices.Max((Device device_0) => device_0.StationNo) + 1,
			Active = device2.Active,
			AutoReconnect = device.AutoReconnect,
			ConnectRetries = device.ConnectRetries,
			ReceivingDelay = device.ReceivingDelay,
			Adapter = device2.Adapter,
			BaseAddress = device2.BaseAddress,
			BlockSize = device2.BlockSize,
			ByteOrder = device2.ByteOrder,
			Description = device2.Description
		};
		device3.Name = $"{device3.Name}-copy{device3.Id}";
		channel.Devices.Add(device3);
		foreach (Group group2 in device2.Groups)
		{
			Group group = new Group
			{
				ChannelId = device3.ChannelId,
				DeviceId = device3.Id,
				Id = group2.Id,
				Name = group2.Name,
				Description = group2.Description
			};
			device3.Groups.Add(group);
			foreach (Tag tag in group2.Tags)
			{
				Tag item = new Tag
				{
					ChannelId = group.ChannelId,
					DeviceId = group.DeviceId,
					GroupId = group.Id,
					Id = tag.Id,
					Name = tag.Name,
					Address = tag.Address,
					DataType = tag.DataType,
					IsScaling = tag.IsScaling,
					AImax = tag.AImax,
					AImin = tag.AImin,
					RLmax = tag.RLmax,
					RLmin = tag.RLmin,
					Mode = tag.Mode,
					Description = tag.Description,
					Offset = tag.Offset
				};
				group.Tags.Add(item);
			}
		}
		return device3;
	}

	public static void RemoveDevice(Device device)
	{
		
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == device.ChannelId);
			if (obj != null)
			{
				Device device2 = ((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Name.ToLower() == device.Name.ToLower());
				if (device2 == null)
				{
					throw new InvalidOperationException("Device does not exist.");
				}
				((Channel)obj).Devices.Remove(device2);
				return;
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={device.ChannelId}.");
	}

	public static Device? GetDeviceByName(string channelName, string deviceName)
	{
		
	
		Device result = null;
		if (IndusProtocol != null && IndusProtocol?.Channels != null)
		{
			IndustrialProtocol? industrialProtocol = IndusProtocol;
			if (industrialProtocol != null && industrialProtocol.Channels.Count > 0)
			{
				Channel channel = IndusProtocol?.Channels.FirstOrDefault((Channel channel_0) => channel_0.Name.ToLower() == channelName.ToLower());
				if (channel != null)
				{
					result = channel.Devices.FirstOrDefault((Device device_0) => device_0.Name.ToLower() == deviceName.ToLower());
				}
			}
		}
		return result;
	}

	public static void AddGroup(Group group)
	{
	
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == group.ChannelId);
			if (obj != null)
			{
				Device device = ((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Id == group.DeviceId);
				if (device == null)
				{
					throw new InvalidOperationException($"Can't find device with Id={group.DeviceId}.");
				}
				if (device.Groups.FirstOrDefault((Group group_0) => group_0.Name.ToLower() == group.Name.ToLower()) != null)
				{
					throw new InvalidOperationException("Group name already exists.");
				}
				if (device.Groups.Count == 0)
				{
					group.Id = 1;
				}
				else
				{
					group.Id = device.Groups.Max((Group group_0) => group_0.Id) + 1;
				}
				device.Groups.Add(group);
				return;
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={group.ChannelId}.");
	}

	public static void EditGroup(Group group)
	{
	
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == group.ChannelId);
			if (obj != null)
			{
				Device? obj2 = ((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Id == group.DeviceId) ?? throw new InvalidOperationException($"Can't find device with Id={group.ChannelId}.");
				Group group2 = obj2.Groups.FirstOrDefault((Group group_0) => group_0.Id == group.Id);
				if (group2 == null)
				{
					throw new InvalidOperationException("Group does not exist.");
				}
				Group group3 = obj2.Groups.FirstOrDefault((Group group_0) => group_0.Name.ToLower() == group.Name.ToLower());
				if (group3 != null && group3.Id != group.Id)
				{
					throw new InvalidOperationException("Group name already exists.");
				}
				group2.Name = group.Name;
				group2.Description = group.Description;
				return;
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={group.ChannelId}.");
	}

	public static Group CopyGroup(Group group)
	{
	
		Channel channel = IndusProtocol?.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == group.ChannelId);
		if (channel == null)
		{
			throw new InvalidOperationException($"Can't find channel with Id={group.ChannelId}.");
		}
		Device device = channel.Devices.FirstOrDefault((Device device_0) => device_0.Id == group.DeviceId);
		if (device == null)
		{
			throw new InvalidOperationException($"Can't find device with Id={group.ChannelId}.");
		}
		Group group2 = device.Groups.FirstOrDefault((Group group_0) => group_0.Id == group.Id);
		if (group2 == null)
		{
			throw new InvalidOperationException("Group does not exist.");
		}
		Group group3 = new Group
		{
			ChannelId = channel.Id,
			DeviceId = device.Id,
			Id = device.Groups.Max((Group group_0) => group_0.Id) + 1,
			Name = group2.Name,
			Description = group2.Description
		};
		group3.Name = $"{group3.Name}-copy{group3.Id}";
		device.Groups.Add(group3);
		foreach (Tag tag in group2.Tags)
		{
			Tag item = new Tag
			{
				ChannelId = group3.ChannelId,
				DeviceId = group3.DeviceId,
				GroupId = group3.Id,
				Id = tag.Id,
				Name = tag.Name,
				Address = tag.Address,
				DataType = tag.DataType,
				IsScaling = tag.IsScaling,
				AImax = tag.AImax,
				AImin = tag.AImin,
				RLmax = tag.RLmax,
				RLmin = tag.RLmin,
				Mode = tag.Mode,
				Description = tag.Description,
				Offset = tag.Offset
			};
			group3.Tags.Add(item);
		}
		return group3;
	}

	public static void RemoveGroup(Group group)
	{
	
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == group.ChannelId);
			if (obj != null)
			{
				Device? obj2 = ((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Id == group.DeviceId) ?? throw new InvalidOperationException($"Can't find device with Id={group.ChannelId}.");
				Group group2 = obj2.Groups.FirstOrDefault((Group group_0) => group_0.Id == group.Id);
				if (group2 == null)
				{
					throw new InvalidOperationException("Group does not exist.");
				}
				obj2.Groups.Remove(group2);
				return;
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={group.ChannelId}.");
	}

	public static void GetGroup(int channelId, int deviceId, int groupId)
	{
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == channelId);
			if (obj != null)
			{
				if ((((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Id == deviceId) ?? throw new InvalidOperationException($"Can't find device with Id={channelId}.")).Groups.FirstOrDefault((Group group_0) => group_0.Id == groupId) == null)
				{
					throw new InvalidOperationException("Group does not exist.");
				}
				return;
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={channelId}.");
	}

	public static List<Tag> AddTagRange(TRange range)
	{
		
		if (IndusProtocol == null)
		{
			throw new InvalidOperationException("IndusProtocol is null.");
		}
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == range.Template.ChannelId);
			if (obj != null)
			{
				Device device = ((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Id == range.Template.DeviceId);
				if (device == null)
				{
					throw new InvalidOperationException($"Can't find device with Id={range.Template.DeviceId}.");
				}
				Group group = device.Groups.FirstOrDefault((Group group_0) => group_0.Id == range.Template.GroupId);
				if (group == null)
				{
					throw new InvalidOperationException($"Can't find Group with Id={range.Template.GroupId}.");
				}
				Tag tag = null;
				List<Tag> list = new List<Tag>();
				for (int i = 0; i < range.Quantity; i++)
				{
					Tag tg = null;
					if (i == 0)
					{
						tg = (Tag)range.Template.Clone();
					}
					else
					{
						tg = tag;
						switch (range.Protocol)
						{
						case IpsProtocolType.S7_TCP:
							S7Utility.IncrementAddress(tg);
							break;
						case IpsProtocolType.CNET_XGT_PROTOCOL:
						case IpsProtocolType.FENET_XGT_PROTOCOL:
							XgtUtility.IncrementWordAddress(device, tg);
							break;
						case IpsProtocolType.MEWTOCOL_PROTOCOL:
							MewtocolUtility.IncrementWordAddress(tg);
							break;
						case IpsProtocolType.MISUBISHI_MC_PROTOCOL:
							NetStudio.Mitsubishi.MC.MCUtility.IncrementWordAddress(device, tg);
							break;
						case IpsProtocolType.MISUBISHI_SLMP_PROTOCOL:
							SLMPUtility.IncrementWordAddress(device, tg);
							break;
						case IpsProtocolType.DEDICATED1_PROTOCOL:
							DedicatedUtility.IncrementWordAddress(tg);
							break;
						case IpsProtocolType.DEDICATED4_PROTOCOL:
							DedicatedUtility.IncrementWordAddress(tg);
							break;
						case IpsProtocolType.FX_SERIAL_PROTOCOL:
							FXSerialUtility.IncrementByteAddress(tg);
							break;
						case IpsProtocolType.FINS_TCP_PROTOCOL:
						case IpsProtocolType.FINS_UDP_PROTOCOL:
						case IpsProtocolType.HOSTLINK_FINS_PROTOCOL:
						case IpsProtocolType.HOSTLINK_CMODE_PROTOCOL:
							OmronUtility.IncrementWordAddress(tg);
							break;
						case IpsProtocolType.MODBUS_TCP:
						case IpsProtocolType.MODBUS_RTU:
						case IpsProtocolType.MODBUS_ASCII:
							ModbusUtility.IncrementAddress(tg);
							break;
						case IpsProtocolType.VS_PROTOCOL:
							VSUtility.IncrementWordAddress(tg);
							break;
						case IpsProtocolType.VB_PROTOCOL:
							VBUtility.IncrementWordAddress(tg);
							break;
						case IpsProtocolType.FATEK_PROTOCOL:
							FatekUtility.IncrementWordAddress(tg);
							break;
						case IpsProtocolType.DELTA_ASCII:
						case IpsProtocolType.DELTA_RTU:
						case IpsProtocolType.DELTA_TCP:
							DeltaUtility.IncrementAddress(tg);
							break;
						case IpsProtocolType.KEYENCE_MC_PROTOCOL:
							NetStudio.Keyence.MC.MCUtility.IncrementWordAddress(device, tg);
							break;
						case IpsProtocolType.S7_MPI:
						case IpsProtocolType.S7_PPI:
						case IpsProtocolType.ASCII_PROTOCOL:
							break;
						default:
							throw new NotSupportedException();
						}
					}
					tg.Name = $"{range.Template.Name}{i}";
					tag = (Tag)tg.Clone();
					if (group.Tags.FirstOrDefault((Tag tag_1) => tag_1.Name.ToLower() == tg.Name.ToLower()) == null)
					{
						Tag tag2 = group.Tags.FirstOrDefault((Tag tag_1) => tag_1.Address == tg.Address);
						if (tag2 == null || tg.DataType != tag2.DataType)
						{
							if (group.Tags.Count == 0)
							{
								tg.Id = 1;
							}
							else
							{
								tg.Id = group.Tags.Max((Tag tg) => tg.Id) + 1;
							}
							group.Tags.Add(tg);
							list.Add(tg);
							continue;
						}
						throw new InvalidOperationException("The address[" + tg.Address + "] already exists in the group.");
					}
					throw new InvalidOperationException("Tag name already exists.");
				}
				return list;
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={range.Template.ChannelId}.");
	}

	public static void AddTag(Tag tg)
	{
		
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == tg.ChannelId);
			if (obj != null)
			{
				Group group = (((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Id == tg.DeviceId) ?? throw new InvalidOperationException($"Can't find device with Id={tg.DeviceId}.")).Groups.FirstOrDefault((Group group_0) => group_0.Id == tg.GroupId);
				if (group == null)
				{
					throw new InvalidOperationException($"Can't find Group with Id={tg.GroupId}.");
				}
				if (group.Tags.FirstOrDefault((Tag tag_1) => tag_1.Name.ToLower() == tg.Name.ToLower()) != null)
				{
					throw new InvalidOperationException("Tag name already exists.");
				}
				Tag tag = group.Tags.FirstOrDefault((Tag tag_1) => tag_1.Address == tg.Address);
				if (tag != null && tg.DataType == tag.DataType)
				{
					throw new InvalidOperationException("The address[" + tg.Address + "] already exists in the group.");
				}
				if (group.Tags.Count == 0)
				{
					tg.Id = 1;
				}
				else
				{
					tg.Id = group.Tags.Max((Tag tg) => tg.Id) + 1;
				}
				group.Tags.Add(tg);
				return;
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={tg.ChannelId}.");
	}

	public static void EditTag(Tag tg)
	{
		
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == tg.ChannelId);
			if (obj != null)
			{
				Group? obj2 = (((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Id == tg.DeviceId) ?? throw new InvalidOperationException($"Can't find device with Id={tg.ChannelId}.")).Groups.FirstOrDefault((Group group_0) => group_0.Id == tg.GroupId) ?? throw new InvalidOperationException("Group does not exist.");
				Tag tag = obj2.Tags.FirstOrDefault((Tag tag_1) => tag_1.Id == tg.Id);
				if (tag == null)
				{
					throw new InvalidOperationException("Tag does not exist.");
				}
				Tag tag2 = obj2.Tags.FirstOrDefault((Tag tag_1) => tag_1.Name.ToLower() == tg.Name.ToLower());
				if (tag2 != null && tag2.Id != tg.Id)
				{
					throw new InvalidOperationException("Tag name already exists.");
				}
				Tag tag3 = obj2.Tags.FirstOrDefault((Tag tag_1) => tag_1.Address == tg.Address);
				if (tag3 != null && tg.DataType == tag3.DataType && tg.Id != tag3.Id)
				{
					throw new InvalidOperationException("The address[" + tg.Address + "] already exists in the group.");
				}
				tag.ChannelId = tg.ChannelId;
				tag.DeviceId = tg.DeviceId;
				tag.GroupId = tg.GroupId;
				tag.Id = tg.Id;
				tag.Name = tg.Name;
				tag.Address = tg.Address;
				tag.DataType = tg.DataType;
				tag.IsScaling = tg.IsScaling;
				tag.Resolution = tg.Resolution;
				tag.AImax = tg.AImax;
				tag.AImin = tg.AImin;
				tag.RLmax = tg.RLmax;
				tag.RLmin = tg.RLmin;
				tag.Mode = tg.Mode;
				tag.Description = tg.Description;
				tag.Offset = tg.Offset;
				return;
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={tg.ChannelId}.");
	}

	public static Tag CopyTag(Tag tg)
	{
		
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == tg.ChannelId);
			if (obj != null)
			{
				Tag tag = ((((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Id == tg.DeviceId) ?? throw new InvalidOperationException($"Can't find device with Id={tg.ChannelId}.")).Groups.FirstOrDefault((Group group_0) => group_0.Id == tg.GroupId) ?? throw new InvalidOperationException("Group does not exist.")).Tags.FirstOrDefault((Tag tag_1) => tag_1.Id == tg.Id);
				if (tag == null)
				{
					throw new InvalidOperationException("Tag does not exist.");
				}
				return new Tag
				{
					ChannelId = tag.ChannelId,
					DeviceId = tag.DeviceId,
					GroupId = tag.GroupId,
					Id = tag.Id,
					Name = tag.Name,
					Address = tag.Address,
					DataType = tag.DataType,
					IsScaling = tag.IsScaling,
					Resolution = tag.Resolution,
					AImax = tag.AImax,
					AImin = tag.AImin,
					RLmax = tag.RLmax,
					RLmin = tag.RLmin,
					Mode = tag.Mode,
					Description = tag.Description,
					Offset = tag.Offset
				};
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={tg.ChannelId}.");
	}

	public static void RemoveTag(Tag tg)
	{
		
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.Channels.FirstOrDefault((Channel channel_0) => channel_0.Id == tg.ChannelId);
			if (obj != null)
			{
				Group? obj2 = (((Channel)obj).Devices.FirstOrDefault((Device device_0) => device_0.Id == tg.DeviceId) ?? throw new InvalidOperationException($"Can't find device with Id={tg.ChannelId}.")).Groups.FirstOrDefault((Group group_0) => group_0.Id == tg.GroupId) ?? throw new InvalidOperationException("Group does not exist.");
				Tag tag = obj2.Tags.FirstOrDefault((Tag tag_1) => tag_1.Id == tg.Id);
				if (tag == null)
				{
					throw new InvalidOperationException("Tag does not exist.");
				}
				obj2.Tags.Remove(tag);
				return;
			}
		}
		throw new InvalidOperationException($"Can't find channel with Id={tg.ChannelId}.");
	}

	public static void AddCycle(LoggingCycle cycle)
	{
		
		if (IndusProtocol?.LoggingCycles.FirstOrDefault((LoggingCycle loggingCycle_0) => loggingCycle_0.CycleTime == cycle.CycleTime && loggingCycle_0.CycleUnit == cycle.CycleUnit) != null)
		{
			throw new InvalidOperationException("This cycle time already exists.");
		}
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		if (industrialProtocol != null && industrialProtocol.LoggingCycles.Count == 0)
		{
			cycle.Id = 1;
		}
		else
		{
			cycle.Id = IndusProtocol.LoggingCycles.Max((LoggingCycle loggingCycle_0) => loggingCycle_0.Id) + 1;
		}
		IndusProtocol?.LoggingCycles.Add(cycle);
	}

	public static void SaveCycles(List<LoggingCycle> cycles)
	{
		IndusProtocol.LoggingCycles = cycles;
	}

	public static void EditCycle(LoggingCycle cycle)
	{
		
		IndustrialProtocol? industrialProtocol = IndusProtocol;
		object obj;
		if (industrialProtocol == null)
		{
			obj = null;
		}
		else
		{
			obj = industrialProtocol.LoggingCycles.FirstOrDefault((LoggingCycle loggingCycle_0) => loggingCycle_0.Id == cycle.Id);
			if (obj != null)
			{
				LoggingCycle loggingCycle = IndusProtocol?.LoggingCycles.FirstOrDefault((LoggingCycle loggingCycle_0) => loggingCycle_0.CycleTime == cycle.CycleTime && loggingCycle_0.CycleUnit == cycle.CycleUnit);
				if (loggingCycle != null && loggingCycle.Id != cycle.Id)
				{
					throw new InvalidOperationException("This cycle time already exists.");
				}
				((LoggingCycle)obj).Id = cycle.Id;
				((LoggingCycle)obj).CycleTime = cycle.CycleTime;
				((LoggingCycle)obj).CycleUnit = cycle.CycleUnit;
				return;
			}
		}
		throw new InvalidOperationException("Cycle time does not exist.");
	}

	public static LoggingCycle CopyCycle(LoggingCycle Cycle)
	{
		
		LoggingCycle loggingCycle = IndusProtocol?.LoggingCycles.FirstOrDefault((LoggingCycle loggingCycle_0) => loggingCycle_0.Id == Cycle.Id);
		if (loggingCycle == null)
		{
			throw new InvalidOperationException("Cycle time does not exist.");
		}
		LoggingCycle loggingCycle2 = new LoggingCycle
		{
			Id = IndusProtocol.LoggingCycles.Max((LoggingCycle loggingCycle_0) => loggingCycle_0.Id) + 1,
			CycleTime = loggingCycle.CycleTime,
			CycleUnit = loggingCycle.CycleUnit
		};
		loggingCycle2.CycleTime = IndusProtocol.LoggingCycles.Max((LoggingCycle loggingCycle_0) => loggingCycle_0.CycleTime);
		loggingCycle2.CycleTime++;
		IndusProtocol?.LoggingCycles.Add(loggingCycle2);
		return loggingCycle2;
	}

	public static void RemoveCycle(LoggingCycle cycle)
	{
		
		LoggingCycle loggingCycle = IndusProtocol?.LoggingCycles.FirstOrDefault((LoggingCycle loggingCycle_0) => loggingCycle_0.Id == cycle.Id);
		if (loggingCycle != null)
		{
			IndusProtocol?.LoggingCycles.Remove(loggingCycle);
		}
	}

	public static List<LoggingCycle> GetCycles()
	{
		IndusProtocol = IndusProtocol ?? new IndustrialProtocol();
		return IndusProtocol.LoggingCycles;
	}

	public IndustrialProtocol GetIndustrialProtocol()
	{
		return IndusProtocol;
	}

	public static void SaveHistoricalData(List<DataLog> dataLogs)
	{
		IndusProtocol.DataLogs = dataLogs;
	}

	public int UpdateLoggingTags(Channel channel_0, bool IsDelete = false)
	{
		
		int result = 0;
		foreach (DataLog dataLog in IndusProtocol.DataLogs)
		{
			List<LoggingTag> list = dataLog.LoggingTags.Where((LoggingTag loggingtg) => loggingtg.ChannelId == channel_0.Id).ToList();
			if (list == null || list.Count <= 0)
			{
				continue;
			}
			if (IsDelete)
			{
				foreach (LoggingTag item in list)
				{
					dataLog.LoggingTags.Remove(item);
				}
				continue;
			}
			result = list.Count;
			foreach (LoggingTag item2 in list)
			{
				string[] array = item2.TagName.Split('.');
				item2.TagName = $"{channel_0.Name}.{array[1]}.{array[2]}.{array[3]}";
			}
		}
		return result;
	}

	public int UpdateLoggingTags(Device device_0, bool IsDelete = false)
	{
		
		int result = 0;
		foreach (DataLog item in IndusProtocol?.DataLogs)
		{
			List<LoggingTag> list = item.LoggingTags.Where((LoggingTag loggingtg) => loggingtg.ChannelId == device_0.ChannelId && loggingtg.DeviceId == device_0.Id).ToList();
			if (list == null)
			{
				continue;
			}
			if (IsDelete)
			{
				foreach (LoggingTag item2 in list)
				{
					item.LoggingTags.Remove(item2);
				}
				continue;
			}
			result = list.Count;
			foreach (LoggingTag item3 in list)
			{
				string[] array = item3.TagName.Split('.');
				item3.TagName = $"{array[0]}.{device_0.Name}.{array[2]}.{array[3]}";
			}
		}
		return result;
	}

	public int UpdateLoggingTags(Group group_0, bool IsDelete = false)
	{
		
		int result = 0;
		foreach (DataLog item in IndusProtocol?.DataLogs)
		{
			List<LoggingTag> list = item.LoggingTags.Where((LoggingTag loggingtg) => loggingtg.ChannelId == group_0.ChannelId && loggingtg.DeviceId == group_0.DeviceId && loggingtg.GroupId == group_0.Id).ToList();
			if (list == null)
			{
				continue;
			}
			if (IsDelete)
			{
				foreach (LoggingTag item2 in list)
				{
					item.LoggingTags.Remove(item2);
				}
				continue;
			}
			result = list.Count;
			foreach (LoggingTag item3 in list)
			{
				string[] array = item3.TagName.Split('.');
				item3.TagName = $"{array[0]}.{array[1]}.{group_0.Name}.{array[3]}";
			}
		}
		return result;
	}

	public int UpdateLoggingTags(Tag tg, bool IsDelete = false)
	{
		
		int result = 0;
		foreach (DataLog item in IndusProtocol?.DataLogs)
		{
			LoggingTag loggingTag = item.LoggingTags.FirstOrDefault((LoggingTag loggingtg) => loggingtg.ChannelId == tg.ChannelId && loggingtg.DeviceId == tg.DeviceId && loggingtg.GroupId == tg.GroupId && loggingtg.TagId == tg.Id);
			if (loggingTag != null)
			{
				if (IsDelete)
				{
					item.LoggingTags.Remove(loggingTag);
					continue;
				}
				string[] array = loggingTag.TagName.Split('.');
				loggingTag.TagName = $"{array[0]}.{array[1]}.{array[2]}.{tg.Name}";
				result = 1;
			}
		}
		return result;
	}

	public static void AddAsrsTable(AsrsTable table)
	{
		
		if (IndusProtocol != null && IndusProtocol?.AsrsServer != null)
		{
			if (string.IsNullOrEmpty(table.Name) || string.IsNullOrEmpty(table.Name))
			{
				throw new Exception("Table name cannot be empty.");
			}
			if (IndusProtocol?.AsrsServer.Tables.Where((AsrsTable asrsTable_0) => asrsTable_0.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null)
			{
				throw new Exception("Table name(" + table.Name + ") already exists");
			}
			table.Id = IndusProtocol.AsrsServer.Tables.Count + 1;
			IndusProtocol?.AsrsServer.Tables.Add(table);
		}
	}

	public static void EditAsrsTable(AsrsTable table)
	{
		
		if (IndusProtocol != null && IndusProtocol?.AsrsServer != null)
		{
			if (string.IsNullOrEmpty(table.Name) || string.IsNullOrEmpty(table.Name))
			{
				throw new Exception("Table name cannot be empty.");
			}
			if (IndusProtocol?.AsrsServer.Tables.Where((AsrsTable asrsTable_0) => asrsTable_0.Id != table.Id && asrsTable_0.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null)
			{
				throw new Exception("Table name(" + table.Name + ") already exists");
			}
			AsrsTable asrsTable = IndusProtocol?.AsrsServer.Tables.Where((AsrsTable asrsTable_0) => asrsTable_0.Id == table.Id).FirstOrDefault();
			if (asrsTable != null)
			{
				asrsTable.Name = table.Name;
			}
		}
	}

	public static void RemoveAsrsTable(int y)
	{
		if (IndusProtocol != null && IndusProtocol?.AsrsServer != null)
		{
			AsrsTable asrsTable = IndusProtocol?.AsrsServer.Tables.Where((AsrsTable asrsTable_0) => asrsTable_0.Id == y).FirstOrDefault();
			if (asrsTable != null)
			{
				IndusProtocol?.AsrsServer.Tables.Remove(asrsTable);
			}
		}
	}

	public static void RemoveAsrsTableAll()
	{
		if (IndusProtocol != null && IndusProtocol?.AsrsServer != null)
		{
			IndusProtocol?.AsrsServer.Tables.Clear();
		}
	}

	public static void Invalidate(AsrsTable table)
	{
		
		if (IndusProtocol == null || IndusProtocol?.AsrsServer == null)
		{
			return;
		}
		if (table.Id == 0)
		{
			if (string.IsNullOrEmpty(table.Name) || string.IsNullOrEmpty(table.Name))
			{
				throw new Exception("Table name cannot be empty.");
			}
			if (IndusProtocol?.AsrsServer.Tables.Where((AsrsTable asrsTable_0) => asrsTable_0.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null)
			{
				throw new Exception("Table name(" + table.Name + ") already exists");
			}
		}
		else
		{
			if (string.IsNullOrEmpty(table.Name) || string.IsNullOrEmpty(table.Name))
			{
				throw new Exception("Table name cannot be empty.");
			}
			if (IndusProtocol?.AsrsServer.Tables.Where((AsrsTable asrsTable_0) => asrsTable_0.Id != table.Id && asrsTable_0.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() != null)
			{
				throw new Exception("Table name(" + table.Name + ") already exists");
			}
		}
	}

	public static AsrsTable? GetAsrsTable(int y)
	{
		AsrsTable result = null;
		if (IndusProtocol != null && IndusProtocol?.AsrsServer != null)
		{
			result = IndusProtocol?.AsrsServer.Tables.Where((AsrsTable asrsTable_0) => asrsTable_0.Id == y).FirstOrDefault();
		}
		return result;
	}
}
