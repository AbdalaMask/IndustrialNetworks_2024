using System;
using System.Collections.Generic;

namespace NetStudio.Common.Manager;

public class CList<T> : List<T>, ICloneable
{
	public object Clone()
	{
		return MemberwiseClone();
	}
}
