using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using NetStudio.Common.Alarms;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Models;

namespace NetStudio.DriverComm.Interfaces;

[ServiceKnownType(typeof(BOOL))]
[ServiceKnownType(typeof(DINT))]
[ServiceKnownType(typeof(UDINT))]
[ServiceKnownType(typeof(DWORD))]
[ServiceKnownType(typeof(REAL))]
[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IDataChangeEvent))]
[ServiceKnownType(typeof(LINT))]
[ServiceKnownType(typeof(ULINT))]
[ServiceKnownType(typeof(LREAL))]
[ServiceKnownType(typeof(STRING))]
[ServiceKnownType(typeof(TIME16))]
[ServiceKnownType(typeof(TIME32))]
[ServiceKnownType(typeof(BOOL[]))]
[ServiceKnownType(typeof(BYTE[]))]
[ServiceKnownType(typeof(INT[]))]
[ServiceKnownType(typeof(UINT[]))]
[ServiceKnownType(typeof(WORD[]))]
[ServiceKnownType(typeof(DINT[]))]
[ServiceKnownType(typeof(UDINT[]))]
[ServiceKnownType(typeof(UINT))]
[ServiceKnownType(typeof(LWORD))]
[ServiceKnownType(typeof(BYTE))]
[ServiceKnownType(typeof(INT))]
[ServiceKnownType(typeof(DWORD[]))]
[ServiceKnownType(typeof(WORD))]
[ServiceKnownType(typeof(REAL[]))]
[ServiceKnownType(typeof(LINT[]))]
[ServiceKnownType(typeof(ULINT[]))]
[ServiceKnownType(typeof(LWORD[]))]
[ServiceKnownType(typeof(LREAL[]))]
[ServiceKnownType(typeof(STRING[]))]
[ServiceKnownType(typeof(TIME16[]))]
[ServiceKnownType(typeof(TIME32[]))]
public interface IDriverServerManager
{
	[OperationContract(IsOneWay = false)]
	Task<bool> Connect(ClientInfo clientInfo);

	[OperationContract(IsOneWay = false)]
	Task<bool> Disconnect(ClientInfo clientInfo);

	[OperationContract(IsOneWay = false)]
	Task<IPSResult> WriteTag(string tagName, dynamic value);

	[OperationContract(IsOneWay = false)]
	Task<IPSResult> WriteBlock(Block block);

	[OperationContract(IsOneWay = false)]
	Task<ApiResponse> ClearLog();

	[OperationContract(IsOneWay = false)]
	Task<ApiResponse> StartAsync();

	[OperationContract(IsOneWay = false)]
	Task<ApiResponse> StopAsync();

	[OperationContract(IsOneWay = false)]
	Task<ApiResponse> Restart();

	[OperationContract(IsOneWay = false)]
	Task<ApiResponse> CheckConnectAsync();

	[OperationContract(IsOneWay = false, Name = "GetChannels")]
	List<Channel>? GetChannels();

	[OperationContract(IsOneWay = false, Name = "GetChannelsAsync")]
	Task<List<Channel>?> GetChannelsAsync();

	[OperationContract(IsOneWay = false, Name = "GetAlarmClasses")]
	List<AlarmClass> GetAlarmClasses();

	[OperationContract(IsOneWay = false, Name = "GetAlarmClassesAsync")]
	Task<List<AlarmClass>> GetAlarmClassesAsync();

	[OperationContract(IsOneWay = false, Name = "GetAlarms")]
	List<AnalogAlarm> GetAlarms();

	[OperationContract(IsOneWay = false, Name = "GetAlarmsAsync")]
	Task<List<AnalogAlarm>> GetAlarmsAsync();

	[OperationContract(IsOneWay = false, Name = "OnAcknowledge")]
	ApiResponse OnAcknowledge(AnalogAlarm alarm);

	[OperationContract(IsOneWay = false, Name = "OnAcknowledgeAsync")]
	Task<ApiResponse> OnAcknowledgeAsync(AnalogAlarm alarm);

	[OperationContract(IsOneWay = false, Name = "OnAcknowledgeAll")]
	ApiResponse OnAcknowledgeAll(List<AnalogAlarm> alarms);

	[OperationContract(IsOneWay = false, Name = "OnAcknowledgeAllAsync")]
	Task<ApiResponse> OnAcknowledgeAllAsync(List<AnalogAlarm> alarms);
}
