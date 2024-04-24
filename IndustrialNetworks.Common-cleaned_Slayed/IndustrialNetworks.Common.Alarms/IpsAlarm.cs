using System.Collections.Generic;

namespace NetStudio.Common.Alarms;

public class IpsAlarm
{
	public List<AlarmClass> AlarmClasses { get; set; }

	public List<AnalogAlarm> AnalogAlarms { get; set; }

	public List<DiscreteAlarm> DiscreteAlarms { get; set; }

	public IpsAlarm()
	{
		AlarmClasses = new List<AlarmClass>();
		AnalogAlarms = new List<AnalogAlarm>();
		DiscreteAlarms = new List<DiscreteAlarm>();
	}
}
