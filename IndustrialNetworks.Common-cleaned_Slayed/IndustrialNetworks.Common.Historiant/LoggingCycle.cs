using System.Collections.Generic;

namespace NetStudio.Common.Historiant;

public class LoggingCycle
{
	public int Id { get; set; }

	public int CycleTime { get; set; }

	public CycleUnit CycleUnit { get; set; } = CycleUnit.Seconds;


	public string CycleName => $"{CycleTime} {CycleUnit}";

	public int Seconds
	{
		get
		{
            return CycleUnit switch
			{
				CycleUnit.Millisecond => CycleTime / 1000, 
				CycleUnit.Seconds => CycleTime, 
				CycleUnit.Minutes => 60 * CycleTime, 
				CycleUnit.Hours => 3600 * CycleTime, 
				CycleUnit.Days => 86400 * CycleTime, 
				_ => CycleTime, 
			};
		}
	}

	public static List<LoggingCycle> GetDataSource()
	{
		return new List<LoggingCycle>
		{
			new LoggingCycle
			{
				CycleTime = 1,
				CycleUnit = CycleUnit.Seconds
			},
			new LoggingCycle
			{
				CycleTime = 2,
				CycleUnit = CycleUnit.Seconds
			},
			new LoggingCycle
			{
				CycleTime = 3,
				CycleUnit = CycleUnit.Seconds
			},
			new LoggingCycle
			{
				CycleTime = 4,
				CycleUnit = CycleUnit.Seconds
			},
			new LoggingCycle
			{
				CycleTime = 5,
				CycleUnit = CycleUnit.Seconds
			},
			new LoggingCycle
			{
				CycleTime = 10,
				CycleUnit = CycleUnit.Seconds
			},
			new LoggingCycle
			{
				CycleTime = 15,
				CycleUnit = CycleUnit.Seconds
			},
			new LoggingCycle
			{
				CycleTime = 1,
				CycleUnit = CycleUnit.Minutes
			},
			new LoggingCycle
			{
				CycleTime = 2,
				CycleUnit = CycleUnit.Minutes
			},
			new LoggingCycle
			{
				CycleTime = 5,
				CycleUnit = CycleUnit.Minutes
			},
			new LoggingCycle
			{
				CycleTime = 10,
				CycleUnit = CycleUnit.Minutes
			},
			new LoggingCycle
			{
				CycleTime = 1,
				CycleUnit = CycleUnit.Hours
			},
			new LoggingCycle
			{
				CycleTime = 2,
				CycleUnit = CycleUnit.Hours
			},
			new LoggingCycle
			{
				CycleTime = 5,
				CycleUnit = CycleUnit.Hours
			},
			new LoggingCycle
			{
				CycleTime = 1,
				CycleUnit = CycleUnit.Days
			}
		};
	}
}
