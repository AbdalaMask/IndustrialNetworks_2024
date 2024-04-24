using System.ServiceModel;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.DriverComm.Models;

namespace NetStudio.DriverComm.Interfaces;

[ServiceContract]
public interface IAppSettingManager
{
	[OperationContract(Name = "GetAppSettings")]
	ApiResponse GetAppSettings();

	[OperationContract(Name = "SetAppSettings")]
	ApiResponse SetAppSettings(AppSettings setting);

	[OperationContract(Name = "GetLicense")]
	ApiResponse GetLicense();

	[OperationContract(Name = "SetLicense")]
	ApiResponse SetLicense(License license);
}
