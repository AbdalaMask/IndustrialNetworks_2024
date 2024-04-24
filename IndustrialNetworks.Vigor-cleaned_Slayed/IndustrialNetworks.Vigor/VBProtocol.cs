using System;
using System.IO;
using System.Threading.Tasks;
using NetStudio.Common.Authors;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Common.Security;

namespace NetStudio.Vigor;

public class VBProtocol : VBBuilder
{
	private INetworkAdapter adapter;

	private const int FORMAT_READ = 10;

	private const int FORMAT_WRITE = 10;

	public Author Author => new Author();

	public VBProtocol(INetworkAdapter adapter)
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
				if (RP.NumOfBytes > 254)
				{
					throw new OverflowException("The amount of data exceeds the allowed number of devices");
				}
				int num = 2 * RP.NumOfBytes;
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
								Task.Delay(RP.ReceivingDelay);
							}
							text = adapter.ReadString(10 + num);
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
					while ((num2 != RP.SendMsg.Length || text.Length < num || (text.Length >= num && text[0] != '\u0006')) && num3 <= RP.ConnectRetries);
				}
				if (num2 == RP.SendMsg.Length && text.Length != 0 && (text.Length <= 0 || text[0] == '\u0006'))
				{
					string text2 = text.Substring(5, 2);
					if (text2 == "00")
					{
						string s = text.Substring(7, num);
						iPSResult.Values = Convert.FromHexString(s);
						iPSResult.Status = CommStatus.Success;
						iPSResult.Message = "Read request successfully.";
					}
					else
					{
						VBUtility.Validate(text2);
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
				Message = "Write data: failure."
			};
			try
			{
				string text = string.Empty;
				int num = 0;
				int num2 = 0;
				string text2 = ((!WP.IsBit) ? WriteMsg(WP.StationNo, WP.ByteAddress, WP.ValueDec) : WriteBitMsg(WP.StationNo, WP.BitAddress, WP.ValueHex));
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
								Task.Delay(WP.ReceivingDelay);
							}
							text = adapter.ReadString(10);
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
					while ((num != text2.Length || text.Length < 10 || (text.Length >= 10 && text[0] != '\u0006')) && num2 <= WP.ConnectRetries);
				}
				if (num == text2.Length && text.Length != 0 && (text.Length <= 0 || text[0] == '\u0006'))
				{
					string text3 = text.Substring(5, 2);
					if (text3 != "00")
					{
						VBUtility.Validate(text3);
					}
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
