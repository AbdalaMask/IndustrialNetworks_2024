using System;
using System.IO;
using System.Threading.Tasks;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;

namespace NetStudio.Mitsubishi.FXSerial;

public class FXSerialProtocol : FXSerialBuilder
{
	private INetworkAdapter adapter;

	private const int FORMAT_READ = 4;

	private const int FORMAT_WRITE = 1;

	public FXSerialProtocol(INetworkAdapter adapter)
	{
		this.adapter = adapter;
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

	public IPSResult OnInitializeCommunication()
	{
		IPSResult iPSResult = new IPSResult
		{
			Status = CommStatus.Error,
			Message = "Initialization failed."
		};
		string text = string.Empty;
		int num = 0;
		int num2 = 0;
		int num3 = 3;
		lock (adapter)
		{
			do
			{
				try
				{
					num2++;
					num = adapter.Write(new byte[1] { 5 });
					text = adapter.ReadString(1);
				}
				catch (Exception ex)
				{
					if (num2 >= num3)
					{
						iPSResult.Status = CommStatus.Timeout;
						iPSResult.Message = ex.Message;
						return iPSResult;
					}
				}
			}
			while ((num != 1 || text.Length < 1 || (text.Length > 0 && text[0] != '\u0006')) && num2 <= num3);
		}
		if (num == 1 && text.Length != 0 && (text.Length <= 0 || text[0] == '\u0006'))
		{
			iPSResult.Status = CommStatus.Success;
			iPSResult.Message = "Write data: successfully.";
		}
		else
		{
			iPSResult.Status = CommStatus.Error;
			iPSResult.Message = "The communication frame is not in the correct format.";
		}
		return iPSResult;
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
				int num = 4 + RP.NumOfchars;
				string text = string.Empty;
				int num2 = 0;
				int num3 = 0;
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
								Task.Delay(RP.ReceivingDelay).Wait();
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
				if (num2 == RP.SendMsg.Length && text.Length != 0 && (text.Length <= 0 || text[0] == '\u0002'))
				{
					iPSResult.Values_Hex = text.Substring(1, RP.NumOfchars);
					iPSResult.Status = CommStatus.Success;
					iPSResult.Message = "Read request successfully.";
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
				Message = "Write data: failure."
			};
			try
			{
				string text = string.Empty;
				int num = 0;
				int num2 = 0;
				string text2;
				if (WP.IsBit)
				{
					text2 = WriteBitMsg(WP.StartAddress, WP.ValueHex);
				}
				else
				{
					byte numOfBytes = (byte)(WP.ValueHex.Length / 2);
					text2 = WriteBytesMsg(WP.StartAddress, numOfBytes, WP.ValueHex);
				}
				lock (adapter)
				{
					do
					{
						try
						{
							num2++;
							num = adapter.Write(text2);
							if (WP.ReceivingDelay > 0)
							{
								Task.Delay(WP.ReceivingDelay).Wait();
							}
							text = adapter.ReadString(1);
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
					while ((num != text2.Length || text.Length < 1 || (text.Length == 1 && text[0] != '\u0006')) && num2 <= WP.ConnectRetries);
				}
				if (num == text2.Length && text.Length != 0 && (text.Length <= 0 || text[0] == '\u0006'))
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
