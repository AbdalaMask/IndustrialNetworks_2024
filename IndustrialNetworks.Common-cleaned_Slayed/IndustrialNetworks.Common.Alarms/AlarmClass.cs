using System.Collections.Generic;

namespace NetStudio.Common.Alarms;

public class AlarmClass
{
	public int Id { get; set; }

	public string DisplayName { get; set; } = string.Empty;


	public string Name { get; set; } = string.Empty;


	public string StatusMachine { get; set; } = string.Empty;


	public string? EmailAddress { get; set; }

	public byte BackgroundIncoming { get; set; } = 35;


	public byte TextColorIncoming { get; set; } = 141;


	public byte AcknowledgedColorIncoming { get; set; } = 104;


	public byte BackgroundOutcoming { get; set; } = 35;


	public byte TextColorOutcoming { get; set; } = 164;


	public byte AcknowledgedColorOutcoming { get; set; } = 104;


	public List<AlarmClass> GetAlarmClasses()
	{
		return new List<AlarmClass>
		{
			new AlarmClass
			{
				Id = 1,
				DisplayName = "!",
				Name = "Errors",
				StatusMachine = "Alarm with single-mode acknowledgment",
				BackgroundIncoming = 141,
				TextColorIncoming = 164,
				AcknowledgedColorIncoming = 79,
				BackgroundOutcoming = 141,
				TextColorOutcoming = 164,
				AcknowledgedColorOutcoming = 79
			},
			new AlarmClass
			{
				Id = 2,
				DisplayName = "",
				Name = "Warnings",
				StatusMachine = "Alarm without acknowledgment",
				BackgroundIncoming = 166,
				TextColorIncoming = 35,
				AcknowledgedColorIncoming = 79,
				BackgroundOutcoming = 141,
				TextColorOutcoming = 164,
				AcknowledgedColorOutcoming = 79
			},
			new AlarmClass
			{
				Id = 3,
				DisplayName = "$",
				Name = "System",
				StatusMachine = "Alarm without acknowledgment",
				BackgroundIncoming = 141,
				TextColorIncoming = 164,
				AcknowledgedColorIncoming = 79,
				BackgroundOutcoming = 141,
				TextColorOutcoming = 164,
				AcknowledgedColorOutcoming = 79
			},
			new AlarmClass
			{
				Id = 4,
				DisplayName = "A",
				Name = "Acknowledgement",
				StatusMachine = "Alarm with single-mode acknowledgment",
				BackgroundIncoming = 141,
				TextColorIncoming = 164,
				AcknowledgedColorIncoming = 79,
				BackgroundOutcoming = 141,
				TextColorOutcoming = 164,
				AcknowledgedColorOutcoming = 79
			},
			new AlarmClass
			{
				Id = 5,
				DisplayName = "NA",
				Name = "No Acknowledgement",
				StatusMachine = "Alarm without acknowledgment",
				BackgroundIncoming = 141,
				TextColorIncoming = 164,
				AcknowledgedColorIncoming = 79,
				BackgroundOutcoming = 141,
				TextColorOutcoming = 164,
				AcknowledgedColorOutcoming = 79
			}
		};
	}
}
