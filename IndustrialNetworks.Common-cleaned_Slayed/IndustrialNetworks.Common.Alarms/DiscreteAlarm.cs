using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace NetStudio.Common.Alarms;

public class DiscreteAlarm
{
	public int Id { get; set; }

	public string AlarmName { get; set; }

	public string AlarmText { get; set; }

	public string TagName { get; set; }

	public int AlarmClassesId { get; set; }

	public decimal LimitValue { get; set; }

	[XmlIgnore]
	[JsonIgnore]
	public DateTime DTime { get; set; } = DateTime.Now;


	[JsonIgnore]
	[XmlIgnore]
	public AlarmStatus Status { get; set; }

	[JsonIgnore]
	[XmlIgnore]
	[Browsable(false)]
	public AlarmClass AlarmClasses { get; set; }

	[Browsable(false)]
	public int TagId { get; set; }

	[Browsable(false)]
	public int GroupId { get; set; }

	[Browsable(false)]
	public int DeviceId { get; set; }

	[Browsable(false)]
	public int ChannelId { get; set; }

	public bool Logging { get; set; } = true;


	public DiscreteAlarm()
	{
		AlarmName = string.Empty;
		AlarmText = string.Empty;
		AlarmClasses = new AlarmClass();
	}

	public override string ToString()
	{
		return $"{Id}.{AlarmName}.{AlarmText}.{TagName}.{AlarmClassesId}.{LimitValue}";
	}
}
