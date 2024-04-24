using System;
using System.IO;
using System.Threading.Tasks;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;
using NetStudio.Siemens.Models;

namespace NetStudio.Siemens.Serial;

public class PpiProtocol
{
	private INetworkAdapter adapter;

	private const int MAX_RESENT = 3;

	private const int FORMAT_READ = 4;

	private const int FORMAT_WRITE = 1;

	public ConnectionStatus ConnectionStatus { get; set; }

	public int WaitingTime { get; set; }

	public PpiProtocol(INetworkAdapter adapter)
	{
		this.adapter = adapter;
	}

	public void SetWaitingTime(int waitingTime)
	{
		WaitingTime = waitingTime;
	}

 
	public void Connect()
	{
		
		adapter.Connect();
	}

	public void Disconnect()
	{
		adapter.Disconnect();
		ConnectionStatus = ConnectionStatus.Disconnected;
	}

	public async Task ConnectAsync()
	{
		
		await adapter.ConnectAsync();
	}

	public async Task DisconnectAsync()
	{
		await adapter.DisconnectAsync();
		ConnectionStatus = ConnectionStatus.Disconnected;
	}

	public async Task<IPSResult> ReadAsync(ReadPacket RP)
	{
		IPSResult result = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		};
		if (RP.Quantity <= 10)
		{
			return await Task.Run(() => result);
		}
		result = await ReadBigAsync(RP);
		return result;
	}

	private async Task<IPSResult> ReadBigAsync(ReadPacket RP)
	{
		return await Task.Run(() => new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Read request failed."
		});
	}

	public async Task<IPSResult> WriteAsync(ReadPacket RP)
	{
		return await Task.Run(() => new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Write data: failure."
		});
	}
}
