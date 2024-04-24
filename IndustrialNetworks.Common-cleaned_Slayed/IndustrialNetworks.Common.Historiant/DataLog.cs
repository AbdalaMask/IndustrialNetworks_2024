using System.Collections.Generic;

namespace NetStudio.Common.Historiant;

public class DataLog
{
	public int Id { get; set; }

	public string? OldDataLogName { get; set; }

	public string DataLogName { get; set; }

	public StorageType StorageType { get; set; } = StorageType.Database;


	public int DataRecordsPerLog { get; set; } = 500;


	public string? Path { get; set; }

	public string? ServerName { get; set; }

	public string? Login { get; set; }

	public string? Password { get; set; }

	public bool Active { get; set; } = true;


	public string? Description { get; set; }

	public List<LoggingTag> LoggingTags { get; set; }

	public DataLog()
	{
		LoggingTags = new List<LoggingTag>();
	}

	public override string ToString()
	{
		return $"{DataLogName}.{StorageType}.{DataRecordsPerLog}.{Path}.{ServerName}.{Login}.{Password}.{Active}.{Description}";
	}
}
