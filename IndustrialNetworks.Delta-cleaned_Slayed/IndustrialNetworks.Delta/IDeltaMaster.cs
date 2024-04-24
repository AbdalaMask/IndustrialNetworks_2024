using System.Threading.Tasks;
using NetStudio.Common.Authors;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Delta.Models;

namespace NetStudio.Delta;

public interface IDeltaMaster
{
	Author Author { get; }

	bool Connect();

	bool Reconnect();

	bool Disconnect();

	Task<bool> ConnectAsync();

	Task<bool> ReconnectAsync();

	Task<bool> DisconnectAsync();

	Task<IPSResult> ReadRegisterAsync(ReadPacket RP);

	Task<IPSResult> ReadStatusAsync(ReadPacket RP);

	Task<IPSResult> WriteCoilAsync(byte slaveAddr, int address, BOOL[] values);

	Task<IPSResult> WriteCoilAsync(WritePacket WP);

	Task<IPSResult> WriteRegisterAsync(WritePacket WP);
}
