using System.Collections.Generic;
using NetStudio.Common.Alarms;
using NetStudio.Common.AsrsLink;
using NetStudio.Common.Historiant;

namespace NetStudio.Common.Manager;

public class IndustrialProtocol
{
	public string Copyright { get; set; } = "Industrial Networks";


	public string Developer { get; set; } = "Hoang Van Luu";


	public string Phone { get; set; } = "(+84) 0909-886-483";


	public string Email { get; set; } = "hoangluu.automation@gmail.com";


	public string Youtube { get; set; } = "https://www.youtube.com/NetStudio";


	public List<Channel> Channels { get; set; }

	public List<LoggingCycle> LoggingCycles { get; set; }

	public List<DataLog> DataLogs { get; set; }

	public IpsAlarm Alarms { get; set; }

	public AsrsServer AsrsServer { get; set; }

	public IndustrialProtocol()
	{
		Channels = new List<Channel>();
		LoggingCycles = new List<LoggingCycle>();
		DataLogs = new List<DataLog>();
		Alarms = new IpsAlarm();
		AsrsServer = new AsrsServer();
	}
}
