using System;
using System.Collections.Generic;

namespace NetStudio.Common.Manager;

public class Group : ICloneable
{
	public int ChannelId { get; set; }

	public int DeviceId { get; set; }

	public int Id { get; set; }

	public string Name { get; set; }

	public string? Description { get; set; }

	public List<Tag> Tags { get; set; }

	public Group()
	{
		Tags = new List<Tag>();
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
