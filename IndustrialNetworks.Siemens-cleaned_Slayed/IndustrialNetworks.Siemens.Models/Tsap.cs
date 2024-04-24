using System;

namespace NetStudio.Siemens.Models;

public class Tsap
{
	public byte FirstByte { get; set; }

	public byte SecondByte { get; set; }

	public Tsap()
	{
	}

	public Tsap(byte firstByte, byte secondByte)
	{
		FirstByte = firstByte;
		SecondByte = secondByte;
	}

	public static Tsap[] GetSingle(CPUType cputype_0)
	{
		return cputype_0 switch
		{
			CPUType.S7200 => new Tsap[2]
			{
				new Tsap(16, 0),
				new Tsap(16, 0)
			}, 
			CPUType.S7200Smart => new Tsap[2]
			{
				new Tsap(16, 0),
				new Tsap(3, 1)
			}, 
			CPUType.S7300 => new Tsap[2]
			{
				new Tsap(1, 0),
				new Tsap(3, 2)
			}, 
			CPUType.S7400 => new Tsap[2]
			{
				new Tsap(16, 0),
				new Tsap(3, 3)
			}, 
			CPUType.S71200 => new Tsap[2]
			{
				new Tsap(16, 0),
				new Tsap(3, 1)
			}, 
			CPUType.S71500 => new Tsap[2]
			{
				new Tsap(16, 0),
				new Tsap(3, 1)
			}, 
			CPUType.WinLC => new Tsap[2]
			{
				new Tsap(1, 0),
				new Tsap(1, 2)
			}, 
			_ => throw new NotSupportedException($"{cputype_0}: This CPU type is not supported."), 
		};
	}
}
