using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace NetStudio.Common.Historiant;

public class HistoricalData
{
	[XmlIgnore]
	[JsonIgnore]
	public string DataString { get; set; }

	public LoggingCycle Cycle { get; set; }

	public DataLog DataLog { get; set; }

	public Dictionary<string, List<LoggingTag>>? LoggingTags { get; set; }
}
