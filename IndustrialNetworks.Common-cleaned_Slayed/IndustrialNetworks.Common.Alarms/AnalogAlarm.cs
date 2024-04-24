namespace NetStudio.Common.Alarms;

public class AnalogAlarm : DiscreteAlarm
{
	public LimitMode LimitMode { get; set; } = LimitMode.Lower;

}
