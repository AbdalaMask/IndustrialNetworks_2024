using System.Threading.Tasks;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Siemens.Models;

namespace NetStudio.Siemens;

public interface IS7Protocol
{
	bool Connect();

	bool Reconnect();

	bool Disconnect();

	Task<bool> ConnectAsync();

	Task<bool> ReconnectAsync();

	Task<bool> DisconnectAsync();

	bool Start();

	bool Stop();

	bool RunMode();

	Task<IPSResult> ReadAsync(ReadPacket RP);

	Task<IPSResult> WriteAsync(WritePacket WP);
}
