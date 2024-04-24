using System;
using System.IO;

namespace NetStudio.IPS.Entity;

public class LocalAppSettings
{
	public string Directory { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");


	public string FileName { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "IPS.json");


	public string IP { get; set; }

	public int Port { get; set; }

	public string Username { get; set; }

	public string Password { get; set; }

	public bool Mode { get; set; }

	public bool Scaling { get; set; }

	public bool Offset { get; set; }

	public bool Resolution { get; set; }

	public string License { get; set; }

	public LocalAppSettings()
	{
		IP = "127.0.0.1";
		Port = 5012;
		base.MemberwiseClone();
	}
}
