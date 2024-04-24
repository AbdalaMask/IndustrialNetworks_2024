using System.ComponentModel;

namespace NetStudio.Common.Alarms;

public enum AlarmStatus
{
	[Description("None")]
	None,
	[Description("Acknowledge")]
	Acknowledge
}
