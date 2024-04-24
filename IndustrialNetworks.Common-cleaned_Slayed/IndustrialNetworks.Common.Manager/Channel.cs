using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using NetStudio.Common.IndusCom;

namespace NetStudio.Common.Manager;

[XmlInclude(typeof(SerialAdapter))]
[KnownType(typeof(SerialAdapter))]
[KnownType(typeof(EthernetAdapter))]
[XmlInclude(typeof(EthernetAdapter))]
public class Channel : ICloneable
{
	public int Id { get; set; }

	public string Name { get; set; }

	public string? Description { get; set; }

	public ConnectionType ConnectionType { get; set; } = ConnectionType.Serial;


	public Manufacturer Manufacturer { get; set; }

	public IpsProtocolType Protocol { get; set; } = IpsProtocolType.MODBUS_RTU;


	public SerialAdapter Adapter { get; set; }

	public List<Device> Devices { get; set; }

	public Channel()
	{
		Devices = new List<Device>();
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public override string ToString()
	{
		return $"{Name}(Mfg: {Manufacturer.GetDescription()}, Protocol: {Protocol.GetDescription()})";
	}

	public string GetInfos()
	{
		string text = string.Empty;
		try
		{
			text = $"{Name}(Mfg: {Manufacturer.GetDescription()}, Protocol: {Protocol.GetDescription()}, Connection type: {ConnectionType.GetDescription()}";
			text = ((Adapter == null) ? (text + ")") : (text + Adapter.ToString() + ")"));
		}
		catch (Exception)
		{
		}
		return text;
	}
}
