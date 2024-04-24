using System;
using System.IO;
using System.Text.Json.Serialization;
using NetStudio.Common.Files;

namespace NetStudio.Common.Manager;

public class Settings
{
	public string IP { get; set; }

	[JsonConverter(typeof(IntegerJsonConverter))]
	public int Port { get; set; }

	public string Directory { get; set; }

	public string FileName { get; set; }

	public Settings()
	{
		IP = "127.0.0.1";
		Port = 502;
		Directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
		FileName = "IPS";
		base.MemberwiseClone();
	}
}
