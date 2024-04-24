using System.Collections.Generic;

namespace NetStudio.IPS.Entity;

public class DataMonitor
{
	public List<RtTag> DataMonitors { get; set; }

	public DataMonitor()
	{
		DataMonitors = new List<RtTag>();
	}
}
