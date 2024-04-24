using System;
using System.Collections.Generic;

namespace NetStudio.IPS.Entity;

public class RtGroup : ICloneable
{
	public int ChannelId { get; set; }

	public int DeviceId { get; set; }

	public int Id { get; set; }

	public string Name { get; set; }

	public string? Description { get; set; }

	public List<RtTag> RtTags { get; set; }

	public RtGroup()
	{
		RtTags = new List<RtTag>();
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
