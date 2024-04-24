using System;
using System.Collections.Generic;

namespace NetStudio.Common.AsrsLink;

public class AsrsTable : ICloneable
{
	public int Id { get; set; }

	public string Name { get; set; }

	public List<AsrsRow> Rows { get; set; }

	public AsrsTable()
	{
		Rows = new List<AsrsRow>();
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
