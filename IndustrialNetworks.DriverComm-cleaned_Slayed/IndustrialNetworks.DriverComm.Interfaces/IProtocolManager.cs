using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Models;

namespace NetStudio.DriverComm.Interfaces;

[ServiceContract]
public interface IProtocolManager
{
	Task StartAsync();

	Task StopAsync();

	Task<ApiResponse> RestartAsync();

	Task<IPSResult> WriteTagAsync(string tagName, dynamic value);

	Task<IPSResult> WriteBlockAsync(Block block);

	Task<ApiResponse> ClearLogAsync();

	List<Channel> GetChannels();
}
