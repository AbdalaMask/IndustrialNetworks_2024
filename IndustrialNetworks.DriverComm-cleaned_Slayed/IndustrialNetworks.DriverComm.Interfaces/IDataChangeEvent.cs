using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading.Tasks;
using NetStudio.Common.Alarms;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;

namespace NetStudio.DriverComm.Interfaces;

[ServiceKnownType(typeof(WORD[]))]
[ServiceKnownType(typeof(DINT[]))]
[ServiceKnownType(typeof(UDINT[]))]
[ServiceKnownType(typeof(DWORD[]))]
[ServiceKnownType(typeof(UINT[]))]
[ServiceKnownType(typeof(LINT[]))]
[ServiceKnownType(typeof(ULINT[]))]
[ServiceKnownType(typeof(LWORD[]))]
[ServiceKnownType(typeof(LREAL[]))]
[ServiceKnownType(typeof(STRING[]))]
[ServiceKnownType(typeof(TIME16[]))]
[ServiceKnownType(typeof(TIME32[]))]
[ServiceKnownType(typeof(INT[]))]
[ServiceKnownType(typeof(BYTE))]
[ServiceKnownType(typeof(INT))]
[ServiceKnownType(typeof(UINT))]
[ServiceKnownType(typeof(WORD))]
[ServiceKnownType(typeof(DINT))]
[ServiceKnownType(typeof(REAL[]))]
[ServiceKnownType(typeof(UDINT))]
[ServiceKnownType(typeof(DWORD))]
[ServiceKnownType(typeof(REAL))]
[ServiceKnownType(typeof(BOOL))]
[ServiceKnownType(typeof(LWORD))]
[ServiceKnownType(typeof(LINT))]
[ServiceKnownType(typeof(ULINT))]
[ServiceKnownType(typeof(LREAL))]
[ServiceKnownType(typeof(STRING))]
[ServiceKnownType(typeof(TIME16))]
[ServiceKnownType(typeof(TIME32))]
[ServiceKnownType(typeof(BOOL[]))]
[ServiceKnownType(typeof(BYTE[]))]
public interface IDataChangeEvent
{
	[OperationContract(IsOneWay = true, Name = "DeviceChanged")]
	void DeviceChanged(string deviceName, DeviceStatus Status, bool active, bool autoReconnect);

	[OperationContract(Name = "DeviceChangedAsync")]
	Task DeviceChangedAsync(string deviceName, DeviceStatus Status, bool active, bool autoReconnect);

	[OperationContract(Name = "TagChanged")]
	void TagChanged(string tagName, TagStatus status, dynamic value);

	[OperationContract(Name = "TagChangedAsync")]
	Task TagChangedAsync(string tagName, TagStatus status, dynamic value);

	[OperationContract(IsOneWay = true, Name = "TagsChanged")]
	void TagsChanged(List<Tag> tags);

	[OperationContract(Name = "AnalogAlarmChangedAsync")]
	void AnalogAlarmChangedAsync(AnalogAlarm alarm, ListChangedType listChangedType);

	[OperationContract(IsOneWay = true, Name = "AnalogAlarmListChanged")]
	void AnalogAlarmListChanged(List<AnalogAlarm> alarms);

	[OperationContract(Name = "LogChangedAsync")]
	void LogChangedAsync(IpsLog ipsLog_0);

	[OperationContract(IsOneWay = true, Name = "LogListChanged")]
	void LogListChanged(List<IpsLog> logs);
}
