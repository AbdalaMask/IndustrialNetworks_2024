using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace NetStudio.Common.Historiant;

public class LoggingTag
{
	public string LogName { get; set; }

	public int Id { get; set; }

	public int ChannelId { get; set; }

	public int DeviceId { get; set; }

	public int GroupId { get; set; }

	public int TagId { get; set; }

	[JsonIgnore]
	[XmlIgnore]
	public DateTime DTime { get; set; } = DateTime.MinValue;


	[JsonIgnore]
	[XmlIgnore]
	public decimal Value { get; set; }

	public string TagName { get; set; } = "<Double click>";


	public LoggingMode Mode { get; set; } = LoggingMode.OnChange;


	public LoggingType LogType { get; set; }

	public LoggingCycle? Cycle { get; set; }

	[XmlIgnore]
	[JsonIgnore]
	public string CycleName
	{
		get
		{
			string result = string.Empty;
			if (Mode == LoggingMode.Cyclic)
			{
				result = ((Cycle == null || Cycle.CycleTime <= 0) ? "<Double click>" : Cycle.CycleName);
			}
			return result;
		}
	}

	public LoggingLimit LoggingLimit { get; set; }

	public decimal HighLimit { get; set; }

	public decimal LowLimit { get; set; }

	public string? Description { get; set; }

	[JsonIgnore]
	[XmlIgnore]
	public float Offset { get; set; }

	public LoggingTag()
	{
		Cycle = new LoggingCycle();
	}

	public override string ToString()
	{
		return $"{TagName}.{Mode}.{CycleName}.{LoggingLimit}.{LowLimit}.{Description}";
	}
}
