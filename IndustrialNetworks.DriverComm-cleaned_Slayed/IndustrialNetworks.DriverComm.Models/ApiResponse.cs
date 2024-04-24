using System.Collections.Generic;
using System.Runtime.Serialization;
using NetStudio.Common.Alarms;
using NetStudio.Common.AsrsLink;
using NetStudio.Common.Historiant;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;

namespace NetStudio.DriverComm.Models;

[KnownType(typeof(List<DataLog>))]
[KnownType(typeof(List<Tag>))]
[KnownType(typeof(List<AlarmClass>))]
[KnownType(typeof(AnalogAlarm))]
[KnownType(typeof(List<AnalogAlarm>))]
[KnownType(typeof(AsrsTable))]
[KnownType(typeof(List<Group>))]
[KnownType(typeof(AsrsRow))]
[KnownType(typeof(List<AsrsRow>))]
[KnownType(typeof(List<SqlColumnInfo>))]
[KnownType(typeof(IndustrialProtocol))]
[KnownType(typeof(MachineInfo))]
[KnownType(typeof(DataLog))]
[KnownType(typeof(List<AsrsTable>))]
[KnownType(typeof(AlarmClass))]
[KnownType(typeof(License))]
[KnownType(typeof(Channel))]
[KnownType(typeof(Device))]
[KnownType(typeof(Group))]
[KnownType(typeof(Tag))]
[KnownType(typeof(List<Channel>))]
[KnownType(typeof(List<Device>))]
public class ApiResponse
{
	public bool Success { get; set; }

	public string Message { get; set; } = "Read request failed.";


	public object? Data { get; set; }
}
