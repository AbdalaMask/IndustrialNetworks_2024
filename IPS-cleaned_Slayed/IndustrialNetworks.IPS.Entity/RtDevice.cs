using System.Collections.Generic;
using NetStudio.Common.IndusCom;

namespace NetStudio.IPS.Entity;

public class RtDevice
{
	public int ChannelId { get; set; }

	public int Id { get; set; }

	public string Name { get; set; }

	public int StationNo { get; set; }

	public string? Description { get; set; }

	public List<RtGroup> RtGroups { get; set; }

	public EthernetAdapter Adapter { get; set; }

	public bool Active { get; set; }

	public RtDevice()
	{
		RtGroups = new List<RtGroup>();
	}
}
