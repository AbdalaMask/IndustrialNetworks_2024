using System;

namespace NetStudio.Common.Manager;

public class ClientInfo
{
	public string Id { get; set; } = Guid.NewGuid().ToString();


	public string Name { get; set; }

	public string IP { get; set; }

	public int Port { get; set; } = 102;


	public override string ToString()
	{
		return $"Id={Id}, Name={Name}, IP Address={IP}";
	}
}
