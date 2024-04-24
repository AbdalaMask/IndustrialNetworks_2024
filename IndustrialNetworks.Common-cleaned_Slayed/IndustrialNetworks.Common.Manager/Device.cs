using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;

namespace NetStudio.Common.Manager;

public class Device : ICloneable, INotifyPropertyChanged
{
	private bool active = true;

	private DeviceStatus status = DeviceStatus.Disconnected;

	private bool autoReconnect = true;

	public int CompanyId { get; set; }

	public int ChannelId { get; set; }

	public int Id { get; set; }

	public string Name { get; set; }

	public int StationNo { get; set; }

	public byte BaseNo { get; set; }

	public byte SlotNo { get; set; }

	public string? Description { get; set; }

	public int BaseAddress { get; set; } = 1;


	public int BlockSize { get; set; } = 100;


	public ByteOrder ByteOrder { get; set; }

	public List<Group> Groups { get; set; }

	public EthernetAdapter Adapter { get; set; }

	public int DeviceType { get; set; }

	public bool Active
	{
		get
		{
			return active;
		}
		set
		{
			if (active != value)
			{
				active = value;
				NotifyPropertyChanged("Active");
			}
		}
	}

	[JsonIgnore]
	[XmlIgnore]
	public DeviceStatus Status
	{
		get
		{
			return status;
		}
		set
		{
			if (status != value)
			{
				status = value;
				NotifyPropertyChanged("Status");
			}
		}
	}

	public bool AutoReconnect
	{
		get
		{
			return autoReconnect;
		}
		set
		{
			if (autoReconnect != value)
			{
				autoReconnect = value;
				NotifyPropertyChanged("AutoReconnect");
			}
		}
	}

	[JsonIgnore]
	[XmlIgnore]
	public bool ReconnectFlag { get; set; }

	[XmlIgnore]
	[JsonIgnore]
	public DateTime startDisconnected { get; set; } = DateTime.Now;


	public int ConnectRetries { get; set; } = 3;


	public int ReceivingDelay { get; set; }

	[XmlIgnore]
	[JsonIgnore]
	public bool ReconnectManual { get; set; }

	public event PropertyChangedEventHandler? PropertyChanged;

	private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public Device()
	{
		Groups = new List<Group>();
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public override string ToString()
	{
		if (Adapter == null)
		{
			return $"{Name}(Station No.= {StationNo})";
		}
		return Name + Adapter.ToString();
	}

	public string GetInfos()
	{
		string text = $"{Name}(Station No.= {StationNo}, Active={Active}, Block size={BlockSize}, Auto reconnect={AutoReconnect}, Byte Order = {ByteOrder.GetDescription()})";
		if (Adapter != null)
		{
			return text + Adapter.ToString() + ")";
		}
		return text + ")";
	}

	public int GetBlockSize(IpsProtocolType protocol)
	{
		int num = 60;
		switch (protocol)
		{
		case IpsProtocolType.S7_TCP:
			if (Adapter != null && Adapter.Port == 0)
			{
				Adapter.Port = 102;
			}
			break;
		case IpsProtocolType.CNET_XGT_PROTOCOL:
			num = 60;
			ByteOrder = ByteOrder.LittleEndian;
			break;
		case IpsProtocolType.FENET_XGT_PROTOCOL:
			num = 250;
			if (Adapter != null && Adapter.Port == 0)
			{
				Adapter.Port = 2004;
			}
			ByteOrder = ByteOrder.LittleEndian;
			break;
		case IpsProtocolType.MEWTOCOL_PROTOCOL:
			num = 450;
			ByteOrder = ByteOrder.LittleEndian;
			break;
		case IpsProtocolType.MISUBISHI_MC_PROTOCOL:
			num = 960;
			if (Adapter != null && Adapter.Port == 0)
			{
				Adapter.Port = 1025;
			}
			break;
		case IpsProtocolType.DEDICATED1_PROTOCOL:
		case IpsProtocolType.DEDICATED4_PROTOCOL:
			num = 64;
			break;
		case IpsProtocolType.FX_SERIAL_PROTOCOL:
			num = 64;
			break;
		case IpsProtocolType.FINS_TCP_PROTOCOL:
		case IpsProtocolType.FINS_UDP_PROTOCOL:
			num = 490;
			if (Adapter != null && Adapter.Port == 0)
			{
				Adapter.Port = 9600;
			}
			break;
		case IpsProtocolType.HOSTLINK_FINS_PROTOCOL:
			num = 57;
			break;
		case IpsProtocolType.MODBUS_TCP:
			if (num < 100)
			{
				num = 100;
			}
			else if (num > 125)
			{
				num = 125;
			}
			if (Adapter != null && Adapter.Port == 0)
			{
				Adapter.Port = 502;
			}
			break;
		case IpsProtocolType.MODBUS_RTU:
			if (num < 100)
			{
				num = 100;
			}
			else if (num > 125)
			{
				num = 125;
			}
			break;
		case IpsProtocolType.MODBUS_ASCII:
			if (num > 60)
			{
				num = 60;
			}
			else if (num < 50)
			{
				num = 50;
			}
			break;
		case IpsProtocolType.VS_PROTOCOL:
			num = 64;
			if (Adapter != null && Adapter.Port == 0)
			{
				Adapter.Port = 5000;
			}
			break;
		default:
			num = 60;
			break;
		case IpsProtocolType.FATEK_PROTOCOL:
			num = 125;
			if (Adapter != null && Adapter.Port == 0)
			{
				Adapter.Port = 500;
			}
			break;
		case IpsProtocolType.DELTA_ASCII:
			num = 60;
			break;
		case IpsProtocolType.DELTA_RTU:
		case IpsProtocolType.DELTA_TCP:
			num = 100;
			if (Adapter != null && Adapter.Port == 0)
			{
				Adapter.Port = 502;
			}
			break;
		case IpsProtocolType.KEYENCE_MC_PROTOCOL:
			num = 960;
			if (Adapter != null && Adapter.Port == 0)
			{
				Adapter.Port = 5000;
			}
			break;
		case IpsProtocolType.S7_MPI:
		case IpsProtocolType.S7_PPI:
		case IpsProtocolType.ASCII_PROTOCOL:
			break;
		}
		return num;
	}
}
