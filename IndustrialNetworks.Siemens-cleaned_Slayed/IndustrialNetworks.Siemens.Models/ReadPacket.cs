using System.Collections.Generic;
using NetStudio.Common.Manager;

namespace NetStudio.Siemens.Models;

public sealed class ReadPacket : PacketBase
{
	public byte[] SendBytes { get; set; }

	public List<Tag> Tags { get; set; }

	public ReadPacket()
	{
		Tags = new List<Tag>();
	}

	public override string ToString()
	{
		return $"DBNumber: {base.DBNumber}, Memory: {base.Memory}, Start address: {base.Address}, Quantity: {base.Quantity}";
	}
}
