using System;
using System.Collections.Generic;

namespace NetStudio.Common.AsrsLink;

public class AsrsServer : ICloneable
{
	public int Id { get; set; }

	public string ServerName { get; set; } = "Server name";


	public string DatabaseName { get; set; } = "Database name";


	public string Login { get; set; } = "sa";


	public string Password { get; set; } = "Admin@12345";


	public bool Active { get; set; } = true;


	public bool Synchronized { get; set; } = true;


	public List<AsrsTable> Tables { get; set; }

	public AsrsServer()
	{
		Tables = new List<AsrsTable>();
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
