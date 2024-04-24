using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetStudio.Common.Manager;

public class IpsLog : IComparable<IpsLog>, INotifyPropertyChanged
{
	private DateTime time = DateTime.Now;

	private uint counter;

	public DateTime Time
	{
		get
		{
			return time;
		}
		set
		{
			time = value;
			NotifyPropertyChanged("Time");
		}
	}

	public uint Counter
	{
		get
		{
			return counter;
		}
		set
		{
			counter = value;
			NotifyPropertyChanged("Counter");
		}
	}

	public string Source { get; set; } = "Unknow";


	public string Message { get; set; } = string.Empty;


	public EvenType EvenType { get; set; }

	public IpsLogType LogType { get; set; }

	public event PropertyChangedEventHandler? PropertyChanged;

	private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public int CompareTo(IpsLog? other)
	{
		return DateTime.Compare(Time, other.Time);
	}
}
