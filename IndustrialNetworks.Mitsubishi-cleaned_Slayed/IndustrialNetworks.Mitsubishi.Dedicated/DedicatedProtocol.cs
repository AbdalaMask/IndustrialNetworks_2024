using System;
using System.IO;
using System.Threading.Tasks;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;

namespace NetStudio.Mitsubishi.Dedicated;

public sealed class DedicatedProtocol
{
	private INetworkAdapter adapter;

	private const int FORMAT_READ = 8;

	private const int FORMAT_WRITE = 5;

	private DedicatedBuilder builder;

	private ControlProcedure ctrlProcedure;

	public DedicatedProtocol(INetworkAdapter adapter, ControlProcedure controlProcedure)
	{
		this.adapter = adapter;
		ctrlProcedure = controlProcedure;
		builder = new DedicatedBuilder(ctrlProcedure);
	}

	 

	public bool Connect()
	{
		
		return adapter.Connect();
	}

	public bool Reconnect()
	{
		
		bool flag = adapter.Disconnect();
		return adapter.Connect() && flag;
	}

	public bool Disconnect()
	{
		return adapter.Disconnect();
	}

	public async Task<bool> ConnectAsync()
	{
		
		return await adapter.ConnectAsync();
	}

	public async Task<bool> ReconnectAsync()
	{
		
		bool disconnect = await adapter.DisconnectAsync();
		return await adapter.ConnectAsync() && disconnect;
	}

	public async Task<bool> DisconnectAsync()
	{
		return await adapter.DisconnectAsync();
	}

	public async Task<IPSResult> ReadAsync(ReadPacket RP)
	{
		
		return await Task.Run(delegate
		{
			IPSResult iPSResult = new IPSResult
			{
				Status = CommStatus.Error,
				Message = "Read request failed."
			};
			try
			{
				int num = 8 + RP.NumOfchars;
				int num2 = 0;
				int num3 = 0;
				string text = string.Empty;
				lock (adapter)
				{
					do
					{
						try
						{
							num3++;
							num2 = adapter.Write(RP.SendMsg);
							if (RP.ReceivingDelay > 0)
							{
								Task.Delay(RP.ReceivingDelay);
							}
							text = adapter.ReadString(num);
						}
						catch (Exception ex)
						{
							if (num3 >= RP.ConnectRetries)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
					}
					while ((num2 != RP.SendMsg.Length || text.Length < num || (text.Length >= num && text[0] != '\u0002')) && num3 <= RP.ConnectRetries);
				}
				if (text.Length != 0 && (text.Length < num || text[0] == '\u0002'))
				{
					switch (text[0])
					{
					default:
						iPSResult.Message = "An unknown error.";
						break;
					case '\u0015':
					{
						string key = text.Substring(5, 2);
						if (builder.ErrorCodes.ContainsKey(key))
						{
							iPSResult.Message = builder.ErrorCodes[key];
						}
						break;
					}
					case '\u0002':
						iPSResult.Values_Hex = text.Substring(5, RP.NumOfchars);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
						break;
					}
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "The communication frame is not in the correct format.";
				}
			}
			catch (Exception ex2)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = ex2.Message;
			}
			return iPSResult;
		});
	}

	public async Task<IPSResult> WriteAsync(WritePacket WP)
	{
	
		return await Task.Run(delegate
		{
			IPSResult iPSResult = new IPSResult
			{
				Status = CommStatus.Error,
				Message = "Read request failed."
			};
			try
			{
				int num = 0;
				int num2 = 0;
				string text = string.Empty;
				string data = builder.WriteMsg(WP);
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(data);
							if (WP.ReceivingDelay > 0)
							{
								Task.Delay(WP.ReceivingDelay);
							}
							text = adapter.ReadString(5);
						}
						catch (Exception ex)
						{
							if (num2 >= WP.ConnectRetries)
							{
								iPSResult.Status = CommStatus.Timeout;
								iPSResult.Message = ex.Message;
								return iPSResult;
							}
						}
					}
					while ((num != WP.ValueHex.Length || text.Length < 5 || (text.Length >= 5 && text[0] != '\u0006')) && num2 <= WP.ConnectRetries);
				}
				if (text.Length != 0 && (text.Length < 5 || text[0] == '\u0006'))
				{
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Write data: successfully.";
				}
				else
				{
					iPSResult.Status = CommStatus.Error;
					iPSResult.Message = "The communication frame is not in the correct format.";
				}
			}
			catch (Exception ex2)
			{
				iPSResult.Status = CommStatus.Error;
				iPSResult.Message = ex2.Message;
			}
			return iPSResult;
		});
	}
}
