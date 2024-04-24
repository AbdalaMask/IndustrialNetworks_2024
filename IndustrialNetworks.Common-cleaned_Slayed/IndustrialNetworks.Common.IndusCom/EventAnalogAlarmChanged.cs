using System.ComponentModel;
using NetStudio.Common.Alarms;

namespace NetStudio.Common.IndusCom;

public delegate void EventAnalogAlarmChanged(AnalogAlarm? alarm, ListChangedType type);
